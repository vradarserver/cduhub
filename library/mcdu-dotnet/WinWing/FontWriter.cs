﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using Newtonsoft.Json;

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Handles the sending of font glyphs to the WinWing panels.
    /// </summary>
    class FontWriter
    {
        private UsbWriter _UsbWriter;

        public FontWriter(UsbWriter usbWriter)
        {
            _UsbWriter = usbWriter;
        }

        public void SendFont(
            McduFontFile fontFileContent,
            string commandPrefix,
            bool useFullWidth,
            int currentDisplayBrightnessPercent,
            int currentDisplayXOffset,
            int currentDisplayYOffset
        )
        {
            _UsbWriter.LockForOutput(() => {
                byte[] mapBytes;
                switch(fontFileContent.GlyphHeight) {
                    case 29:    mapBytes = CduResources.WinWingFontPacketMap_3x29_json; break;
                    case 31:    mapBytes = CduResources.WinWingFontPacketMap_3x31_json; break;
                    default:    throw new NotImplementedException($"Need packet map for {fontFileContent.GlyphHeight} pixel high fonts");
                }

                var mapJson = Encoding.UTF8.GetString(mapBytes);
                var packetMap = JsonConvert.DeserializeObject<McduFontPacketMap>(mapJson);
                var glyphWidth = fontFileContent.GlyphWidth;
                if(useFullWidth && fontFileContent.GlyphFullWidth > 0) {
                    glyphWidth = fontFileContent.GlyphFullWidth;
                }
                packetMap.OverwritePacketsWithFontFileContent(
                    commandPrefix,
                    Percent.ToByte(currentDisplayBrightnessPercent),
                    glyphWidth,
                    fontFileContent.GlyphHeight,
                    0x24 + currentDisplayXOffset + XOffsetForGlyphWidth(glyphWidth),
                    0x14 + currentDisplayYOffset + YOffsetForGlyphHeight(fontFileContent.GlyphHeight),
                    fontFileContent?.LargeGlyphs,
                    fontFileContent?.SmallGlyphs
                );
                foreach(var packet in packetMap.Packets) {
                    _UsbWriter.SendStringPacket(packet);
                }
            });
        }

        private static int XOffsetForGlyphWidth(int glyphWidth)
        {
            var excess = Metrics.DisplayWidthPixels - (glyphWidth * Metrics.Columns);
            return excess / 2;
        }

        private static int YOffsetForGlyphHeight(int glyphHeight)
        {
            switch(glyphHeight) {
                case 29:    return 17;
                case 30:    return 4;
                case 31:    return 0;
                default:    throw new NotImplementedException($"Need base YOffset for {glyphHeight} glyphHeight");
            }
        }
    }
}
