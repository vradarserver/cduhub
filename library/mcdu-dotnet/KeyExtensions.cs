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

namespace wwDevicesDotNet
{
    /// <summary>
    /// Extension methods for the <see cref="Key"/> enum.
    /// </summary>
    public static class KeyExtensions
    {
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

        public static (int Number, bool IsLeft) ToLineSelectNumber(this Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   return (1, true);
                case Key.LineSelectLeft2:   return (2, true);
                case Key.LineSelectLeft3:   return (3, true);
                case Key.LineSelectLeft4:   return (4, true);
                case Key.LineSelectLeft5:   return (5, true);
                case Key.LineSelectLeft6:   return (6, true);
                case Key.LineSelectRight1:  return (1, false);
                case Key.LineSelectRight2:  return (2, false);
                case Key.LineSelectRight3:  return (3, false);
                case Key.LineSelectRight4:  return (4, false);
                case Key.LineSelectRight5:  return (5, false);
                case Key.LineSelectRight6:  return (6, false);
                default:                    return (-1, false);
            }
        }

        public static string Describe(this Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   return "L1";
                case Key.LineSelectLeft2:   return "L2";
                case Key.LineSelectLeft3:   return "L3";
                case Key.LineSelectLeft4:   return "L4";
                case Key.LineSelectLeft5:   return "L5";
                case Key.LineSelectLeft6:   return "L6";
                case Key.LineSelectRight1:  return "R1";
                case Key.LineSelectRight2:  return "R2";
                case Key.LineSelectRight3:  return "R3";
                case Key.LineSelectRight4:  return "R4";
                case Key.LineSelectRight5:  return "R5";
                case Key.LineSelectRight6:  return "R6";
                case Key.AtcComm:           return "ATC COMM";
                case Key.DepArr:            return "DEP ARR";
                case Key.FmcComm:           return "FMC COMM";
                case Key.FPln:              return "F-PLN";
                case Key.FuelPred:          return "FUEL PRED";
                case Key.InitRef:           return "INIT REF";
                case Key.McduMenu:          return "MCDU MENU";
                case Key.NavRad:            return "NAV RAD";
                case Key.NextPage:          return "NEXT PAGE";
                case Key.PrevPage:          return "PREV PAGE";
                case Key.RadNav:            return "RAD NAV";
                case Key.SecFPln:           return "SEC F-PLN";
                case Key.LeftArrow:         return "←";
                case Key.UpArrow:           return "↑";
                case Key.DownArrow:         return "↓";
                case Key.RightArrow:        return "→";
                case Key.Digit1:            return "1";
                case Key.Digit2:            return "2";
                case Key.Digit3:            return "3";
                case Key.Digit4:            return "4";
                case Key.Digit5:            return "5";
                case Key.Digit6:            return "6";
                case Key.Digit7:            return "7";
                case Key.Digit8:            return "8";
                case Key.Digit9:            return "9";
                case Key.Digit0:            return "0";
                case Key.DecimalPoint:      return ".";
                case Key.PositiveNegative:  return "+/-";
                case Key.Slash:             return "/";
                case Key.Space:             return "SP";
                case (Key)(-1):             return "N/A";
                default:                    return key.ToString().ToUpper();
            }
        }

        public static CommonKey ToCommonKey(this Key key)
        {
            switch(key) {
                case Key.Data:
                case Key.Dir:
                case Key.FuelPred:
                case Key.Perf:
                case Key.UpArrow:
                case Key.DownArrow:
                case Key.Legs:
                case Key.Exec:
                case Key.Fix:
                case Key.Hold:
                case Key.VNav:              return CommonKey.DeviceSpecific;

                case Key.AtcComm:
                case Key.FmcComm:           return CommonKey.AtcCommOrFmcComm;
                case Key.Init:
                case Key.InitRef:           return CommonKey.InitOrInitRef;
                case Key.FPln:
                case Key.Rte:               return CommonKey.FPlnOrRte;
                case Key.RadNav:
                case Key.NavRad:            return CommonKey.RadNavOrNavRad;
                case Key.SecFPln:
                case Key.Altn:              return CommonKey.SecFPlnOrAltn;
                case Key.McduMenu:
                case Key.Menu:              return CommonKey.McduMenuOrMenu;
                case Key.Airport:
                case Key.DepArr:            return CommonKey.AirportOrDepArr;
                case Key.LeftArrow:
                case Key.PrevPage:          return CommonKey.LeftArrowOrPrevPage;
                case Key.RightArrow:
                case Key.NextPage:          return CommonKey.RightArrowOrNextPage;

                default:                    return (CommonKey)key;
            }
        }

        public static string ToFenixEfbMcduKeyName(this Key key)
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

        public static string ToSimBridgeRemoteMcduKeyName(this Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   return "L1";
                case Key.LineSelectLeft2:   return "L2";
                case Key.LineSelectLeft3:   return "L3";
                case Key.LineSelectLeft4:   return "L4";
                case Key.LineSelectLeft5:   return "L5";
                case Key.LineSelectLeft6:   return "L6";
                case Key.LineSelectRight1:  return "R1";
                case Key.LineSelectRight2:  return "R2";
                case Key.LineSelectRight3:  return "R3";
                case Key.LineSelectRight4:  return "R4";
                case Key.LineSelectRight5:  return "R5";
                case Key.LineSelectRight6:  return "R6";
                case Key.Dir:               return "DIR";
                case Key.Prog:              return "PROG";
                case Key.Perf:              return "PERF";
                case Key.Init:              return "INIT";
                case Key.Data:              return "DATA";
                case Key.Blank1:            return "";
                case Key.Brt:               return "";
                case Key.FPln:              return "FPLN";
                case Key.RadNav:            return "RAD";
                case Key.FuelPred:          return "FUEL";
                case Key.SecFPln:           return "SEC";
                case Key.AtcComm:           return "ATC";
                case Key.McduMenu:          return "MENU";
                case Key.Dim:               return "";
                case Key.Airport:           return "AIRPORT";
                case Key.Blank2:            return "";
                case Key.LeftArrow:         return "PREVPAGE";
                case Key.UpArrow:           return "UP";
                case Key.RightArrow:        return "NEXTPAGE";
                case Key.DownArrow:         return "DOWN";
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
                case Key.PositiveNegative:  return "PLUSMINUS";
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
                case Key.Slash:             return "DIV";
                case Key.Space:             return "SP";
                case Key.Ovfy:              return "OVFY";
                case Key.Clr:               return "CLR";
                default:                    return "";
            }
        }

        public static string ToXPlaneCommand(this Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   return "ls_1l";
                case Key.LineSelectLeft2:   return "ls_2l";
                case Key.LineSelectLeft3:   return "ls_3l";
                case Key.LineSelectLeft4:   return "ls_4l";
                case Key.LineSelectLeft5:   return "ls_5l";
                case Key.LineSelectLeft6:   return "ls_6l";
                case Key.LineSelectRight1:  return "ls_1r";
                case Key.LineSelectRight2:  return "ls_2r";
                case Key.LineSelectRight3:  return "ls_3r";
                case Key.LineSelectRight4:  return "ls_4r";
                case Key.LineSelectRight5:  return "ls_5r";
                case Key.LineSelectRight6:  return "ls_6r";

                case Key.Airport:           return "airport";
                case Key.Altn:              return "";
                case Key.AtcComm:           return "";          // <-- can't see anything obvious and they don't work in the XPlane-12 A330
                case Key.Blank1:            return "";
                case Key.Blank2:            return "";
                case Key.Brt:               return "";
                case Key.Clb:               return "clb";
                case Key.Crz:               return "crz";
                case Key.Data:              return "data";
                case Key.DepArr:            return "dep_arr";
                case Key.Des:               return "des";
                case Key.Dim:               return "";
                case Key.Dir:               return "dir_intc";
                case Key.Exec:              return "exec";
                case Key.Fix:               return "fix";
                case Key.FmcComm:           return "";
                case Key.FPln:              return "fpln";
                case Key.FuelPred:          return "fuel_pred";
                case Key.Hold:              return "hold";
                case Key.Init:              return "index";
                case Key.InitRef:           return "index";
                case Key.Legs:              return "legs";
                case Key.McduMenu:          return "menu";
                case Key.Menu:              return "menu";
                case Key.N1Limit:           return "";
                case Key.NavRad:            return "navrad";
                case Key.NextPage:          return "next";
                case Key.Perf:              return "perf";
                case Key.PrevPage:          return "prev";
                case Key.Prog:              return "prog";
                case Key.RadNav:            return "navrad";
                case Key.Rte:               return "fpln";
                case Key.SecFPln:           return "";          // <-- can't see anything obvious and they don't work in the XPlane-12 A330
                case Key.VNav:              return "";

                case Key.LeftArrow:         return "prev";
                case Key.UpArrow:           return "up";
                case Key.RightArrow:        return "next";
                case Key.DownArrow:         return "down";
                case Key.Digit1:            return "key_1";
                case Key.Digit2:            return "key_2";
                case Key.Digit3:            return "key_3";
                case Key.Digit4:            return "key_4";
                case Key.Digit5:            return "key_5";
                case Key.Digit6:            return "key_6";
                case Key.Digit7:            return "key_7";
                case Key.Digit8:            return "key_8";
                case Key.Digit9:            return "key_9";
                case Key.DecimalPoint:      return "key_period";
                case Key.Digit0:            return "key_0";
                case Key.PositiveNegative:  return "key_minus";
                case Key.A:                 return "key_A";
                case Key.B:                 return "key_B";
                case Key.C:                 return "key_C";
                case Key.D:                 return "key_D";
                case Key.E:                 return "key_E";
                case Key.F:                 return "key_F";
                case Key.G:                 return "key_G";
                case Key.H:                 return "key_H";
                case Key.I:                 return "key_I";
                case Key.J:                 return "key_J";
                case Key.K:                 return "key_K";
                case Key.L:                 return "key_L";
                case Key.M:                 return "key_M";
                case Key.N:                 return "key_N";
                case Key.O:                 return "key_O";
                case Key.P:                 return "key_P";
                case Key.Q:                 return "key_Q";
                case Key.R:                 return "key_R";
                case Key.S:                 return "key_S";
                case Key.T:                 return "key_T";
                case Key.U:                 return "key_U";
                case Key.V:                 return "key_V";
                case Key.W:                 return "key_W";
                case Key.X:                 return "key_X";
                case Key.Y:                 return "key_Y";
                case Key.Z:                 return "key_Z";
                case Key.Slash:             return "key_slash";
                case Key.Space:             return "key_space";
                case Key.Ovfy:              return "key_overfly";
                case Key.Clr:               return "key_clear";
                case Key.Del:               return "key_delete";
                default:                    return "";
            }
        }

        public static string ToToLissCommand(this Key key, int mcduNumber)
        {
            var suffix = "";
            switch(key) {
                case Key.LineSelectLeft1:   suffix = "LSK1L"; break;
                case Key.LineSelectLeft2:   suffix = "LSK2L"; break;
                case Key.LineSelectLeft3:   suffix = "LSK3L"; break;
                case Key.LineSelectLeft4:   suffix = "LSK4L"; break;
                case Key.LineSelectLeft5:   suffix = "LSK5L"; break;
                case Key.LineSelectLeft6:   suffix = "LSK6L"; break;
                case Key.LineSelectRight1:  suffix = "LSK1R"; break;
                case Key.LineSelectRight2:  suffix = "LSK2R"; break;
                case Key.LineSelectRight3:  suffix = "LSK3R"; break;
                case Key.LineSelectRight4:  suffix = "LSK4R"; break;
                case Key.LineSelectRight5:  suffix = "LSK5R"; break;
                case Key.LineSelectRight6:  suffix = "LSK6R"; break;
                case Key.Dir:               suffix = "DirTo"; break;
                case Key.Prog:              suffix = "Prog"; break;
                case Key.Perf:              suffix = "Perf"; break;
                case Key.Init:              suffix = "Init"; break;
                case Key.Data:              suffix = "Data"; break;
                case Key.Blank1:            break;
                case Key.Brt:               suffix = "KeyBright"; break;
                case Key.FPln:              suffix = "Fpln"; break;
                case Key.RadNav:            suffix = "RadNav"; break;
                case Key.FuelPred:          suffix = "FuelPred"; break;
                case Key.SecFPln:           suffix = "SecFpln"; break;
                case Key.AtcComm:           suffix = "ATC"; break;
                case Key.McduMenu:          suffix = "Menu"; break;
                case Key.Dim:               suffix = "KeyDim"; break;
                case Key.Airport:           suffix = "Airport"; break;
                case Key.Blank2:            break;
                case Key.LeftArrow:         suffix = "SlewLeft"; break;
                case Key.UpArrow:           suffix = "SlewUp"; break;
                case Key.RightArrow:        suffix = "SlewRight"; break;
                case Key.DownArrow:         suffix = "SlewDown"; break;
                case Key.Digit1:            suffix = "Key1"; break;
                case Key.Digit2:            suffix = "Key2"; break;
                case Key.Digit3:            suffix = "Key3"; break;
                case Key.Digit4:            suffix = "Key4"; break;
                case Key.Digit5:            suffix = "Key5"; break;
                case Key.Digit6:            suffix = "Key6"; break;
                case Key.Digit7:            suffix = "Key7"; break;
                case Key.Digit8:            suffix = "Key8"; break;
                case Key.Digit9:            suffix = "Key9"; break;
                case Key.DecimalPoint:      suffix = "KeyDecimal"; break;
                case Key.Digit0:            suffix = "Key0"; break;
                case Key.PositiveNegative:  suffix = "KeyPM"; break;
                case Key.A:                 suffix = "KeyA"; break;
                case Key.B:                 suffix = "KeyB"; break;
                case Key.C:                 suffix = "KeyC"; break;
                case Key.D:                 suffix = "KeyD"; break;
                case Key.E:                 suffix = "KeyE"; break;
                case Key.F:                 suffix = "KeyF"; break;
                case Key.G:                 suffix = "KeyG"; break;
                case Key.H:                 suffix = "KeyH"; break;
                case Key.I:                 suffix = "KeyI"; break;
                case Key.J:                 suffix = "KeyJ"; break;
                case Key.K:                 suffix = "KeyK"; break;
                case Key.L:                 suffix = "KeyL"; break;
                case Key.M:                 suffix = "KeyM"; break;
                case Key.N:                 suffix = "KeyN"; break;
                case Key.O:                 suffix = "KeyO"; break;
                case Key.P:                 suffix = "KeyP"; break;
                case Key.Q:                 suffix = "KeyQ"; break;
                case Key.R:                 suffix = "KeyR"; break;
                case Key.S:                 suffix = "KeyS"; break;
                case Key.T:                 suffix = "KeyT"; break;
                case Key.U:                 suffix = "KeyU"; break;
                case Key.V:                 suffix = "KeyV"; break;
                case Key.W:                 suffix = "KeyW"; break;
                case Key.X:                 suffix = "KeyX"; break;
                case Key.Y:                 suffix = "KeyY"; break;
                case Key.Z:                 suffix = "KeyZ"; break;
                case Key.Slash:             suffix = "KeySlash"; break;
                case Key.Space:             suffix = "KeySpace"; break;
                case Key.Ovfy:              suffix = "KeyOverfly"; break;
                case Key.Clr:               suffix = "KeyClear"; break;
                default:                    suffix = ""; break;
            }

            return suffix == ""
                ? ""
                : $"MCDU{mcduNumber}{suffix}";
        }
    }
}
