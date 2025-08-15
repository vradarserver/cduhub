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
using System.CommandLine.Parsing;
using System.Drawing;
using System.Text;

namespace ConvertFont
{
    class Options
    {
        public static readonly Option<float> BrightnessThresholdOption = new("--brightness", "-b") {
            Description = "Brightness threshold (from 0 to 1) below which a pixel is considered set",
            DefaultValueFactory = _ => 0.8F,
            Validators = {
                arg => {
                    var value = arg.GetValueOrDefault<float>();
                    if(value < 0F || value > 1F) {
                        arg.AddError("Brightness threshold must be between 0 and 1");
                    }
                }
            },
        };

        public static readonly Option<char> CharacterOption = new("--char") {
            Description = "Character to render a glyph for",
            DefaultValueFactory = _ => 'W',
            CustomParser = arg => ParseChar(arg),
        };

        public static readonly Option<int> DrawXOption = new("--drawx", "-x") {
            Description = "X coordinate to use when rendering font glyphs into bitmaps to extract pixels",
            DefaultValueFactory = _ => 0,
        };

        public static readonly Option<int> DrawYOption = new("--drawy", "-y") {
            Description = "Y coordinate to use when rendering font glyphs into bitmaps to extract pixels",
            DefaultValueFactory = _ => 0,
        };

        public static readonly Option<FileInfo> FileOption = new("--file", "-f") {
            Description = "The name of the font file to write (uses font family name if not specified)",
            Required = false,
        };

        public static readonly Option<FontFamily> FontFamilyOption = new("--font") {
            Description = "The font family to convert",
            Required = true,
            CustomParser = arg => ParseFontFamily(arg),
        };

        public static readonly Option<FontStyle> FontStyleOption = new("--style") {
            Description = "The font style to use during conversion",
            DefaultValueFactory = _ => FontStyle.Regular,
        };

        public static readonly Option<int> GlyphHeightOption = new("--glyphHeight", "-h") {
            Description = "The glyph height in pixels",
            DefaultValueFactory = _ => 29,
        };

        public static readonly Option<int> GlyphWidthOption = new("--glyphWidth", "-w") {
            Description = "The glyph width in pixels",
            DefaultValueFactory = _ => 23,
        };

        public static readonly Option<int> GlyphFullWidthOption = new("--glyphFullWidth") {
            Description = "The glyph full width in pixels",
            DefaultValueFactory = _ => 23,
        };

        public static readonly Option<FileInfo> LargePixelFontFile = new("--large", "-l") {
            Description = "The large pixel font file",
            Required = true,
        };

        public static readonly Option<FileInfo> MandatoryFileOption = new("--file", "-f") {
            Description = "The name of the file to write",
            Required = true,
        };

        public static readonly Option<FileInfo> OptionsFileOption = new("--options") {
            Description = "The JSON file containing options for conversion",
            Required = true,
        };

        public static readonly Option<bool> OverwriteOptionsFileOption = new("--overwrite") {
            Description = "Overwrite the options file instead of modifying it",
            DefaultValueFactory = _ => false,
        };

        public static readonly Option<float> PointSizeOption = new("--point") {
            Description = "The point size to use when rendering font glyphs into bitmaps to extract pixels",
            DefaultValueFactory = _ => 20F,
        };

        public static readonly Option<int> ScaleFactorOption = new("--scale") {
            Description = "The scaling factor to use when scaling pixel fonts",
            DefaultValueFactory = _ => 1,
        };

        public static readonly Option<FileInfo> SmallPixelFontFile = new("--small", "-s") {
            Description = "The small pixel font file",
            Required = true,
        };

        public static char ParseChar(ArgumentResult arg)
        {
            char result = default;

            if(arg.Tokens.Count == 1) {
                result = arg.Tokens[0].Value.FirstOrDefault();
            }

            return result;
        }

        public static FontFamily ParseFontFamily(ArgumentResult arg)
        {
            FontFamily result = null;

            if(arg.Tokens.Count == 1) {
                result = FontFamily.Families.FirstOrDefault(
                    family => String.Equals(family.Name, arg.Tokens[0].Value, StringComparison.InvariantCultureIgnoreCase)
                );
                if(result == null) {
                    result = FontFamily.Families.FirstOrDefault(
                        family => String.Equals(family.Name.Replace(" ", ""), arg.Tokens[0].Value, StringComparison.InvariantCultureIgnoreCase)
                    );
                }
                if(result == null) {
                    arg.AddError($"Font family {arg.Tokens[0].Value} does not exist");
                }
            }

            return result;
        }

        public static string SanitiseFileName(string fileName, char replaceInvalidWith = '-')
        {
            var result = new StringBuilder(fileName);
            var invalid = Path.GetInvalidFileNameChars();
            for(var idx = 0;idx < result.Length;++idx) {
                if(invalid.Contains(result[idx])) {
                    result[idx] = replaceInvalidWith;
                }
            }

            return result.ToString();
        }
    }
}
