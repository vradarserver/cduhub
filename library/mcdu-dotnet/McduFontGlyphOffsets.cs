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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace McduDotNet
{
    [DataContract]
    public class McduFontGlyphOffsets
    {
        [DataMember]
        public char Character { get; set; }

        [DataMember]
        public int[] CodepointMap { get; set; } = Array.Empty<int>();

        [DataMember]
        public int[] GlyphMap { get; set; } = Array.Empty<int>();

        public static int[] CompressOffsetMap(int[] offsets)
        {
            var result = new List<int>();

            int? previousOffset = null;
            for(var idx = 0;idx < offsets.Length;++idx) {
                var offset = offsets[idx];
                if(offset - 1 == previousOffset) {
                    previousOffset = offset;
                    result[result.Count - 1] = offset;
                } else {
                    result.Add(offset);
                    if(result.Count >= 2) {
                        if(result[result.Count - 2] == offset - 1) {
                            result[result.Count - 1] = result[result.Count - 2];
                            result[result.Count - 2] = -1;
                            result.Add(offset);
                            previousOffset = offset;
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public static int[] DecompressMap(int[] map)
        {
            var result = new List<int>();

            for(var idx = 0;idx < map.Length;++idx) {
                var offset = map[idx];
                if(offset != -1) {
                    result.Add(offset);
                } else if(offset == -1 && idx + 2 < map.Length) {
                    var endOffset = map[idx + 2];
                    for(offset = map[idx + 1];offset <= endOffset;++offset) {
                        result.Add(offset);
                    }
                    idx += 2;
                }
            }

            return result.ToArray();
        }
    }
}
