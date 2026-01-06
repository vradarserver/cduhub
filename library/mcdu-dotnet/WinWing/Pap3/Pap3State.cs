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
    /// Represents the display state for a PAP-3 Primary Autopilot Panel.
    /// </summary>
    public class Pap3State : IFrontpanelState
    {
        /// <summary>
        /// Speed value. Mach speeds are sent without the decimal point (e.g., Mach 0.82 is sent as 82).
        /// </summary>
        public int? Speed { get; set; }

        /// <summary>
        /// Course value (0-359 degrees).
        /// </summary>
        public int? PltCourse { get; set; }

        /// <summary>
        /// Course value (0-359 degrees).
        /// </summary>
        public int? CplCourse { get; set; }

        /// <summary>
        /// Heading value (0-359 degrees).
        /// </summary>
        public int? Heading { get; set; }

        /// <summary>
        /// Altitude value (feet).
        /// </summary>
        public int? Altitude { get; set; }

        /// <summary>
        /// Vertical speed value (feet per minute).
        /// </summary>
        public int? VerticalSpeed { get; set; }

        /// <summary>
        /// True if speed display is in Mach mode, false for knots.
        /// </summary>
        public bool SpeedIsMach { get; set; } = false;

        /// <summary>
        /// True if altitude is displayed as flight level.
        /// </summary>
        public bool AltitudeIsFlightLevel { get; set; } = false;

        /// <summary>
        /// True if heading display is in Track mode, false for Heading mode.
        /// </summary>
        public bool HeadingIsTrack { get; set; } = false;

        /// <summary>
        /// True if vertical speed display is in FPA mode, false for V/S mode.
        /// </summary>
        public bool VsIsFpa { get; set; } = false;

        /// <summary>
        /// True if the magnetic altitude hold solenoid should be engaged (locked).
        /// When true, the altitude knob is locked down and cannot pop up.
        /// When false, the altitude knob is free to pop up when pulled.
        /// </summary>
        public bool MagneticActivated { get; set; } = false;
    }
}
