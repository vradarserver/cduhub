// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using McduDotNet.WinWingMcdu;
using Newtonsoft.Json;

namespace McduDotNet
{
    /// <summary>
    /// The internal representation of an MCDU.
    /// </summary>
    class Mcdu : IMcdu
    {
        private readonly int _ProcessingPauseMilliseconds = 40;
        private readonly object _OutputLock = new Object();
        private HidDevice _HidDevice;
        private HidStream _HidStream;
        private readonly byte[] _DisplayPacket = new byte[64];
        private readonly char[] _DisplayCharacterBuffer = new char[1];
        private int _DisplayPacketOffset = 0;
        private string _DisplayDuplicateCheckString;
        private CancellationTokenSource _InputLoopCancellationTokenSource;
        private Task _InputLoopTask;
        private readonly InputReport01 _InputReport01_Previous = new InputReport01();
        private readonly InputReport01 _InputReport01_Current = new InputReport01();
        private (UInt64, UInt64, UInt64) _PreviousInputReport01Digest = (0,0,0);
        private Leds _PreviousLeds;

        /// <inheritdoc/>
        public ProductId ProductId { get; }

        /// <inheritdoc/>
        public Screen Screen { get; }

        /// <inheritdoc/>
        public Compositor Output { get; }

        /// <inheritdoc/>
        public Leds Leds { get; }

        /// <inheritdoc/>
        public event EventHandler<KeyEventArgs> KeyDown;

        /// <inheritdoc/>
        public int XOffset { get; set; }

        /// <inheritdoc/>
        public int YOffset { get; set; }

        /// <summary>
        /// Raises <see cref="KeyDown"/>. Doesn't bother creating args unless something is listening.
        /// </summary>
        /// <param name="createArgs"></param>
        protected virtual void OnKeyDown(Func<KeyEventArgs> createArgs)
        {
            if(KeyDown != null) {
                KeyDown?.Invoke(this, createArgs());
            }
        }

        /// <inheritdoc/>
        public event EventHandler<KeyEventArgs> KeyUp;

        /// <summary>
        /// Raises <see cref="KeyUp"/>. Doesn't bother creating args unless something is listening.
        /// </summary>
        /// <param name="createArgs"></param>
        protected virtual void OnKeyUp(Func<KeyEventArgs> createArgs)
        {
            if(KeyUp != null) {
                KeyUp?.Invoke(this, createArgs());
            }
        }

        /// <summary>
        /// Raised when a disconnection of the MCDU is detected.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Raises <see cref="Disconnected"/>.
        /// </summary>
        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="productId"></param>
        public Mcdu(HidDevice hidDevice, ProductId productId)
        {
            _HidDevice = hidDevice;
            ProductId = productId;
            Leds = new Leds();
            Screen = new Screen();
            Output = new Compositor(Screen);
            HidSharp.DeviceList.Local.Changed += HidSharpDeviceList_Changed;
        }

        /// <inheritdoc/>
        ~Mcdu()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                HidSharp.DeviceList.Local.Changed -= HidSharpDeviceList_Changed;

                _InputLoopCancellationTokenSource?.Cancel();
                _InputLoopTask?.Wait(5000);
                _InputLoopTask = null;

                if(_HidStream != null) {
                    _HidStream.Dispose();
                    _HidStream = null;
                }
            }
        }

        public void Initialise()
        {
            var maxOutputReportLength = _HidDevice.GetMaxOutputReportLength();
            if(maxOutputReportLength < 64) {
                throw new McduException(
                    $"HID device {_HidDevice} reported an invalid max output report length of {maxOutputReportLength}"
                );
            }
            if(!_HidDevice.TryOpen(out _HidStream)) {
                throw new McduException($"Could not open a stream to {_HidDevice}");
            }

            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => InputLoop(_InputLoopCancellationTokenSource.Token));

            UseMobiFlightInitialisationSequence();
            RefreshLeds();
        }

        /// <inheritdoc/>
        public void Cleanup()
        {
            Screen.Clear();
            Leds.TurnAllOn(false);
            Leds.Brightness = .5;
            RefreshDisplay();
            RefreshLeds();
        }

        private void UseMobiFlightInitialisationSequence()
        {
            SendMobiFlightf0Initialisation();
            SendBacklightInitialisation();
        }

        private void SendMobiFlightf0Initialisation()
        {
            var packets = new string[] {
                "f000013832bb00001e0100005f633100000000000032bb0000180100005f6331000008000000340018000e00180032bb0000190100005f633100000e00000000",
                "f0000238000000010005000000020000000000000032bb0000190100005f633100000e000000010006000000030000000000000032bb00001901000000000000",
                "f00003385f633100000e0000000200000000ff040000000000000032bb0000190100005f633100000e000000020000a5ffff050000000000000032bb00000000",
                "f00004380000190100005f633100000e0000000200ffffffff060000000000000032bb0000190100005f633100000e0000000200ffff00ff0700000000000000",
                "f00005380000000032bb0000190100005f633100000e00000002003dff00ff080000000000000032bb0000190100005f633100000e0000000200ff6300000000",
                "f0000638ffff090000000000000032bb0000190100005f633100000e00000002000000ffff0a0000000000000032bb0000190100005f633100000e0000000000",
                "f00007380000020000ffffff0b0000000000000032bb0000190100005f633100000e0000000200425c61ff0c0000000000000032bb0000190100005f00000000",
                "f0000838633100000e0000000200777777ff0d0000000000000032bb0000190100005f633100000e00000002005e7379ff0e0000000000000032bb0000000000",
                "f000093800190100005f633100000e0000000300000000ff0f0000000000000032bb0000190100005f633100000e000000030000a5ffff100000000000000000",
                "f0000a3800000032bb0000190100005f633100000e0000000300ffffffff110000000000000032bb0000190100005f633100000e0000000300ffff0000000000",
                "f0000b38ff120000000000000032bb0000190100005f633100000e00000003003dff00ff130000000000000032bb0000190100005f633100000e000000000000",
                "f0000c38000300ff63ffff140000000000000032bb0000190100005f633100000e00000003000000ffff150000000000000032bb0000190100005f6300000000",
                "f0000d383100000e000000030000ffffff160000000000000032bb0000190100005f633100000e0000000300425c61ff170000000000000032bb000000000000",
                "f0000e38190100005f633100000e0000000300777777ff180000000000000032bb0000190100005f633100000e00000003005e7379ff19000000000000000000",
                "f0000f38000032bb0000190100005f633100000e0000000400000000001a0000000000000032bb0000190100005f633100000e00000004000100000000000000",
                "f00010381b0000000000000032bb0000190100005f633100000e0000000400020000001c0000000000000032bb00001a0100005f633100000100000000000000",
                "f00011120232bb00001c0100005f6331000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
            };
            foreach(var packet in packets) {
                SendStringPacket(packet);
            }
        }

        private void SendBacklightInitialisation()
        {
            var packets = new string[] {
                "0232bb0000034900cc0000000000", // <-- set button backlight to 0xcc out of 0xff
                "0232bb0000034901ff0000000000", // <-- set display backlight to full
            };
            foreach(var packet in packets) {
                SendStringPacket(packet);
            }
        }

        /// <inheritdoc/>
        public void UseFont(McduFontFile fontFileContent, bool useCorrectAspectRatio)
        {
            var packetMapJson = Encoding.UTF8.GetString(
                CduResources.WinwingMcduFontPacketMapJson
            );
            var packetMap = JsonConvert.DeserializeObject<McduFontPacketMap>(packetMapJson);
            packetMap.OverwritePacketsWithFontFileContent(
                XOffset + 0x24,
                YOffset + 0x14,
                !useCorrectAspectRatio
                    ? fontFileContent.NormalDimensions
                    : fontFileContent.CorrectAspectRatioDimensions,
                Metrics.DisplayWidthPixels,
                Metrics.DisplayHeightPixels,
                fontFileContent?.LargeGlyphs,
                fontFileContent?.SmallGlyphs
            );
            foreach(var packet in packetMap.Packets) {
                SendStringPacket(packet);
            }
        }

        /// <inheritdoc/>
        public void RefreshDisplay(bool skipDuplicateCheck = false)
        {
            lock(_OutputLock) {
                var duplicateCheckString = Screen.BuildDuplicateCheckString();
                if(skipDuplicateCheck || _DisplayDuplicateCheckString != duplicateCheckString) {
                    InitialiseDisplayPacket();
                    for(var rowIdx = 0;rowIdx < Screen.Rows.Length;++rowIdx) {
                        var row = Screen.Rows[rowIdx];
                        for(var cellIdx = 0;cellIdx < row.Cells.Length;++cellIdx) {
                            var cell = row.Cells[cellIdx];
                            AddCellToDisplayPacket(
                                cell,
                                isFirstCell: rowIdx == 0 && cellIdx == 0,
                                isLastCell:  rowIdx + 1 == Screen.Rows.Length && cellIdx + 1 == row.Cells.Length
                            );
                        }
                    }
                    PadAndSendDisplayPacket();
                    _DisplayDuplicateCheckString = duplicateCheckString;

                    if(_ProcessingPauseMilliseconds > 0) {
                        // If we send packets too quickly then the device can freak out. I don't see any errors coming
                        // back in WireShark, it just doesn't update the screen properly. This forces a pause so that
                        // a set of very fast updates won't corrupt the display.
                        Thread.Sleep(_ProcessingPauseMilliseconds);
                    }
                }
            }
        }

        public void RefreshLeds(bool skipDuplicateCheck = false)
        {
            if(skipDuplicateCheck || !(_PreviousLeds?.Equals(Leds) ?? false)) {
                void sendBright(double? previous, double current, byte indicatorCode)
                {
                    if(previous != current) {
                        SendLedOrBrightnessPacket(indicatorCode, (byte)(0xff * current));
                    }
                }
                void sendLight(bool? previous, bool current, byte indicatorCode)
                {
                    if(previous != current) {
                        SendLedOrBrightnessPacket(indicatorCode, current ? (byte)1 : (byte)0);
                    }
                }

                sendBright(_PreviousLeds?.Brightness,   Leds.Brightness,    0x02);
                sendLight(_PreviousLeds?.Fail,          Leds.Fail,          0x08);
                sendLight(_PreviousLeds?.Fm,            Leds.Fm,            0x09);
                sendLight(_PreviousLeds?.Mcdu,          Leds.Mcdu,          0x0a);
                sendLight(_PreviousLeds?.Menu,          Leds.Menu,          0x0b);
                sendLight(_PreviousLeds?.Fm1,           Leds.Fm1,           0x0c);
                sendLight(_PreviousLeds?.Ind,           Leds.Ind,           0x0d);
                sendLight(_PreviousLeds?.Rdy,           Leds.Rdy,           0x0e);
                sendLight(_PreviousLeds?.Line,          Leds.Line,          0x0f);
                sendLight(_PreviousLeds?.Fm2,           Leds.Fm2,           0x10);

                if(_PreviousLeds == null) {
                    _PreviousLeds = new Leds();
                }
                _PreviousLeds.CopyFrom(Leds);
            }
        }

        private void InitialiseDisplayPacket()
        {
            _DisplayPacket[0] = 0xF2;
            _DisplayPacketOffset = 1;
        }

        private void AddCellToDisplayPacket(Cell cell, bool isFirstCell, bool isLastCell)
        {
            _DisplayCharacterBuffer[0] = cell.Character;
            var utf8Bytes = Encoding.UTF8.GetBytes(_DisplayCharacterBuffer);
            AddColourAndBytesToDisplayPacket(cell.Colour, cell.Small, isFirstCell, isLastCell);
            for(var chIdx = 0;chIdx < utf8Bytes.Length;++chIdx) {
                AddToDisplayPacketSendWhenFull(utf8Bytes[chIdx]);
            }
        }

        private void AddColourAndBytesToDisplayPacket(Colour colour, bool isSmallFont, bool isFirstCell, bool isLastCell)
        {
            (var b1, var b2) = colour.ToUsbColourAndFontCode(isSmallFont);
            if(isFirstCell) {
                b1 += 1;
            } else if(isLastCell) {
                b1 += 2;
            }
            AddToDisplayPacketSendWhenFull(b1);
            AddToDisplayPacketSendWhenFull(b2);
        }

        private void AddToDisplayPacketSendWhenFull(byte value)
        {
            _DisplayPacket[_DisplayPacketOffset++] = value;
            if(_DisplayPacketOffset == _DisplayPacket.Length) {
                SendPacket(_DisplayPacket);
                InitialiseDisplayPacket();
            }
        }

        private void PadAndSendDisplayPacket()
        {
            if(_DisplayPacketOffset > 1) {
                for(var idx = _DisplayPacketOffset;idx < _DisplayPacket.Length;++idx) {
                    _DisplayPacket[idx] = 0;
                }
                SendPacket(_DisplayPacket);
                InitialiseDisplayPacket();
            }
        }

        private void SendStringPacket(string packet)
        {
            var bytes = packet.ToByteArray();
            SendPacket(bytes);
        }

        private byte[] _LedOrBrightnessPacket = new byte[] {
            0x02, 0x32, 0xbb, 0x00, 0x00, 0x03, 0x49,
            0x00, 0x00,     // <-- these two change with each call
            0x00, 0x00, 0x00, 0x00, 0x00
        };

        internal void SendLedOrBrightnessPacket(byte indicatorCode, byte value)
        {
            lock(_OutputLock) {
                const int indicatorOffset = 7;
                _LedOrBrightnessPacket[indicatorOffset] = indicatorCode;
                _LedOrBrightnessPacket[indicatorOffset + 1] = value;
                SendPacket(_LedOrBrightnessPacket);
            }
        }

        private void SendPacket(byte[] bytes)
        {
            lock(_OutputLock) {
                var stream = _HidStream;
                try {
                    stream?.Write(bytes);
                } catch(IOException) {
                    // This can happen when the device is disconnected mid-write
                    ;
                }
            }
        }

        private void InputLoop(CancellationToken cancellationToken)
        {
            var readBuffer = new byte[InputReport01.PacketLength];

            while(!cancellationToken.IsCancellationRequested) {
                try {
                    var stream = _HidStream;
                    if(stream?.CanRead ?? false) {
                        ClearHidStreamBuffer(stream);
                        stream.ReadTimeout = 1000;
                        try {
                            var bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                            if (bytesRead > 0) {
                                if(readBuffer[0] == 1 && bytesRead >= InputReport01.PacketLength) {
                                    ProcessInputReport1(readBuffer, bytesRead);
                                }
                            }
                        } catch(TimeoutException) {
                            // ugh
                        }
                    }
                } catch(IOException) {
                    // These will happen when the device is disconnected. Under Windows we can look for the Win32
                    // exception and tell for sure, but that won't fly on other platforms. For now I'm going to
                    // assume that any IO exception during the input loop is indicative of the device being
                    // disconnected.
                    //
                    // There is a strong argument for raising the disconnected event here. However, we're also
                    // listening to HidSharp's device change event and raising from there, so we risk a reentrant
                    // raise if we do. Also if the event handler disposes of this MCDU on a disconnect then the
                    // dispose will block waiting for us to finish, but we would be blocking waiting on the event
                    // handler... the wait would timeout eventually but it wouldn't be very nice.
                }
                Thread.Sleep(1);
            }
        }

        private readonly byte[] _ClearBuffer = new byte[InputReport01.PacketLength];
        private void ClearHidStreamBuffer(HidStream stream)
        {
            stream.ReadTimeout = 1;
            try {
                while(stream.Read(_ClearBuffer, 0, _ClearBuffer.Length) > 0) {
                    ;
                }
            } catch(TimeoutException) {
                // ugh
            }
        }

        private void ProcessInputReport1(byte[] readBuffer, int bytesRead)
        {
            _InputReport01_Current.CopyFrom(readBuffer, 0, bytesRead);
            var digest = _InputReport01_Current.ToDigest();
            if(   digest.Item1 != _PreviousInputReport01Digest.Item1
               || digest.Item2 != _PreviousInputReport01Digest.Item2
               || digest.Item3 != _PreviousInputReport01Digest.Item3
            ) {
                try {
                    foreach(Key key in Enum.GetValues(typeof(Key))) {
                        var pressed = _InputReport01_Current.IsKeyPressed(key);
                        var wasPressed = _InputReport01_Previous.IsKeyPressed(key);
                        if(pressed != wasPressed) {
                            if(pressed) {
                                OnKeyDown(() => new KeyEventArgs(key, pressed));
                            } else {
                                OnKeyUp(() => new KeyEventArgs(key, pressed));
                            }
                        }
                    }
                } catch {
                    // Swallow the exception for now - ultimately we want the events raised on a different thread
                }
                _InputReport01_Previous.CopyFrom(_InputReport01_Current);
                _PreviousInputReport01Digest = digest;
            }
        }

        private void HidSharpDeviceList_Changed(object sender, DeviceListChangedEventArgs e)
        {
            var mcduPresent = HidSharp
                .DeviceList
                .Local
                .GetHidDevices()
                .Any(device => device.DevicePath == _HidDevice.DevicePath);
            if(!mcduPresent) {
                OnDisconnected();
            }
        }
    }
}
