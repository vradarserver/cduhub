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
using System.Linq;

namespace McduDotNet
{
    /// <summary>
    /// A dictionary of characters to 7-segment digits.
    /// </summary>
    public static class S7CharacterSet
    {
        private static readonly object _SyncLock = new();
        private static Dictionary<char, S7> _CharacterSet = null!;

        private static readonly Dictionary<char, S7> _GenericDigits = new() {
            { '0', S7.TL | S7.TT | S7.TR | S7.BL | S7.BR | S7.BB },
            { '1', S7.TR | S7.BR },
            { '2', S7.TT | S7.TR | S7.MM | S7.BL | S7.BB },
            { '3', S7.TT | S7.TR | S7.MM | S7.BR | S7.BB },
            { '4', S7.TL | S7.TR | S7.MM | S7.BR },
            { '5', S7.TL | S7.TT | S7.MM | S7.BR | S7.BB },
            { '6', S7.TL | S7.MM | S7.BL | S7.BR | S7.BB },
            { '7', S7.TT | S7.TR | S7.BR },
            { '8', S7.TL | S7.TT | S7.TR | S7.MM | S7.BL | S7.BR | S7.BB },
            { '9', S7.TL | S7.TT | S7.TR | S7.MM | S7.BR },
        };

        /// <summary>
        /// A generic set of digits from 0 through 9.
        /// </summary>
        public static IReadOnlyDictionary<char, S7> GenericDigits => _GenericDigits;

        private static readonly Dictionary<char, S7> _Punctuation = new() {
            { ' ', (S7)0 },
            { '-', S7.MM },
            { '|', S7.TC | S7.BC },
            { ':', S7.TC | S7.BC },
            { '+', S7.MM | S7.TC | S7.BC },
            { '_', S7.BB },
            { '=', S7.MM | S7.BB },
            { '"', S7.TL | S7.TR },
            { '\'', S7.TR },
        };

        /// <summary>
        /// A set of punctuation characters.
        /// </summary>
        public static IReadOnlyDictionary<char, S7> Punctuation => _Punctuation;

        /// <inheritdoc/>
        static S7CharacterSet()
        {
            ComposeCharacterSet(_GenericDigits, _Punctuation);
        }

        /// <summary>
        /// Returns the S7 character set.
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<char, S7> CharacterSet() => _CharacterSet;

        /// <summary>
        /// Composes a new character set built from the discrete character sets passed across.
        /// </summary>
        /// <param name="characterSets"></param>
        public static void ComposeCharacterSet(params IDictionary<char, S7>[] characterSets)
        {
            lock(_SyncLock) {
                var newDictionary = new Dictionary<char, S7>();
                if(characterSets != null) {
                    foreach(var characterSet in characterSets.OfType<IDictionary<char, S7>>()) {
                        foreach(var kvp in characterSet) {
                            newDictionary[kvp.Key] = kvp.Value;
                        }
                    }
                }
                _CharacterSet = newDictionary;
            }
        }
    }
}
