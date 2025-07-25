// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using McduDotNet;
using Newtonsoft.Json;

namespace Characters
{
    class Program
    {
        static void Main(string[] args)
        {
            McduFontFile fontFile = null;

            void loadFont()
            {
                if(args.Length == 1) {
                    var fontFileName = args[0];
                    if(!File.Exists(fontFileName)) {
                        Console.WriteLine($"{fontFileName} does not exist");
                    } else {
                        Console.WriteLine($"Loading font file {fontFileName}");
                        var json = File.ReadAllText(fontFileName);
                        fontFile = JsonConvert.DeserializeObject<McduFontFile>(json);
                    }
                }
            }

            loadFont();
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");

                void uploadFont()
                {
                    if(fontFile != null) {
                        Console.WriteLine("Uploading font");
                        mcdu.UseFont(fontFile, useCorrectAspectRatio: false);
                    }
                }

                void showCharacters()
                {
                    mcdu.Output
                        .Clear()
                        .UseLowercaseFont()
                        .WriteLine(" 0123456789ABCDEF UDLR")
                        .WriteLine("2<grey> !\"#$%&'()*+,-./ ↑↓←→")
                        .WriteLine("3<grey>0123456789:;<=>? ▲▼◀▶")
                        .WriteLine("4<grey>@ABCDEFGHIJKLMNO ☐°Δ⬡")
                        .WriteLine("5<grey>PQRSTUVWXYZ[\\]^_ █□■")
                        .WriteLine("6<grey>`abcdefghijklmno")
                        .WriteLine("7<grey>pqrstuvwxyz{|}~")
                        .Newline()
                        .Small()
                        .WriteLine("2<grey> !\"#$%&'()*+,-./ ↑↓←→")
                        .WriteLine("3<grey>0123456789:;<=>? ▲▼◀▶")
                        .WriteLine("4<grey>@ABCDEFGHIJKLMNO ☐°Δ⬡")
                        .WriteLine("5<grey>PQRSTUVWXYZ[\\]^_ █□■")
                        .WriteLine("6<grey>`abcdefghijklmno")
                        .WriteLine("7<grey>pqrstuvwxyz{|}~")
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
                            loadFont();
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
