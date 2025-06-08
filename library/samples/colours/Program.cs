// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using McduDotNet;

namespace Colours
{
    class Program
    {
        static Colour[] _Colours = [
            Colour.Gray,
            Colour.White,
            Colour.Amber,
            Colour.Red,
            Colour.Khaki,
            Colour.Green,
            Colour.Cyan,
            Colour.Magenta,
            Colour.Brown,
            Colour.Yellow,
        ];
        static int _StartColourOffset;
        static bool _FirstSetIsSmall;

        static void Main(string[] _)
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");
                mcdu.KeyDown += Mcdu_KeyDown;

                DrawScreen(mcdu);

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                mcdu.Cleanup();
            }
        }

        static void DrawScreen(IMcdu mcdu)
        {
            mcdu.Screen.Clear();

            ShowColourPairs(mcdu.Output, _Colours, _StartColourOffset, smallFont: _FirstSetIsSmall);

            mcdu.Output
                .Line(6)
                .White()
                .CentreFor("←↑→↓ and DIR")
                .Large().Write("←↑→↓")
                .Small().Write(" and ")
                .Large().Write("DIR")
                .Line(-5);

            ShowColourPairs(mcdu.Output, _Colours, _StartColourOffset, smallFont: !_FirstSetIsSmall);

            mcdu.RefreshDisplay();
        }

        static void ShowColourPairs(Compositor output, Colour[] colours, int startOffset, bool smallFont)
        {
            var offset = startOffset;

            Colour nextColour()
            {
                var result = colours[offset];
                if(++offset == colours.Length) {
                    offset = 0;
                }
                return result;
            }

            for(var pairIdx = 0;pairIdx < colours.Length / 2;++pairIdx) {
                var left = nextColour();
                var right = nextColour();
                ShowColourPair(output, left, right, smallFont);
            }
        }

        static void ShowColourPair(Compositor output, Colour left, Colour right, bool smallFont)
        {
            output
                .Small(smallFont)
                .LeftToRight().Colour(left).Write(left)
                .RightToLeft().Color(right).Write(right)
                .LeftToRight().Newline();
        }

        static void Mcdu_KeyDown(object sender, KeyEventArgs args)
        {
            var mcdu = (IMcdu)sender;

            void scrollBackwards(int offset)
            {
                _StartColourOffset -= offset;
                if(_StartColourOffset < 0) {
                    _StartColourOffset += _Colours.Length;
                }
            }
            void scrollForwards(int offset)
            {
                _StartColourOffset += offset;
                if(_StartColourOffset >= _Colours.Length) {
                    _StartColourOffset -= _Colours.Length;
                }
            }

            var redrawScreen = true;
            switch(args.Key) {
                case Key.DownArrow:     scrollBackwards(2); break;
                case Key.UpArrow:       scrollForwards(2); break;
                case Key.RightArrow:    scrollBackwards(1); break;
                case Key.LeftArrow:     scrollForwards(1); break;
                case Key.Dir:           _FirstSetIsSmall = !_FirstSetIsSmall; break;
                default:                redrawScreen = false; break;
            }

            if(redrawScreen) {
                DrawScreen(mcdu);
            }
        }
    }
}
