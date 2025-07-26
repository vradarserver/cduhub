// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Drawing;
using System.Runtime.Serialization;

namespace ConvertFont
{
    [DataContract]
    class FontConversionOptions
    {
        [DataMember(Name = "font")]
        public string FontFamily { get; set; }

        [DataMember(Name = "point")]
        public float PointSize { get; set; }

        [DataMember(Name = "style")]
        public FontStyle Style { get; set; } = FontStyle.Regular;

        [DataMember(Name = "drawX")]
        public int DrawX { get; set; }

        [DataMember(Name = "drawY")]
        public int DrawY { get; set; }

        [DataMember(Name = "brightness")]
        public float BrightnessThreshold { get; set; } = 1F;
    }

    [DataContract]
    class ConvertOptions
    {
        [DataMember(Name = "characters")]
        public string Characters { get; set; }

        [DataMember(Name = "glyphWidth")]
        public int GlyphWidth { get; set; } = 21;

        [DataMember(Name = "glyphHeight")]
        public int GlyphHeight { get; set; } = 31;

        [DataMember(Name = "glyphFullWidth")]
        public int GlyphFullWidth { get; set; } = 23;

        [DataMember(Name = "large")]
        public FontConversionOptions Large { get; set; } = new FontConversionOptions() {
            PointSize = 30,
        };

        [DataMember(Name = "small")]
        public FontConversionOptions Small { get; set; } = new FontConversionOptions() {
            PointSize = 20,
        };
    }
}
