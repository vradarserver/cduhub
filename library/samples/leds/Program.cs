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

namespace Leds
{
    class Program
    {
        static void Main(string[] _)
        {
            using(var cdu = CduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {cdu.DeviceId}");

                cdu.Output
                    .Small()
                    .Grey()
                    .LeftLabel(1, ">FAIL")
                    .LeftLabel(2, ">FM")
                    .LeftLabel(3, ">FM1")
                    .LeftLabel(4, ">FM2")
                    .LeftLabel(5, ">IND")
                    .LeftLabel(6, ">LINE")
                    .RightLabel(1, "MCDU<")
                    .RightLabel(2, "MENU<")
                    .RightLabel(3, "RDY<")
                    .RightLabel(5, "BRIGHT -5%<")
                    .RightLabel(6, "BRIGHT +5%<");
                cdu.RefreshDisplay();

                cdu.KeyDown += (_, args) => {
                    var refreshLeds = true;
                    switch(args.Key) {
                        case Key.LineSelectLeft1:   cdu.Leds.Fail = !cdu.Leds.Fail; break;
                        case Key.LineSelectLeft2:   cdu.Leds.Fm = !cdu.Leds.Fm; break;
                        case Key.LineSelectLeft3:   cdu.Leds.Fm1 = !cdu.Leds.Fm1; break;
                        case Key.LineSelectLeft4:   cdu.Leds.Fm2 = !cdu.Leds.Fm2; break;
                        case Key.LineSelectLeft5:   cdu.Leds.Ind = !cdu.Leds.Ind; break;
                        case Key.LineSelectLeft6:   cdu.Leds.Line = !cdu.Leds.Line; break;
                        case Key.LineSelectRight1:  cdu.Leds.Mcdu = !cdu.Leds.Mcdu; break;
                        case Key.LineSelectRight2:  cdu.Leds.Menu = !cdu.Leds.Menu; break;
                        case Key.LineSelectRight3:  cdu.Leds.Rdy = !cdu.Leds.Rdy; break;
                        case Key.LineSelectRight5:  cdu.LedBrightnessPercent = Math.Max(0, cdu.LedBrightnessPercent - 5); break;
                        case Key.LineSelectRight6:  cdu.LedBrightnessPercent = Math.Min(100, cdu.LedBrightnessPercent + 5); break;
                        default:                    refreshLeds = false; break;
                    }

                    if(refreshLeds) {
                        cdu.RefreshLeds();
                    }
                };

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                cdu.Cleanup();
            }
        }
    }
}
