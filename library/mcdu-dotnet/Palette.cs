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

namespace wwDevicesDotNet
{
    /// <summary>
    /// Describes the colours that make up the palette for an MCDU device.
    /// </summary>
    public class Palette
    {
        public const string DefaultBlackRgb =   "000000";
        public const string DefaultAmberRgb =   "FFA500";
        public const string DefaultWhiteRgb =   "FFFFFF";
        public const string DefaultCyanRgb =    "00FFFF";
        public const string DefaultGreenRgb =   "00FF3D";
        public const string DefaultMagentaRgb = "FF63FF";
        public const string DefaultRedRgb =     "FF0000";
        public const string DefaultYellowRgb =  "FFFF00";
        public const string DefaultBrownRgb =   "615C42";
        public const string DefaultGreyRgb =    "777777";
        public const string DefaultKhakiRgb =   "79735E";

        public PaletteColour Black { get; } = PaletteColour.Parse(DefaultBlackRgb);

        public PaletteColour Amber { get; } = PaletteColour.Parse(DefaultAmberRgb);

        public PaletteColour White { get; } = PaletteColour.Parse(DefaultWhiteRgb);

        public PaletteColour Cyan { get; } = PaletteColour.Parse(DefaultCyanRgb);

        public PaletteColour Green { get; } = PaletteColour.Parse(DefaultGreenRgb);

        public PaletteColour Magenta { get; } = PaletteColour.Parse(DefaultMagentaRgb);

        public PaletteColour Red { get; } = PaletteColour.Parse(DefaultRedRgb);

        public PaletteColour Yellow { get; } = PaletteColour.Parse(DefaultYellowRgb);

        public PaletteColour Brown { get; } = PaletteColour.Parse(DefaultBrownRgb);

        public PaletteColour Grey { get; } = PaletteColour.Parse(DefaultGreyRgb);

        public PaletteColour Khaki { get; } = PaletteColour.Parse(DefaultKhakiRgb);

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

        public void CopyFrom(Palette other)
        {
            Black.CopyFrom(other?.Black);
            Amber.CopyFrom(other?.Amber);
            White.CopyFrom(other?.White);
            Cyan.CopyFrom(other?.Cyan);
            Green.CopyFrom(other?.Green);
            Magenta.CopyFrom(other?.Magenta);
            Red.CopyFrom(other?.Red);
            Yellow.CopyFrom(other?.Yellow);
            Brown.CopyFrom(other?.Brown);
            Grey.CopyFrom(other?.Grey);
            Khaki.CopyFrom(other?.Khaki);
        }

        public void CopyTo(Palette other) => other?.CopyFrom(this);
    }
}
