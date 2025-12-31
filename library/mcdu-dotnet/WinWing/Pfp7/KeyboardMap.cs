// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace wwDevicesDotNet.WinWing.Pfp7
{
    static class KeyboardMap
    {
        public static (int,int) InputReport01FlagAndOffset(Key key)
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
                case Key.InitRef:           return (0x10, 2);
                case Key.Rte:               return (0x20, 2);
                case Key.DepArr:            return (0x40, 2);
                case Key.Altn:              return (0x80, 2);
                case Key.VNav:              return (0x01, 3);
                case Key.Dim:               return (0x02, 3);
                case Key.Brt:               return (0x04, 3);
                case Key.Fix:               return (0x08, 3);
                case Key.Legs:              return (0x10, 3);
                case Key.Hold:              return (0x20, 3);
                case Key.FmcComm:           return (0x40, 3);
                case Key.Prog:              return (0x80, 3);
                case Key.Exec:              return (0x01, 4);
                case Key.Menu:              return (0x02, 4);
                case Key.NavRad:            return (0x04, 4);
                case Key.PrevPage:          return (0x08, 4);
                case Key.NextPage:          return (0x10, 4);
                case Key.Digit1:            return (0x20, 4);
                case Key.Digit2:            return (0x40, 4);
                case Key.Digit3:            return (0x80, 4);
                case Key.Digit4:            return (0x01, 5);
                case Key.Digit5:            return (0x02, 5);
                case Key.Digit6:            return (0x04, 5);
                case Key.Digit7:            return (0x08, 5);
                case Key.Digit8:            return (0x10, 5);
                case Key.Digit9:            return (0x20, 5);
                case Key.DecimalPoint:      return (0x40, 5);
                case Key.Digit0:            return (0x80, 5);
                case Key.PositiveNegative:  return (0x01, 6);
                case Key.A:                 return (0x02, 6);
                case Key.B:                 return (0x04, 6);
                case Key.C:                 return (0x08, 6);
                case Key.D:                 return (0x10, 6);
                case Key.E:                 return (0x20, 6);
                case Key.F:                 return (0x40, 6);
                case Key.G:                 return (0x80, 6);
                case Key.H:                 return (0x01, 7);
                case Key.I:                 return (0x02, 7);
                case Key.J:                 return (0x04, 7);
                case Key.K:                 return (0x08, 7);
                case Key.L:                 return (0x10, 7);
                case Key.M:                 return (0x20, 7);
                case Key.N:                 return (0x40, 7);
                case Key.O:                 return (0x80, 7);
                case Key.P:                 return (0x01, 8);
                case Key.Q:                 return (0x02, 8);
                case Key.R:                 return (0x04, 8);
                case Key.S:                 return (0x08, 8);
                case Key.T:                 return (0x10, 8);
                case Key.U:                 return (0x20, 8);
                case Key.V:                 return (0x40, 8);
                case Key.W:                 return (0x80, 8);
                case Key.X:                 return (0x01, 9);
                case Key.Y:                 return (0x02, 9);
                case Key.Z:                 return (0x04, 9);
                case Key.Space:             return (0x08, 9);
                case Key.Del:               return (0x10, 9);
                case Key.Slash:             return (0x20, 9);
                case Key.Clr:               return (0x40, 9);
                default:                    return (0,0);
            }
        }
    }
}
