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
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace wwDevicesDotNet.FlightSim
{
    /// <summary>
    /// A set of utility methods for interacting with the Fenix Simulations A320 MCDU as exposed
    /// by a GraphQL server hosted by Fenix.
    /// </summary>
    public static class FenixA320GraphQL
    {
        public const string GraphQLMcdu1DisplayName =       "aircraft.mcdu1.display";
        public const string GraphQLMcdu2DisplayName =       "aircraft.mcdu2.display";
        public const string GraphQLMcdu1LedFailName =       "system.indicators.I_CDU1_FAIL";
        public const string GraphQLMcdu1LedFmName =         "system.indicators.I_CDU1_FM";
        public const string GraphQLMcdu1LedFm1Name =        "system.indicators.I_CDU1_FM1";
        public const string GraphQLMcdu1LedFm2Name =        "system.indicators.I_CDU1_FM2";
        public const string GraphQLMcdu1LedIndName =        "system.indicators.I_CDU1_IND";
        public const string GraphQLMcdu1LedMcduMenuName =   "system.indicators.I_CDU1_MCDU_MENU";
        public const string GraphQLMcdu1LedRdyName =        "system.indicators.I_CDU1_RDY";
        public const string GraphQLMcdu2LedFailName =       "system.indicators.I_CDU2_FAIL";
        public const string GraphQLMcdu2LedFmName =         "system.indicators.I_CDU2_FM";
        public const string GraphQLMcdu2LedFm1Name =        "system.indicators.I_CDU2_FM1";
        public const string GraphQLMcdu2LedFm2Name =        "system.indicators.I_CDU2_FM2";
        public const string GraphQLMcdu2LedIndName =        "system.indicators.I_CDU2_IND";
        public const string GraphQLMcdu2LedMcduMenuName =   "system.indicators.I_CDU2_MCDU_MENU";
        public const string GraphQLMcdu2LedRdyName =        "system.indicators.I_CDU2_RDY";
        public const string GraphQLSystemSwitchesPrefix =   "system.switches";

        /// <summary>
        /// Converts from a <see cref="DeviceUser"/> to a Fenix display / CDU number.
        /// </summary>
        /// <param name="deviceUser"></param>
        /// <returns></returns>
        public static int DeviceUserToFenixMcduNumber(DeviceUser deviceUser)
        {
            return deviceUser == DeviceUser.Captain ? 1 : 2;
        }

        /// <summary>
        /// Parses the value sent by Fenix's GraphQL aircraft.mcdu???.display subscription into an MCDU screen
        /// object.
        /// </summary>
        /// <param name="mcduValue"></param>
        /// <param name="screen"></param>
        public static void ParseGraphQLMcduValueToScreen(string mcduValue, Screen screen)
        {
            if(!String.IsNullOrEmpty(mcduValue) && screen != null) {
                using(var stringReader = new StringReader(mcduValue)) {
                    var xdoc = XDocument.Load(stringReader);
                    var root = xdoc.Document.Descendants("root").FirstOrDefault();
                    if(root != null) {
                        screen.Clear();
                        foreach(var lineElement in root.Elements()) {
                            screen.Colour = Colour.White;
                            screen.Small = false;
                            switch(lineElement.Name.LocalName) {
                                case "title":
                                case "line":
                                case "scratchpad":
                                    CopyFenixGraphQLLineToScreen(screen, lineElement.Value);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static void CopyFenixGraphQLLineToScreen(Screen screen, string line)
        {
            foreach(var ch in line) {
                char? putch = null;
                switch(ch) {
                    case 'a': screen.Colour = Colour.Amber; break;
                    case 'c': screen.Colour = Colour.Cyan; break;
                    case 'g': screen.Colour = Colour.Green; break;
                    case 'k': screen.Colour = Colour.Khaki; break;    // GUESS - not seen example
                    case 'l': screen.Small = false; break;
                    case 'm': screen.Colour = Colour.Magenta; break;
                    case 'r': screen.Colour = Colour.Red; break;      // GUESS - not seen example
                    case 's': screen.Small = true; break;
                    case 'w': screen.Colour = Colour.White; break;    // and maybe grey?
                    case 'y': screen.Colour = Colour.Yellow; break;
                    case '#': putch = '☐'; break;
                    case '&': putch = 'Δ'; break;
                    case '¤': putch = '↑'; break;
                    case '¥': putch = '↓'; break;
                    case '¢': putch = '→'; break;
                    case '£': putch = '←'; break;
                    default:  putch = ch; break;
                }
                if(putch != null) {
                    screen.Put(putch.Value, advanceColumn: true);
                }
            }
            screen.Goto(screen.Line + 1, 0);
        }

        public static void ParseGraphQLIndicatorValueToLeds(string indicatorName, string indicatorValue, Leds leds)
        {
            if(indicatorValue != null && leds != null) {
                var on = indicatorValue != "0";
                switch(indicatorName) {
                    case GraphQLMcdu1LedFailName:
                    case GraphQLMcdu2LedFailName:
                        leds.Fail = on;
                        break;
                    case GraphQLMcdu1LedFmName:
                    case GraphQLMcdu2LedFmName:
                        leds.Fm = on;
                        break;
                    case GraphQLMcdu1LedFm1Name:
                    case GraphQLMcdu2LedFm1Name:
                        leds.Fm1 = on;
                        break;
                    case GraphQLMcdu1LedFm2Name:
                    case GraphQLMcdu2LedFm2Name:
                        leds.Fm2 = on;
                        break;
                    case GraphQLMcdu1LedIndName:
                    case GraphQLMcdu2LedIndName:
                        leds.Ind = on;
                        break;
                    case GraphQLMcdu1LedMcduMenuName:
                    case GraphQLMcdu2LedMcduMenuName:
                        leds.Mcdu = leds.Menu = on;
                        break;
                    case GraphQLMcdu1LedRdyName:
                    case GraphQLMcdu2LedRdyName:
                        leds.Rdy = on;
                        break;
                }
            }
        }

        /// <summary>
        /// Builds Fenix's _CDU???_KEY_??? code for GraphQL messages from an MCDU key enum and an MCDU product
        /// ID.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="deviceUser"></param>
        /// <returns></returns>
        public static string GraphQLKeyName(Key key, DeviceUser deviceUser)
        {
            var cduKey = key.ToFenixEfbMcduKeyName();
            var cduNum = DeviceUserToFenixMcduNumber(deviceUser);

            return cduKey == ""
                ? ""
                : $"S_CDU{cduNum}_KEY_{cduKey}";
        }
    }
}
