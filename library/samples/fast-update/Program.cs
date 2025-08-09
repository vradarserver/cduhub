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

namespace FastUpdate
{
    class Program
    {
        static readonly string _Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!<>()# ";
        static readonly Colour[] _Colours = [
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
        ];

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

            using(var cdu = CduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {cdu.DeviceId}");

                Console.WriteLine($"Press Q to quit");
                while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                    var now = DateTime.Now;

                    var screen = cdu.Screen;
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
                    cdu.RefreshDisplay();
                }

                cdu.Cleanup();
            }
        }
    }
}
