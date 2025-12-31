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

namespace wwDevicesDotNet
{
    public struct DisplayColour
    {
        /// <summary>
        /// The packed colour in RRGGBBAA format.
        /// </summary>
        public UInt32 PackedValue { get; set; }

        public byte Red
        {
            get => (byte)((PackedValue & 0xff000000) >> 24);
            set => PackedValue = ((uint)value << 24) | (PackedValue & 0x00ffffff);
        }

        public byte Green
        {
            get => (byte)((PackedValue & 0x00ff0000) >> 16);
            set => PackedValue = ((uint)value << 16) | (PackedValue & 0xff00ffff);
        }

        public byte Blue
        {
            get => (byte)((PackedValue & 0x0000ff00) >> 8);
            set => PackedValue = ((uint)value << 8) | (PackedValue & 0xffff00ff);
        }

        public byte Alpha
        {
            get => (byte)(PackedValue & 0x000000ff);
            set => PackedValue = (uint)value | (PackedValue & 0xffffff00);
        }

        public byte R
        {
            get => Red;
            set => Red = value;
        }

        public byte G
        {
            get => Green;
            set => Green = value;
        }

        public byte B
        {
            get => Blue;
            set => Blue = value;
        }

        public byte A
        {
            get => Alpha;
            set => Alpha = value;
        }

        public string WinWingColourString => $"{B:X2}{G:X2}{R:X2}{A:X2}";

        public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";

        public bool CopyFrom(PaletteColour paletteColour)
        {
            if(paletteColour == null) {
                throw new ArgumentNullException(nameof(paletteColour));
            }
            var originalValue = PackedValue;

            PackedValue =
                  (uint)(paletteColour.R << 24)
                | (uint)(paletteColour.G << 16)
                | (uint)(paletteColour.B << 8)
                | (uint)paletteColour.A;

            return PackedValue != originalValue;
        }

        public void CopyTo(PaletteColour paletteColour)
        {
            if(paletteColour == null) {
                throw new ArgumentNullException(nameof(paletteColour));
            }
            paletteColour.R = R;
            paletteColour.G = G;
            paletteColour.B = B;
            paletteColour.A = A;
        }

        public void CopyFrom(DisplayColour other) => PackedValue = other.PackedValue;

        public void CopyTo(DisplayColour other) => other.PackedValue = PackedValue;
    }
}
