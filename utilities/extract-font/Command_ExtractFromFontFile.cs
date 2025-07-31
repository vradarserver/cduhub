// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using McduDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ExtractFont
{
    class Command_ExtractFromFontFile : Command_Base
    {
        public static bool Run(
            FontFileType fontFileType,
            FileInfo optionsFileInfo,
            FileInfo outputFileInfo
        )
        {
            switch(fontFileType) {
                case FontFileType.BiosFont:
                    ExtractBiosFont(optionsFileInfo, outputFileInfo);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        private static void ExtractBiosFont(FileInfo optionsFileInfo, FileInfo outputFileInfo)
        {
            var options = LoadBiosFontExtractionOptions(optionsFileInfo);
            var extractor = new BiosFontFileExtractor();
            var biosGlyphs = extractor.ExtractGlyphs(options);

            Console.WriteLine($"Loaded {biosGlyphs.Count} BIOS glyphs from {options.BiosFontFileName}");

            string[] findBiosGlyphForCharacter(char ch) => biosGlyphs
                .FirstOrDefault(bg => bg.BiosCodepoint != 0 && bg.Character == ch)
                ?.Bitmap;
            McduFontGlyph[] convertBitmapToGlyphs(ScaleOptions scale) => ConvertBitmapToGlyphs(
                options.Characters,
                findBiosGlyphForCharacter,
                scale,
                options.McduGlyphWidth,
                options.McduGlyphHeight
            );

            var largeGlyphs = convertBitmapToGlyphs(options.LargeFontScale);
            var smallGlyphs = convertBitmapToGlyphs(options.SmallFontScale);

            var fontFileContent = new McduFontFile() {
                Name =              options.McduFontName,
                GlyphWidth =        options.McduGlyphWidth,
                GlyphHeight =       options.McduGlyphHeight,
                GlyphFullWidth  =   options.McduGlyphFullWidth,
                LargeGlyphs =       largeGlyphs,
                SmallGlyphs =       smallGlyphs,
            };
            var json = JsonConvert.SerializeObject(
                fontFileContent,
                Formatting.Indented,
                new StringEnumConverter()
            );
            File.WriteAllText(outputFileInfo.FullName, json);

            Console.WriteLine($"Saved {json.Length} bytes to {outputFileInfo}");
        }
    }
}
