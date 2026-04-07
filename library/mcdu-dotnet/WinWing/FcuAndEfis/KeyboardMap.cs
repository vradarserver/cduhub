// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// The keyboard map for FCU devices.
    /// </summary>
    static class KeyboardMap
    {
        public static (int,int) InputReport01FlagAndOffset(FcuKey key)
        {
            switch(key) {
                case FcuKey.FcuSpdMach:         return (0x01, 1);
                case FcuKey.FcuLoc:             return (0x02, 1);
                case FcuKey.FcuHdgTrkVsFpa:     return (0x04, 1);
                case FcuKey.FcuAp1:             return (0x08, 1);
                case FcuKey.FcuAp2:             return (0x10, 1);
                case FcuKey.FcuAThr:            return (0x20, 1);
                case FcuKey.FcuExped:           return (0x40, 1);
                case FcuKey.FcuMetricAlt:       return (0x80, 1);
                case FcuKey.FcuAppr:            return (0x01, 2);
                case FcuKey.FcuSpdDec:          return (0x02, 2);
                case FcuKey.FcuSpdInc:          return (0x04, 2);
                case FcuKey.FcuSpdPush:         return (0x08, 2);
                case FcuKey.FcuSpdPull:         return (0x10, 2);
                case FcuKey.FcuHdgDec:          return (0x20, 2);
                case FcuKey.FcuHdgInc:          return (0x40, 2);
                case FcuKey.FcuHdgPush:         return (0x80, 2);
                case FcuKey.FcuHdgPull:         return (0x01, 3);
                case FcuKey.FcuAltDec:          return (0x02, 3);
                case FcuKey.FcuAltInc:          return (0x04, 3);
                case FcuKey.FcuAltPush:         return (0x08, 3);
                case FcuKey.FcuAltPull:         return (0x10, 3);
                case FcuKey.FcuVsDec:           return (0x20, 3);
                case FcuKey.FcuVsInc:           return (0x40, 3);
                case FcuKey.FcuVsPush:          return (0x80, 3);
                case FcuKey.FcuVsPull:          return (0x01, 4);
                case FcuKey.FcuAlt100:          return (0x02, 4);
                case FcuKey.FcuAlt1000:         return (0x04, 4);

                case FcuKey.LeftFd:             return (0x01, 5);
                case FcuKey.LeftLs:             return (0x02, 5);
                case FcuKey.LeftCstr:           return (0x04, 5);
                case FcuKey.LeftWpt:            return (0x08, 5);
                case FcuKey.LeftVorD:           return (0x10, 5);
                case FcuKey.LeftNdb:            return (0x20, 5);
                case FcuKey.LeftArpt:           return (0x40, 5);
                case FcuKey.LeftBaroPush:       return (0x80, 5);
                case FcuKey.LeftBaroPull:       return (0x01, 6);
                case FcuKey.LeftBaroDec:        return (0x02, 6);
                case FcuKey.LeftBaroInc:        return (0x04, 6);
                case FcuKey.LeftInHg:           return (0x08, 6);
                case FcuKey.LeftHPa:            return (0x10, 6);
                case FcuKey.LeftModeLs:         return (0x20, 6);
                case FcuKey.LeftModeVor:        return (0x40, 6);
                case FcuKey.LeftModeNav:        return (0x80, 6);
                case FcuKey.LeftModeArc:        return (0x01, 7);
                case FcuKey.LeftModePlan:       return (0x02, 7);
                case FcuKey.LeftRange10:        return (0x04, 7);
                case FcuKey.LeftRange20:        return (0x08, 7);
                case FcuKey.LeftRange40:        return (0x10, 7);
                case FcuKey.LeftRange80:        return (0x20, 7);
                case FcuKey.LeftRange160:       return (0x40, 7);
                case FcuKey.LeftRange320:       return (0x80, 7);
                case FcuKey.LeftNeedle1Adf:     return (0x01, 8);
                case FcuKey.LeftNeedle1Off:     return (0x02, 8);
                case FcuKey.LeftNeedle1Vor:     return (0x04, 8);
                case FcuKey.LeftNeedle2Adf:     return (0x08, 8);
                case FcuKey.LeftNeedle2Off:     return (0x10, 8);
                case FcuKey.LeftNeedle2Vor:     return (0x20, 8);

                case FcuKey.RightFd:            return (0x01, 9);
                case FcuKey.RightLs:            return (0x02, 9);
                case FcuKey.RightCstr:          return (0x04, 9);
                case FcuKey.RightWpt:           return (0x08, 9);
                case FcuKey.RightVorD:          return (0x10, 9);
                case FcuKey.RightNdb:           return (0x20, 9);
                case FcuKey.RightArpt:          return (0x40, 9);
                case FcuKey.RightBaroPush:      return (0x80, 9);
                case FcuKey.RightBaroPull:      return (0x01, 10);
                case FcuKey.RightBaroDec:       return (0x02, 10);
                case FcuKey.RightBaroInc:       return (0x04, 10);
                case FcuKey.RightInHg:          return (0x08, 10);
                case FcuKey.RightHPa:           return (0x10, 10);
                case FcuKey.RightModeLs:        return (0x20, 10);
                case FcuKey.RightModeVor:       return (0x40, 10);
                case FcuKey.RightModeNav:       return (0x80, 10);
                case FcuKey.RightModeArc:       return (0x01, 11);
                case FcuKey.RightModePlan:      return (0x02, 11);
                case FcuKey.RightRange10:       return (0x04, 11);
                case FcuKey.RightRange20:       return (0x08, 11);
                case FcuKey.RightRange40:       return (0x10, 11);
                case FcuKey.RightRange80:       return (0x20, 11);
                case FcuKey.RightRange160:      return (0x40, 11);
                case FcuKey.RightRange320:      return (0x80, 11);
                case FcuKey.RightNeedle1Vor:    return (0x01, 12);
                case FcuKey.RightNeedle1Off:    return (0x02, 12);
                case FcuKey.RightNeedle1Adf:    return (0x04, 12);
                case FcuKey.RightNeedle2Vor:    return (0x08, 12);
                case FcuKey.RightNeedle2Off:    return (0x10, 12);
                case FcuKey.RightNeedle2Adf:    return (0x20, 12);

                default:                        return (0,0);
            }
        }
    }
}
