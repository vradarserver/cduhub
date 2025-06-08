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
        /// Parses the value sent by Fenix's GraphQL aircraft.mcduN.display subscription.
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
                                    var line = lineElement.Value;
                                    foreach(var ch in line) {
                                        char? putch = null;
                                        switch(ch) {
                                            case 'a':   screen.Colour = Colour.Amber; break;
                                            case 'c':   screen.Colour = Colour.Cyan; break;
                                            case 'g':   screen.Colour = Colour.Green; break;
                                            case 'k':   screen.Colour = Colour.Khaki; break;    // GUESS - not seen example
                                            case 'l':   screen.Small = false; break;
                                            case 'm':   screen.Colour = Colour.Magenta; break;
                                            case 'r':   screen.Colour = Colour.Red; break;      // GUESS - not seen example
                                            case 's':   screen.Small = true; break;
                                            case 'w':   screen.Colour = Colour.White; break;    // and maybe grey?
                                            case 'y':   screen.Colour = Colour.Yellow; break;
                                            case '#':   putch = '☐'; break;
                                            case '&':   putch = 'Δ'; break;
                                            case '¤':   putch = '↑'; break;
                                            case '¥':   putch = '↓'; break;
                                            case '¢':   putch = '→'; break;
                                            case '£':   putch = '←'; break;
                                            default:    putch = ch; break;
                                        }
                                        if(putch != null) {
                                            screen.Put(putch.Value);
                                            screen.Column = Math.Min(screen.Column + 1, Metrics.Columns - 1);
                                        }
                                    }
                                    screen.Goto(screen.Line + 1, 0);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public static string GraphQLKeyName(Key key, ProductId mcduProduct)
        {
            var cduKey = key.ToFenixCduKeyName();
            var cduNum = mcduProduct == ProductId.Captain
                ? 1
                : 2;

            return cduKey == ""
                ? ""
                : $"S_CDU{cduNum}_KEY_{cduKey}";
        }
    }
}
