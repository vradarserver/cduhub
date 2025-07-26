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
using System.Runtime.Serialization;
using System.Text;

namespace McduDotNet
{
    [DataContract]
    public class McduFontPacketMap
    {
        /// <summary>
        /// The width in pixels for each glyph.
        /// </summary>
        [DataMember]
        public int GlyphWidth { get; set; }

        /// <summary>
        /// The height in pixels for each glyph.
        /// </summary>
        [DataMember]
        public int GlyphHeight { get; set; }

        /// <summary>
        /// The packets in hexstring format with mandatory substitution bytes replaced
        /// with underscores etc.
        /// </summary>
        /// <remarks>
        /// Underscore = glyph bitmap byte, XX = XOffset, YY = YOffset, HH = glyph height,
        /// WW = glyph width.
        /// </remarks>
        [DataMember]
        public string[] Packets { get; set; } = Array.Empty<string>();

        [DataMember]
        public int XOffsetOffset { get; set; } = -1;

        [DataMember]
        public int YOffsetOffset { get; set; } = -1;

        [DataMember]
        public int[] GlyphWidthOffsets { get; set; } = Array.Empty<int>();

        [DataMember]
        public int[] GlyphHeightOffsets { get; set; } = Array.Empty<int>();

        [DataMember]
        public McduFontGlyphOffsets[] LargeGlyphOffsets { get; set; } = Array.Empty<McduFontGlyphOffsets>();

        [DataMember]
        public McduFontGlyphOffsets[] SmallGlyphOffsets { get; set; } = Array.Empty<McduFontGlyphOffsets>();

        public void OverwritePacketsWithFontFileContent(
            int glyphWidth,
            int glyphHeight,
            int xOffset,
            int yOffset,
            McduFontGlyph[] largeGlyphs,
            McduFontGlyph[] smallGlyphs
        )
        {
            if(glyphHeight != GlyphHeight) {
                throw new InvalidOperationException("Glyph height mismatch - font is {glyphHeight}, map is {GlyphHeight}");
            }
            if(glyphWidth / 8 != GlyphWidth / 8) {
                throw new InvalidOperationException($"Glyph width mismatch - font bytes per row is {glyphWidth / 8}, map is {GlyphWidth / 8}");
            }

            var packetBlob = BuildPacketBlob();

            FillPacketsWithGlyphs(packetBlob, largeGlyphs, LargeGlyphOffsets, isLarge: true);
            FillPacketsWithGlyphs(packetBlob, smallGlyphs, SmallGlyphOffsets, isLarge: false);
            FillGlyphDimensions(
                packetBlob,
                glyphWidth,
                glyphHeight,
                xOffset,
                yOffset
            );

            Packets = RebuildPacketsFromBlob(packetBlob);
        }

        private byte[] BuildPacketBlob()
        {
            var blobSize = Packets
                .Select(packet => packet.Length / 2)
                .Sum();
            var result = new byte[blobSize];
            var offset = 0;
            foreach(var packet in Packets) {
                var buffer = new StringBuilder(packet);
                for(var idx = 0;idx < buffer.Length;++idx) {
                    switch(buffer[idx]) {
                        case '_':
                        case 'H':
                        case 'W':
                        case 'X':
                        case 'Y':
                            buffer[idx] = '0';
                            break;
                    }
                }
                var packetBytes = buffer
                    .ToString()
                    .ToByteArray();
                Array.Copy(
                    packetBytes, 0,
                    result, offset,
                    packetBytes.Length
                );
                offset += packetBytes.Length;
            }

            return result;
        }

        private string[] RebuildPacketsFromBlob(byte[] packetBlob)
        {
            var result = new List<string>();

            var offset = 0;
            foreach(var packet in Packets) {
                var packetLength = packet.Length / 2;
                var packetBytes = new byte[packetLength];
                Array.Copy(packetBlob, offset, packetBytes, 0, packetLength);
                offset += packetLength;

                result.Add(
                    String.Join("", packetBytes.Select(r => r.ToString("x2")))
                );
            }

            return result.ToArray();
        }

        private void FillPacketsWithGlyphs(
            byte[] packetBlob,
            McduFontGlyph[] fontGlyphs,
            McduFontGlyphOffsets[] fontGlyphOffsets,
            bool isLarge
        )
        {
            var indexedOffsets = fontGlyphOffsets
                .GroupBy(r => r.Character)
                .ToDictionary(k => k.Key, g => g.First());

            foreach(var glyph in (fontGlyphs ?? Array.Empty<McduFontGlyph>())) {
                if(indexedOffsets.TryGetValue(glyph.Character, out var glyphOffsets)) {
                    var offsets = McduFontGlyphOffsets.DecompressMap(glyphOffsets.GlyphMap);
                    var glyphBitmap = glyph.GetBytes();
                    if(offsets.Length != glyphBitmap.Length) {
                        throw new InvalidOperationException(
                            $"{(isLarge ? "Large" : "Small")} character '{glyph.Character}' " +
                            $"is {glyphBitmap.Length} bytes but {offsets.Length} have been mapped"
                        );
                    }
                    var rows = glyphBitmap.GetLength(0);
                    var cols = glyphBitmap.GetLength(1);
                    var offsetIdx = 0;
                    for(var rowIdx = 0;rowIdx < rows;++rowIdx) {
                        for(var colIdx = 0;colIdx < cols;++colIdx) {
                            var packetOffset = offsets[offsetIdx++];
                            var glyphByte = glyphBitmap[rowIdx, colIdx];
                            packetBlob[packetOffset] = glyphByte;
                        }
                    }
                }
            }
        }

        private void FillGlyphDimensions(
            byte[] packetBlob,
            int glyphWidth,
            int glyphHeight,
            int xOffset,
            int yOffset
        )
        {
            void setXY(int blobOffset, int offset)
            {
                if(blobOffset > -1) {
                    packetBlob[blobOffset] = (byte)offset;
                }
            }
            setXY(XOffsetOffset, xOffset);
            setXY(YOffsetOffset, yOffset);

            void setWidthHeight(int[] offsets, int value)
            {
                foreach(var offset in offsets) {
                    packetBlob[offset] = (byte)value;
                }
            }
            setWidthHeight(GlyphWidthOffsets, glyphWidth);
            setWidthHeight(GlyphHeightOffsets, glyphHeight);
        }
    }
}
