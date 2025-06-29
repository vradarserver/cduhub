// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace Cduhub.FlightSim
{
    /// <summary>
    /// A subset of <see cref="BitConverter"/> that assumes the use of little-endian values even if the
    /// architecture is big-endian.
    /// </summary>
    public static class LittleEndian
    {
        public static byte[] GetBytes(int value) => ToLittleEndian(BitConverter.GetBytes(value));

        public static int ToInt32(byte[] buffer, int offset) => BitConverter.ToInt32(FromLittleEndian(buffer, offset, 4), 0);

        public static float ToSingle(byte[] buffer, int offset) => BitConverter.ToSingle(FromLittleEndian(buffer, offset, 4), 0);

        private static byte[] FromLittleEndian(byte[] buffer, int offset, int length)
        {
            var result = new byte[length];
            Array.Copy(buffer, offset, result, 0, length);
            return ToLittleEndian(result);
        }

        private static byte[] ToLittleEndian(byte[] bytes)
        {
            var result = bytes;

            if(!BitConverter.IsLittleEndian) {
                var left = 0;
                var right = bytes.Length - 1;
                for(;left < right;++left, --right) {
                    (result[right], result[left]) = (result[left], result[right]);
                }
            }

            return result;
        }
    }
}
