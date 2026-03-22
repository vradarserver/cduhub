// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet
{
    /// <summary>
    /// Common masks for <see cref="S7"/> digits.
    /// </summary>
    public static class S7Masks
    {
        /// <summary>
        /// Masks out the seven digit segments in a seven-segment digit display.
        /// </summary>
        public const S7 Digit = S7.TT | S7.TR | S7.MM | S7.BR | S7.BB | S7.BL | S7.TL;

        /// <summary>
        /// Masks out the digit and leading decimal segments in a seven-segment digit display.
        /// </summary>
        public const S7 DigitLeftDecimal = Digit | S7.DL;

        /// <summary>
        /// Masks out the digit and trailing decimal segments in a seven-segment digit display.
        /// </summary>
        public const S7 DigitRightDecimal = Digit | S7.DR;

        /// <summary>
        /// Masks out both decimal place segments in a seven-segment digit
        /// display.
        /// </summary>
        public const S7 DecimalPoints = S7.DL | S7.DR;

        /// <summary>
        /// Masks out the vertical part of a PLUS symbol in a seven-segment digit display.
        /// </summary>
        public const S7 VerticalCentreBar = S7.TC | S7.BC;

        /// <summary>
        /// Masks out the vertical centre bar and the middle horizontal segments in a
        /// seven-segment digit display.
        /// </summary>
        public const S7 Plus = VerticalCentreBar | S7.MM;
    }
}
