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

namespace McduDotNet
{
    public class Compositor
    {
        private Screen _Screen;

        public Compositor(Screen screen)
        {
            _Screen = screen;
        }

        public Compositor Clear()
        {
            _Screen.Clear();
            return this;
        }

        public Compositor ClearRow()
        {
            _Screen.CurrentRow.Clear();
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

        public Compositor Amber() => Colour(McduDotNet.Colour.Amber);

        public Compositor Brown() => Colour(McduDotNet.Colour.Brown);

        public Compositor Cyan() => Colour(McduDotNet.Colour.Cyan);

        public Compositor Grey() => Colour(McduDotNet.Colour.Grey);

        public Compositor Gray() => Colour(McduDotNet.Colour.Grey);

        public Compositor Green() => Colour(McduDotNet.Colour.Green);

        public Compositor Khaki() => Colour(McduDotNet.Colour.Khaki);

        public Compositor Magenta() => Colour(McduDotNet.Colour.Magenta);

        public Compositor Red() => Colour(McduDotNet.Colour.Red);

        public Compositor White() => Colour(McduDotNet.Colour.White);

        public Compositor Yellow() => Colour(McduDotNet.Colour.Yellow);

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

        public Compositor Newline()
        {
            _Screen.Line = Math.Min(_Screen.Rows.Length - 1, _Screen.Line + 1);
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
            _Screen.WriteCentred(text);
            return this;
        }

        public Compositor LeftLabel(int line, string label)
        {
            _Screen.LeftLineSelect(line, label);
            return this;
        }

        public Compositor RightLabel(int line, string label)
        {
            _Screen.RightLineSelect(line, label);
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
            _Screen.Write(text);
            return this;
        }

        public Compositor Centered(string text) => Centred(text);

        public Compositor Write(char value) => Write(value.ToString());

        public Compositor Write(byte value) => Write(value.ToString());

        public Compositor Write(byte value, string format) => Write(value.ToString(format));

        public Compositor Write(byte value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(byte value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(short value) => Write(value.ToString());

        public Compositor Write(short value, string format) => Write(value.ToString(format));

        public Compositor Write(short value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(short value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(int value) => Write(value.ToString());

        public Compositor Write(int value, string format) => Write(value.ToString(format));

        public Compositor Write(int value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(int value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(long value) => Write(value.ToString());

        public Compositor Write(long value, string format) => Write(value.ToString(format));

        public Compositor Write(long value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(long value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(float value) => Write(value.ToString());

        public Compositor Write(float value, string format) => Write(value.ToString(format));

        public Compositor Write(float value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(float value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(double value) => Write(value.ToString());

        public Compositor Write(double value, string format) => Write(value.ToString(format));

        public Compositor Write(double value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(double value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(decimal value) => Write(value.ToString());

        public Compositor Write(decimal value, string format) => Write(value.ToString(format));

        public Compositor Write(decimal value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(decimal value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(DateTime value) => Write(value.ToString());

        public Compositor Write(DateTime value, string format) => Write(value.ToString(format));

        public Compositor Write(DateTime value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(DateTime value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(TimeSpan value) => Write(value.ToString());

        public Compositor Write(TimeSpan value, string format) => Write(value.ToString(format));

        public Compositor Write(TimeSpan value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(DateTimeOffset value) => Write(value.ToString());

        public Compositor Write(DateTimeOffset value, string format) => Write(value.ToString(format));

        public Compositor Write(DateTimeOffset value, IFormatProvider formatProvider) => Write(value.ToString(formatProvider));

        public Compositor Write(DateTimeOffset value, string format, IFormatProvider formatProvider) => Write(value.ToString(format, formatProvider));

        public Compositor Write(object obj) => Write(obj?.ToString() ?? "");
    }
}
