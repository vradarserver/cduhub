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
using System.Runtime.Serialization;

namespace WwDevicesDotNet
{
    /// <summary>
    /// Holds the collections of glyphs that together describe a font for a CDU device.
    /// </summary>
    [DataContract]
    public class McduFontFile
    {
        public const string CharacterSet =
            " !\"#$%&'()*+,-./0123456789" +
            ":;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "[\\]^_`abcdefghijklmnopqrstuvwxyz" +
            "{|}~°☐←↑→↓Δ⬡◀▶█▲▼■□";

        /// <summary>
        /// The name of the font.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int GlyphWidth { get; set; }

        [DataMember]
        public int GlyphHeight { get; set; }

        [DataMember]
        public int GlyphFullWidth { get; set; }

        /// <summary>
        /// A collection of glyphs that together describe the CDU's large font.
        /// </summary>
        [DataMember]
        public McduFontGlyph[] LargeGlyphs { get; set; } = Array.Empty<McduFontGlyph>();

        /// <summary>
        /// A collection of glyphs that together describe the CDU's small font.
        /// </summary>
        [DataMember]
        public McduFontGlyph[] SmallGlyphs { get; set; } = Array.Empty<McduFontGlyph>();
    }
}
