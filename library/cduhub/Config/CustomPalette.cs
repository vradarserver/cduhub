// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.Serialization;
using WwDevicesDotNet;

namespace Cduhub.Config
{
    [DataContract]
    public class CustomPalette
    {
        [DataMember]
        public string SettingsName { get; set; }

        [DataMember]
        public bool Enable { get; set; }

        [DataMember]
        public string BlackRGB { get; set; } = Palette.DefaultBlackRgb;

        [DataMember]
        public string AmberRGB { get; set; } = Palette.DefaultAmberRgb;

        [DataMember]
        public string WhiteRGB { get; set; } = Palette.DefaultWhiteRgb;

        [DataMember]
        public string CyanRGB { get; set; } = Palette.DefaultCyanRgb;

        [DataMember]
        public string GreenRGB { get; set; } = Palette.DefaultGreenRgb;

        [DataMember]
        public string MagentaRGB { get; set; } = Palette.DefaultMagentaRgb;

        [DataMember]
        public string RedRGB { get; set; } = Palette.DefaultRedRgb;

        [DataMember]
        public string YellowRGB { get; set; } = Palette.DefaultYellowRgb;

        [DataMember]
        public string BrownRGB { get; set; } = Palette.DefaultBrownRgb;

        [DataMember]
        public string GreyRGB { get; set; } = Palette.DefaultGreyRgb;

        [DataMember]
        public string KhakiRGB { get; set; } = Palette.DefaultKhakiRgb;

        public string NormalisedSettingsName()
        {
            return Settings.NormaliseMcduUsableName(SettingsName);
        }

        public Palette ToPalette()
        {
            var result = new Palette();

            Parse(result.Black,     BlackRGB,   Palette.DefaultBlackRgb);
            Parse(result.Amber,     AmberRGB,   Palette.DefaultAmberRgb);
            Parse(result.White,     WhiteRGB,   Palette.DefaultWhiteRgb);
            Parse(result.Cyan,      CyanRGB,    Palette.DefaultCyanRgb);
            Parse(result.Green,     GreenRGB,   Palette.DefaultGreenRgb);
            Parse(result.Magenta,   MagentaRGB, Palette.DefaultMagentaRgb);
            Parse(result.Red,       RedRGB,     Palette.DefaultRedRgb);
            Parse(result.Yellow,    YellowRGB,  Palette.DefaultYellowRgb);
            Parse(result.Brown,     BrownRGB,   Palette.DefaultBrownRgb);
            Parse(result.Grey,      GreyRGB,    Palette.DefaultGreyRgb);
            Parse(result.Khaki,     KhakiRGB,   Palette.DefaultKhakiRgb);

            return result;
        }

        private void Parse(PaletteColour colour, string rgbString, string defaultRgb)
        {
            if(!PaletteColour.TryParse(rgbString, out var parsed)) {
                PaletteColour.TryParse(defaultRgb, out parsed);
            }
            colour.CopyFrom(parsed);
        }
    }
}
