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
using System.Text.RegularExpressions;

namespace ConvertFont
{
    public class PixelFontFile
    {
        public int GlyphWidth { get; set; }

        public int GlyphHeight { get; set; }

        public bool PadLeft { get; set; }

        public bool PadTop { get; set; }

        public Dictionary<char, string[]> Glyphs { get; } = new Dictionary<char, string[]>();

        public static PixelFontFile Load(FileInfo fileInfo)
        {
            var lines = File.ReadAllLines(fileInfo.FullName);
            var result = new PixelFontFile();
            var glyphStartLineIdx = -1;
            var glyphCharacter = '\0';
            var glyphLines = new List<string>();

            for(var lineIdx = 0;lineIdx < lines.Length;++lineIdx) {
                var line = lines[lineIdx].Trim();
                if(line != "") {
                    var nameValueSeparatorIdx = line.IndexOf(':');
                    if(nameValueSeparatorIdx != -1) {
                        if(glyphStartLineIdx > -1) {
                            throw new PixelFontFileException($"Unexpected name-value on line {lineIdx + 1}");
                        }
                        ProcessNameValue(result, line, nameValueSeparatorIdx);
                    } else {
                        if(line.StartsWith('[') && line.EndsWith(']')) {
                            ProcessGlyph(
                                result,
                                glyphStartLineIdx,
                                glyphCharacter,
                                glyphLines
                            );
                            glyphStartLineIdx = lineIdx;
                            glyphCharacter = line[1];
                        } else {
                            if(glyphStartLineIdx > -1) {
                                glyphLines.Add(line);
                            }
                        }
                    }
                }
            }

            if(glyphStartLineIdx > -1) {
                ProcessGlyph(
                    result,
                    glyphStartLineIdx,
                    glyphCharacter,
                    glyphLines
                );
            }

            return result;
        }

        private static void ProcessGlyph(
            PixelFontFile result,
            int glyphStartLineIdx,
            char glyphCharacter,
            List<string> glyphLines
        )
        {
            if(glyphCharacter != '\0') {
                var expandedLines = new List<string>();
                var buffer = new StringBuilder();
                var emptyLine = new String('.', result.GlyphWidth);

                foreach(var line in glyphLines) {
                    buffer.Clear();

                    int getCount()
                    {
                        var countText = line[1..];
                        if(!int.TryParse(countText, out var count)) {
                            throw new PixelFontFileException($"Glyph starting line {glyphStartLineIdx + 1} has unparseable counter");
                        }
                        return count;
                    }

                    switch(line[0]) {
                        case '+':
                            var expandCount = getCount();
                            for(var idx = 0;idx < expandCount;++idx) {
                                expandedLines.Add(emptyLine);
                            }
                            break;
                        case '*':
                            if(expandedLines.Count == 0) {
                                throw new PixelFontFileException($"Glyph starting line {glyphStartLineIdx + 1} is repeating at start of glyph");
                            }
                            var repeatCount = getCount();
                            for(var idx = 0;idx < repeatCount;++idx) {
                                expandedLines.Add(expandedLines[^1]);
                            }
                            break;
                        default:
                            for(var idx = 0;idx < line.Length;++idx) {
                                var ch = line[idx];
                                switch(ch) {
                                    case '.':
                                    case 'X':
                                    case 'x':
                                        buffer.Append(ch);
                                        break;
                                }
                            }
                            if(buffer.Length > result.GlyphWidth) {
                                throw new PixelFontFileException($"Glyph starting line {glyphStartLineIdx + 1} is too wide");
                            }
                            while(buffer.Length < result.GlyphWidth) {
                                if(result.PadLeft) {
                                    buffer.Insert(0, '.');
                                } else {
                                    buffer.Append('.');
                                }
                            }
                            expandedLines.Add(buffer.ToString());
                            break;
                    }
                }

                if(expandedLines.Count > result.GlyphHeight) {
                    throw new PixelFontFileException($"Glyph starting line {glyphStartLineIdx + 1} is too tall");
                }
                while(expandedLines.Count != result.GlyphHeight) {
                    if(result.PadTop) {
                        expandedLines.Insert(0, emptyLine);
                    } else {
                        expandedLines.Add(emptyLine);
                    }
                }

                if(result.Glyphs.ContainsKey(glyphCharacter)) {
                    throw new PixelFontFileException($"Glyph starting line {glyphStartLineIdx + 1} is a second instance of character code {(int)glyphCharacter} ('{glyphCharacter}')");
                }
                result.Glyphs.Add(glyphCharacter, expandedLines.ToArray());
            }

            glyphLines.Clear();
        }

        static Regex _SizeRegex = new Regex(@"^(?<width>\d+)\s*x\s*(?<height>\d+)$");

        private static void ProcessNameValue(PixelFontFile result, string line, int nameValueSeparatorIdx)
        {
            var name = line[..nameValueSeparatorIdx].Trim();
            var value = line[(nameValueSeparatorIdx + 1)..].Trim();

            switch(name.ToLower()) {
                case "size":
                    var sizeMatch = _SizeRegex.Match(value);
                    if(!sizeMatch.Success) {
                        throw new PixelFontFileException($"The size expressed in {line} is invalid");
                    }
                    result.GlyphWidth = int.Parse(sizeMatch.Groups["width"].Value);
                    result.GlyphHeight = int.Parse(sizeMatch.Groups["height"].Value);
                    break;
                case "origin":
                    result.PadLeft = value.Contains("right");
                    result.PadTop = value.Contains("bottom");
                    break;
            }
        }
    }
}
