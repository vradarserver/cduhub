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
using System.Text;

namespace WwDevicesDotNet
{
    public class Compositor
    {
        private Screen _Screen;
        private bool _ShowLowercaseInSmallUppercase = true;

        public Compositor(Screen screen)
        {
            _Screen = screen;
        }

        public Compositor Clear()
        {
            _Screen.Clear();
            return this;
        }

        public Compositor ClearRow(int countRows = 1)
        {
            for(var count = 0;count < countRows;++count) {
                var row = Math.Min(_Screen.Rows.Length - 1, _Screen.Line + count);
                _Screen.Rows[row].Clear();
            }
            return this;
        }

        /// <summary>
        /// Assume that the current font has lowercase glyphs and use them instead of
        /// turning lowercase text into small uppercase.
        /// </summary>
        /// <param name="useLowercaseFont"></param>
        /// <returns></returns>
        public Compositor UseLowercaseFont(bool useLowercaseFont = true)
        {
            _ShowLowercaseInSmallUppercase = !useLowercaseFont;
            return this;
        }

        public Compositor Line(int line, bool resetColumn = true)
        {
            if(line >= 0) {
                _Screen.Line = line;
            } else {
                BottomLine();
                _Screen.Line += line;
            }
            if(resetColumn) {
                StartOfLine();
            }
            return this;
        }

        public Compositor Y(int line, bool resetColumn = true) => Line(line, resetColumn);

        public Compositor TopLine(bool resetColumn = true) => Line(0, resetColumn);

        public Compositor BottomLine(bool resetColumn = true) => Line(_Screen.Rows.Length - 1, resetColumn);

        public Compositor MiddleLine(bool resetColumn = true) => Line(_Screen.Rows.Length / 2, resetColumn);

        public Compositor LabelLine(int line, bool resetColumn = true) => Line(line * 2, resetColumn);

        public Compositor LabelTitleLine(int line, bool resetColumn = true) => Line((line * 2) - 1, resetColumn);

        public Compositor Column(int column)
        {
            _Screen.Column = column;
            return this;
        }

        public Compositor X(int column) => Column(column);

        public Compositor StartOfLine()
        {
            _Screen.GotoStartOfLine();
            return this;
        }

        public Compositor Sol() => StartOfLine();

        public Compositor EndOfLine()
        {
            _Screen.GotoEndOfLine();
            return this;
        }

        public Compositor Eol() => EndOfLine();

        public Compositor CentreFor(string text)
        {
            _Screen.CentreColumnFor(text ?? "");
            return this;
        }

        public Compositor CenterFor(string text) => CentreFor(text);

        public Compositor Colour(Colour colour)
        {
            _Screen.Colour = colour;
            return this;
        }

        public Compositor Color(Colour colour) => Colour(colour);

        public Compositor Amber() => Colour(WwDevicesDotNet.Colour.Amber);

        public Compositor Brown() => Colour(WwDevicesDotNet.Colour.Brown);

        public Compositor Cyan() => Colour(WwDevicesDotNet.Colour.Cyan);

        public Compositor Grey() => Colour(WwDevicesDotNet.Colour.Grey);

        public Compositor Gray() => Colour(WwDevicesDotNet.Colour.Grey);

        public Compositor Green() => Colour(WwDevicesDotNet.Colour.Green);

        public Compositor Khaki() => Colour(WwDevicesDotNet.Colour.Khaki);

        public Compositor Magenta() => Colour(WwDevicesDotNet.Colour.Magenta);

        public Compositor Red() => Colour(WwDevicesDotNet.Colour.Red);

        public Compositor White() => Colour(WwDevicesDotNet.Colour.White);

        public Compositor Yellow() => Colour(WwDevicesDotNet.Colour.Yellow);

        public Compositor BackgroundColour(Colour colour)
        {
            _Screen.BackgroundColour = colour;
            return this;
        }

        public Compositor BackgroundColor(Colour colour) => BackgroundColour(colour);

        public Compositor BGColour(Colour colour) => BackgroundColour(colour);

        public Compositor BGColor(Colour colour) => BackgroundColour(colour);

        public Compositor BGAmber() => BackgroundColour(WwDevicesDotNet.Colour.Amber);

        public Compositor BGBrown() => BackgroundColour(WwDevicesDotNet.Colour.Brown);

        public Compositor BGCyan() => BackgroundColour(WwDevicesDotNet.Colour.Cyan);

        public Compositor BGGrey() => BackgroundColour(WwDevicesDotNet.Colour.Grey);

        public Compositor BGGray() => BackgroundColour(WwDevicesDotNet.Colour.Grey);

        public Compositor BGGreen() => BackgroundColour(WwDevicesDotNet.Colour.Green);

        public Compositor BGKhaki() => BackgroundColour(WwDevicesDotNet.Colour.Khaki);

        public Compositor BGMagenta() => BackgroundColour(WwDevicesDotNet.Colour.Magenta);

        public Compositor BGRed() => BackgroundColour(WwDevicesDotNet.Colour.Red);

        public Compositor BGWhite() => BackgroundColour(WwDevicesDotNet.Colour.White);

        public Compositor BGYellow() => BackgroundColour(WwDevicesDotNet.Colour.Yellow);

        public Compositor BGBlack() => BackgroundColour(WwDevicesDotNet.Colour.Black);

        public Compositor InvertColours()
        {
            var temp = _Screen.Colour;
            _Screen.Colour = _Screen.BackgroundColour;
            _Screen.BackgroundColour = temp;
            return this;
        }

        public Compositor InvertColors() => InvertColours();

        public Compositor Small(bool on = true)
        {
            _Screen.Small = on;
            return this;
        }

        public Compositor Large(bool on = true) => Small(!on);

        public Compositor RightToLeft()
        {
            _Screen.ForRightToLeft();
            return this;
        }

        public Compositor LeftToRight()
        {
            _Screen.ForLeftToRight();
            return this;
        }

        public Compositor Newline(int count = 1)
        {
            _Screen.Line = Math.Min(_Screen.Rows.Length - 1, _Screen.Line + count);
            return StartOfLine();
        }

        public Compositor NewLine() => Newline();

        public Compositor ScrollUp(int startRow = 0, int endRow = Metrics.Lines - 1)
        {
            _Screen.ScrollRows(startRow, endRow);
            return this;
        }

        public Compositor OverwriteRow(char? character = null, Colour? colour = null, bool? small = null)
        {
            foreach(var cell in _Screen.CurrentRow.Cells) {
                cell.Set(
                    character ?? cell.Character,
                    colour ?? cell.Colour,
                    small ?? cell.Small
                );
            }
            return this;
        }

        public Compositor Centred(string text)
        {
            var cstring = new CompositorString(text);
            _Screen.CentreColumnFor(cstring.Text);
            ApplyCompositorString(cstring);
            return this;
        }

        public Compositor LeftLabelTitle(int line, string labelTitle)
        {
            LabelTitleLine(line, resetColumn: false);
            _Screen.ForLeftToRight();
            ApplyCompositorString(labelTitle);
            return this;
        }

        public Compositor LeftLabel(int line, string label)
        {
            LabelLine(line, resetColumn: false);
            _Screen.ForLeftToRight();
            ApplyCompositorString(label);
            return this;
        }

        public Compositor RightLabelTitle(int line, string labelTitle)
        {
            LabelTitleLine(line, resetColumn: false);
            _Screen.ForRightToLeft();
            ApplyCompositorString(labelTitle);
            _Screen.ForLeftToRight();
            return this;
        }

        public Compositor RightLabel(int line, string label)
        {
            LabelLine(line, resetColumn: false);
            _Screen.ForRightToLeft();
            ApplyCompositorString(label);
            _Screen.ForLeftToRight();
            return this;
        }

        public Compositor LabelTitle(bool left, int line, string labelTitle)
        {
            if(left) {
                LeftLabelTitle(line, labelTitle);
            } else {
                RightLabelTitle(line, labelTitle);
            }
            return this;
        }

        public Compositor Label(bool left, int line, string label)
        {
            if(left) {
                LeftLabel(line, label);
            } else {
                RightLabel(line, label);
            }
            return this;
        }

        public Compositor Write(string text)
        {
            ApplyCompositorString(text);
            return this;
        }

        public Compositor WriteLine(string text)
        {
            ApplyCompositorString(text);
            Newline();
            return this;
        }

        private Compositor WriteRaw(string text)
        {
            _Screen.Write(text, showLowercaseInSmallUppercase: _ShowLowercaseInSmallUppercase);
            return this;
        }

        public Compositor Centered(string text) => Centred(text);

        public Compositor Write(char value) => WriteRaw(value.ToString());

        public Compositor Write(byte value) => WriteRaw(value.ToString());

        public Compositor Write(byte value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(byte value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(byte value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(short value) => WriteRaw(value.ToString());

        public Compositor Write(short value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(short value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(short value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(int value) => WriteRaw(value.ToString());

        public Compositor Write(int value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(int value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(int value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(long value) => WriteRaw(value.ToString());

        public Compositor Write(long value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(long value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(long value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(float value) => WriteRaw(value.ToString());

        public Compositor Write(float value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(float value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(float value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(double value) => WriteRaw(value.ToString());

        public Compositor Write(double value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(double value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(double value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(decimal value) => WriteRaw(value.ToString());

        public Compositor Write(decimal value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(decimal value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(decimal value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(DateTime value) => WriteRaw(value.ToString());

        public Compositor Write(DateTime value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(DateTime value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(DateTime value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(TimeSpan value) => WriteRaw(value.ToString());

        public Compositor Write(TimeSpan value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(TimeSpan value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(DateTimeOffset value) => WriteRaw(value.ToString());

        public Compositor Write(DateTimeOffset value, string format) => WriteRaw(value.ToString(format));

        public Compositor Write(DateTimeOffset value, IFormatProvider formatProvider) => WriteRaw(value.ToString(formatProvider));

        public Compositor Write(DateTimeOffset value, string format, IFormatProvider formatProvider) => WriteRaw(value.ToString(format, formatProvider));

        public Compositor Write(object obj) => WriteRaw(obj?.ToString() ?? "");

        public Compositor WrapText(
            string text,
            int maxLines = 2,
            bool clearLines = false
        )
        {
            return Lines(text?.WrapAtWhitespace(Metrics.Columns), 0, maxLines, clearLines);
        }

        public Compositor Lines(
            IReadOnlyList<string> lines,
            int offset = 0,
            int maxLines = 2,
            bool clearLines = false
        )
        {
            for(var count = 0;count < maxLines;++count) {
                if(clearLines) {
                    ClearRow();
                }
                var lineIdx = count + offset;
                var line = lineIdx < lines.Count ? lines[lineIdx] : null;
                if(line != null) {
                    WriteRaw(line);
                }
                Newline();
            }
            return this;
        }

        private void ApplyCompositorString(string textWithEmbedding)
        {
            var cstring = new CompositorString(textWithEmbedding);
            ApplyCompositorString(cstring);
        }

        private void ApplyCompositorString(CompositorString cstring)
        {
            var restoreSmall = _Screen.Small;
            var restoreColour = _Screen.Colour;
            var restoreBackgroundColour = _Screen.BackgroundColour;

            var styleChangeIdx = -1;
            CompositorStringStyleChange nextStyleChange;
            void selectNextStyleChange()
            {
                nextStyleChange = ++styleChangeIdx < cstring.StyleChanges.Length
                    ? cstring.StyleChanges[styleChangeIdx]
                    : null;
            }
            selectNextStyleChange();

            if(_Screen.RightToLeft && cstring.Text.Length > 0) {
                _Screen.Column = Math.Max(0, _Screen.Column - (cstring.Text.Length - 1));
            }

            for(var textIdx = 0;textIdx < cstring.Text.Length;++textIdx) {
                while(textIdx == nextStyleChange?.Index) {
                    switch(nextStyleChange.Style) {
                        case CompositorStringStyle.Amber:       _Screen.Colour = WwDevicesDotNet.Colour.Amber; break;
                        case CompositorStringStyle.Brown:       _Screen.Colour = WwDevicesDotNet.Colour.Brown; break;
                        case CompositorStringStyle.Cyan:        _Screen.Colour = WwDevicesDotNet.Colour.Cyan; break;
                        case CompositorStringStyle.Green:       _Screen.Colour = WwDevicesDotNet.Colour.Green; break;
                        case CompositorStringStyle.Grey:        _Screen.Colour = WwDevicesDotNet.Colour.Grey; break;
                        case CompositorStringStyle.Khaki:       _Screen.Colour = WwDevicesDotNet.Colour.Khaki; break;
                        case CompositorStringStyle.Large:       _Screen.Small = false; break;
                        case CompositorStringStyle.Magenta:     _Screen.Colour = WwDevicesDotNet.Colour.Magenta; break;
                        case CompositorStringStyle.Red:         _Screen.Colour = WwDevicesDotNet.Colour.Red; break;
                        case CompositorStringStyle.Small:       _Screen.Small = true; break;
                        case CompositorStringStyle.White:       _Screen.Colour = WwDevicesDotNet.Colour.White; break;
                        case CompositorStringStyle.Yellow:      _Screen.Colour = WwDevicesDotNet.Colour.Yellow; break;
                    }
                    selectNextStyleChange();
                }
                _Screen.Put(
                    cstring.Text[textIdx],
                    showLowercaseInSmallUppercase: _ShowLowercaseInSmallUppercase
                );
                _Screen.Column = Math.Min(_Screen.CurrentRow.Cells.Length - 1, _Screen.Column + 1);
            }

            _Screen.Small = restoreSmall;
            _Screen.Colour = restoreColour;
            _Screen.BackgroundColour = restoreBackgroundColour;
        }
    }
}
