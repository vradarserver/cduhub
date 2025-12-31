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
using WwDevicesDotNet;
using Newtonsoft.Json;

namespace ExtractFont
{
    class Command_Base
    {
        protected static BiosFontExtractionOptions LoadBiosFontExtractionOptions(FileInfo optionsFileInfo)
        {
            var json = File.ReadAllText(optionsFileInfo.FullName);
            var result = JsonConvert.DeserializeObject<BiosFontExtractionOptions>(json);
            if(String.IsNullOrEmpty(result.BiosFontFileName)) {
                throw new InvalidOperationException($"{optionsFileInfo} does not have the {nameof(result.BiosFontFileName)} value set");
            }
            if(!File.Exists(result.BiosFontFileName)) {
                throw new FileNotFoundException($"{result.BiosFontFileName} does not exist");
            }

            return result;
        }

        protected static McduFontGlyph[] ConvertBitmapToGlyphs(
            string characters,
            Func<char, string[]> getBitmapForCharacter,
            ScaleOptions scale,
            int fontGlyphWidth,
            int fontGlyphHeight
        )
        {
            var result = new List<McduFontGlyph>();
            if(String.IsNullOrEmpty(characters)) {
                characters = McduFontFile.CharacterSet;
            }

            foreach(var ch in characters) {
                var bitmap = getBitmapForCharacter(ch);
                var glyph = ConvertBitmapToGlyph(
                    ch,
                    bitmap,
                    scale,
                    fontGlyphWidth,
                    fontGlyphHeight
                );
                if(glyph != null) {
                    result.Add(glyph);
                }
            }

            return [..result];
        }

        protected static McduFontGlyph ConvertBitmapToGlyph(
            char character,
            string[] originalBitmap,
            ScaleOptions scale,
            int fontGlyphWidth,
            int fontGlyphHeight
        )
        {
            McduFontGlyph result = null;

            if((originalBitmap?.Length ?? 0) > 0) {
                var scaledBitmap = Scaler.ScaleToOptions(originalBitmap, scale);

                var fontRows = new string[fontGlyphHeight];
                var fontRow = new StringBuilder();
                var fontRowIdx = fontRows.Length - 1;
                for(var bitmapRowIdx = scaledBitmap.Length - 1;bitmapRowIdx >= 0;--bitmapRowIdx) {
                    var bitmapRow = scaledBitmap[bitmapRowIdx];
                    fontRow.Clear();
                    fontRow.Append(bitmapRow.Length > fontGlyphWidth
                        ? bitmapRow[..fontGlyphWidth]
                        : bitmapRow
                    );
                    while(fontRow.Length < fontGlyphWidth) {
                        fontRow.Append('.');
                    }
                    if(fontRowIdx >= 0) {
                        fontRows[fontRowIdx--] = fontRow.ToString();
                    }
                }

                if(fontRowIdx >= 0) {
                    var blankRow = new String('.', fontGlyphWidth);
                    while(fontRowIdx >= 0) {
                        fontRows[fontRowIdx--] = blankRow;
                    }
                }

                result = new McduFontGlyph() {
                    Character = character,
                    BitArray = fontRows,
                };
            }

            return result;
        }
    }
}
