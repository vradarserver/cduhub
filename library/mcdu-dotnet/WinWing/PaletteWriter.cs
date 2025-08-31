// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Manages the sending of colour palettes to the WinWing MCDU device.
    /// </summary>
    class PaletteWriter
    {
#pragma warning disable IDE1006 // Naming Styles
        private readonly string CP; // See notes elsewhere as to why this name is exactly 2 chars
#pragma warning restore IDE1006 // Naming Styles
        private UsbWriter _UsbWriter;
        private PaletteColour[] _CurrentPaletteColourArray;

        public PaletteWriter(UsbWriter usbWriter, string commandPrefix)
        {
            _UsbWriter = usbWriter;
            CP = commandPrefix;
        }

        /// <summary>
        /// See the notes against <see cref="McduDevice.UseFont"/> as to why this exists.
        /// It will resend the palette if one has been established, and then in all cases
        /// it refreshes the display.
        /// </summary>
        public void ReestablishPaletteAndRefreshDisplay(
            ScreenWriter screenWriter,
            Screen screen
        )
        {
            if(_CurrentPaletteColourArray == null) {
                screenWriter.SendScreenToDisplay(
                    screen,
                    skipDuplicateCheck: true,
                    suppressUpdatingDisplayCallback: false
                );
            } else {
                SendPalette(
                    _CurrentPaletteColourArray,
                    screenWriter,
                    screen,
                    skipDuplicateCheck: true,
                    forceDisplayRefresh: true
                );
            }
        }

        public void SendPalette(
            PaletteColour[] colourArray,
            ScreenWriter screenWriter,
            Screen screen,
            bool skipDuplicateCheck = false,
            bool forceDisplayRefresh = true
        )
        {
            _UsbWriter.LockForOutput(() => {
                var duplicateCheckString = Palette.BuildDuplicateCheckString(colourArray);
                var currentDuplicateCheckString = Palette.BuildDuplicateCheckString(_CurrentPaletteColourArray);
                if(skipDuplicateCheck || currentDuplicateCheckString != duplicateCheckString) {
                    _CurrentPaletteColourArray = colourArray;
                    byte seq = 1;

                    var buffer = new StringBuilder();

                    AddToPacketBuffer(buffer, 0x1f, ref seq, $"{CP}00001901000004170100000e00000001000500000002");
                    SendPacketBuffer(buffer);
                    AddToPacketBuffer(buffer, 0x3c, ref seq, $"{CP}00001901000004170100000e0000000100060000000300000000000000");

                    var colourSeq = 4;
                    foreach(var colour in colourArray) {
                        var setForeground = $"{CP}00001901000004170100000e0000000200{colour.ToWinwingColourString()}{colourSeq++:x2}00000000000000";
                        AddToPacketBuffer(buffer, 0x3c, ref seq, setForeground);
                    }
                    foreach(var colour in colourArray) {
                        var setBackground = $"{CP}00001901000004170100000e0000000300{colour.ToWinwingColourString()}{colourSeq++:x2}00000000000000";
                        AddToPacketBuffer(buffer, 0x3c, ref seq, setBackground);
                    }
                    AddToPacketBuffer(buffer, 0x3c, ref seq, $"{CP}00001901000004170100000e000000040000000000{colourSeq++:x2}00000000000000");
                    AddToPacketBuffer(buffer, 0x3c, ref seq, $"{CP}00001901000004170100000e000000040001000000{colourSeq++:x2}00000000000000");
                    AddToPacketBuffer(buffer, 0x2b, ref seq, $"{CP}00001901000004170100000e000000040002000000{colourSeq++:x2}00000000000000");
                    AddToPacketBuffer(buffer, 0x2b, ref seq, $"{CP}0000050100000417010001");
                    SendPacketBuffer(buffer);
                    AddToPacketBuffer(buffer, 0x34, ref seq, $"{CP}00001a01000025170100000100000002");
                    AddToPacketBuffer(buffer, 0x34, ref seq, $"{CP}00001c010000251701000000000000");
                    AddToPacketBuffer(buffer, 0x34, ref seq, $"{CP}0000050100002517010001");
                    SendPacketBuffer(buffer);

                    if(forceDisplayRefresh) {
                        screenWriter.SendScreenToDisplay(
                            screen,
                            skipDuplicateCheck: true,
                            suppressUpdatingDisplayCallback: false
                        );
                    }
                }
            });
        }

        void SendPacketBuffer(StringBuilder packetBuffer)
        {
            if(packetBuffer.Length > 0) {
                while(packetBuffer.Length < 128) {
                    packetBuffer.Append("00");
                }
                _UsbWriter.SendStringPacket(packetBuffer.ToString());
                packetBuffer.Clear();
            }
        }

        void AddToPacketBuffer(
            StringBuilder packetBuffer,
            int f0Code,
            ref byte sequenceNumber,
            string chunk
        )
        {
            for(var idx = 0;idx < chunk.Length;++idx) {
                if(packetBuffer.Length == 128) {
                    SendPacketBuffer(packetBuffer);
                }
                if(packetBuffer.Length == 0) {
                    packetBuffer.Append("f000");
                    packetBuffer.Append(sequenceNumber++.ToString("x2"));
                    packetBuffer.Append(f0Code.ToString("x2"));
                }
                packetBuffer.Append(chunk[idx]);
            }
        }
    }
}
