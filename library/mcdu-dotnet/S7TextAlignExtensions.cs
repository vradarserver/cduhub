// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// Extends the <see cref="S7TextAlign"/> enum.
    /// </summary>
    public static class S7TextAlignExtensions
    {
        /// <summary>
        /// Returns the string padded to the length and alignment specified.
        /// </summary>
        /// <param name="textAlign"></param>
        /// <param name="text"></param>
        /// <param name="toLength"></param>
        /// <param name="ignoreDecimals">
        /// True if decimals do not count towards string length. They're still
        /// included in the output.
        /// </param>
        /// <returns></returns>
        public static string PadText(
            this S7TextAlign textAlign,
            string? text,
            int toLength,
            bool ignoreDecimals
        )
        {
            var result = new StringBuilder();

            var textLength = 0;
            if(text != null) {
                for(var idx = 0;idx < text.Length;++idx) {
                    var ch = text[idx];
                    result.Append(ch);
                    if(!ignoreDecimals || ch != '.') {
                        ++textLength;
                    }
                }
            }

            while(textLength < toLength) {
                switch(textAlign) {
                    case S7TextAlign.Left:
                        result.Append(' ');
                        break;
                    case S7TextAlign.Right:
                        result.Insert(0, ' ');
                        break;
                    case S7TextAlign.Centre:
                        if(textLength % 2 == 0) {
                            goto case S7TextAlign.Right;
                        }
                        goto case S7TextAlign.Left;
                }
                ++textLength;
            }

            return result.ToString();
        }
    }
}
