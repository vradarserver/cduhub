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
    /// CDU device backlight intensities.
    /// </summary>
    public class CduBacklights
    {
        private int _DisplayPercent;
        /// <summary>
        /// Gets and sets the display backlight as a percentage between 0 and 100.
        /// </summary>
        public int DisplayPercent
        {
            get => _DisplayPercent;
            set => _DisplayPercent = Math.Min(100, Math.Max(0, value));
        }

        private int _KeyboardPercent;
        /// <summary>
        /// Gets and sets the keyboard backlight as a percentage between 0 and 100.
        /// </summary>
        public int KeyboardPercent
        {
            get => _KeyboardPercent;
            set => _KeyboardPercent = Math.Min(100, Math.Max(0, value));
        }

        private int _LedPercent;
        /// <summary>
        /// Gets and sets the LED lamp intensity as a percentage between 0 and 100.
        /// </summary>
        public int LedPercent
        {
            get => _LedPercent;
            set => _LedPercent = Math.Min(100, Math.Max(0, value));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is CduBacklights other) {
                result = DisplayPercent ==  other.DisplayPercent
                      && KeyboardPercent == other.KeyboardPercent
                      && LedPercent ==      other.LedPercent;
            }
            return result;
        }

        // doesn't need to be any good, we are not using these as keys
        public override int GetHashCode() => DisplayPercent.GetHashCode();

        public void CopyFrom(CduBacklights other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            DisplayPercent =    other.DisplayPercent;
            KeyboardPercent =   other.KeyboardPercent;
            LedPercent =        other.LedPercent;
        }

        public void CopyTo(CduBacklights? to) => to?.CopyFrom(this);
    }
}
