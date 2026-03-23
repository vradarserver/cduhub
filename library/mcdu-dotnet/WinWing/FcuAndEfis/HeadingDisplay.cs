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
    /// Bitmaps etc. for the heading portion of the FCU's speed panel. These occupy
    /// bytes 3 through 6 in the FCU payload.
    /// </summary>
    static class HeadingDisplay
    {
        // Another single digit pattern that gets repeated across all digits in the
        // display, but this one spans two bytes starting at payload offset 3.
        public static readonly IReadOnlyList<S7Bitmap> DigitBitmap = new S7Bitmap[] {
            new(S7.DL, 0, 0x10),
            new(S7.BL, 0, 0x20),
            new(S7.MM, 0, 0x40),
            new(S7.TL, 0, 0x80),
            new(S7.BB, 1, 0x01),
            new(S7.BR, 1, 0x02),
            new(S7.TR, 1, 0x04),
            new(S7.TT, 1, 0x08),
        };

        public static readonly ByteBitmap DotBit = new(6, 0x10);

        public static readonly ByteBitmap LatBit = new(6, 0x20);

        public static readonly ByteBitmap TrkBit = new(6, 0x40);

        public static readonly ByteBitmap HdgBit = new(6, 0x80);
    }
}
