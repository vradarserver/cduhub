using System;
using McduDotNet;

namespace Characters
{
    class Program
    {
        static void Main(string[] _)
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");
                Console.WriteLine($"Press Q to quit");
                mcdu.Output
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
