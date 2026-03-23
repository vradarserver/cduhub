// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;

namespace McduDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// Bitmaps etc. for the EFIS baro segmented display.
    /// </summary>
    static class BaroDisplay
    {
        // All of the barometer digits are contained within a single byte, so we just need
        // one bitmap and repeat it across four bytes.
        public static readonly IReadOnlyList<S7Bitmap> DigitBitmap = new S7Bitmap[] {
            new(S7.TL, 0, 0x01),
            new(S7.MM, 0, 0x02),
            new(S7.BL, 0, 0x04),
            new(S7.BB, 0, 0x08),
            new(S7.TT, 0, 0x10),
            new(S7.TR, 0, 0x20),
            new(S7.BR, 0, 0x40),
            new(S7.DR, 0, 0x80),
        };

        /// <summary>
        /// Offset and bit for the QFE segment where offset 0 is the start of the digits.
        /// </summary>
        public static readonly ByteBitmap QfeBit = new(4, 0x01);

        /// <summary>
        /// Offset and bit for the QNH segment where offset 0 is the start of the digits.
        /// </summary>
        public static readonly ByteBitmap QnhBit = new(4, 0x02);
    }
}
