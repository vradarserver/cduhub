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
    /// <summary>
    /// Describes the content of the display. This is similar to <see cref="Screen"/> but
    /// it is at a lower level. It is not intended as a random write buffer, it is
    /// intended to be populated with an entire screen buffer in one go and to represent
    /// that buffer's contents reasonably efficiently.
    /// </summary>
    public class DisplayBuffer
    {
        /// <summary>
        /// The number of rows of cells in the buffer.
        /// </summary>
        public int CountRows => Characters.GetLength(0);

        /// <summary>
        /// The number of cells in each row of the buffer.
        /// </summary>
        public int CountCells => Characters.GetLength(1);

        /// <summary>
        /// A two-dimensional array of characters in the buffer. Rows are in the first
        /// dimension, cells in the second.
        /// </summary>
        public char[,] Characters { get; }

        /// <summary>
        /// A two-dimensional array of fonts and colours in the buffer. Rows are in the
        /// first dimension, cells in the second.
        /// </summary>
        public DisplayBufferFontAndColour[,] FontsAndColours { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="countRows"></param>
        /// <param name="countCells"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DisplayBuffer(int countRows, int countCells)
        {
            if(countRows < 1) {
                throw new ArgumentOutOfRangeException(nameof(countRows));
            }
            if(countCells < 1) {
                throw new ArgumentOutOfRangeException(nameof(countCells));
            }
            Characters = new char[countRows, countCells];
            FontsAndColours = new DisplayBufferFontAndColour[countRows, countCells];
        }

        /// <inheritdoc/>
        public override string ToString() => $"DisplayBuffer[{CountRows},{CountCells}]";

        /// <summary>
        /// Copies the content of a screen buffer into the display buffer.
        /// </summary>
        /// <param name="screen"></param>
        /// <returns>
        /// True if the copy changed something in the buffer, false if the buffer already
        /// contained an exact copy of the <paramref name="screen"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool CopyFrom(Screen screen)
        {
            AssertScreenMatchesDimensions(screen);

            var result = false;

            for(var rowIdx = 0;rowIdx < screen.Rows.Length;++rowIdx) {
                var row = screen.Rows[rowIdx];
                for(var colIdx = 0;colIdx < row.Cells.Length;++colIdx) {
                    var cell = row.Cells[colIdx];
                    if(cell.Character != Characters[rowIdx, colIdx]) {
                        result = true;
                        Characters[rowIdx, colIdx] = cell.Character;
                    }
                    result = FontsAndColours[rowIdx, colIdx].CopyFrom(cell) || result;
                }
            }

            return result;
        }

        /// <summary>
        /// Copies the content of this display buffer into a screen.
        /// </summary>
        /// <param name="screen"></param>
        public void CopyTo(Screen screen)
        {
            AssertScreenMatchesDimensions(screen);

            for(var rowIdx = 0;rowIdx < screen.Rows.Length;++rowIdx) {
                var row = screen.Rows[rowIdx];
                for(var colIdx = 0;colIdx < row.Cells.Length;++colIdx) {
                    var cell = row.Cells[colIdx];
                    cell.Character = Characters[rowIdx, colIdx];
                    FontsAndColours[rowIdx, colIdx].CopyTo(cell);
                }
            }
        }

        /// <summary>
        /// Copies the content of another display buffer into this one.
        /// </summary>
        /// <param name="other"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyFrom(DisplayBuffer other)
        {
            if(other == null) {
                throw new ArgumentNullException();
            }
            if(other.CountRows != CountRows || other.CountCells != CountCells) {
                throw new ArgumentOutOfRangeException();
            }
            Array.Copy(other.Characters, Characters, Characters.Length);
            Array.Copy(other.FontsAndColours, FontsAndColours, FontsAndColours.Length);
        }

        /// <summary>
        /// Copies the content of this display buffer to another display buffer.
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(DisplayBuffer other) => other?.CopyFrom(this);

        /// <summary>
        /// Creates an independent copy of this buffer.
        /// </summary>
        /// <returns></returns>
        public DisplayBuffer Clone()
        {
            var result = new DisplayBuffer(CountRows, CountCells);
            result.CopyFrom(this);
            return result;
        }

        private void AssertScreenMatchesDimensions(Screen screen)
        {
            if(screen == null) {
                throw new ArgumentNullException(nameof(screen));
            }
            if(screen.Rows.Length != CountRows) {
                throw new ArgumentOutOfRangeException(nameof(screen));
            }
            if(screen.Rows[0].Cells.Length != CountCells) {
                throw new ArgumentOutOfRangeException(nameof(screen));
            }
        }
    }
}
