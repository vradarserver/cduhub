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

namespace CookedInput
{
    class Program
    {
        static void Main(string[] _)
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");

                mcdu.Screen.WriteLine("     Press buttons");
                mcdu.RefreshDisplay();

                mcdu.KeyDown += (_, args) => {
                    ShowKeyEvent(mcdu, "Dn", args);
                };
                mcdu.KeyUp += (_, args) => {
                    ShowKeyEvent(mcdu, "Up", args);
                };

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                mcdu.Screen.Clear();
                mcdu.RefreshDisplay();
            }
        }

        private static void ShowKeyEvent(IMcdu mcdu, string eventName, KeyEventArgs args)
        {
            var screen = mcdu.Screen;
            screen.ScrollRows(1);
            screen.Goto(Screen.Lines - 1);
            screen.CurrentRow.Clear();

            var oldRow = screen.Rows[Screen.Lines - 2];
            for(var idx = 0;idx < oldRow.Cells.Length;++idx) {
                var oldCell = oldRow.Cells[idx];
                oldRow.Cells[idx] = new Cell(oldCell.Character, Colour.White, small: true);
            }

            screen.Colour = Colour.Green;
            screen.Small = false;
            screen.Write($"{eventName}: {args.Key} (\"{args.Character}\")");
            mcdu.RefreshDisplay();

            Console.WriteLine($"{eventName}: {args.Key} (\"{args.Character}\")");
        }
    }
}
