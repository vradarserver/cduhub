﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.Serialization;

namespace ExtractFont
{
    [DataContract]
    class BiosFontExtractionOptions
    {
        [DataMember]
        public string McduFontName { get; set; }

        [DataMember]
        public string BiosFontFileName { get; set; }

        [DataMember]
        public int BiosGlyphWidth { get; set; } = 8;

        [DataMember]
        public int BiosGlyphHeight { get; set; } = 14;

        [DataMember]
        public int McduGlyphWidth { get; set; } = 21;

        [DataMember]
        public int McduGlyphHeight { get; set; } = 31;

        [DataMember]
        public int McduGlyphFullWidth { get; set; } = 23;

        [DataMember]
        public ScaleOptions LargeFontScale { get; set; } = new ScaleOptions(2, 1);

        [DataMember]
        public ScaleOptions SmallFontScale { get; set; } = new ScaleOptions(3, 2);

        [DataMember]
        public string Characters { get; set; }

        [DataMember]
        public bool IsIbmPCFont { get; set; } = true;

        [DataMember]
        public Dictionary<int, char> CharacterMap { get; set; } = [];
    }
}
