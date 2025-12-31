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

namespace WwDevicesDotNet
{
    /// <summary>
    /// Describes a row of cells on the MCDU display.
    /// </summary>
    public class Row
    {
        public Cell[] Cells { get; } = new Cell[Metrics.Columns];

        public Row()
        {
            for(var idx = 0;idx < Cells.Length;++idx) {
                Cells[idx] = new Cell();
            }
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            foreach(var cell in Cells) {
                buffer.Append(cell.Character);
            }
            return buffer.ToString();
        }

        public void Clear()
        {
            for(var idx = 0;idx < Cells.Length;++idx) {
                Cells[idx].Clear();
            }
        }

        public void CopyTo(Row other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            for(var idx = 0;idx < Cells.Length;++idx) {
                other.Cells[idx].CopyFrom(Cells[idx]);
            }
        }

        public void CopyFrom(Row other) => other?.CopyTo(this);

        public void ShiftRight(int startColumn, int length, int count)
        {
            if(count > 0 && length > 0) {
                for(var idx = length - 1;idx >= 0;--idx) {
                    var fromIdx = startColumn + idx;
                    var toIdx = fromIdx + count;
                    Cells[fromIdx].CopyTo(Cells[toIdx]);
                }
                for(var idx = 0;idx < count;++idx) {
                    Cells[startColumn + idx].Clear();
                }
            }
        }
    }
}
