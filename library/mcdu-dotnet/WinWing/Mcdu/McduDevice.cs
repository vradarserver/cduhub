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
using System.Collections.Generic;
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
    class McduDevice : CommonWinWingPanel, ICdu, IMcdu
#pragma warning restore CS0618
    {
        protected override byte CommandPrefix => 0x32;

        private static readonly Dictionary<Led, byte> _LedIndicatorCodeMap = new Dictionary<Led, byte>() {
            { Led.Fail, 0x08 },
            { Led.Fm, 0x09 },
            { Led.Mcdu, 0x0a },
            { Led.Menu, 0x0b },
            { Led.Fm1, 0x0c },
            { Led.Ind, 0x0d },
            { Led.Rdy, 0x0e },
            { Led.Line, 0x0f },
            { Led.Fm2, 0x10 },
        };
        protected override Dictionary<Led, byte> LedIndicatorCodeMap => _LedIndicatorCodeMap;

        protected override Func<Key, (int Flag, int Offset)> KeyToFlagOffsetCallback => KeyboardMap.InputReport01FlagAndOffset;

        private FontWriter _FontWriter;
        private PaletteWriter _PaletteWriter;

        /// <inheritdoc/>
        public ProductId ProductId => DeviceId.GetLegacyProductId();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="deviceId"></param>
        public McduDevice(HidDevice hidDevice, DeviceIdentifier deviceId) : base(hidDevice, deviceId)
        {
        }

        /// <inheritdoc/>
        ~McduDevice() => Dispose(false);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing) {
                _FontWriter = null;
                _PaletteWriter = null;
            }
        }

        protected override void PanelSpecificInitialisation()
        {
            _FontWriter = new FontWriter(_UsbWriter);
            _PaletteWriter = new PaletteWriter(_UsbWriter);
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
    }
}
