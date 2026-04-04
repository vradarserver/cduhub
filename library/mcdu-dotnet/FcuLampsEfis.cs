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
    /// The LED lights on an EFIS device.
    /// </summary>
    public class FcuLampsEfis
    {
        public bool FD { get; set; }

        public bool LS { get; set; }

        public bool Cstr { get; set; }

        public bool Wpt { get; set; }

        public bool VorD { get; set; }

        public bool Ndb { get; set; }

        public bool Arpt { get; set; }

        /// <summary>
        /// Switches all of the LEDs on or off.
        /// </summary>
        /// <param name="on"></param>
        public void TurnAllOn(bool on)
        {
            FD = on;
            LS = on;
            Cstr = on;
            Wpt = on;
            VorD = on;
            Ndb = on;
            Arpt = on;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is FcuLampsEfis other) {
                result = FD == other.FD
                    && LS == other.LS
                    && Cstr == other.Cstr
                    && Wpt == other.Wpt
                    && VorD == other.VorD
                    && Ndb == other.Ndb
                    && Arpt == other.Arpt;
            }

            return result;
        }

        // Just needs to be technically correct, we're not using these as keys.
        /// <inheritdoc/>
        public override int GetHashCode() => FD ? 1 : 0;

        public void CopyFrom(FcuLampsEfis other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            FD = other.FD;
            LS = other.LS;
            Cstr = other.Cstr;
            Wpt = other.Wpt;
            VorD = other.VorD;
            Ndb = other.Ndb;
            Arpt = other.Arpt;
        }

        public void CopyTo(FcuLampsEfis other) => other?.CopyFrom(this);
    }
}
