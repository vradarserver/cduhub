// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet
{
    /// <summary>
    /// Extension methods for the <see cref="CduKey"/> enum.
    /// </summary>
    public static class CduKeyExtensions
    {
        public static string ToCharacter(this CduKey key)
        {
            var result = "";

            if((key >= CduKey.A && key <= CduKey.Z)) {
                result = key.ToString();
            } else {
                switch(key) {
                    case CduKey.Digit0:         result = "0"; break;
                    case CduKey.Digit1:         result = "1"; break;
                    case CduKey.Digit2:         result = "2"; break;
                    case CduKey.Digit3:         result = "3"; break;
                    case CduKey.Digit4:         result = "4"; break;
                    case CduKey.Digit5:         result = "5"; break;
                    case CduKey.Digit6:         result = "6"; break;
                    case CduKey.Digit7:         result = "7"; break;
                    case CduKey.Digit8:         result = "8"; break;
                    case CduKey.Digit9:         result = "9"; break;
                    case CduKey.DecimalPoint:   result = "."; break;
                    case CduKey.Slash:          result = "/"; break;
                    case CduKey.Space:          result = " "; break;
                }
            }

            return result;
        }

        public static (int Number, bool IsLeft) ToLineSelectNumber(this CduKey key)
        {
            switch(key) {
                case CduKey.LineSelectLeft1:    return (1, true);
                case CduKey.LineSelectLeft2:    return (2, true);
                case CduKey.LineSelectLeft3:    return (3, true);
                case CduKey.LineSelectLeft4:    return (4, true);
                case CduKey.LineSelectLeft5:    return (5, true);
                case CduKey.LineSelectLeft6:    return (6, true);
                case CduKey.LineSelectRight1:   return (1, false);
                case CduKey.LineSelectRight2:   return (2, false);
                case CduKey.LineSelectRight3:   return (3, false);
                case CduKey.LineSelectRight4:   return (4, false);
                case CduKey.LineSelectRight5:   return (5, false);
                case CduKey.LineSelectRight6:   return (6, false);
                default:                        return (-1, false);
            }
        }

        public static string Describe(this CduKey key)
        {
            switch(key) {
                case CduKey.LineSelectLeft1:    return "L1";
                case CduKey.LineSelectLeft2:    return "L2";
                case CduKey.LineSelectLeft3:    return "L3";
                case CduKey.LineSelectLeft4:    return "L4";
                case CduKey.LineSelectLeft5:    return "L5";
                case CduKey.LineSelectLeft6:    return "L6";
                case CduKey.LineSelectRight1:   return "R1";
                case CduKey.LineSelectRight2:   return "R2";
                case CduKey.LineSelectRight3:   return "R3";
                case CduKey.LineSelectRight4:   return "R4";
                case CduKey.LineSelectRight5:   return "R5";
                case CduKey.LineSelectRight6:   return "R6";
                case CduKey.AtcComm:            return "ATC COMM";
                case CduKey.DepArr:             return "DEP ARR";
                case CduKey.FmcComm:            return "FMC COMM";
                case CduKey.FPln:               return "F-PLN";
                case CduKey.FuelPred:           return "FUEL PRED";
                case CduKey.InitRef:            return "INIT REF";
                case CduKey.McduMenu:           return "MCDU MENU";
                case CduKey.NavRad:             return "NAV RAD";
                case CduKey.NextPage:           return "NEXT PAGE";
                case CduKey.PrevPage:           return "PREV PAGE";
                case CduKey.RadNav:             return "RAD NAV";
                case CduKey.SecFPln:            return "SEC F-PLN";
                case CduKey.LeftArrow:          return "←";
                case CduKey.UpArrow:            return "↑";
                case CduKey.DownArrow:          return "↓";
                case CduKey.RightArrow:         return "→";
                case CduKey.Digit1:             return "1";
                case CduKey.Digit2:             return "2";
                case CduKey.Digit3:             return "3";
                case CduKey.Digit4:             return "4";
                case CduKey.Digit5:             return "5";
                case CduKey.Digit6:             return "6";
                case CduKey.Digit7:             return "7";
                case CduKey.Digit8:             return "8";
                case CduKey.Digit9:             return "9";
                case CduKey.Digit0:             return "0";
                case CduKey.DecimalPoint:       return ".";
                case CduKey.PositiveNegative:   return "+/-";
                case CduKey.Slash:              return "/";
                case CduKey.Space:              return "SP";
                case (CduKey)(-1):              return "N/A";
                default:                        return key.ToString().ToUpper();
            }
        }

        public static CommonCduKey ToCommonKey(this CduKey key)
        {
            switch(key) {
                case CduKey.Data:
                case CduKey.Dir:
                case CduKey.FuelPred:
                case CduKey.Perf:
                case CduKey.UpArrow:
                case CduKey.DownArrow:
                case CduKey.Legs:
                case CduKey.Exec:
                case CduKey.Fix:
                case CduKey.Hold:
                case CduKey.VNav:           return CommonCduKey.DeviceSpecific;

                case CduKey.AtcComm:
                case CduKey.FmcComm:        return CommonCduKey.AtcCommOrFmcComm;
                case CduKey.Init:
                case CduKey.InitRef:        return CommonCduKey.InitOrInitRef;
                case CduKey.FPln:
                case CduKey.Rte:            return CommonCduKey.FPlnOrRte;
                case CduKey.RadNav:
                case CduKey.NavRad:         return CommonCduKey.RadNavOrNavRad;
                case CduKey.SecFPln:
                case CduKey.Altn:           return CommonCduKey.SecFPlnOrAltn;
                case CduKey.McduMenu:
                case CduKey.Menu:           return CommonCduKey.McduMenuOrMenu;
                case CduKey.Airport:
                case CduKey.DepArr:         return CommonCduKey.AirportOrDepArr;
                case CduKey.LeftArrow:
                case CduKey.PrevPage:       return CommonCduKey.LeftArrowOrPrevPage;
                case CduKey.RightArrow:     
                case CduKey.NextPage:       return CommonCduKey.RightArrowOrNextPage;

                default:                    return (CommonCduKey)key;
            }
        }

        public static string ToFenixEfbMcduKeyName(this CduKey key)
        {
            switch(key) {
                case CduKey.LineSelectLeft1:    return "LSK1L";
                case CduKey.LineSelectLeft2:    return "LSK2L";
                case CduKey.LineSelectLeft3:    return "LSK3L";
                case CduKey.LineSelectLeft4:    return "LSK4L";
                case CduKey.LineSelectLeft5:    return "LSK5L";
                case CduKey.LineSelectLeft6:    return "LSK6L";
                case CduKey.LineSelectRight1:   return "LSK1R";
                case CduKey.LineSelectRight2:   return "LSK2R";
                case CduKey.LineSelectRight3:   return "LSK3R";
                case CduKey.LineSelectRight4:   return "LSK4R";
                case CduKey.LineSelectRight5:   return "LSK5R";
                case CduKey.LineSelectRight6:   return "LSK6R";
                case CduKey.Dir:                return "DIR";
                case CduKey.Prog:               return "PROG";
                case CduKey.Perf:               return "PERF";
                case CduKey.Init:               return "INIT";
                case CduKey.Data:               return "DATA";
                case CduKey.Blank1:             return "";
                case CduKey.Brt:                return "BRT";
                case CduKey.FPln:               return "FPLN";
                case CduKey.RadNav:             return "RAD_NAV";
                case CduKey.FuelPred:           return "FUEL_PRED";
                case CduKey.SecFPln:            return "SEC_FPLN";
                case CduKey.AtcComm:            return "ATC_COM";
                case CduKey.McduMenu:           return "MENU";
                case CduKey.Dim:                return "DIM";
                case CduKey.Airport:            return "AIRPORT";
                case CduKey.Blank2:             return "";
                case CduKey.LeftArrow:          return "ARROW_LEFT";
                case CduKey.UpArrow:            return "ARROW_UP";
                case CduKey.RightArrow:         return "ARROW_RIGHT";
                case CduKey.DownArrow:          return "ARROW_DOWN";
                case CduKey.Digit1:             return "1";
                case CduKey.Digit2:             return "2";
                case CduKey.Digit3:             return "3";
                case CduKey.Digit4:             return "4";
                case CduKey.Digit5:             return "5";
                case CduKey.Digit6:             return "6";
                case CduKey.Digit7:             return "7";
                case CduKey.Digit8:             return "8";
                case CduKey.Digit9:             return "9";
                case CduKey.DecimalPoint:       return "DOT";
                case CduKey.Digit0:             return "0";
                case CduKey.PositiveNegative:   return "MINUS";
                case CduKey.A:                  return "A";
                case CduKey.B:                  return "B";
                case CduKey.C:                  return "C";
                case CduKey.D:                  return "D";
                case CduKey.E:                  return "E";
                case CduKey.F:                  return "F";
                case CduKey.G:                  return "G";
                case CduKey.H:                  return "H";
                case CduKey.I:                  return "I";
                case CduKey.J:                  return "J";
                case CduKey.K:                  return "K";
                case CduKey.L:                  return "L";
                case CduKey.M:                  return "M";
                case CduKey.N:                  return "N";
                case CduKey.O:                  return "O";
                case CduKey.P:                  return "P";
                case CduKey.Q:                  return "Q";
                case CduKey.R:                  return "R";
                case CduKey.S:                  return "S";
                case CduKey.T:                  return "T";
                case CduKey.U:                  return "U";
                case CduKey.V:                  return "V";
                case CduKey.W:                  return "W";
                case CduKey.X:                  return "X";
                case CduKey.Y:                  return "Y";
                case CduKey.Z:                  return "Z";
                case CduKey.Slash:              return "SLASH";
                case CduKey.Space:              return "SPACE";
                case CduKey.Ovfy:               return "OVFLY";
                case CduKey.Clr:                return "CLEAR";
                default:                        return "";
            }
        }

        public static string ToSimBridgeRemoteMcduKeyName(this CduKey key)
        {
            switch(key) {
                case CduKey.LineSelectLeft1:    return "L1";
                case CduKey.LineSelectLeft2:    return "L2";
                case CduKey.LineSelectLeft3:    return "L3";
                case CduKey.LineSelectLeft4:    return "L4";
                case CduKey.LineSelectLeft5:    return "L5";
                case CduKey.LineSelectLeft6:    return "L6";
                case CduKey.LineSelectRight1:   return "R1";
                case CduKey.LineSelectRight2:   return "R2";
                case CduKey.LineSelectRight3:   return "R3";
                case CduKey.LineSelectRight4:   return "R4";
                case CduKey.LineSelectRight5:   return "R5";
                case CduKey.LineSelectRight6:   return "R6";
                case CduKey.Dir:                return "DIR";
                case CduKey.Prog:               return "PROG";
                case CduKey.Perf:               return "PERF";
                case CduKey.Init:               return "INIT";
                case CduKey.Data:               return "DATA";
                case CduKey.Blank1:             return "";
                case CduKey.Brt:                return "";
                case CduKey.FPln:               return "FPLN";
                case CduKey.RadNav:             return "RAD";
                case CduKey.FuelPred:           return "FUEL";
                case CduKey.SecFPln:            return "SEC";
                case CduKey.AtcComm:            return "ATC";
                case CduKey.McduMenu:           return "MENU";
                case CduKey.Dim:                return "";
                case CduKey.Airport:            return "AIRPORT";
                case CduKey.Blank2:             return "";
                case CduKey.LeftArrow:          return "PREVPAGE";
                case CduKey.UpArrow:            return "UP";
                case CduKey.RightArrow:         return "NEXTPAGE";
                case CduKey.DownArrow:          return "DOWN";
                case CduKey.Digit1:             return "1";
                case CduKey.Digit2:             return "2";
                case CduKey.Digit3:             return "3";
                case CduKey.Digit4:             return "4";
                case CduKey.Digit5:             return "5";
                case CduKey.Digit6:             return "6";
                case CduKey.Digit7:             return "7";
                case CduKey.Digit8:             return "8";
                case CduKey.Digit9:             return "9";
                case CduKey.DecimalPoint:       return "DOT";
                case CduKey.Digit0:             return "0";
                case CduKey.PositiveNegative:   return "PLUSMINUS";
                case CduKey.A:                  return "A";
                case CduKey.B:                  return "B";
                case CduKey.C:                  return "C";
                case CduKey.D:                  return "D";
                case CduKey.E:                  return "E";
                case CduKey.F:                  return "F";
                case CduKey.G:                  return "G";
                case CduKey.H:                  return "H";
                case CduKey.I:                  return "I";
                case CduKey.J:                  return "J";
                case CduKey.K:                  return "K";
                case CduKey.L:                  return "L";
                case CduKey.M:                  return "M";
                case CduKey.N:                  return "N";
                case CduKey.O:                  return "O";
                case CduKey.P:                  return "P";
                case CduKey.Q:                  return "Q";
                case CduKey.R:                  return "R";
                case CduKey.S:                  return "S";
                case CduKey.T:                  return "T";
                case CduKey.U:                  return "U";
                case CduKey.V:                  return "V";
                case CduKey.W:                  return "W";
                case CduKey.X:                  return "X";
                case CduKey.Y:                  return "Y";
                case CduKey.Z:                  return "Z";
                case CduKey.Slash:              return "DIV";
                case CduKey.Space:              return "SP";
                case CduKey.Ovfy:               return "OVFY";
                case CduKey.Clr:                return "CLR";
                default:                        return "";
            }
        }

        public static string ToXPlaneCommand(this CduKey key)
        {
            switch(key) {
                case CduKey.LineSelectLeft1:    return "ls_1l";
                case CduKey.LineSelectLeft2:    return "ls_2l";
                case CduKey.LineSelectLeft3:    return "ls_3l";
                case CduKey.LineSelectLeft4:    return "ls_4l";
                case CduKey.LineSelectLeft5:    return "ls_5l";
                case CduKey.LineSelectLeft6:    return "ls_6l";
                case CduKey.LineSelectRight1:   return "ls_1r";
                case CduKey.LineSelectRight2:   return "ls_2r";
                case CduKey.LineSelectRight3:   return "ls_3r";
                case CduKey.LineSelectRight4:   return "ls_4r";
                case CduKey.LineSelectRight5:   return "ls_5r";
                case CduKey.LineSelectRight6:   return "ls_6r";

                case CduKey.Airport:            return "airport";
                case CduKey.Altn:               return "";
                case CduKey.AtcComm:            return "";          // <-- can't see anything obvious and they don't work in the XPlane-12 A330
                case CduKey.Blank1:             return "";
                case CduKey.Blank2:             return "";
                case CduKey.Brt:                return "";
                case CduKey.Clb:                return "clb";
                case CduKey.Crz:                return "crz";
                case CduKey.Data:               return "data";
                case CduKey.DepArr:             return "dep_arr";
                case CduKey.Des:                return "des";
                case CduKey.Dim:                return "";
                case CduKey.Dir:                return "dir_intc";
                case CduKey.Exec:               return "exec";
                case CduKey.Fix:                return "fix";
                case CduKey.FmcComm:            return "";
                case CduKey.FPln:               return "fpln";
                case CduKey.FuelPred:           return "fuel_pred";
                case CduKey.Hold:               return "hold";
                case CduKey.Init:               return "index";
                case CduKey.InitRef:            return "index";
                case CduKey.Legs:               return "legs";
                case CduKey.McduMenu:           return "menu";
                case CduKey.Menu:               return "menu";
                case CduKey.N1Limit:            return "";
                case CduKey.NavRad:             return "navrad";
                case CduKey.NextPage:           return "next";
                case CduKey.Perf:               return "perf";
                case CduKey.PrevPage:           return "prev";
                case CduKey.Prog:               return "prog";
                case CduKey.RadNav:             return "navrad";
                case CduKey.Rte:                return "fpln";
                case CduKey.SecFPln:            return "";          // <-- can't see anything obvious and they don't work in the XPlane-12 A330
                case CduKey.VNav:               return "";

                case CduKey.LeftArrow:          return "prev";
                case CduKey.UpArrow:            return "up";
                case CduKey.RightArrow:         return "next";
                case CduKey.DownArrow:          return "down";
                case CduKey.Digit1:             return "key_1";
                case CduKey.Digit2:             return "key_2";
                case CduKey.Digit3:             return "key_3";
                case CduKey.Digit4:             return "key_4";
                case CduKey.Digit5:             return "key_5";
                case CduKey.Digit6:             return "key_6";
                case CduKey.Digit7:             return "key_7";
                case CduKey.Digit8:             return "key_8";
                case CduKey.Digit9:             return "key_9";
                case CduKey.DecimalPoint:       return "key_period";
                case CduKey.Digit0:             return "key_0";
                case CduKey.PositiveNegative:   return "key_minus";
                case CduKey.A:                  return "key_A";
                case CduKey.B:                  return "key_B";
                case CduKey.C:                  return "key_C";
                case CduKey.D:                  return "key_D";
                case CduKey.E:                  return "key_E";
                case CduKey.F:                  return "key_F";
                case CduKey.G:                  return "key_G";
                case CduKey.H:                  return "key_H";
                case CduKey.I:                  return "key_I";
                case CduKey.J:                  return "key_J";
                case CduKey.K:                  return "key_K";
                case CduKey.L:                  return "key_L";
                case CduKey.M:                  return "key_M";
                case CduKey.N:                  return "key_N";
                case CduKey.O:                  return "key_O";
                case CduKey.P:                  return "key_P";
                case CduKey.Q:                  return "key_Q";
                case CduKey.R:                  return "key_R";
                case CduKey.S:                  return "key_S";
                case CduKey.T:                  return "key_T";
                case CduKey.U:                  return "key_U";
                case CduKey.V:                  return "key_V";
                case CduKey.W:                  return "key_W";
                case CduKey.X:                  return "key_X";
                case CduKey.Y:                  return "key_Y";
                case CduKey.Z:                  return "key_Z";
                case CduKey.Slash:              return "key_slash";
                case CduKey.Space:              return "key_space";
                case CduKey.Ovfy:               return "key_overfly";
                case CduKey.Clr:                return "key_clear";
                case CduKey.Del:                return "key_delete";
                default:                        return "";
            }
        }

        public static string ToToLissCommand(this CduKey key, int mcduNumber)
        {
            var suffix = "";
            switch(key) {
                case CduKey.LineSelectLeft1:    suffix = "LSK1L"; break;
                case CduKey.LineSelectLeft2:    suffix = "LSK2L"; break;
                case CduKey.LineSelectLeft3:    suffix = "LSK3L"; break;
                case CduKey.LineSelectLeft4:    suffix = "LSK4L"; break;
                case CduKey.LineSelectLeft5:    suffix = "LSK5L"; break;
                case CduKey.LineSelectLeft6:    suffix = "LSK6L"; break;
                case CduKey.LineSelectRight1:   suffix = "LSK1R"; break;
                case CduKey.LineSelectRight2:   suffix = "LSK2R"; break;
                case CduKey.LineSelectRight3:   suffix = "LSK3R"; break;
                case CduKey.LineSelectRight4:   suffix = "LSK4R"; break;
                case CduKey.LineSelectRight5:   suffix = "LSK5R"; break;
                case CduKey.LineSelectRight6:   suffix = "LSK6R"; break;
                case CduKey.Dir:                suffix = "DirTo"; break;
                case CduKey.Prog:               suffix = "Prog"; break;
                case CduKey.Perf:               suffix = "Perf"; break;
                case CduKey.Init:               suffix = "Init"; break;
                case CduKey.Data:               suffix = "Data"; break;
                case CduKey.Blank1:             break;
                case CduKey.Brt:                suffix = "KeyBright"; break;
                case CduKey.FPln:               suffix = "Fpln"; break;
                case CduKey.RadNav:             suffix = "RadNav"; break;
                case CduKey.FuelPred:           suffix = "FuelPred"; break;
                case CduKey.SecFPln:            suffix = "SecFpln"; break;
                case CduKey.AtcComm:            suffix = "ATC"; break;
                case CduKey.McduMenu:           suffix = "Menu"; break;
                case CduKey.Dim:                suffix = "KeyDim"; break;
                case CduKey.Airport:            suffix = "Airport"; break;
                case CduKey.Blank2:             break;
                case CduKey.LeftArrow:          suffix = "SlewLeft"; break;
                case CduKey.UpArrow:            suffix = "SlewUp"; break;
                case CduKey.RightArrow:         suffix = "SlewRight"; break;
                case CduKey.DownArrow:          suffix = "SlewDown"; break;
                case CduKey.Digit1:             suffix = "Key1"; break;
                case CduKey.Digit2:             suffix = "Key2"; break;
                case CduKey.Digit3:             suffix = "Key3"; break;
                case CduKey.Digit4:             suffix = "Key4"; break;
                case CduKey.Digit5:             suffix = "Key5"; break;
                case CduKey.Digit6:             suffix = "Key6"; break;
                case CduKey.Digit7:             suffix = "Key7"; break;
                case CduKey.Digit8:             suffix = "Key8"; break;
                case CduKey.Digit9:             suffix = "Key9"; break;
                case CduKey.DecimalPoint:       suffix = "KeyDecimal"; break;
                case CduKey.Digit0:             suffix = "Key0"; break;
                case CduKey.PositiveNegative:   suffix = "KeyPM"; break;
                case CduKey.A:                  suffix = "KeyA"; break;
                case CduKey.B:                  suffix = "KeyB"; break;
                case CduKey.C:                  suffix = "KeyC"; break;
                case CduKey.D:                  suffix = "KeyD"; break;
                case CduKey.E:                  suffix = "KeyE"; break;
                case CduKey.F:                  suffix = "KeyF"; break;
                case CduKey.G:                  suffix = "KeyG"; break;
                case CduKey.H:                  suffix = "KeyH"; break;
                case CduKey.I:                  suffix = "KeyI"; break;
                case CduKey.J:                  suffix = "KeyJ"; break;
                case CduKey.K:                  suffix = "KeyK"; break;
                case CduKey.L:                  suffix = "KeyL"; break;
                case CduKey.M:                  suffix = "KeyM"; break;
                case CduKey.N:                  suffix = "KeyN"; break;
                case CduKey.O:                  suffix = "KeyO"; break;
                case CduKey.P:                  suffix = "KeyP"; break;
                case CduKey.Q:                  suffix = "KeyQ"; break;
                case CduKey.R:                  suffix = "KeyR"; break;
                case CduKey.S:                  suffix = "KeyS"; break;
                case CduKey.T:                  suffix = "KeyT"; break;
                case CduKey.U:                  suffix = "KeyU"; break;
                case CduKey.V:                  suffix = "KeyV"; break;
                case CduKey.W:                  suffix = "KeyW"; break;
                case CduKey.X:                  suffix = "KeyX"; break;
                case CduKey.Y:                  suffix = "KeyY"; break;
                case CduKey.Z:                  suffix = "KeyZ"; break;
                case CduKey.Slash:              suffix = "KeySlash"; break;
                case CduKey.Space:              suffix = "KeySpace"; break;
                case CduKey.Ovfy:               suffix = "KeyOverfly"; break;
                case CduKey.Clr:                suffix = "KeyClear"; break;
                default:                        suffix = ""; break;
            }

            return suffix == ""
                ? ""
                : $"MCDU{mcduNumber}{suffix}";
        }
    }
}
