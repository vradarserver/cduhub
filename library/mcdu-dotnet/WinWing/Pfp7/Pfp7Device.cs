// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using HidSharp;

namespace McduDotNet.WinWing.Pfp7
{
    /// <summary>
    /// Implements <see cref="ICdu"/> for a WinWing PFP-7.
    /// </summary>
    class Pfp7Device : CommonWinWingPanel, ICdu
    {
        protected override byte CommandPrefix => 0x33;

        public int DisplayBrightnessPercent { get; set; }

        public int BacklightBrightnessPercent { get; set; }

        public int LedBrightnessPercent { get; set; }

        public bool HasAmbientLightSensor { get; }

        public int LeftAmbientLightNative { get; }

        public int RightAmbientLightNative { get; }

        public int AmbientLightPercent { get; }

        public AutoBrightnessSettings AutoBrightness { get; }

        public event EventHandler LeftAmbientLightChanged;

        public event EventHandler RightAmbientLightChanged;

        public event EventHandler AmbientLightChanged;

        public Pfp7Device(HidDevice hidDevice, DeviceIdentifier deviceId) : base(hidDevice, deviceId)
        {
        }

        /// <inheritdoc/>
        ~Pfp7Device() => Dispose(false);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing) {
            }
        }

        public void ApplyAutoBrightness()
        {
        }

        public void Cleanup(int backlightBrightnessPercent = 0, int displayBrightnessPercent = 0, int ledBrightnessPercent = 0)
        {
        }

        public void RefreshBrightnesses()
        {
        }

        public void RefreshDisplay(bool skipDuplicateCheck = false)
        {
            _ScreenWriter?.SendScreenToDisplay(Screen, skipDuplicateCheck);
        }

        public void RefreshLeds(bool skipDuplicateCheck = false)
        {
        }

        public void RefreshPalette(bool skipDuplicateCheck = false, bool forceDisplayRefresh = true)
        {
        }

        public void UseFont(McduFontFile fontFileContent, bool useFullWidth)
        {
        }
    }
}
