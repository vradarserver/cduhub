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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace McduDotNet.WinWing.Mcdu
{
    /// <summary>
    /// The implementation of <see cref="IMcdu"/> for the WinWing MCDU.
    /// </summary>
#pragma warning disable CS0618 // Stop it moaning about IMcdu being flagged as obsolete
    class McduDevice : ICdu, IMcdu
#pragma warning restore CS0618
    {
        private readonly Screen _EmptyScreen = new Screen();
        private HidDevice _HidDevice;
        private HidStream _HidStream;
        private UsbWriter _UsbWriter;
        private ScreenWriter _ScreenWriter;
        private IlluminationWriter _IlluminationWriter;
        private FontWriter _FontWriter;
        private PaletteWriter _PaletteWriter;
        private KeyboardReader _KeyboardReader;
        private CancellationTokenSource _InputLoopCancellationTokenSource;
        private Task _InputLoopTask;

        /// <inheritdoc/>
        public ProductId ProductId => DeviceId.GetLegacyProductId();

        /// <inheritdoc/>
        public DeviceIdentifier DeviceId { get; }

        /// <inheritdoc/>
        public Screen Screen { get; }

        /// <inheritdoc/>
        public Compositor Output { get; }

        /// <inheritdoc/>
        public Leds Leds { get; }

        /// <inheritdoc/>
        public Palette Palette { get; }

        /// <inheritdoc/>
        public event EventHandler<KeyEventArgs> KeyDown;

        /// <inheritdoc/>
        public int XOffset { get; set; }

        /// <inheritdoc/>
        public int YOffset { get; set; }

        private int _DisplayBrightnessPercent = 100;
        /// <inheritdoc/>
        public int DisplayBrightnessPercent
        {
            get => _DisplayBrightnessPercent;
            set {
                var normalised = Percent.Normalise(value);
                if(normalised != DisplayBrightnessPercent) {
                    _DisplayBrightnessPercent = normalised;
                    _IlluminationWriter?.SendDisplayBrightnessPercent(_DisplayBrightnessPercent);
                }
            }
        }

        private int _BacklightBrightnessPercent = 0;
        public int BacklightBrightnessPercent
        {
            get => _BacklightBrightnessPercent;
            set {
                var normalised = Percent.Normalise(value);
                if(normalised != BacklightBrightnessPercent) {
                    _BacklightBrightnessPercent = normalised;
                    _IlluminationWriter.SendBacklightPercent(_BacklightBrightnessPercent);
                }
            }
        }

        private int _LedBrightnessPercent = 100;
        /// <inheritdoc/>
        public int LedBrightnessPercent
        {
            get => _LedBrightnessPercent;
            set {
                var normalised = Percent.Normalise(value);
                if(normalised != LedBrightnessPercent) {
                    _LedBrightnessPercent = normalised;
                    _IlluminationWriter.SendLedBrightnessPercent(_LedBrightnessPercent);
                }
            }
        }

        /// <inheritdoc/>
        public bool HasAmbientLightSensor => true;

        /// <inheritdoc/>
        public int LeftAmbientLightNative { get; private set; }

        /// <inheritdoc/>
        public int RightAmbientLightNative { get; private set; }

        /// <inheritdoc/>
        public int AmbientLightPercent { get; private set; }

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

        /// <inheritdoc/>
        public event EventHandler LeftAmbientLightChanged;

        protected virtual void OnLeftAmbientLightChanged() => LeftAmbientLightChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler RightAmbientLightChanged;

        protected virtual void OnRightAmbientLightChanged() => RightAmbientLightChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler AmbientLightChanged;

        protected virtual void OnAmbientLightChanged() => AmbientLightChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler Disconnected;

        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="deviceId"></param>
        public McduDevice(HidDevice hidDevice, DeviceIdentifier deviceId)
        {
            _HidDevice = hidDevice;
            DeviceId = deviceId;
            Leds = new Leds();
            Screen = new Screen();
            Output = new Compositor(Screen);
            Palette = new Palette();
            HidSharp.DeviceList.Local.Changed += HidSharpDeviceList_Changed;
        }

        /// <inheritdoc/>
        ~McduDevice() => Dispose(false);

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
                _KeyboardReader = null;

                _UsbWriter = null;
                _ScreenWriter = null;
                _IlluminationWriter = null;
                _FontWriter = null;
                _PaletteWriter = null;

                var hidStream = _HidStream;
                _HidStream = null;
                try {
                    hidStream?.Dispose();
                } catch {
                    ;
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
            _UsbWriter = new UsbWriter(_HidStream);
            _KeyboardReader = new KeyboardReader(
                _HidStream,
                ProcessKeyboardEvent,
                ProcessAmbientLightChange
            );

            _ScreenWriter = new ScreenWriter(_UsbWriter);
            _IlluminationWriter = new IlluminationWriter(_UsbWriter);
            _FontWriter = new FontWriter(_UsbWriter);
            _PaletteWriter = new PaletteWriter(_UsbWriter);

            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => _KeyboardReader.RunInputLoop(
                _InputLoopCancellationTokenSource.Token
            ));

            UseMobiFlightInitialisationSequence();
            RefreshLeds();
        }

        private void ProcessKeyboardEvent(Key key, bool pressed)
        {
            if(pressed) {
                OnKeyDown(() => new KeyEventArgs(key, pressed));
            } else {
                OnKeyUp(() => new KeyEventArgs(key, pressed));
            }
        }

        private void ProcessAmbientLightChange(UInt16 leftSensor, UInt16 rightSensor)
        {
            var left = LeftAmbientLightNative;
            var right = RightAmbientLightNative;
            var avg = AmbientLightPercent;

            LeftAmbientLightNative = leftSensor;
            RightAmbientLightNative = rightSensor;
            var mul = ((double)LeftAmbientLightNative + (double)RightAmbientLightNative) / 2.0;
            mul /= 0xfff;
            AmbientLightPercent = (int)(100.0 * mul);

            if(left != LeftAmbientLightNative) {
                OnLeftAmbientLightChanged();
            }
            if(right != RightAmbientLightNative) {
                OnRightAmbientLightChanged();
            }
            if(avg != AmbientLightPercent) {
                OnAmbientLightChanged();
            }
        }

        /// <inheritdoc/>
        public void Cleanup(
            int ledBrightnessPercent = 0,
            int displayBrightnessPercent = 0,
            int backlightBrightnessPercent = 0
        )
        {
            Screen.Clear();
            Leds.TurnAllOn(false);
            _IlluminationWriter?.SendLedBrightnessPercent(ledBrightnessPercent);
            _IlluminationWriter?.SendDisplayBrightnessPercent(displayBrightnessPercent);
            _IlluminationWriter?.SendBacklightPercent(backlightBrightnessPercent);
            RefreshDisplay();
            RefreshLeds();
        }

        private void UseMobiFlightInitialisationSequence()
        {
            SendDefaultFontInitialisation();
            RefreshBrightnesses();
        }

        private void SendDefaultFontInitialisation()
        {
            // Nicked from Mobiflight

            _UsbWriter?.LockForOutput(() => {
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
                    _UsbWriter.SendStringPacket(packet);
                }
            });
        }

        public void RefreshBrightnesses()
        {
            _IlluminationWriter?.SendBacklightPercent(BacklightBrightnessPercent);
            _IlluminationWriter?.SendDisplayBrightnessPercent(DisplayBrightnessPercent);
            _IlluminationWriter?.SendLedBrightnessPercent(LedBrightnessPercent);
        }

        /// <inheritdoc/>
        public void UseFont(McduFontFile fontFileContent, bool useFullWidth)
        {
            _UsbWriter?.LockForOutput(() => {
                _ScreenWriter.SendScreenToDisplay(_EmptyScreen, skipDuplicateCheck: false);
                _FontWriter.SendFont(
                    fontFileContent,
                    useFullWidth,
                    DisplayBrightnessPercent,
                    XOffset,
                    YOffset
                );

                // As of time of writing the packet map includes a pile of 32bb...1901 commands to
                // set the colours to WinWing's defaults. If I remove this then the font goes weird.
                // So for now I'm just resending the colour palette to override the colours that the
                // font set up. This will need refining at some point once I understand the meaning
                // of the 32bbs being sent at the end of the font setup.
                // TODO: Try to remove colour setup from font upload.
                //
                // One advantage of resending the palette is that we also refresh the display, which
                // we need to do anyway. If SendPalette() is removed in the future then you will have
                // to replace it with RefreshDisplay.
                _PaletteWriter.ReestablishPaletteAndRefreshDisplay(_ScreenWriter, Screen);
            });
        }

        /// <inheritdoc/>
        public void RefreshDisplay(bool skipDuplicateCheck = false)
        {
            _ScreenWriter?.SendScreenToDisplay(Screen, skipDuplicateCheck);
        }

        /// <inheritdoc/>
        public void RefreshLeds(bool skipDuplicateCheck = false)
        {
            _IlluminationWriter?.ApplyLeds(Leds, skipDuplicateCheck);
        }

        /// <inheritdoc/>
        public void RefreshPalette(
            bool skipDuplicateCheck = false,
            bool forceDisplayRefresh = true
        )
        {
            _PaletteWriter?.SendPalette(
                Palette?.ToWinWingOrdinalColours(),
                _ScreenWriter,
                Screen,
                skipDuplicateCheck,
                forceDisplayRefresh
            );
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
