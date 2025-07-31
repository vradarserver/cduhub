// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace ExtractFont
{
    class Command_DumpFontFile : Command_Base
    {
        public static bool Run(
            FontFileType fontFileType,
            FileInfo optionsFileInfo,
            FileInfo outputFileInfo
        )
        {
            switch(fontFileType) {
                case FontFileType.BiosFont:
                    DumpBiosFont(optionsFileInfo, outputFileInfo);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        private static void DumpBiosFont(FileInfo optionsFileInfo, FileInfo outputFileInfo)
        {
            var options = LoadBiosFontExtractionOptions(optionsFileInfo);
            var extractor = new BiosFontFileExtractor();
            var glyphs = extractor.ExtractGlyphs(options);

            using(var fileStream = new StreamWriter(outputFileInfo.FullName)) {
                fileStream.WriteLine($"Filename: {options.BiosFontFileName}");
                fileStream.WriteLine($"Width:    {options.BiosGlyphWidth}");
                fileStream.WriteLine($"Height:   {options.BiosGlyphHeight}");

                foreach(var glyph in glyphs) {
                    fileStream.WriteLine();
                    fileStream.WriteLine($"[{glyph.BiosCodepoint}] = '{DescribeCharacter(glyph.Character)}'");
                    foreach(var row in glyph.Bitmap) {
                        fileStream.WriteLine(row);
                    }
                }
            }

            Console.WriteLine($"Dumped {options.BiosFontFileName} to {outputFileInfo}");
        }

        private static string DescribeCharacter(char character)
        {
            return character < 32
                ? $"(char){(int)character}"
                : new String(character, 1);
        }
    }
}
