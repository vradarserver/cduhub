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

        /// <summary>
        /// The colour of the next character added to the display.
        /// </summary>
        public Colour Colour { get; set; } = Colour.White;

        /// <summary>
        /// Alias for <see cref="Colour"/>.
        /// </summary>
        public Colour Color
        {
            get => Colour;
            set => Colour = value;
        }

        /// <summary>
        /// The 0-based column of the next character to add.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// The 0-based line of the next character to add.
        /// </summary>
        public int Line { get; set; }

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

        public void Put(char ch)
        {
            if(Line < Metrics.Lines && Column < Metrics.Columns) {
                Rows[Line].Cells[Column] = ch == ' '
                    ? Cell.Space
                    : new Cell(ch, Colour, Small);
            }
        }

        public void Write(string text)
        {
            if(!RightToLeft) {
                foreach(var ch in text) {
                    Put(ch);
                    Column = Math.Min(Column + 1, Metrics.Columns - 1);
                }
            } else {
                for(var idx = text.Length - 1;idx >= 0;--idx) {
                    Put(text[idx]);
                    Column = Math.Max(Column - 1, 0);
                }
            }
        }

        public void WriteLine(string text)
        {
            Write(text);
            ++Line;
            Sol();
        }

        public void CentreColumnFor(string text)
        {
            Column = Math.Max(0, (Metrics.Columns - text.Length) / 2);
        }

        public void Goto(int line, int column = 0)
        {
            Line = line >= 0
                ? line
                : Metrics.Lines + line;
            Column = column >= 0
                ? column
                : Metrics.Columns + column;
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

        public void Eol()
        {
            Column = RightToLeft ? 0 : Metrics.Columns - 1;
        }

        public void Sol()
        {
            Column = RightToLeft ? Metrics.Columns - 1 : 0;
        }

        public void ForRightToLeft()
        {
            RightToLeft = true;
            Sol();
        }

        public void ForLeftToRight()
        {
            RightToLeft = false;
            Sol();
        }

        public void ScrollRows(int startRow = 0, int endRow = Metrics.Lines - 1)
        {
            if(startRow < 0 || startRow > endRow) {
                throw new ArgumentOutOfRangeException(nameof(startRow));
            }
            if(endRow < startRow || endRow > Metrics.Lines - 1) {
                throw new ArgumentOutOfRangeException(nameof(endRow));
            }
            for(var rowIdx = startRow;rowIdx < endRow;++rowIdx) {
                Rows[rowIdx].CopyFrom(Rows[rowIdx + 1]);
            }
            Rows[endRow].Clear();
        }
    }
}
