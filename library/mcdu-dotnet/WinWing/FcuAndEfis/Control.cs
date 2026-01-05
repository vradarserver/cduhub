// Copyright © 2025 onwards, Laurent Andre
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
    /// An enumeration of all of the buttons and controls on the FCU and EFIS panels.
    /// </summary>
    public enum Control
    {
        // FCU controls
        FcuSpdMach,
        FcuLoc,
        FcuHdgTrkVsFpa,
        FcuAp1,
        FcuAp2,
        FcuAThr,
        FcuExped,
        FcuMetricAlt,
        FcuAppr,
        FcuSpdDec,
        FcuSpdInc,
        FcuSpdPush,
        FcuSpdPull,
        FcuHdgDec,
        FcuHdgInc,
        FcuHdgPush,
        FcuHdgPull,
        FcuAltDec,
        FcuAltInc,
        FcuAltPush,
        FcuAltPull,
        FcuVsDec,
        FcuVsInc,
        FcuVsPush,
        FcuVsPull,
        FcuAlt100,
        FcuAlt1000,

        // Left EFIS controls
        LeftFd,
        LeftLs,
        LeftCstr,
        LeftWpt,
        LeftVorD,
        LeftNdb,
        LeftArpt,
        LeftBaroPush,
        LeftBaroPull,
        LeftBaroDec,
        LeftBaroInc,
        LeftInHg,
        LeftHPa,
        LeftModeLs,
        LeftModeVor,
        LeftModeNav,
        LeftModeArc,
        LeftModePlan,
        LeftRange10,
        LeftRange20,
        LeftRange40,
        LeftRange80,
        LeftRange160,
        LeftRange320,
        LeftNeedle1Adf,
        LeftNeedle1Off,
        LeftNeedle1Vor,
        LeftNeedle2Adf,
        LeftNeedle2Off,
        LeftNeedle2Vor,

        // Right EFIS controls
        RightFd,
        RightLs,
        RightCstr,
        RightWpt,
        RightVorD,
        RightNdb,
        RightArpt,
        RightBaroPush,
        RightBaroPull,
        RightBaroDec,
        RightBaroInc,
        RightInHg,
        RightHPa,
        RightModeLs,
        RightModeVor,
        RightModeNav,
        RightModeArc,
        RightModePlan,
        RightRange10,
        RightRange20,
        RightRange40,
        RightRange80,
        RightRange160,
        RightRange320,
        RightNeedle1Vor,
        RightNeedle1Off,
        RightNeedle1Adf,
        RightNeedle2Vor,
        RightNeedle2Off,
        RightNeedle2Adf,
    }
}
