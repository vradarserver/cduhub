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

namespace McduDotNet
{
    /// <summary>
    /// Extensions to the <see cref="S7"/> seven segment digit bitflags enum.
    /// </summary>
    public static class S7Extensions
    {
        /// <summary>
        /// Returns the value with the decimal point masked out.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static S7 Digit(this S7 value) => value & ~S7.DP;

        /// <summary>
        /// True if the value has the decimal point bit set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasDecimal(this S7 value) => (value & S7.DP) != 0;

        /// <summary>
        /// Returns the character associated with the S7 value in the character set passed
        /// across or null if no match could be found.
        /// </summary>
        /// <param name="characterSet"></param>
        /// <returns></returns>
        public static char? FindMatchingCharacter(this S7 value, IReadOnlyDictionary<char, S7>? characterSet)
        {
            char? result = null;

            if(characterSet != null) {
                var digit = value.Digit();
                foreach(var kvp in characterSet) {
                    if(kvp.Value == digit) {
                        result = kvp.Key;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
