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
using System.IO;
using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// Describes a colour in the device's colour palette.
    /// </summary>
    public class PaletteColour
    {
        public byte Red { get; set; }

        public byte R
        {
            get => Red;
            set => Red = value;
        }

        public byte Green { get; set; }

        public byte G
        {
            get => Green;
            set => Green = value;
        }

        public byte Blue { get; set; }

        public byte B
        {
            get => Blue;
            set => Blue = value;
        }

        public byte Alpha { get; set; } = 0xff;

        public byte A
        {
            get => Alpha;
            set => Alpha = value;
        }

        public PaletteColour() : this(0, 0, 0, 0xff)
        {
        }

        public PaletteColour(byte red, byte green, byte blue) : this(red, green, blue, 0xff)
        {
        }

        public PaletteColour(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public override string ToString() => $"{R:X2}{G:X2}{B:X2}@{A:X2}";

        public static PaletteColour Parse(string rgba)
        {
            if(!TryParse(rgba, out var result)) {
                throw new InvalidDataException($"{rgba} is not a valid palette colour");
            }
            return result;
        }

        public static bool TryParse(string? rgba, out PaletteColour result)
        {
            var parsed = false;
            result = new PaletteColour();

            if(rgba != null) {
                parsed = true;
                var normalised = new StringBuilder();
                for(var i = 0;i < rgba.Length;++i) {
                    var ch = rgba[i];
                    if(StringExtensions.TryConvertNibble(ch, out var _)) {
                        normalised.Append(ch);
                    }
                }

                switch(normalised.Length) {
                    case 3:
                        result.R = DecodeHex(normalised, 0, 1);
                        result.G = DecodeHex(normalised, 1, 1);
                        result.B = DecodeHex(normalised, 2, 1);
                        break;
                    case 4:
                        result.A = DecodeHex(normalised, 3, 1);
                        goto case 3;
                    case 6:
                        result.R = DecodeHex(normalised, 0, 2);
                        result.G = DecodeHex(normalised, 2, 2);
                        result.B = DecodeHex(normalised, 4, 2);
                        break;
                    case 8:
                        result.A = DecodeHex(normalised, 6, 2);
                        goto case 6;
                    default:
                        parsed = false;
                        break;
                }
            }

            return parsed;
        }

        private static byte DecodeHex(StringBuilder buffer, int start, int length)
        {
            byte value = 0;
            for(var count = 0;count < length;++count) {
                var ch = buffer[start + count];
                if(StringExtensions.TryConvertNibble(ch, out var nibble)) {
                    value = (byte)(value << (4 * count));
                    value |= nibble;
                }
            }

            if(length == 1) {
                value |= (byte)(value << 4);
            }

            return value;
        }

        public void Set(byte red, byte green, byte blue, byte alpha = 0xff)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public void CopyFrom(PaletteColour other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            Set(other.Red, other.Green, other.Blue, other.Alpha);
        }

        public void CopyTo(PaletteColour other) => other?.CopyFrom(this);
    }
}
