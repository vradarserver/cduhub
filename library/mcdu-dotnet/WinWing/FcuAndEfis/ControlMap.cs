// Copyright © 2025 onwards, Andrew Whewell, Laurent andre
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace WwDevicesDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// Maps FCU and EFIS controls to their input report byte offsets and bit flags.
    /// </summary>
    static class ControlMap
    {
        /// <summary>
        /// Returns the flag (bit mask) and offset (byte position) in the input report for a given control.
        /// </summary>
        /// <param name="control">The control to look up.</param>
        /// <returns>A tuple containing the flag (bit mask) and offset (byte position).</returns>
        public static (int Flag, int Offset) InputReport01FlagAndOffset(Control control)
        {
            switch(control) {
                // FCU controls - offset 1
                case Control.FcuSpdMach:        return (0x01, 1);
                case Control.FcuLoc:            return (0x02, 1);
                case Control.FcuHdgTrkVsFpa:    return (0x04, 1);
                case Control.FcuAp1:            return (0x08, 1);
                case Control.FcuAp2:            return (0x10, 1);
                case Control.FcuAThr:           return (0x20, 1);
                case Control.FcuExped:          return (0x40, 1);
                case Control.FcuMetricAlt:      return (0x80, 1);

                // FCU controls - offset 2
                case Control.FcuAppr:           return (0x01, 2);
                case Control.FcuSpdDec:         return (0x02, 2);
                case Control.FcuSpdInc:         return (0x04, 2);
                case Control.FcuSpdPush:        return (0x08, 2);
                case Control.FcuSpdPull:        return (0x10, 2);
                case Control.FcuHdgDec:         return (0x20, 2);
                case Control.FcuHdgInc:         return (0x40, 2);
                case Control.FcuHdgPush:        return (0x80, 2);

                // FCU controls - offset 3
                case Control.FcuHdgPull:        return (0x01, 3);
                case Control.FcuAltDec:         return (0x02, 3);
                case Control.FcuAltInc:         return (0x04, 3);
                case Control.FcuAltPush:        return (0x08, 3);
                case Control.FcuAltPull:        return (0x10, 3);
                case Control.FcuVsDec:          return (0x20, 3);
                case Control.FcuVsInc:          return (0x40, 3);
                case Control.FcuVsPush:         return (0x80, 3);

                // FCU controls - offset 4
                case Control.FcuVsPull:         return (0x01, 4);
                case Control.FcuAlt100:         return (0x02, 4);
                case Control.FcuAlt1000:        return (0x04, 4);

                // Left EFIS controls - offset 5
                case Control.LeftFd:            return (0x01, 5);
                case Control.LeftLs:            return (0x02, 5);
                case Control.LeftCstr:          return (0x04, 5);
                case Control.LeftWpt:           return (0x08, 5);
                case Control.LeftVorD:          return (0x10, 5);
                case Control.LeftNdb:           return (0x20, 5);
                case Control.LeftArpt:          return (0x40, 5);
                case Control.LeftBaroPush:      return (0x80, 5);

                // Left EFIS controls - offset 6
                case Control.LeftBaroPull:      return (0x01, 6);
                case Control.LeftBaroDec:       return (0x02, 6);
                case Control.LeftBaroInc:       return (0x04, 6);
                case Control.LeftInHg:          return (0x08, 6);
                case Control.LeftHPa:           return (0x10, 6);
                case Control.LeftModeLs:        return (0x20, 6);
                case Control.LeftModeVor:       return (0x40, 6);
                case Control.LeftModeNav:       return (0x80, 6);

                // Left EFIS controls - offset 7
                case Control.LeftModeArc:       return (0x01, 7);
                case Control.LeftModePlan:      return (0x02, 7);
                case Control.LeftRange10:       return (0x04, 7);
                case Control.LeftRange20:       return (0x08, 7);
                case Control.LeftRange40:       return (0x10, 7);
                case Control.LeftRange80:       return (0x20, 7);
                case Control.LeftRange160:      return (0x40, 7);
                case Control.LeftRange320:      return (0x80, 7);

                // Left EFIS controls - offset 8
                case Control.LeftNeedle1Adf:    return (0x01, 8);
                case Control.LeftNeedle1Off:    return (0x02, 8);
                case Control.LeftNeedle1Vor:    return (0x04, 8);
                case Control.LeftNeedle2Adf:    return (0x08, 8);
                case Control.LeftNeedle2Off:    return (0x10, 8);
                case Control.LeftNeedle2Vor:    return (0x20, 8);

                // Right EFIS controls - offset 9
                case Control.RightFd:           return (0x01, 9);
                case Control.RightLs:           return (0x02, 9);
                case Control.RightCstr:         return (0x04, 9);
                case Control.RightWpt:          return (0x08, 9);
                case Control.RightVorD:         return (0x10, 9);
                case Control.RightNdb:          return (0x20, 9);
                case Control.RightArpt:         return (0x40, 9);
                case Control.RightBaroPush:     return (0x80, 9);

                // Right EFIS controls - offset 10
                case Control.RightBaroPull:     return (0x01, 10);
                case Control.RightBaroDec:      return (0x02, 10);
                case Control.RightBaroInc:      return (0x04, 10);
                case Control.RightInHg:         return (0x08, 10);
                case Control.RightHPa:          return (0x10, 10);
                case Control.RightModeLs:       return (0x20, 10);
                case Control.RightModeVor:      return (0x40, 10);
                case Control.RightModeNav:      return (0x80, 10);

                // Right EFIS controls - offset 11
                case Control.RightModeArc:      return (0x01, 11);
                case Control.RightModePlan:     return (0x02, 11);
                case Control.RightRange10:      return (0x04, 11);
                case Control.RightRange20:      return (0x08, 11);
                case Control.RightRange40:      return (0x10, 11);
                case Control.RightRange80:      return (0x20, 11);
                case Control.RightRange160:     return (0x40, 11);
                case Control.RightRange320:     return (0x80, 11);

                // Right EFIS controls - offset 12
                case Control.RightNeedle1Vor:   return (0x01, 12);
                case Control.RightNeedle1Off:   return (0x02, 12);
                case Control.RightNeedle1Adf:   return (0x04, 12);
                case Control.RightNeedle2Vor:   return (0x08, 12);
                case Control.RightNeedle2Off:   return (0x10, 12);
                case Control.RightNeedle2Adf:   return (0x20, 12);

                default:                        return (0, 0);
            }
        }
    }
}
