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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// A specialised collection of <see cref="S7Digit"/> where the type of each digit and
    /// the number of digits is established by the constructor and remains immutable over
    /// the lifetime of the collection, but the segments associated with each digit can be
    /// modified.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsReadOnly"/> property returns false because you are allowed to modify
    /// the content of the collection, but any function that adds or removes elements will
    /// throw a <see cref="NotSupportedException"/> because you are not allowed to change the
    /// length of the collection after construction.
    /// </remarks>
    public class S7DigitCollection : IList<S7Digit>
    {
        /// <summary>
        /// The underlying fixed-length array of <see cref="S7Digit"/> values.
        /// </summary>
        protected S7Digit[] _S7Digits;

        /// <inheritdoc/>
        public int Count => _S7Digits.Length;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the digit at the index specified. Note that the type of the digit
        /// assigned to <see cref="index"/> is ignored, only the segments can be overwritten.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public S7Digit this[int index]
        {
            get => _S7Digits[index];
            set => SetAt(index, value.Segments);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="masks"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public S7DigitCollection(params S7[]? masks)
        {
            _S7Digits = (masks ?? Array.Empty<S7>())
                .Select(mask => new S7Digit(mask))
                .ToArray();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="other"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public S7DigitCollection(S7DigitCollection other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            _S7Digits = other
                ._S7Digits
                .ToArray();
        }

        /// <inheritdoc/>
        public override string ToString() => ToString("?");

        /// <summary>
        /// Converts the digits to a string.
        /// </summary>
        /// <param name="fallbackTextFormat">
        /// Passed through to <see cref="S7Digit.ToString"/>.
        /// </param>
        /// <returns></returns>
        public string ToString(string? fallbackTextFormat)
        {
            var buffer = new StringBuilder();
            var characterSet = S7CharacterSet.CharacterSet();
            foreach(var digit in _S7Digits) {
                digit.AppendToBuffer(characterSet, buffer, fallbackTextFormat);
            }

            return buffer.ToString();
        }


        /// <inheritdoc/>
        public IEnumerator<S7Digit> GetEnumerator() => ((IEnumerable<S7Digit>)_S7Digits).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_S7Digits).GetEnumerator();

        /// <inheritdoc/>
        public void Add(S7Digit _)  => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Clear() => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool Contains(S7Digit item) => IndexOf(item) != -1;

        /// <inheritdoc/>
        public void CopyTo(S7Digit[] array, int arrayIndex)
        {
            Array.Copy(
                _S7Digits, 0,
                array, arrayIndex,
                _S7Digits.Length
            );
        }

        /// <inheritdoc/>
        public int IndexOf(S7Digit item)
        {
            int result = -1;
            for(var idx = 0;idx < _S7Digits.Length;++idx) {
                if(_S7Digits[idx] == item) {
                    result = idx;
                    break;
                }
            }
            return result;
        }

        /// <inheritdoc/>
        public void Insert(int index, S7Digit item) => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool Remove(S7Digit item) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void RemoveAt(int index) => throw new NotSupportedException();

        /// <summary>
        /// Sets the segments of the digit at index <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="segments">
        /// The segments to try to set. The mask is applied to this value, so the segments
        /// actually set might be different.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetAt(int index, S7 segments)
        {
            if(index < 0 || index >= _S7Digits.Length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var extant = _S7Digits[index];
            var masked = extant.Mask & segments;
            if(masked != extant.Segments) {
                _S7Digits[index] = extant.NewSegments(masked);
            }
        }

        /// <summary>
        /// Switches off all segments in the display.
        /// </summary>
        public void ClearDisplay() => FillDisplay(0);

        /// <summary>
        /// Fills the display with the segments passed across;
        /// </summary>
        /// <param name="segments"></param>
        public void FillDisplay(S7 segments)
        {
            for(var idx = 0;idx < _S7Digits.Length;++idx) {
                SetAt(idx, segments);
            }
        }

        /// <summary>
        /// Sets the digits using the character set returned by <see
        /// cref="S7CharacterSet.CharacterSet"/>(). Unknown characters are ignored.
        /// </summary>
        /// <param name="text"></param>
        public void SetFrom(string? text)
        {
            SetFrom(text, S7CharacterSet.CharacterSet(), S7TextAlign.Left);
        }

        /// <summary>
        /// Sets the digits using the character set returned by <see
        /// cref="S7CharacterSet.CharacterSet"/>(). Unknown characters are ignored.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textAlign"></param>
        public void SetFrom(string? text, S7TextAlign textAlign)
        {
            SetFrom(text, S7CharacterSet.CharacterSet(), textAlign);
        }

        /// <summary>
        /// Sets the digits according to the text and character set passed across.
        /// Unknown characters are ignored.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="characterSet"></param>
        /// <param name="textAlign"></param>
        public void SetFrom(
            string? text,
            IReadOnlyDictionary<char, S7> characterSet,
            S7TextAlign textAlign
        )
        {
            text = text ?? "";
            ClearDisplay();

            if(_S7Digits.Length > 0) {
                if(textAlign != S7TextAlign.Left) {
                    text = textAlign.PadText(text, _S7Digits.Length, ignoreDecimals: true);
                }

                for(int textIdx = 0, s7Idx = 0;textIdx < text.Length && s7Idx <= _S7Digits.Length;++textIdx) {
                    var ch = text[textIdx];
                    switch(ch) {
                        case '.':
                            var setDecimalSegment = false;

                            if(!setDecimalSegment && s7Idx < _S7Digits.Length) {
                                var currentDigit = _S7Digits[s7Idx];
                                if(currentDigit.Mask.IsSet(S7.DL)) {
                                    setDecimalSegment = true;
                                    _S7Digits[s7Idx] = currentDigit.SetSegments(S7.DL);
                                }
                            }
                            if(!setDecimalSegment && s7Idx > 0) {
                                var previousDigit = _S7Digits[s7Idx - 1];
                                if(previousDigit.Mask.IsSet(S7.DR)) {
                                    setDecimalSegment = true;
                                    _S7Digits[s7Idx - 1] = previousDigit.SetSegments(S7.DR);
                                }
                            }

                            break;
                        default:
                            if(characterSet.TryGetValue(ch, out var segments)) {
                                if(s7Idx < _S7Digits.Length) {
                                    _S7Digits[s7Idx] = _S7Digits[s7Idx].SetSegments(segments);
                                }
                                ++s7Idx;
                            } else {
                                if(ch == ' ') {
                                    ++s7Idx;
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void CopyFrom(S7DigitCollection other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            for(var idx = 0;idx < _S7Digits.Length;++idx) {
                var segments = idx < other._S7Digits.Length
                    ? other._S7Digits[idx].Segments
                    : (S7)0;
                SetAt(idx, segments);
            }
        }

        public void CopyTo(S7DigitCollection? other) => other?.CopyFrom(this);
    }
}
