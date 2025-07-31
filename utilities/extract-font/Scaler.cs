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

namespace ExtractFont
{
    /// <summary>
    /// Scales a bitmap array by averaging pixels.
    /// </summary>
    class Scaler
    {
        public static string[] ScaleToOptions(string[] pixelBitmap, ScaleOptions options)
        {
            return options.Divider == 1
                ? MulScale(pixelBitmap, options.Multiplier)
                : MulDivScale(pixelBitmap, options.Multiplier, options.Divider);
        }

        public static string[] MulDivScale(string[] pixelBitmap, int multiplyBy, int divideBy)
        {
            var buffer = new StringBuilder();

            var largeBitmap = new List<string>();
            var largeRowLength = -1;
            foreach(var pixelLine in pixelBitmap) {
                buffer.Clear();
                foreach(var ch in pixelLine) {
                    for(var i = 0;i < multiplyBy;++i) {
                        buffer.Append(ch);
                    }
                }

                var largeLine = buffer.ToString();
                for(var i = 0;i < multiplyBy;++i) {
                    largeBitmap.Add(largeLine);
                }

                if(largeRowLength == -1) {
                    largeRowLength = largeLine.Length;
                } else if(largeRowLength != largeLine.Length) {
                    throw new InvalidOperationException("Cannot scale bitmaps of unequal row lengths");
                }
            }

            var scaledBitmap = new List<string>();
            for(var lineIdx = 0;lineIdx + divideBy <= largeBitmap.Count;lineIdx += divideBy) {
                buffer.Clear();
                var lineEnd = lineIdx + divideBy;
                for(var colIdx = 0;colIdx + divideBy <= largeRowLength;colIdx += divideBy) {
                    var colEnd = colIdx + divideBy;

                    var countCells = 0;
                    var countSet = 0;

                    for(var pixLineIdx = lineIdx;pixLineIdx < lineEnd;++pixLineIdx) {
                        var pixLine = largeBitmap[pixLineIdx];
                        for(var pixColIdx = colIdx;pixColIdx < colEnd;++pixColIdx) {
                            ++countCells;
                            switch(pixLine[pixColIdx]) {
                                case '.':
                                case ' ':
                                    break;
                                default:
                                    ++countSet;
                                    break;
                            }
                        }
                    }

                    buffer.Append(countSet < countCells / 2
                        ? '.'
                        : 'X'
                    );
                }
                scaledBitmap.Add(buffer.ToString());
            }

            return [..scaledBitmap];
        }

        public static string[] MulScale(string[] pixelBitmap, int multiplier)
        {
            var result = new List<string>();
            var buffer = new StringBuilder();

            foreach(var row in pixelBitmap) {
                buffer.Clear();
                foreach(var ch in row) {
                    for(var i = 0;i < multiplier;++i) {
                        buffer.Append(ch);
                    }
                }

                var bufferText = buffer.ToString();
                for(var i = 0;i < multiplier;++i) {
                    result.Add(bufferText);
                }
            }

            return [..result];
        }
    }
}
