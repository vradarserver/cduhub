// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet.WinWing.Pfp3N
{
    static class KeyboardMap
    {
        public static (int,int) InputReport01FlagAndOffset(CduKey key)
        {
            switch(key) {
                case CduKey.LineSelectLeft1:    return (0x01, 1);
                case CduKey.LineSelectLeft2:    return (0x02, 1);
                case CduKey.LineSelectLeft3:    return (0x04, 1);
                case CduKey.LineSelectLeft4:    return (0x08, 1);
                case CduKey.LineSelectLeft5:    return (0x10, 1);
                case CduKey.LineSelectLeft6:    return (0x20, 1);
                case CduKey.LineSelectRight1:   return (0x40, 1);
                case CduKey.LineSelectRight2:   return (0x80, 1);
                case CduKey.LineSelectRight3:   return (0x01, 2);
                case CduKey.LineSelectRight4:   return (0x02, 2);
                case CduKey.LineSelectRight5:   return (0x04, 2);
                case CduKey.LineSelectRight6:   return (0x08, 2);
                case CduKey.InitRef:            return (0x10, 2);
                case CduKey.Rte:                return (0x20, 2);
                case CduKey.Clb:                return (0x40, 2);
                case CduKey.Crz:                return (0x80, 2);
                case CduKey.Des:                return (0x01, 3);
                case CduKey.Dim:                return (0x02, 3);
                case CduKey.Brt:                return (0x04, 3);
                case CduKey.Menu:               return (0x08, 3);
                case CduKey.Legs:               return (0x10, 3);
                case CduKey.DepArr:             return (0x20, 3);
                case CduKey.Hold:               return (0x40, 3);
                case CduKey.Prog:               return (0x80, 3);
                case CduKey.Exec:               return (0x01, 4);
                case CduKey.N1Limit:            return (0x02, 4);
                case CduKey.Fix:                return (0x04, 4);
                case CduKey.PrevPage:           return (0x08, 4);
                case CduKey.NextPage:           return (0x10, 4);
                case CduKey.Digit1:             return (0x20, 4);
                case CduKey.Digit2:             return (0x40, 4);
                case CduKey.Digit3:             return (0x80, 4);
                case CduKey.Digit4:             return (0x01, 5);
                case CduKey.Digit5:             return (0x02, 5);
                case CduKey.Digit6:             return (0x04, 5);
                case CduKey.Digit7:             return (0x08, 5);
                case CduKey.Digit8:             return (0x10, 5);
                case CduKey.Digit9:             return (0x20, 5);
                case CduKey.DecimalPoint:       return (0x40, 5);
                case CduKey.Digit0:             return (0x80, 5);
                case CduKey.PositiveNegative:   return (0x01, 6);
                case CduKey.A:                  return (0x02, 6);
                case CduKey.B:                  return (0x04, 6);
                case CduKey.C:                  return (0x08, 6);
                case CduKey.D:                  return (0x10, 6);
                case CduKey.E:                  return (0x20, 6);
                case CduKey.F:                  return (0x40, 6);
                case CduKey.G:                  return (0x80, 6);
                case CduKey.H:                  return (0x01, 7);
                case CduKey.I:                  return (0x02, 7);
                case CduKey.J:                  return (0x04, 7);
                case CduKey.K:                  return (0x08, 7);
                case CduKey.L:                  return (0x10, 7);
                case CduKey.M:                  return (0x20, 7);
                case CduKey.N:                  return (0x40, 7);
                case CduKey.O:                  return (0x80, 7);
                case CduKey.P:                  return (0x01, 8);
                case CduKey.Q:                  return (0x02, 8);
                case CduKey.R:                  return (0x04, 8);
                case CduKey.S:                  return (0x08, 8);
                case CduKey.T:                  return (0x10, 8);
                case CduKey.U:                  return (0x20, 8);
                case CduKey.V:                  return (0x40, 8);
                case CduKey.W:                  return (0x80, 8);
                case CduKey.X:                  return (0x01, 9);
                case CduKey.Y:                  return (0x02, 9);
                case CduKey.Z:                  return (0x04, 9);
                case CduKey.Space:              return (0x08, 9);
                case CduKey.Del:                return (0x10, 9);
                case CduKey.Slash:              return (0x20, 9);
                case CduKey.Clr:                return (0x40, 9);
                default:                        return (0,0);
            }
        }
    }
}
