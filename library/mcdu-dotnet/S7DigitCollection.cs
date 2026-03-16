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
        /// <param name="digitTypes"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public S7DigitCollection(params S7Type[]? digitTypes)
        {
            _S7Digits = (digitTypes ?? Array.Empty<S7Type>())
                .Select(type => new S7Digit(type))
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
        public override string ToString()
        {
            var buffer = new StringBuilder();
            var characterSet = S7CharacterSet.CharacterSet();
            foreach(var digit in _S7Digits) {
                digit.AppendToBuffer(characterSet, buffer);
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
        /// <param name="segments"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetAt(int index, S7 segments)
        {
            if(index < 0 || index >= _S7Digits.Length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _S7Digits[index] = new S7Digit(_S7Digits[index].Type, segments);
        }
    }
}
