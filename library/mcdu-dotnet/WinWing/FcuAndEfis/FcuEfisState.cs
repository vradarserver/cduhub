// Copyright © 2025 onwards, Andrew Whewell, Laurent Andre
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
    public class FcuEfisState : IFrontpanelState
    {
        /// <summary>
        /// Mach speed are sent without the decimal point. For example, Mach 0.82 is sent as 082
        /// </summary>
        public int? Speed { get; set; }
        public int? Heading { get; set; }
        public int? Altitude { get; set; }
        public int? VerticalSpeed { get; set; }

        public bool SpeedIsMach { get; set; } = false;
        public bool HeadingIsTrack { get; set; } = false;
        public bool VsIsFpa { get; set; } = false;

        public bool AltitudeIsFlightLevel { get; set; } = false;

        public bool SpeedManaged { get; set; } = false;
        public bool HeadingManaged { get; set; } = false;
        public bool AltitudeManaged { get; set; } = false;

        public bool LatIndicator { get; set; } = false;
        
        public bool LvlIndicator { get; set; } = false;
        public bool LvlLeftBracket { get; set; } = false;
        public bool LvlRightBracket { get; set; } = false;
        public bool VsHorzIndicator { get; set; } = false;

        public int? LeftBaroPressure { get; set; }
        public bool LeftBaroQnh { get; set; }
        public bool LeftBaroQfe { get; set; }

        public int? RightBaroPressure { get; set; }
        public bool RightBaroQnh { get; set; }
        public bool RightBaroQfe { get; set; }
    }
}
