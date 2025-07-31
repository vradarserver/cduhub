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

namespace ExtractFont
{
    /// <summary>
    /// Loads a BIOS font file into memory.
    /// </summary>
    class BiosFontFileExtractor
    {
        public IReadOnlyList<BiosFontGlyph> ExtractGlyphs(BiosFontExtractionOptions options)
        {
            return ExtractGlyphs(
                options.BiosFontFileName,
                options.BiosGlyphWidth,
                options.BiosGlyphHeight,
                options.IsIbmPCFont,
                options.CharacterMap
            );
        }

        public IReadOnlyList<BiosFontGlyph> ExtractGlyphs(
            string fileName,
            int glyphWidth,
            int glyphHeight,
            bool isIbmPCFont,
            Dictionary<int, char> characterMap
        )
        {
            var result = new List<BiosFontGlyph>();

            if(File.Exists(fileName)) {
                var fileBytes = File.ReadAllBytes(fileName);
                var biosCodepoint = 0;
                var row = new StringBuilder();
                BiosFontGlyph glyph = null;
                var glyphBitmap = new List<string>();

                foreach(var fileByte in fileBytes) {
                    for(var bit = 0x80;bit > 0;bit >>= 1) {
                        var isolated = (fileByte & bit) != 0 ? 'X' : '.';
                        row.Append(isolated);
                        if(row.Length == glyphWidth) {
                            glyph ??= new BiosFontGlyph() {
                                BiosCodepoint = biosCodepoint++,
                            };
                            glyphBitmap.Add(row.ToString());
                            row.Clear();
                            if(glyphBitmap.Count == glyphHeight) {
                                glyph.Character = CalculateCharacter(glyph.BiosCodepoint, isIbmPCFont, characterMap);
                                glyph.Bitmap = [..glyphBitmap];
                                result.Add(glyph);

                                glyphBitmap.Clear();
                                glyph = null;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static char CalculateCharacter(
            int biosCodepoint,
            bool isIbmPCFont,
            Dictionary<int, char> characterMap
        )
        {
            var result = '\0';

            if(!(characterMap?.TryGetValue(biosCodepoint, out result) ?? false)) {
                if(isIbmPCFont) {
                    const string controlCharacters = " ☺☻♥♦♣♠•◘○◙♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼";
                    if(biosCodepoint < 32) {
                        result = controlCharacters[biosCodepoint];
                    } else {
                        switch(biosCodepoint) {
                            case 0x7f:  result = 'Δ'; break;
                            case 0xdb:  result = '█'; break;
                            case 0xf8:  result = '°'; break;
                            case 0xfe:  result = '■'; break;
                            default:
                                result = biosCodepoint < 0x7f
                                    ? (char)biosCodepoint
                                    : '\0';
                                break;
                        }
                    }
                }
            }

            return result;
        }
    }
}
