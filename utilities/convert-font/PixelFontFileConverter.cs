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

namespace ConvertFont
{
    // "Pixel font files" are a proprietary text file format. It starts with a
    // set of NAME:VALUE sets, of which two names are currently defined:
    //   Size: X x Y
    //   Origin: bottom left
    // All other name-value pairs are ignored.
    // There then follows an array of glyphs interspersed with optional blank
    // lines. The glyph format for character "C" is:
    //
    // [C]
    // .XXXXX.
    // *2
    // +1
    //
    // Where C in [C] is the character being defined.
    // . denotes a 0 bit, X a 1 bit
    // *2 indicates that the previous line is to be repeated twice
    // +1 inserts one blank line
    //
    // Each glyph is padded on each axis to match the size specified in the header.
    //
    // The converter takes two font files and a linear scaling value, typically 2.
    // It emits an McduFontFile format font built from the two font files.

    public class PixelFontFileConverter
    {
        public int GlyphWidth { get; set; }

        public int GlyphHeight { get; set; }

        public int GlyphFullWidth { get; set; }

        public FileInfo LargePixelFontFileInfo { get; set; }

        public FileInfo SmallPixelFontFileInfo { get; set; }

        public int LargeScaleFactor { get; set; }

        public int SmallScaleFactor { get; set; }

        public McduFontFile Convert()
        {
            var largePixelFont = PixelFontFile.Load(LargePixelFontFileInfo);
            var smallPixelFont = PixelFontFile.Load(SmallPixelFontFileInfo);

            var result = new McduFontFile() {
                GlyphWidth = GlyphWidth,
                GlyphHeight = GlyphHeight,
                GlyphFullWidth = GlyphFullWidth,
                Name = "CONVERTED",

                LargeGlyphs = ExpandPixelFont(largePixelFont, LargeScaleFactor),
                SmallGlyphs = ExpandPixelFont(smallPixelFont, SmallScaleFactor),
            };

            return result;
        }

        private McduFontGlyph[] ExpandPixelFont(PixelFontFile pixelFont, int scaleFactor)
        {
            var result = new List<McduFontGlyph>();

            var emptyLine = new String('.', GlyphWidth);

            var buffer = new StringBuilder();
            foreach(var pixelKvp in pixelFont.Glyphs.OrderBy(kvp => kvp.Key)) {
                var mcduGlyph = new McduFontGlyph() {
                    Character = pixelKvp.Key,
                };
                var mcduBitmap = new List<string>();
                foreach(var pixelLine in pixelKvp.Value) {
                    buffer.Clear();
                    foreach(var ch in pixelLine) {
                        for(var count = 0;count < scaleFactor;++count) {
                            if(buffer.Length < GlyphWidth) {
                                buffer.Append(ch);
                            }
                        }
                    }

                    while(buffer.Length < GlyphWidth) {
                        buffer.Append('.');
                    }

                    var mcduLine = buffer.ToString();
                    for(var count = 0;count < scaleFactor;++count) {
                        if(mcduBitmap.Count < GlyphHeight) {
                            mcduBitmap.Add(mcduLine);
                        }
                    }
                }

                while(mcduBitmap.Count < GlyphHeight) {
                    mcduBitmap.Insert(0, emptyLine);
                }

                mcduGlyph.BitArray = mcduBitmap.ToArray();
                result.Add(mcduGlyph);
            }

            return result.ToArray();
        }
    }
}
