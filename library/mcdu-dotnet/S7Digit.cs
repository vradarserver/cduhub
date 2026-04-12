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
        /// A set of bit flags indicating which of the available segments within the
        /// seven segment display are available.
        /// </summary>
        public readonly S7 Mask { get; }

        /// <summary>
        /// A set of bit flags describing which segments are lit.
        /// </summary>
        public readonly S7 Segments { get; }

        /// <summary>
        /// True if the two digits have the same mask and segments.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator==(S7Digit lhs, S7Digit rhs)
        {
            return lhs.Mask == rhs.Mask
                && lhs.Segments == rhs.Segments;
        }

        /// <summary>
        /// True if the two digits do not have the same mask and segments.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator!=(S7Digit lhs, S7Digit rhs) => !(lhs == rhs);

        public static implicit operator S7Digit(S7 segments) => new S7Digit(segments, segments);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="segments"></param>
        public S7Digit(S7 mask, S7 segments)
        {
            Mask = mask;
            Segments = segments & mask;
        }

        /// <summary>
        /// Creates a new object with no segments lit.
        /// </summary>
        /// <param name="type"></param>
        public S7Digit(S7 mask) : this(mask, (S7)0)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="other"></param>
        public S7Digit(S7Digit other) : this(other.Mask, other.Segments)
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
        public override int GetHashCode() => ((ushort)Mask << 16) | (ushort)Segments;

        /// <inheritdoc/>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            AppendToBuffer(S7CharacterSet.CharacterSet(), buffer);
            return buffer.ToString();
        }

        /// <summary>
        /// Appends the character represented by this S7 digit to the buffer passed
        /// across.
        /// </summary>
        /// <param name="characterSet"></param>
        /// <param name="buffer"></param>
        /// <param name="fallbackTextFormat">
        /// The text to use if the digit is not represented in the character set. Default
        /// is a question mark. Supports limited sequences: "{X4}" is replaced with 4
        /// digit S7 bitflag value.
        /// </param>
        public void AppendToBuffer(
            IReadOnlyDictionary<char, S7> characterSet,
            StringBuilder buffer,
            string? fallbackTextFormat = "?"
        )
        {
            if(characterSet == null) {
                throw new ArgumentNullException(nameof(characterSet));
            }
            if(buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }

            if(Segments.IsSet(S7.DL)) {
                buffer.Append('.');
            }

            var character = Segments.FindMatchingCharacter(characterSet);
            if(character != null) {
                buffer.Append(character.Value);
            } else if(!String.IsNullOrEmpty(fallbackTextFormat)) {
                var fallback = fallbackTextFormat!.Replace("{X4}", Segments.ToString("X4"));
                buffer.Append(fallback);
            }

            if(Segments.IsSet(S7.DR)) {
                buffer.Append('.');
            }
        }

        /// <summary>
        /// Returns a digit with the same mask but different segments.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="newSegments"></param>
        /// <returns></returns>
        public S7Digit NewSegments(S7 newSegments)
        {
            return (Mask & newSegments) == Segments
                ? this
                : new(Mask, newSegments);
        }

        /// <summary>
        /// Returns a digit with the same mask but with the segments turned on.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public S7Digit SetSegments(S7 segments)
        {
            return NewSegments(Segments | segments);
        }

        /// <summary>
        /// Returns a digit with the same mask but with the segments turned off.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public S7Digit ClearSegments(S7 segments)
        {
            return NewSegments(Segments & ~segments);
        }

        /// <summary>
        /// Returns a digit with the same mask but with the segments toggled.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public S7Digit ToggleSegments(S7 segments)
        {
            return NewSegments(Segments ^ segments);
        }
    }
}
