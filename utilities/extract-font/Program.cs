// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.CommandLine;
using System.CommandLine.Invocation;
using Newtonsoft.Json;

namespace ExtractFont
{
    class Program
    {
        static int Main(string[] args)
        {
            var exitCode = 0;

            try {
                var worked = true;

                RootCommand rootCommand = new("Builds CDU Hub JSON font files from USB packet captures") {
                    Options.NameOption,
                    Options.PacketsFileOption,
                    Options.FontFileOption,
                    Options.MapFileOption,
                };
                rootCommand.TreatUnmatchedTokensAsErrors = true;
                rootCommand.SetAction(
                    parseResult => {
                        worked = ExtractAndSaveFont(
                            parseResult.GetValue(Options.NameOption),
                            parseResult.GetRequiredValue(Options.PacketsFileOption),
                            parseResult.GetValue(Options.FontFileOption),
                            parseResult.GetValue(Options.MapFileOption)
                        );
                    }
                );
                exitCode = rootCommand.Parse(args).Invoke();
                if(!worked) {
                    exitCode = 1;
                }
            } catch(Exception ex) {
                Console.WriteLine("Caught exception during processing:");
                Console.WriteLine(ex);
                exitCode = 2;
            }

            return exitCode;
        }

        private static bool ExtractAndSaveFont(
            string fontName,
            FileInfo packetsFileInfo,
            FileInfo fontFileInfo,
            FileInfo mapFileInfo
        )
        {
            ArgumentNullException.ThrowIfNullOrEmpty(fontName);
            ArgumentNullException.ThrowIfNull(packetsFileInfo);
            fontFileInfo ??= new(Options.SanitiseFileName(fontName));

            Console.WriteLine($"Extract Font File From USB Packets");
            Console.WriteLine($"Font name:          {fontName}");
            Console.WriteLine($"Packet dump file:   {packetsFileInfo.FullName}");
            Console.WriteLine($"Output file:        {fontFileInfo.FullName}");
            Console.WriteLine($"Map file:           {(mapFileInfo?.FullName ?? "none")}");

            var allGood = true;
            try {
                var extractor = new WinwingMcduUsbExtractor();
                var fontFile = extractor.ExtractFont(ReadByteArraysFromFile(packetsFileInfo));
                fontFile.Name = fontName;

                var fontFileJson = JsonConvert.SerializeObject(fontFile, Formatting.Indented);
                File.WriteAllText(fontFileInfo.FullName, fontFileJson);
                Console.WriteLine($"Saved {fontFileJson.Length:N0} characters to {fontFileInfo.FullName}");

                if(mapFileInfo != null) {
                    var mapFileJson = JsonConvert.SerializeObject(extractor.FontPacketMap, Formatting.Indented);
                    File.WriteAllText(mapFileInfo.FullName, mapFileJson);
                    Console.WriteLine($"Saved {mapFileJson.Length:N0} characters to {mapFileInfo.FullName}");
                }
            } catch(InvalidDataException ex) {
                allGood = false;
                Console.WriteLine($"Found error in {packetsFileInfo.FullName}:");
                Console.WriteLine(ex.Message);
            }

            return allGood;
        }

        private static IEnumerable<byte[]> ReadByteArraysFromFile(FileInfo fileInfo)
        {
            var row = new List<byte>();
            var lines = File.ReadAllLines(fileInfo.FullName);

            for(var lineIdx = 0;lineIdx < lines.Length;++lineIdx) {
                var line = lines[lineIdx];

                row.Clear();
                byte b = 0;
                var low = false;

                for(var chIdx = 0;chIdx < line.Length;++chIdx) {
                    var ch = line[chIdx];
                    switch(ch) {
                        case ' ':
                        case '\t':
                            break;
                        case '#':
                            chIdx = line.Length;
                            break;
                        default:
                            var nibble = ConvertNibble(ch);
                            b |= low
                                ? (byte)nibble
                                : (byte)(nibble << 4);
                            if(low) {
                                row.Add(b);
                                b = 0;
                            }
                            low = !low;
                            break;
                    }
                }

                if(low) {
                    throw new InvalidDataException($"Line {lineIdx + 1} is missing a hex digit");
                }

                yield return [..row];
            }
        }

        private static int ConvertNibble(char ch)
        {
            ch = char.ToLower(ch);
            return ch >= '0' && ch <= '9'
                ? ch - '0'
                : ch >= 'a' && ch <= 'f'
                    ? (ch - 'a') + 10
                    : throw new InvalidDataException($"{ch} is not a hex digit");
        }
    }
}
