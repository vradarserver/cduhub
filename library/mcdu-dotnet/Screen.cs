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

namespace McduDotNet
{
    /// <summary>
    /// Describes and manipulates the content of an MCDU display.
    /// </summary>
    public class Screen
    {
        /// <summary>
        /// The rows of cells on the MCDU display.
        /// </summary>
        public Row[] Rows { get; } = new Row[Metrics.Lines];

        public Row CurrentRow => Rows[Line];

        public Cell CurrentCell => CurrentRow.Cells[Column];

        private Colour _Colour = Colour.White;
        /// <summary>
        /// The colour of the next character added to the display.
        /// </summary>
        public Colour Colour
        {
            get => _Colour;
            set {
                if(!Enum.IsDefined(typeof(Colour), value)) {
                    throw new ArgumentOutOfRangeException(nameof(Colour), value, "Not a valid colour");
                }
                _Colour = value;
            }
        }

        /// <summary>
        /// Alias for <see cref="Colour"/>.
        /// </summary>
        public Colour Color
        {
            get => Colour;
            set => Colour = value;
        }

        private int _Column;
        /// <summary>
        /// The 0-based column of the next character to add.
        /// </summary>
        public int Column
        {
            get => _Column;
            set {
                if(value < 0 || value >= Metrics.Columns) {
                    throw new ArgumentOutOfRangeException(nameof(Column), value, "Not a valid column");
                }
                _Column = value;
            }
        }

        private int _Line;
        /// <summary>
        /// The 0-based line of the next character to add.
        /// </summary>
        public int Line
        {
            get => _Line;
            set {
                if(value < 0 || value >= Rows.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Line), value, "Not a valid line");
                }
                _Line = value;
            }
        }

        /// <summary>
        /// True if the next character to add will be in the small font.
        /// </summary>
        public bool Small { get; set; }

        /// <summary>
        /// True if text is written from right-to-left.
        /// </summary>
        public bool RightToLeft { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <remarks></remarks>
        public Screen()
        {
            for(var idx = 0;idx < Rows.Length;++idx) {
                Rows[idx] = new Row();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach(var row in Rows) {
                if(buffer.Length > 0) {
                    buffer.Append(Environment.NewLine);
                }
                buffer.Append(row);
            }

            return buffer.ToString();
        }

        public string BuildDuplicateCheckString()
        {
            var buffer = new StringBuilder();
            foreach(var row in Rows) {
                row.AppendToDuplicateCheckBuffer(buffer);
            }
            return buffer.ToString();
        }

        public void Clear()
        {
            foreach(var row in Rows) {
                row.Clear();
            }
            RightToLeft = false;
            Colour = Colour.White;
            Column = 0;
            Line = 0;
            Small = false;
        }

        public void CopyFrom(Screen other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            for(var idx = 0;idx < Rows.Length;++idx) {
                Rows[idx].CopyFrom(other.Rows[idx]);
            }
            RightToLeft = other.RightToLeft;
            Colour = other.Colour;
            Column = other.Column;
            Line = other.Line;
            Small = other.Small;
        }

        public void CopyTo(Screen other) => other?.CopyFrom(this);

        public void Put(char ch, bool advanceColumn = false)
        {
            if(ch == ' ') { // <-- non-breaking space
                ch = ' ';   // <-- bog-standard space
            }
            Rows[Line].Cells[Column].Set(ch, Colour, Small);
            if(advanceColumn) {
                if(!RightToLeft) {
                    Column = Math.Min(Column + 1, Metrics.Columns - 1);
                } else {
                    Column = Math.Max(Column - 1, 0);
                }
            }
        }

        public void Write(string text)
        {
            if(!RightToLeft) {
                foreach(var ch in text) {
                    Put(ch, advanceColumn: true);
                }
            } else {
                for(var idx = text.Length - 1;idx >= 0;--idx) {
                    Put(text[idx], advanceColumn: true);
                }
            }
        }

        public void WriteLine(string text)
        {
            Write(text);
            ++Line;
            GotoStartOfLine();
        }

        public void CentreColumnFor(string text)
        {
            Column = Math.Max(0, (Metrics.Columns - text.Length) / 2);
        }

        public void Goto(int line, int column = 0)
        {
            Line = line >= 0
                ? Math.Min(line, Metrics.Lines - 1)
                : Math.Max(0, Metrics.Lines + line);
            Column = column >= 0
                ? Math.Min(column, Metrics.Columns - 1)
                : Math.Max(0, Metrics.Columns + column);
        }

        public void GotoMiddleLine() => Line = Metrics.Lines / 2;

        public void LeftLineSelect(int line, string text)
        {
            Line = line * 2;
            ForLeftToRight();
            Write(text);
        }

        public void RightLineSelect(int line, string text)
        {
            Line = line * 2;
            ForRightToLeft();
            Write(text);
            ForLeftToRight();
        }

        public void WriteCentred(string text)
        {
            CentreColumnFor(text);
            Write(text);
        }

        public void WriteLineCentred(string text)
        {
            CentreColumnFor(text);
            WriteLine(text);
        }

        public void GotoEndOfLine()
        {
            Column = RightToLeft ? 0 : Metrics.Columns - 1;
        }

        public void GotoStartOfLine()
        {
            Column = RightToLeft ? Metrics.Columns - 1 : 0;
        }

        public void ForRightToLeft()
        {
            RightToLeft = true;
            GotoStartOfLine();
        }

        public void ForLeftToRight()
        {
            RightToLeft = false;
            GotoStartOfLine();
        }

        public void ScrollRows(int startRow = 0, int endRow = Metrics.Lines - 1)
        {
            if(startRow < 0 || startRow > endRow) {
                throw new ArgumentOutOfRangeException(nameof(startRow));
            }
            if(endRow < startRow || endRow >= Metrics.Lines) {
                throw new ArgumentOutOfRangeException(nameof(endRow));
            }
            for(var rowIdx = startRow;rowIdx < endRow;++rowIdx) {
                Rows[rowIdx].CopyFrom(Rows[rowIdx + 1]);
            }
            Rows[endRow].Clear();
        }
    }
}
