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
    public static class S7TypeExtensions
    {
        /// <summary>
        /// True if the <see cref="S7Type"/> contains a decimal somewhere.
        /// </summary>
        /// <param name="s7Type"></param>
        /// <returns></returns>
        public static bool ShowDecimal(this S7Type s7Type) => s7Type != S7Type.Digit;

        /// <summary>
        /// True if the decimal place appears before the digit.
        /// </summary>
        /// <param name="s7Type"></param>
        /// <returns></returns>
        public static bool IsDecimalPrefix(this S7Type s7Type) => s7Type == S7Type.DigitLeftDecimal;

        /// <summary>
        /// True if the decimal place appears after the digit.
        /// </summary>
        /// <param name="s7Type"></param>
        /// <returns></returns>
        public static bool IsDecimalSuffix(this S7Type s7Type) => s7Type == S7Type.DigitRightDecimal;
    }
}
