// Copyright © 2025 onwards, Laurent Andre
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace WwDevicesDotNet.WinWing.Pap3
{
    /// <summary>
    /// Maps PAP-3 controls to their input report byte offsets and bit flags.
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

                case Control.N1:            return (0x01, 1);
                case Control.Speed:         return (0x02, 1);
                case Control.Vnav:          return (0x04, 1);
                case Control.LvlChg:        return (0x08, 1);
                case Control.HdgSel:        return (0x10, 1);
                case Control.Lnav:          return (0x20, 1); 
                case Control.VorLoc:        return (0x40, 1);
                case Control.App:           return (0x80, 1);

                case Control.AltHold:       return (0x01, 2);
                case Control.Vs:            return (0x02, 2);
                case Control.CmdA:          return (0x04, 2);
                case Control.CwsA:          return (0x08, 2);
                case Control.CmdB:          return (0x10, 2);
                case Control.CwsB:          return (0x20, 2);
                case Control.CO:            return (0x40, 2);
                case Control.SpdIntv:       return (0x80, 2);

                case Control.AltIntv:       return (0x01, 3);
                case Control.PltCourseDec:  return (0x02, 3);
                case Control.PltCourseInc:  return (0x04, 3);
                case Control.SpdDec:        return (0x08, 3);
                case Control.SpdInc:        return (0x10, 3);
                case Control.HdgDec:        return (0x20, 3);
                case Control.HdgInc:        return (0x40, 3);
                case Control.AltDec:        return (0x80, 3);

                case Control.AltInc:        return (0x01, 4);
                case Control.CplCourseDec:  return (0x02, 4);
                case Control.CplCourseInc:  return (0x04, 4);
                case Control.PltFdOn:       return (0x08, 4);
                case Control.PltFdOff:      return (0x10, 4);
                case Control.CplFdOn:       return (0x20, 4);
                case Control.CplFdOff:      return (0x40, 4);
                case Control.Disengage:     return (0x80, 4);
                
                case Control.Bank10:        return (0x02, 5);
                case Control.Bank15:        return (0x04, 5);
                case Control.Bank20:        return (0x08, 5);
                case Control.Bank25:        return (0x10, 5);
                case Control.Bank30:        return (0x20, 5);

                case Control.VsDn:          return (0x40, 5);
                case Control.VsUp:          return (0x80, 5);

                case Control.ATArmOff:      return (0x02, 6);
                case Control.ATArmOn:       return (0x01, 6);


                default:                        return (0, 0);
            }
        }
    }
}
