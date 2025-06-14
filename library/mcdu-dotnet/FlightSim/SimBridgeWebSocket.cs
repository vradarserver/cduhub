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
using System.Text.RegularExpressions;
using McduDotNet.FlightSim.SimBridgeMcdu;

namespace McduDotNet.FlightSim
{
    /// <summary>
    /// Methods that help when trying to convert between the MCDU-DOTNET world and the world exposed by
    /// SimBridge's WebSocket server.
    /// </summary>
    public static class SimBridgeWebSocket
    {
        enum RowAlign
        {
            Left,
            Right,
            Centre,
        }

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

        private static readonly Regex _LineRegex = new Regex(
            @"(?<sub>\{[a-zA-Z0-9_]*\})|(?<txt>[^{]+|\{)",
            RegexOptions.Compiled
        );

        private static void UpdateScreen(McduContent mcdu, Screen screen)
        {
            var rowBuffer = new Row();
            if(mcdu != null && screen != null) {
                screen.Clear();
                OverlayRow(mcdu.Title, smallRow: false, rowBuffer, RowAlign.Centre, screen, 0);
                OverlayRow(mcdu.TitleLeft, smallRow: false, rowBuffer, RowAlign.Left, screen, 0);
                OverlayRow(mcdu.Page, smallRow: false, rowBuffer, RowAlign.Right, screen, 0);
                if(mcdu.LeftArrow || mcdu.RightArrow) {
                    var arrows = $"{(mcdu.LeftArrow ? "←" : "")}{(mcdu.RightArrow ? "→" : "")}";
                    OverlayRow(arrows, smallRow: false, rowBuffer, RowAlign.Right, screen, 0);
                }

                for(var lineIdx = 0;lineIdx < (mcdu.Lines?.Length ?? 0);++lineIdx) {
                    var rowNumber = lineIdx + 1;
                    var smallRow = lineIdx % 2 == 0;
                    var lines = mcdu.Lines[lineIdx];
                    if(lines?.Length > 2) {
                        OverlayRow(lines[0], smallRow, rowBuffer, RowAlign.Left, screen, rowNumber);
                        OverlayRow(lines[1], smallRow, rowBuffer, RowAlign.Right, screen, rowNumber);
                        OverlayRow(lines[2], smallRow, rowBuffer, RowAlign.Centre, screen, rowNumber);
                    }
                }

                screen.Small = false;
                var bottomLine = screen.Rows.Length - 1;
                OverlayRow(mcdu.ScratchPad, smallRow: false, rowBuffer, RowAlign.Left, screen, bottomLine);

                if(mcdu.DownArrow || mcdu.UpArrow) {
                    // Note that the remote MCDU has these in the opposite order to the simulator MCDU
                    // We're using the simulator order here - up then down.
                    var arrows = $"{(mcdu.UpArrow ? "↑" : " ")}{(mcdu.DownArrow ? "↓" : " ")}";
                    OverlayRow(arrows, smallRow: false, rowBuffer, RowAlign.Right, screen, bottomLine);
                }
            }
        }

        private static void OverlayRow(
            string simBridgeLine,
            bool smallRow,
            Row rowBuffer,
            RowAlign rowAlign,
            Screen screen,
            int rowNumber
        )
        {
            screen.Line = Math.Max(0, Math.Min(rowNumber, Metrics.Lines - 1));
            screen.CurrentRow.CopyTo(rowBuffer);
            screen.CurrentRow.Clear();

            screen.Column = 0;
            screen.Colour = Colour.White;
            screen.Small = smallRow;

            var textLength = 0;
            Colour? resetColour = null;

            if(!String.IsNullOrEmpty(simBridgeLine)) {
                foreach(Match match in _LineRegex.Matches(simBridgeLine)) {
                    var sub = match.Groups["sub"];
                    var txt = match.Groups["txt"];
                    if(txt.Success) {
                        foreach(var ch in txt.Value) {
                            Put(screen, ch, ref textLength);
                        }
                    } else if(sub.Success) {
                        switch(sub.Value.ToLowerInvariant()) {
                            case "{amber}":     SetColour(screen, Colour.Amber, ref resetColour); break;
                            case "{brown}":     SetColour(screen, Colour.Brown, ref resetColour); break; // <-- guess
                            case "{cyan}":      SetColour(screen, Colour.Cyan, ref resetColour); break;
                            case "{green}":     SetColour(screen, Colour.Green, ref resetColour); break;
                            case "{khaki}":     SetColour(screen, Colour.Khaki, ref resetColour); break; // <-- guess
                            case "{magenta}":   SetColour(screen, Colour.Magenta, ref resetColour); break;
                            case "{red}":       SetColour(screen, Colour.Red, ref resetColour); break;   // <-- guess
                            case "{yellow}":    SetColour(screen, Colour.Yellow, ref resetColour); break;// <-- guess
                            case "{white}":     SetColour(screen, Colour.White, ref resetColour); break;
                            case "{big}":       screen.Small = false; break;
                            case "{inop}":      resetColour = screen.Colour = Colour.Grey; break;
                            case "{small}":     screen.Small = true; break;
                            case "{sp}":        Put(screen, ' ', ref textLength); break;
                            case "{end}":
                                screen.Small = smallRow;
                                screen.Colour = resetColour ?? Colour.White;
                                break;
                        }
                    }
                }
            }

            AlignRow(screen, rowAlign, textLength);
            RestorePreviousContent(screen, rowBuffer);
        }

        static void Put(Screen screen, char ch, ref int textLength)
        {
            switch(ch) {
                case '{':   ch = '←'; break;
                case '}':   ch = '→'; break;
                case '_':   ch = '☐'; break;
                case '|':   ch = '/'; break;
            }
            screen.Put(ch, advanceColumn: true);
            ++textLength;
        }

        static void SetColour(Screen screen, Colour colour, ref Colour? resetColour)
        {
            if(resetColour == null) {
                resetColour = colour;
            }
            screen.Colour = colour;
        }

        static void AlignRow(Screen screen, RowAlign rowAlign, int textLength)
        {
            switch(rowAlign) {
                case RowAlign.Left:
                    break;
                case RowAlign.Centre:
                    screen.CurrentRow.ShiftRight(
                        0,
                        screen.Column,
                        (Metrics.Columns - textLength) / 2
                    );
                    break;
                case RowAlign.Right:
                    screen.CurrentRow.ShiftRight(
                        0,
                        screen.Column,
                        Metrics.Columns - textLength
                    );
                    break;
            }
        }

        static void RestorePreviousContent(Screen screen, Row rowBuffer)
        {
            for(var idx = 0;idx < rowBuffer.Cells.Length;++idx) {
                var cell = rowBuffer.Cells[idx];
                if(cell.Character != ' ') {
                    screen.CurrentRow.Cells[idx].CopyFrom(cell);
                }
            }
        }
    }
}
