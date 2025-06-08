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

namespace McduDotNet
{
    /// <summary>
    /// A set of utility methods for interacting with the Fenix Simulations A320 MCDU.
    /// </summary>
    public static class FenixA320Utility
    {
        public const string GraphGLPilotMcduDisplayName =           "aircraft.mcdu1.display";
        public const string GraphGLFirstOfficerMcduDisplayName =    "aircraft.mcdu2.display";
        public const string GraphGLSystemSwitchesPrefix =           "system.switches";

        /// <summary>
        /// Converts from an MCDU product ID to a Fenix display / CDU number.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static int ProductIdToFenixMcduNumber(ProductId productId) => productId == ProductId.Captain ? 1 : 2;

        /// <summary>
        /// Parses the value sent by Fenix's GraphQL aircraft.mcdu???.display subscription into an MCDU screen
        /// object.
        /// </summary>
        /// <param name="mcduValue"></param>
        /// <param name="screen"></param>
        public static void ParseGraphQLMcduValueToScreen(string mcduValue, Screen screen)
        {
            if(!String.IsNullOrEmpty(mcduValue)) {
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
                                    CopyFenixGraphGLLineToScreen(screen, lineElement.Value);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static void CopyFenixGraphGLLineToScreen(Screen screen, string line)
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
                    screen.Put(putch.Value);
                    screen.Column = Math.Min(screen.Column + 1, Metrics.Columns - 1);
                }
            }
            screen.Goto(screen.Line + 1, 0);
        }

        /// <summary>
        /// Builds Fenix's _CDU???_KEY_??? code for GraphGL messages from an MCDU key enum and an MCDU product
        /// ID.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mcduProduct"></param>
        /// <returns></returns>
        public static string GraphQLKeyName(Key key, ProductId mcduProduct)
        {
            var cduKey = key.ToFenixCduKeyName();
            var cduNum = ProductIdToFenixMcduNumber(mcduProduct);

            return cduKey == ""
                ? ""
                : $"S_CDU{cduNum}_KEY_{cduKey}";
        }
    }
}
