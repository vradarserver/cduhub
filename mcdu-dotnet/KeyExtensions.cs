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

namespace McduDotNet
{
    /// <summary>
    /// Extension methods for the <see cref="Key"/> enum.
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        /// Returns the bitflag representing the key and the offset within an input 01 report of the key flag byte
        /// (assuming that offset 0 is the report code).
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static (int,int) InputReport01FlagAndOffset(this Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   return (0x01, 1);
                case Key.LineSelectLeft2:   return (0x02, 1);
                case Key.LineSelectLeft3:   return (0x04, 1);
                case Key.LineSelectLeft4:   return (0x08, 1);
                case Key.LineSelectLeft5:   return (0x10, 1);
                case Key.LineSelectLeft6:   return (0x20, 1);
                case Key.LineSelectRight1:  return (0x40, 1);
                case Key.LineSelectRight2:  return (0x80, 1);
                case Key.LineSelectRight3:  return (0x01, 2);
                case Key.LineSelectRight4:  return (0x02, 2);
                case Key.LineSelectRight5:  return (0x04, 2);
                case Key.LineSelectRight6:  return (0x08, 2);
                case Key.Dir:               return (0x10, 2);
                case Key.Prog:              return (0x20, 2);
                case Key.Perf:              return (0x40, 2);
                case Key.Init:              return (0x80, 2);
                case Key.Data:              return (0x01, 3);
                case Key.Blank1:            return (0x02, 3);
                case Key.Brt:               return (0x04, 3);
                case Key.FPln:              return (0x08, 3);
                case Key.RadNav:            return (0x10, 3);
                case Key.FuelPred:          return (0x20, 3);
                case Key.SecFPln:           return (0x40, 3);
                case Key.AtcComm:           return (0x80, 3);
                case Key.McduMenu:          return (0x01, 4);
                case Key.Dim:               return (0x02, 4);
                case Key.Airport:           return (0x04, 4);
                case Key.Blank2:            return (0x08, 4);
                case Key.LeftArrow:         return (0x10, 4);
                case Key.UpArrow:           return (0x20, 4);
                case Key.RightArrow:        return (0x40, 4);
                case Key.DownArrow:         return (0x80, 4);
                case Key.Digit1:            return (0x01, 5);
                case Key.Digit2:            return (0x02, 5);
                case Key.Digit3:            return (0x04, 5);
                case Key.Digit4:            return (0x08, 5);
                case Key.Digit5:            return (0x10, 5);
                case Key.Digit6:            return (0x20, 5);
                case Key.Digit7:            return (0x40, 5);
                case Key.Digit8:            return (0x80, 5);
                case Key.Digit9:            return (0x01, 6);
                case Key.DecimalPoint:      return (0x02, 6);
                case Key.Digit0:            return (0x04, 6);
                case Key.PositiveNegative:  return (0x08, 6);
                case Key.A:                 return (0x10, 6);
                case Key.B:                 return (0x20, 6);
                case Key.C:                 return (0x40, 6);
                case Key.D:                 return (0x80, 6);
                case Key.E:                 return (0x01, 7);
                case Key.F:                 return (0x02, 7);
                case Key.G:                 return (0x04, 7);
                case Key.H:                 return (0x08, 7);
                case Key.I:                 return (0x10, 7);
                case Key.J:                 return (0x20, 7);
                case Key.K:                 return (0x40, 7);
                case Key.L:                 return (0x80, 7);
                case Key.M:                 return (0x01, 8);
                case Key.N:                 return (0x02, 8);
                case Key.O:                 return (0x04, 8);
                case Key.P:                 return (0x08, 8);
                case Key.Q:                 return (0x10, 8);
                case Key.R:                 return (0x20, 8);
                case Key.S:                 return (0x40, 8);
                case Key.T:                 return (0x80, 8);
                case Key.U:                 return (0x01, 9);
                case Key.V:                 return (0x02, 9);
                case Key.W:                 return (0x04, 9);
                case Key.X:                 return (0x08, 9);
                case Key.Y:                 return (0x10, 9);
                case Key.Z:                 return (0x20, 9);
                case Key.Slash:             return (0x40, 9);
                case Key.Space:             return (0x80, 9);
                case Key.Ovfy:              return (0x01, 10);
                case Key.Clr:               return (0x02, 10);
                default:                    throw new NotImplementedException();
            }
        }

        public static string ToCharacter(this Key key)
        {
            var result = "";

            if((key >= Key.A && key <= Key.Z)) {
                result = key.ToString();
            } else {
                switch(key) {
                    case Key.Digit0:        result = "0"; break;
                    case Key.Digit1:        result = "1"; break;
                    case Key.Digit2:        result = "2"; break;
                    case Key.Digit3:        result = "3"; break;
                    case Key.Digit4:        result = "4"; break;
                    case Key.Digit5:        result = "5"; break;
                    case Key.Digit6:        result = "6"; break;
                    case Key.Digit7:        result = "7"; break;
                    case Key.Digit8:        result = "8"; break;
                    case Key.Digit9:        result = "9"; break;
                    case Key.DecimalPoint:  result = "."; break;
                    case Key.Slash:         result = "/"; break;
                    case Key.Space:         result = " "; break;
                }
            }

            return result;
        }

        public static string ToFenixCduKeyName(this Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   return "LSK1L";
                case Key.LineSelectLeft2:   return "LSK2L";
                case Key.LineSelectLeft3:   return "LSK3L";
                case Key.LineSelectLeft4:   return "LSK4L";
                case Key.LineSelectLeft5:   return "LSK5L";
                case Key.LineSelectLeft6:   return "LSK6L";
                case Key.LineSelectRight1:  return "LSK1R";
                case Key.LineSelectRight2:  return "LSK2R";
                case Key.LineSelectRight3:  return "LSK3R";
                case Key.LineSelectRight4:  return "LSK4R";
                case Key.LineSelectRight5:  return "LSK5R";
                case Key.LineSelectRight6:  return "LSK6R";
                case Key.Dir:               return "DIR";
                case Key.Prog:              return "PROG";
                case Key.Perf:              return "PERF";
                case Key.Init:              return "INIT";
                case Key.Data:              return "DATA";
                case Key.Blank1:            return "";
                case Key.Brt:               return "BRT";
                case Key.FPln:              return "FPLN";
                case Key.RadNav:            return "RAD_NAV";
                case Key.FuelPred:          return "FUEL_PRED";
                case Key.SecFPln:           return "SEC_FPLN";
                case Key.AtcComm:           return "ATC_COM";
                case Key.McduMenu:          return "MENU";
                case Key.Dim:               return "DIM";
                case Key.Airport:           return "AIRPORT";
                case Key.Blank2:            return "";
                case Key.LeftArrow:         return "ARROW_LEFT";
                case Key.UpArrow:           return "ARROW_UP";
                case Key.RightArrow:        return "ARROW_RIGHT";
                case Key.DownArrow:         return "ARROW_DOWN";
                case Key.Digit1:            return "1";
                case Key.Digit2:            return "2";
                case Key.Digit3:            return "3";
                case Key.Digit4:            return "4";
                case Key.Digit5:            return "5";
                case Key.Digit6:            return "6";
                case Key.Digit7:            return "7";
                case Key.Digit8:            return "8";
                case Key.Digit9:            return "9";
                case Key.DecimalPoint:      return "DOT";
                case Key.Digit0:            return "0";
                case Key.PositiveNegative:  return "MINUS";
                case Key.A:                 return "A";
                case Key.B:                 return "B";
                case Key.C:                 return "C";
                case Key.D:                 return "D";
                case Key.E:                 return "E";
                case Key.F:                 return "F";
                case Key.G:                 return "G";
                case Key.H:                 return "H";
                case Key.I:                 return "I";
                case Key.J:                 return "J";
                case Key.K:                 return "K";
                case Key.L:                 return "L";
                case Key.M:                 return "M";
                case Key.N:                 return "N";
                case Key.O:                 return "O";
                case Key.P:                 return "P";
                case Key.Q:                 return "Q";
                case Key.R:                 return "R";
                case Key.S:                 return "S";
                case Key.T:                 return "T";
                case Key.U:                 return "U";
                case Key.V:                 return "V";
                case Key.W:                 return "W";
                case Key.X:                 return "X";
                case Key.Y:                 return "Y";
                case Key.Z:                 return "Z";
                case Key.Slash:             return "SLASH";
                case Key.Space:             return "SPACE";
                case Key.Ovfy:              return "OVFLY";
                case Key.Clr:               return "CLEAR";
                default:                    return "";
            }
        }
    }
}
