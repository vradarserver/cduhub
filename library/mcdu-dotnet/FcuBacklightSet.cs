// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace McduDotNet
{
    /// <summary>
    /// Common FCU backlight settings.
    /// </summary>
    public class FcuBacklightSet
    {
        private int _PanelPercent;
        /// <summary>
        /// Gets or sets the panel's backlight as a percentage from 0 to 100.
        /// </summary>
        public int PanelPercent
        {
            get => _PanelPercent;
            set => _PanelPercent = Math.Min(100, Math.Max(0, value));
        }

        private int _DisplayPercent;
        /// <summary>
        /// Gets or sets the segmented display's backlight as a percentage from 0 to 100.
        /// </summary>
        public int DisplayPercent
        {
            get => _DisplayPercent;
            set => _DisplayPercent = Math.Min(100, Math.Max(0, value));
        }

        private int _GreenLedPercent;
        /// <summary>
        /// Gets or sets the green LED button light intensity as a percentage from 0 to 100.
        /// </summary>
        public int GreenLedPercent
        {
            get => _GreenLedPercent;
            set => _GreenLedPercent = Math.Min(100, Math.Max(0, value));
        }

        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is FcuBacklightSet other) {
                result = PanelPercent == other.PanelPercent
                      && DisplayPercent == other.DisplayPercent
                      && GreenLedPercent == other.GreenLedPercent;
            }
            return result;
        }

        // doesn't need to be any good, we are not using these as keys
        public override int GetHashCode() => PanelPercent.GetHashCode();

        public void CopyFrom(FcuBacklightSet other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            PanelPercent = other.PanelPercent;
            DisplayPercent = other.DisplayPercent;
            GreenLedPercent = other.GreenLedPercent;
        }

        public void CopyTo(FcuBacklightSet? to) => to?.CopyFrom(this);
    }
}
