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

namespace McduDotNet
{
    /// <summary>
    /// Sets bits in byte buffers.
    /// </summary>
    public static class Bitmapper
    {
        /// <summary>
        /// Sets a bit in a buffer if <see cref="setBit"/> is true, otherwise does nothing.
        /// Will not clear bits.
        /// </summary>
        /// <param name="setBit"></param>
        /// <param name="bitmap"></param>
        /// <param name="buffer"></param>
        public static void SetBit(bool setBit, ByteBitmap bitmap, byte[] buffer)
        {
            if(buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }
            CheckedSetBit(setBit, bitmap, buffer, offset: 0);
        }

        /// <summary>
        /// Sets a bit in a buffer if <see cref="setBit"/> is true, otherwise does nothing.
        /// Will not clear bits.
        /// </summary>
        /// <param name="setBit"></param>
        /// <param name="bitmap"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetBit(bool setBit, ByteBitmap bitmap, byte[] buffer, int offset)
        {
            if(buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }
            CheckedSetBit(setBit, bitmap, buffer, offset);
        }

        private static void CheckedSetBit(bool setBit, ByteBitmap bitmap, byte[] buffer, int offset)
        {
            if(setBit) {
                var bufferOffset = bitmap.BufferOffset + offset;
                if(buffer.Length <= bufferOffset) {
                    throw new InvalidOperationException(
                        $"Cannot write to byte {bufferOffset} of a {buffer.Length} byte buffer"
                    );
                }
                buffer[bufferOffset] = (byte)(buffer[bufferOffset] | bitmap.BufferBit);
            }
        }

        /// <summary>
        /// Sets bits corresponding to an S7 segment bitflag and an array of bitmaps that
        /// map segments to offsets and bits. Will not clear bits.
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="bitmaps"></param>
        /// <param name="buffer"></param>
        public static void SetS7Bits(
            S7 segments,
            IReadOnlyList<S7Bitmap> bitmaps,
            byte[] buffer
        )
        {
            SetS7Bits(segments, bitmaps, buffer, offset: 0);
        }

        /// <summary>
        /// Sets bits corresponding to an S7 segment bitflag and an array of bitmaps that
        /// map segments to offsets and bits. Will not clear bits.
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="bitmaps"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetS7Bits(
            S7 segments,
            IReadOnlyList<S7Bitmap> bitmaps,
            byte[] buffer,
            int offset
        )
        {
            if(bitmaps == null) {
                throw new ArgumentNullException(nameof(bitmaps));
            }
            if(buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }

            for(var idx = 0;idx < bitmaps.Count;++idx) {
                var bitmap = bitmaps[idx];
                CheckedSetBit(
                    (segments & bitmap.S7Bit) == bitmap.S7Bit,
                    bitmap.Bitmap,
                    buffer,
                    offset
                );
            }
        }

        public static void SetRepeatingS7DigitCollection(
            S7DigitCollection digits,
            IReadOnlyList<S7Bitmap> bitmaps,
            byte[] buffer,
            int offset
        )
        {
            for(var idx = 0;idx < digits.Count;++idx) {
                var s7 = digits[idx];
                Bitmapper.SetS7Bits(s7.Segments, bitmaps, buffer, offset + idx);
            }
        }
    }
}
