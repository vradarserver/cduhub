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
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");

                if(fontFile != null) {
                    Console.WriteLine("Uploading font");
                    mcdu.UseFont(fontFile);
                }

                Console.WriteLine($"Press Q to quit");
                mcdu.Output
                    .UseLowercaseFont()
                    .WriteLine(" 0123456789ABCDEF UDLR")
                    .WriteLine("2<grey> !\"#$%&'()*+,-./ ↑↓←→")
                    .WriteLine("3<grey>0123456789:;<=>? ▲▼◀▶")
                    .WriteLine("4<grey>@ABCDEFGHIJKLMNO ☐°Δ⬡")
                    .WriteLine("5<grey>PQRSTUVWXYZ[\\]^_ █■□")
                    .WriteLine("6<grey>`abcdefghijklmno")
                    .WriteLine("7<grey>pqrstuvwxyz{|}~")
                    .Newline()
                    .Small()
                    .WriteLine("2<grey> !\"#$%&'()*+,-./ ↑↓←→")
                    .WriteLine("3<grey>0123456789:;<=>? ▲▼◀▶")
                    .WriteLine("4<grey>@ABCDEFGHIJKLMNO ☐°Δ⬡")
                    .WriteLine("5<grey>PQRSTUVWXYZ[\\]^_ █■□")
                    .WriteLine("6<grey>`abcdefghijklmno")
                    .WriteLine("7<grey>pqrstuvwxyz{|}~")
                ;
                mcdu.RefreshDisplay();

                while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                }

                mcdu.Cleanup();
            }
        }
    }
}
