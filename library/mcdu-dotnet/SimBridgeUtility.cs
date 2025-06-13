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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using McduDotNet.SimBridgeMcdu;

namespace McduDotNet
{
    /// <summary>
    /// Methods that help when trying to convert between the MCDU-DOTNET world and the SIMBRIDGE world.
    /// </summary>
    public static class SimBridgeUtility
    {
        public static void ParseSimBridgeUpdateMcduToScreenAndLeds(
            McduContent mcdu,
            Screen screen,
            Leds leds
        )
        {
            UpdateLeds(mcdu, leds);
            UpdateScreen(mcdu, screen);
        }

        private static void UpdateLeds(McduContent mcdu, Leds leds)
        {
            if(mcdu?.Annunciators != null && leds != null) {
                leds.Brightness = mcdu.IntegralBrightness;
                leds.Line = mcdu.Annunciators.Blank;
                leds.Fail = mcdu.Annunciators.Fail;
                leds.Fm1 = mcdu.Annunciators.Fm1;
                leds.Fm2 = mcdu.Annunciators.Fm2;
                leds.Fm = mcdu.Annunciators.Fmgc;
                leds.Ind = mcdu.Annunciators.Ind;
                leds.Mcdu = leds.Menu = mcdu.Annunciators.McduMenu;
                leds.Rdy = mcdu.Annunciators.Rdy;
            }
        }

        private static Regex _LineRegex = new Regex(@"(?<sub>\{[^}]+\})|(?<txt>[^\{]+)", RegexOptions.Compiled);

        private static void UpdateScreen(McduContent mcdu, Screen screen)
        {
            if(mcdu != null && screen != null) {
                screen.Clear();
                screen.Small = false;
                PutLine(0, mcdu.Title, screen);

                for(var lineIdx = 0;lineIdx < (mcdu.Lines?.Length ?? 0);++lineIdx) {
                    screen.Small = lineIdx % 2 == 0;
                    var lines = mcdu.Lines[lineIdx];
                    if(lines?.Length > 0) {
                        for(var overlayIdx = 0;overlayIdx < lines.Length;++overlayIdx) {
                            switch(overlayIdx) {
                                case 1:
                                    screen.ForRightToLeft();
                                    break;
                                default:
                                    screen.ForLeftToRight();
                                    break;
                            }
                            PutLine(1 + lineIdx, lines[overlayIdx], screen);
                        }
                    }
                }

                screen.Small = false;
                var bottomLine = screen.Rows.Length - 1;
                PutLine(bottomLine, mcdu.ScratchPad, screen);
            }
        }

        private static void PutLine(int lineIdx, string line, Screen screen)
        {
            screen.Line = Math.Max(0, Math.Min(lineIdx, Metrics.Lines - 1));
            screen.GotoStartOfLine();
            screen.Colour = Colour.White;

            if(!String.IsNullOrEmpty(line)) {
                foreach(Match match in _LineRegex.Matches(line)) {
                    var sub = match.Groups["sub"];
                    var txt = match.Groups["txt"];
                    if(txt.Success) {
                        screen.Write(txt.Value);
                    } else if(sub.Success) {
                        switch(sub.Value.ToLowerInvariant()) {
                            case "{big}":   screen.Small = false; break;
                            case "{end}":   break;  // ??
                            case "{green}": screen.Colour = Colour.Green; break;
                            case "{inop}":  screen.Colour = Colour.Grey; break;
                            case "{small}": screen.Small = true; break;
                            case "{sp}":    screen.Put(' ', advanceColumn: true); break;
                            case "{white}": screen.Colour = Colour.White; break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
