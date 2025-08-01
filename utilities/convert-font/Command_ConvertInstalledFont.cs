﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Drawing;
using McduDotNet;
using Newtonsoft.Json;

namespace ConvertFont
{
    static class Command_ConvertInstalledFont
    {
        public static bool Run(
            FileInfo optionsFileInfo,
            FileInfo outputFileInfo
        )
        {
            Console.WriteLine($"Reading options from {optionsFileInfo}");
            var json = File.ReadAllText(optionsFileInfo.FullName);
            var options = JsonConvert.DeserializeObject<ConvertOptions>(json);

            FontFamily lookupFontFamily(string name)
            {
                Console.WriteLine($"Loading {name} font");
                var fontFamily = FontFamily
                    .Families
                    .FirstOrDefault(f => String.Equals(f.Name, name, StringComparison.InvariantCultureIgnoreCase));
                var worked = fontFamily != null;
                if(!worked) {
                    Console.WriteLine($"There is no installed font called \"{name}\"");
                }
                return fontFamily;
            }
            var largeFontFamily = lookupFontFamily(options.Large.FontFamily);
            var smallFontFamily = options.Small.FontFamily == options.Large.FontFamily
                ? largeFontFamily
                : lookupFontFamily(options.Small.FontFamily);

            outputFileInfo ??= new FileInfo(Options.SanitiseFileName($"{largeFontFamily.Name}.json"));

            var fontFile = new McduFontFile() {
                GlyphWidth = options.GlyphWidth,
                GlyphHeight = options.GlyphHeight,
                GlyphFullWidth = options.GlyphFullWidth,
                Name = largeFontFamily.Name,
                LargeGlyphs = BuildGlyphs(
                    options.Characters,
                    options.GlyphWidth,
                    options.GlyphHeight,
                    options.Large,
                    largeFontFamily,
                    isLarge: true
                ),
                SmallGlyphs = BuildGlyphs(
                    options.Characters,
                    options.GlyphWidth,
                    options.GlyphHeight,
                    options.Small,
                    smallFontFamily,
                    isLarge: false
                ),
            };
            var fontJson = JsonConvert.SerializeObject(fontFile, Formatting.Indented);
            File.WriteAllText(outputFileInfo.FullName, fontJson);
            Console.WriteLine($"Saved {fontJson.Length} characters to {outputFileInfo}");

            return true;
        }

        private static McduFontGlyph[] BuildGlyphs(
            string characters,
            int glyphWidth,
            int glyphHeight,
            FontConversionOptions options,
            FontFamily fontFamily,
            bool isLarge
        )
        {
            var result = new List<McduFontGlyph>();

            characters = String.IsNullOrEmpty(characters)
                ? McduFontFile.CharacterSet
                : characters;

            Console.WriteLine(
                $"Building {(isLarge ? "large" : "small")} " +
                $"glyphs from {options.PointSize}pt {options.Style} font " +
                $"drawn at {options.DrawX},{options.DrawY} using {options.BrightnessThreshold} brightness"
            );
            using(var font = FontConverter.CreateFont(fontFamily, options.Style, options.PointSize)) {
                foreach(var ch in characters) {
                    var glyph = new McduFontGlyph() {
                        Character = ch,
                        BitArray = FontConverter.CreateBitmap(
                            font,
                            ch,
                            options.DrawX,
                            options.DrawY,
                            options.BrightnessThreshold,
                            glyphWidth,
                            glyphHeight
                        )
                    };
                    result.Add(glyph);
                }
            }

            return [..result];
        }
    }
}
