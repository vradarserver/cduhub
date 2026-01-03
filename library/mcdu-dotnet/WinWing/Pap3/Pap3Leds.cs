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
    /// Represents the LED states for a PAP-3 Primary Autopilot Panel.
    /// LED command codes verified from hardware testing.
    /// </summary>
    public class Pap3Leds : IFrontpanelLeds
    {
        // Autothrottle LEDs (0x03-0x04)
        public bool N1 { get; set; }             // 0x03
        public bool Speed { get; set; }       // 0x04

        // Autopilot mode LEDs (0x05-0x0C)
        public bool Vnav { get; set; }            // 0x05
        public bool LvlChg { get; set; }          // 0x06
        public bool HdgSel { get; set; }          // 0x07
        public bool Lnav { get; set; }            // 0x08
        public bool VorLoc { get; set; }          // 0x09
        public bool App { get; set; }             // 0x0A
        public bool AltHold { get; set; }         // 0x0B
        public bool Vs { get; set; }              // 0x0C

        // Autopilot command LEDs (0x0D-0x10)
        public bool CmdA { get; set; }            // 0x0D (A_CMD)
        public bool CwsA { get; set; }            // 0x0E (A_CWS)
        public bool CmdB { get; set; }            // 0x0F (B_CMD)
        public bool CwsB { get; set; }            // 0x10 (B_CWS)

        // Autothrottle Arm LED (0x11)
        public bool AtArm { get; set; }           // 0x11 (AT_ARM)

        // Flight Director LEDs (0x12-0x13)
        public bool FdL { get; set; }             // 0x12 (L_MA - Master Flight Director Left)
        public bool FdR { get; set; }             // 0x13 (R_MA - Master Flight Director Right)
    }
}
