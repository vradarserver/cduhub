// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.CommandLine;

namespace ConvertFont
{
    public static class Commands
    {
        public static Command ConvertGlyph = new("glyph", "Convert a single character to the console") {
            Options.FontFamilyOption,
            Options.FontStyleOption,
            Options.CharacterOption,
            Options.GlyphWidthOption,
            Options.GlyphHeightOption,
            Options.PointSizeOption,
            Options.DrawXOption,
            Options.DrawYOption,
            Options.BrightnessThresholdOption,
        };

        public static Command ConvertInstalledFont = new("convert", "Convert installed font to an MCDU-DOTNET font file") {
            Options.OptionsFileOption,
            Options.FileOption,
        };

        public static Command CreateConvertOptions = new("create-options", "Write an empty conversion options JSON file") {
            Options.MandatoryFileOption,
            Options.OverwriteOptionsFileOption,
        };

        public static Command DumpFontFamilies = new("fonts", "Show all of the installed fonts") {
        };

        static Commands()
        {
            ConvertGlyph.SetAction(parse => {
                Program.Worked = Command_ConvertGlyph.Run(
                    parse.GetRequiredValue(Options.FontFamilyOption),
                    parse.GetValue(Options.FontStyleOption),
                    parse.GetValue(Options.CharacterOption),
                    parse.GetValue(Options.PointSizeOption),
                    parse.GetValue(Options.DrawXOption),
                    parse.GetValue(Options.DrawYOption),
                    parse.GetValue(Options.BrightnessThresholdOption),
                    parse.GetValue(Options.GlyphWidthOption),
                    parse.GetValue(Options.GlyphHeightOption)
                );
            });

            ConvertInstalledFont.SetAction(parse => {
                Program.Worked = Command_ConvertInstalledFont.Run(
                    parse.GetRequiredValue(Options.OptionsFileOption),
                    parse.GetValue(Options.FileOption)
                );
            });

            CreateConvertOptions.SetAction(parse => {
                Program.Worked = Command_CreateConvertOptions.Run(
                    parse.GetRequiredValue(Options.MandatoryFileOption),
                    parse.GetValue(Options.OverwriteOptionsFileOption)
                );
            });

            DumpFontFamilies.SetAction(_ => {
                Program.Worked = Command_DumpFontFamilies.Run();
            });

        }
    }
}
