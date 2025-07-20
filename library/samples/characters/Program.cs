using System;
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
                        mcdu.UseFont(fontFile);
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
