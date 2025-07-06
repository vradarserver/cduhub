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
using System.Text;

namespace McduDotNet.FlightSim
{
    /// <summary>
    /// A class that holds the content of the screen that ToLiss A320-family simulators send as datarefs.
    /// </summary>
    public class ToLissScreenBuffer
    {
        enum Variant
        {
            Normal = 0,
            Small = 1,
        }

        class RowVariants
        {
            public RowStyles[] RowStyles = new RowStyles[2];
        }

        class RowStyles
        {
            public StringBuilder Style_w = new StringBuilder();
            public StringBuilder Style_g = new StringBuilder();
            public StringBuilder Style_y = new StringBuilder();
            public StringBuilder Style_b = new StringBuilder();
            public StringBuilder Style_a = new StringBuilder();
            public StringBuilder Style_m = new StringBuilder();
            public StringBuilder Style_s = new StringBuilder();
            public StringBuilder Style_Lg = new StringBuilder();
            public StringBuilder Style_Lw = new StringBuilder();
        }

        private RowVariants[] _Rows = new RowVariants[Metrics.Lines];
        private long _VertSlewKeys;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ToLissScreenBuffer()
        {
            for(var idx = 0;idx < _Rows.Length;++idx) {
                _Rows[idx] = new RowVariants();
            }
        }

        /// <summary>
        /// Applies a dataref value to the Title row.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="value"></param>
        public void SetTitle(string style, string value)
        {
            SetText(style, value, 0, Variant.Normal);
        }

        /// <summary>
        /// Applies a character dataref value to the Title row.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="cellIdx"></param>
        /// <param name="ch"></param>
        public void SetTitleCell(string style, int cellIdx, char ch)
        {
            SetCell(style, ch, 0, cellIdx, Variant.Normal);
        }

        /// <summary>
        /// Applies a dataref value to the STitle row.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="value"></param>
        public void SetSTitle(string style, string value)
        {
            SetText(style, value, 0, Variant.Small);
        }

        /// <summary>
        /// Applies a character dataref value to the STitle row.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="cellIdx"></param>
        /// <param name="ch"></param>
        public void SetSTitleCell(string style, int cellIdx, char ch)
        {
            SetCell(style, ch, 0, cellIdx, Variant.Small);
        }

        /// <summary>
        /// Applies a dataref value to the ScratchPad row.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="value"></param>
        public void SetScratchPad(string style, string value)
        {
            SetText(style, value, _Rows.Length - 1, Variant.Normal);
        }

        /// <summary>
        /// Applies a character dataref value to the ScratchPad row.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="cellIdx"></param>
        /// <param name="ch"></param>
        public void SetScratchPadCell(string style, int cellIdx, char ch)
        {
            SetCell(style, ch, _Rows.Length - 1, cellIdx, Variant.Normal);
        }

        /// <summary>
        /// Applies a dataref value to a Label row.
        /// </summary>
        /// <param name="labelNumber"></param>
        /// <param name="style"></param>
        /// <param name="value"></param>
        public void SetLabel(int labelNumber, string style, string value)
        {
            var rowNumber = 1 + ((labelNumber - 1) * 2);
            SetText(style, value, rowNumber, Variant.Normal);
        }

        /// <summary>
        /// Applies a character dataref value to a Label row.
        /// </summary>
        /// <param name="labelNumber"></param>
        /// <param name="style"></param>
        /// <param name="cellIdx"></param>
        /// <param name="ch"></param>
        public void SetLabelCell(int labelNumber, string style, int cellIdx, char ch)
        {
            var rowNumber = 1 + ((labelNumber - 1) * 2);
            SetCell(style, ch, rowNumber, cellIdx, Variant.Normal);
        }

        /// <summary>
        /// Applies a dataref value to a Cont row.
        /// </summary>
        /// <param name="contNumber"></param>
        /// <param name="style"></param>
        /// <param name="value"></param>
        public void SetCont(int contNumber, string style, string value)
        {
            var rowNumber = 2 + ((contNumber - 1) * 2);
            SetText(style, value, rowNumber, Variant.Normal);
        }

        /// <summary>
        /// Applies a character dataref value to a Cont row.
        /// </summary>
        /// <param name="contNumber"></param>
        /// <param name="style"></param>
        /// <param name="cellIdx"></param>
        /// <param name="ch"></param>
        public void SetContCell(int contNumber, string style, int cellIdx, char ch)
        {
            var rowNumber = 2 + ((contNumber - 1) * 2);
            SetCell(style, ch, rowNumber, cellIdx, Variant.Normal);
        }

        /// <summary>
        /// Applies a dataref value to an SCont row.
        /// </summary>
        /// <param name="scontNumber"></param>
        /// <param name="style"></param>
        /// <param name="value"></param>
        public void SetSCont(int scontNumber, string style, string value)
        {
            var rowNumber = 2 + ((scontNumber - 1) * 2);
            SetText(style, value, rowNumber, Variant.Small);
        }

        /// <summary>
        /// Applies a character dataref value to an SCont row.
        /// </summary>
        /// <param name="scontNumber"></param>
        /// <param name="style"></param>
        /// <param name="cellIdx"></param>
        /// <param name="ch"></param>
        public void SetSContCell(int scontNumber, string style, int cellIdx, char ch)
        {
            var rowNumber = 2 + ((scontNumber - 1) * 2);
            SetCell(style, ch, rowNumber, cellIdx, Variant.Small);
        }

        /// <summary>
        /// Applies a dataref value to the Vertical Slew Keys indicator.
        /// </summary>
        /// <param name="value"></param>
        public void SetVertSlewKeys(long value)
        {
            _VertSlewKeys = value;
        }

        private void SetText(string style, string value, int rowNumber, Variant rowVariant)
        {
            if(value != null) {
                var flavours = _Rows[rowNumber];
                var styles = flavours.RowStyles[(int)rowVariant];
                if(styles == null) {
                    styles = new RowStyles();
                    flavours.RowStyles[(int)rowVariant] = styles;
                }

                var bytes = Convert.FromBase64String(value);
                var text = Encoding.UTF8.GetString(bytes);
                text = text.Trim('\0');

                StringBuilder buffer = null;
                switch(style) {
                    case "w":   buffer = styles.Style_w; break;
                    case "g":   buffer = styles.Style_g; break;
                    case "y":   buffer = styles.Style_y; break;
                    case "b":   buffer = styles.Style_b; break;
                    case "a":   buffer = styles.Style_a; break;
                    case "m":   buffer = styles.Style_m; break;
                    case "s":   buffer = styles.Style_s; break;
                    case "Lg":  buffer = styles.Style_Lg; break;
                    case "Lw":  buffer = styles.Style_Lw; break;
                }
                if(buffer != null) {
                    buffer.Clear();
                    buffer.Append(text);
                }
            }
        }

        private void SetCell(string style, char ch, int rowNumber, int columnNumber, Variant rowVariant)
        {
            if(columnNumber >= 0 && columnNumber < Metrics.Columns) {
                var flavours = _Rows[rowNumber];
                var styles = flavours.RowStyles[(int)rowVariant];
                if(styles == null) {
                    styles = new RowStyles();
                    flavours.RowStyles[(int)rowVariant] = styles;
                }

                StringBuilder buffer = null;
                switch(style) {
                    case "w":   buffer = styles.Style_w; break;
                    case "g":   buffer = styles.Style_g; break;
                    case "y":   buffer = styles.Style_y; break;
                    case "b":   buffer = styles.Style_b; break;
                    case "a":   buffer = styles.Style_a; break;
                    case "m":   buffer = styles.Style_m; break;
                    case "s":   buffer = styles.Style_s; break;
                    case "Lg":  buffer = styles.Style_Lg; break;
                    case "Lw":  buffer = styles.Style_Lw; break;
                }
                if(buffer != null) {
                    while(buffer.Length <= columnNumber) {
                        buffer.Append(' ');
                    }
                    buffer[columnNumber] = ch;
                }
            }
        }

        /// <summary>
        /// Copies the content of the ToLiss screen buffer to an MCDU screen buffer.
        /// </summary>
        /// <param name="screen"></param>
        public void CopyToScreen(Screen screen)
        {
            screen.Clear();

            for(var rowIdx = 0;rowIdx < _Rows.Length;++rowIdx) {
                screen.Line = rowIdx;
                var defaultSmall = rowIdx < 13 && rowIdx % 2 == 1;
                ApplyVariant(screen, _Rows[rowIdx], Variant.Normal, defaultSmall);
                ApplyVariant(screen, _Rows[rowIdx], Variant.Small, true);
            }
            ApplyVerticalSlewKeys(screen, _VertSlewKeys);
        }

        private static void ApplyVariant(Screen screen, RowVariants variants, Variant variant, bool defaultSmall)
        {
            var styles = variants.RowStyles[(int)variant];
            if(styles != null) {
                screen.Small = defaultSmall;

                OverlayText(screen, styles.Style_w, () => screen.Colour = Colour.White);
                OverlayText(screen, styles.Style_g, () => screen.Colour = Colour.Green);
                OverlayText(screen, styles.Style_y, () => screen.Colour = Colour.Yellow);
                OverlayText(screen, styles.Style_b, () => screen.Colour = Colour.Cyan);
                OverlayText(screen, styles.Style_a, () => screen.Colour = Colour.Amber);
                OverlayText(screen, styles.Style_m, () => screen.Colour = Colour.Magenta);
                OverlayText(screen, styles.Style_Lg, () => { screen.Small = false; screen.Colour = Colour.Green; });
                OverlayText(screen, styles.Style_Lw, () => { screen.Small = false; screen.Colour = Colour.White; });

                OverlaySubstitutedText(screen, styles.Style_s);
            }
        }

        private static void OverlayText(Screen screen, StringBuilder overlayBuffer, Action setupStyle)
        {
            screen.GotoStartOfLine();
            setupStyle?.Invoke();
            for(var idx = 0;idx < overlayBuffer.Length;++idx) {
                var ch = overlayBuffer[idx];
                var put = ch;
                switch(ch) {
                    case '`':   put = '°'; break;
                    case '|':   put = 'Δ'; break;
                }
                if(ch == ' ' || ch == '\0') {
                    screen.AdvanceColumn();
                } else {
                    screen.Put(put, advanceColumn: true);
                }
            }
        }

        private static void OverlaySubstitutedText(Screen screen, StringBuilder overlayBuffer)
        {
            screen.GotoStartOfLine();
            for(var idx = 0;idx < overlayBuffer.Length;++idx) {
                var ch = overlayBuffer[idx];
                var put = '\0';
                switch(ch) {
                    case 'A':   put = '['; screen.Colour = Colour.Cyan; break;
                    case 'B':   put = ']'; screen.Colour = Colour.Cyan; break;
                    case 'E':   put = '☐'; screen.Colour = Colour.Amber; break;
                    case '0':   put = '←'; screen.Colour = Colour.Cyan; break;
                    case '1':   put = '→'; screen.Colour = Colour.Cyan; break;
                    case '2':   put = '←'; screen.Colour = Colour.White; break;
                    case '3':   put = '→'; screen.Colour = Colour.White; break;
                    case '4':   put = '←'; screen.Colour = Colour.Amber; break;
                    case '5':   put = '→'; screen.Colour = Colour.Amber; break;
                }
                if(put == '\0') {
                    screen.AdvanceColumn();
                } else {
                    screen.Put(put, advanceColumn: true);
                }
            }
        }

        private static void ApplyVerticalSlewKeys(Screen screen, long verticalSlewKeys)
        {
            if(verticalSlewKeys != 0) {
                var arrows = "";
                switch(verticalSlewKeys) {
                    case 1: arrows = "↓↑"; break;
                    case 2: arrows = "↑"; break;
                    case 3: arrows = "↓"; break;
                }
                screen.Colour = Colour.White;
                screen.Small = false;
                screen.Line = screen.Rows.Length - 1;
                screen.ForRightToLeft();
                screen.Write(arrows.ToString());
                screen.ForLeftToRight();
            }
        }
    }
}
