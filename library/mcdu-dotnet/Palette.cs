// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// Describes the colours that make up the palette for an MCDU device.
    /// </summary>
    public class Palette
    {
        public PaletteColour Black { get; } = PaletteColour.Parse("000000");

        public PaletteColour Amber { get; } = PaletteColour.Parse("FFA500");

        public PaletteColour White { get; } = PaletteColour.Parse("FFFFFF");

        public PaletteColour Cyan { get; } = PaletteColour.Parse("00FFFF");

        public PaletteColour Green { get; } = PaletteColour.Parse("00FF3D");

        public PaletteColour Magenta { get; } = PaletteColour.Parse("FF63FF");

        public PaletteColour Red { get; } = PaletteColour.Parse("FF0000");

        public PaletteColour Yellow { get; } = PaletteColour.Parse("FFFF00");

        public PaletteColour Brown { get; } = PaletteColour.Parse("615C42");

        public PaletteColour Grey { get; } = PaletteColour.Parse("777777");

        public PaletteColour Khaki { get; } = PaletteColour.Parse("79735e");

        public PaletteColour[] ToWinWingOrdinalColours()
        {
            return new PaletteColour[] {
                Black,
                Amber,
                White,
                Cyan,
                Green,
                Magenta,
                Red,
                Yellow,
                Brown,
                Grey,
                Khaki,
            };
        }

        public static string BuildDuplicateCheckString(PaletteColour[] colourArray)
        {
            var buffer = new StringBuilder();

            foreach(var colour in colourArray) {
                buffer.Append(colour.ToWinwingColourString());
            }

            return buffer.ToString();
        }
    }
}
