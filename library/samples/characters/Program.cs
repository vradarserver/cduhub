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
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using McduDotNet;
using Newtonsoft.Json;

namespace Characters
{
    class Program
    {
        static int Main(string[] args)
        {
            var exitCode = 0;

            try {
                var worked = true;

                RootCommand rootCommand = new("Display characters on the CDU device") {
                    Options.FontOption,
                    Options.XOffsetOption,
                    Options.YOffsetOption,
                    Options.CorrectAspectRatioOption
                };
                rootCommand.TreatUnmatchedTokensAsErrors = true;
                rootCommand.SetAction(
                    parseResult => {
                        ShowAsciiCharacterSet(
                            parseResult.GetValue(Options.FontOption),
                            parseResult.GetValue(Options.XOffsetOption),
                            parseResult.GetValue(Options.YOffsetOption),
                            parseResult.GetValue(Options.CorrectAspectRatioOption)
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

        private static void ShowAsciiCharacterSet(
            McduFontFile fontFile,
            int xOffset,
            int yOffset,
            bool useCorrectAspectRatio
        )
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");
                Console.WriteLine($"Setting X and Y offsets to {xOffset} / {yOffset}");
                mcdu.XOffset = xOffset;
                mcdu.YOffset = yOffset;

                void uploadFont()
                {
                    if(fontFile != null) {
                        Console.WriteLine($"Uploading font{(useCorrectAspectRatio ? " with corrected aspect ratio" : "")}");
                        mcdu.UseFont(fontFile, useCorrectAspectRatio);
                    }
                }

                void showCharacters()
                {
                    mcdu.Output
                        .Clear()
                        .UseLowercaseFont()
                        .WriteLine("  0123456789ABCDEF UDLR")
                        .WriteLine(" 2<grey> !\"#$%&'()*+,-./ ↑↓←→")
                        .WriteLine("<yellow>><white>3<grey>0123456789:;<=>? ▲▼◀▶<yellow><")
                        .WriteLine(" 4<grey>@ABCDEFGHIJKLMNO ☐°Δ⬡")
                        .WriteLine("<yellow>><white>5<grey>PQRSTUVWXYZ[\\]^_ █□■ <yellow><")
                        .WriteLine(" 6<grey>`abcdefghijklmno")
                        .WriteLine("<yellow>><white>7<grey>pqrstuvwxyz{|}~      <yellow><")
                        .Newline()
                        .Small()
                        .WriteLine("<large><yellow>><white><small>2<grey> !\"#$%&'()*+,-./ ↑↓←→<large><yellow><")
                        .WriteLine(" 3<grey>0123456789:;<=>? ▲▼◀▶")
                        .WriteLine("<large><yellow>><white><small>4<grey>@ABCDEFGHIJKLMNO ☐°Δ⬡<large><yellow><")
                        .WriteLine(" 5<grey>PQRSTUVWXYZ[\\]^_ █□■")
                        .WriteLine("<large><yellow>><white><small>6<grey>`abcdefghijklmno      <large><yellow><")
                        .WriteLine(" 7<grey>pqrstuvwxyz{|}~")
                    ;
                    mcdu.RefreshDisplay();
                }

                uploadFont();
                showCharacters();
                Console.WriteLine($"Press Q to quit{(fontFile == null ? "" : " and R to reload font")}");

                var keepWaiting = true;
                while(keepWaiting) {
                    var key = !Console.KeyAvailable
                        ? default
                        : Console.ReadKey(intercept: true).Key;
                    switch(key) {
                        case ConsoleKey.Q:
                            keepWaiting = false;
                            break;
                        case ConsoleKey.R:
                            uploadFont();
                            showCharacters();
                            break;
                    }
                }

                mcdu.Cleanup();
            }
        }
    }
}
