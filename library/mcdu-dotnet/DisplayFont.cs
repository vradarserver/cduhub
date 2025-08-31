// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace McduDotNet
{
    /// <summary>
    /// Describes a font loaded into the device.
    /// </summary>
    public class DisplayFont
    {
        public int PixelHeight { get; private set; }

        public int PixelThinWidth { get; private set; }

        public int PixelWideWidth { get; private set; }

        public int RowByteWidth { get; private set; }

        private Dictionary<char, byte[,]> _LargeGlyphs = new Dictionary<char, byte[,]>();
        public Dictionary<char, byte[,]> LargeGlyphs => _LargeGlyphs;

        private Dictionary<char, byte[,]> _SmallGlyphs = new Dictionary<char, byte[,]>();
        public Dictionary<char, byte[,]> SmallGlyphs => _SmallGlyphs;

        public bool CopyFrom(McduFontFile fontFile)
        {
            if(fontFile == null) {
                throw new ArgumentNullException(nameof(fontFile));
            }
            var wideWidth = Math.Max(fontFile.GlyphWidth, fontFile.GlyphFullWidth);
            var result = fontFile.GlyphWidth != PixelThinWidth
                      || wideWidth != PixelWideWidth
                      || fontFile.GlyphHeight != fontFile.GlyphHeight;

            int calcRowBytes(int bits) => (bits / 8) + (bits % 8 != 0 ? 1 : 0);

            if(calcRowBytes(fontFile.GlyphWidth) != calcRowBytes(wideWidth)) {
                throw new ArgumentOutOfRangeException(nameof(fontFile));
            }

            PixelThinWidth = fontFile.GlyphWidth;
            PixelWideWidth = wideWidth;
            PixelHeight = fontFile.GlyphHeight;
            RowByteWidth = calcRowBytes(fontFile.GlyphWidth);

            result = CopyGlyphs(fontFile.LargeGlyphs, ref _LargeGlyphs) || result;
            result = CopyGlyphs(fontFile.SmallGlyphs, ref _SmallGlyphs) || result;

            return result;
        }

        private bool CopyGlyphs(McduFontGlyph[] fileGlyphs, ref Dictionary<char, byte[,]> deviceGlyphs)
        {
            var result = false;

            if(deviceGlyphs == null) {
                result = true;
                deviceGlyphs = new Dictionary<char, byte[,]>();
            }
            var unwantedGlyphs = new HashSet<char>(deviceGlyphs?.Select(kvp => kvp.Key));

            foreach(var glyph in fileGlyphs ?? Array.Empty<McduFontGlyph>()) {
                if(!deviceGlyphs.TryGetValue(glyph.Character, out var deviceByteArray)) {
                    result = true;
                }
                var fileByteArray = glyph.GetBytes();
                if(   deviceByteArray?.GetLength(0) != fileByteArray.GetLength(0)
                   || deviceByteArray?.GetLength(1) != fileByteArray.GetLength(1)
                ) {
                    result = true;
                } else {
                    if(result == false) {
                        for(var rowIdx = 0;rowIdx < fileByteArray.GetLength(0);++rowIdx) {
                            for(var colIdx = 0;colIdx < fileByteArray.GetLength(1);++colIdx) {
                                if(fileByteArray[rowIdx, colIdx] != deviceByteArray[rowIdx, colIdx]) {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                deviceGlyphs[glyph.Character] = fileByteArray;
                unwantedGlyphs.Remove(glyph.Character);
            }

            foreach(var unwantedGlyph in unwantedGlyphs) {
                result = true;
                deviceGlyphs.Remove(unwantedGlyph);
            }

            return result;
        }
    }
}
