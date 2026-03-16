// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// Describes a single seven segment digit.
    /// </summary>
    public readonly struct S7Digit
    {
        /// <summary>
        /// Whether the decimal point appears to the left or right of this digit, or
        /// whether this digit has a decimal point at all.
        /// </summary>
        public readonly S7Type Type { get; }

        /// <summary>
        /// A set of bit flags describing which segments are lit.
        /// </summary>
        public readonly S7 Segments { get; }

        /// <summary>
        /// True if the two digits have the same type and segments.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator==(S7Digit lhs, S7Digit rhs)
        {
            return lhs.Type == rhs.Type
                && lhs.Segments == rhs.Segments;
        }

        /// <summary>
        /// True if the two digits do not have the same type and segments.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator!=(S7Digit lhs, S7Digit rhs) => !(lhs == rhs);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="segments"></param>
        public S7Digit(S7Type type, S7 segments)
        {
            Type = type;
            Segments = segments;
        }

        /// <summary>
        /// Creates a new object with no segments lit.
        /// </summary>
        /// <param name="type"></param>
        public S7Digit(S7Type type) : this(type, (S7)0)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="other"></param>
        public S7Digit(S7Digit other) : this(other.Type, other.Segments)
        {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is S7Digit other) {
                result = this == other;
            }
            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => ((byte)Type << 8) | (byte)Segments;

        /// <inheritdoc/>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            AppendToBuffer(S7CharacterSet.CharacterSet(), buffer);
            return buffer.ToString();
        }

        /// <summary>
        /// If the <see cref="Segments"/> correspond with any character in <see cref="characterSet"/>
        /// then the character is appended to <see cref="buffer"/>, otherwise the hex code for the
        /// character is appended. The characters / hex is appended or prepended with a decimal place
        /// if the type allows it and the decimal place is set.
        /// </summary>
        /// <param name="characterSet"></param>
        /// <param name="buffer"></param>
        public void AppendToBuffer(IReadOnlyDictionary<char, S7> characterSet, StringBuilder buffer)
        {
            if(characterSet == null) {
                throw new ArgumentNullException(nameof(characterSet));
            }
            if(buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }
            var showDecimal = Type.ShowDecimal() && Segments.HasDecimal();

            if(showDecimal && Type.IsDecimalPrefix()) {
                buffer.Append('.');
            }

            var character = Segments.FindMatchingCharacter(characterSet);
            if(character != null) {
                buffer.Append(character.Value);
            } else {
                buffer.Append('(');
                buffer.Append("0x");
                buffer.Append(Segments.Digit().ToString("X2"));
                buffer.Append(')');
            }

            if(showDecimal && Type.IsDecimalSuffix()) {
                buffer.Append('.');
            }
        }

        /// <summary>
        /// Creates a new digit with the same type but different segments.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="newSegments"></param>
        /// <returns></returns>
        public static S7Digit NewSegments(S7Digit original, S7 newSegments)
        {
            return new(original.Type, newSegments);
        }
    }
}
