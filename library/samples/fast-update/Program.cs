using System;
using McduDotNet;

namespace FastUpdate
{
    class Program
    {
        static readonly string _Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!<>()# ";
        static readonly Colour[] _Colours = new Colour[] {
            Colour.Amber,
            Colour.Brown,
            Colour.Cyan,
            Colour.Gray,
            Colour.Green,
            Colour.Khaki,
            Colour.Magenta,
            Colour.Red,
            Colour.White,
            Colour.Yellow,
        };

        static void Main(string[] _)
        {
            var chIndex = 0;
            var colourIndex = 0;
            var small = false;

            char nextCharacter()
            {
                var result = _Characters[chIndex++];
                if(chIndex == _Characters.Length) {
                    chIndex = 0;
                }
                return result;
            }
            Colour nextColour()
            {
                var result = _Colours[colourIndex++];
                if(colourIndex == _Colours.Length) {
                    colourIndex = 0;
                }
                return result;
            }

            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");

                Console.WriteLine($"Press Q to quit");
                while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                    var now = DateTime.Now;

                    var screen = mcdu.Screen;
                    for(var rowIdx = 0;rowIdx < screen.Rows.Length;++rowIdx) {
                        var row = screen.Rows[rowIdx];
                        for(var cellIdx = 0;cellIdx < row.Cells.Length;++cellIdx) {
                            var cell = row.Cells[cellIdx];
                            cell.Character = nextCharacter();
                            cell.Colour = nextColour();
                            cell.Small = small;
                            small = !small;
                        }
                    }
                    mcdu.RefreshDisplay();
                }

                mcdu.Cleanup();
            }
        }
    }
}
