// Copyright © 2025 onwards, Andrew Whewell, Laurent Andre
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
    /// An enumeration of all of the buttons and controls on the PAP-3 Primary Autopilot Panel.
    /// </summary>
    public enum Control
    {
        
        // Speed

        N1,
        Speed,
        CO,
        SpdIntv,
        SpdDec,
        SpdInc,

        ATArmOn,
        ATArmOff,

        // Flight director
        PltFdOn,
        CplFdOn,
        PltFdOff,
        CplFdOff,

        // Course section
        PltCourseDec,
        PltCourseInc,
        CplCourseDec,
        CplCourseInc,

        // Heading section
        HdgDec,
        HdgInc,

        // Altitude section
        AltDec,
        AltInc,

        // Vertical Speed section
        VsDn,
        VsUp,

        // Autopilot buttons
        CmdA,
        CmdB,
        CwsA,
        CwsB,

        // Autopilot modes
        Lnav,
        Vnav,
        LvlChg,
        HdgSel,
        VorLoc,
        App,
        AltHold,
        Vs,
        AltIntv,

        // Other controls
        Disengage,
        Bank10,
        Bank15,
        Bank20,
        Bank25,
        Bank30
    }
}
