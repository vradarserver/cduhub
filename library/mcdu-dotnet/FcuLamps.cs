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
    /// Describes the LED lights on an FCU device.
    /// </summary>
    public class FcuLamps
    {
        public FcuLampsEfis LeftEfis { get; } = new(isLeft: true);

        public FcuLampsEfis RightEfis { get; } = new(isLeft: false);

        public bool Loc { get; set; }

        public bool Ap1 { get; set; }

        public bool Ap2 { get; set; }

        public bool AThr { get; set; }

        public bool Exped { get; set; }

        public bool Appr { get; set; }

        /// <summary>
        /// Switches all of the LEDs on or off.
        /// </summary>
        /// <param name="on"></param>
        public void TurnAllOn(bool on)
        {
            LeftEfis.TurnAllOn(on);
            RightEfis.TurnAllOn(on);
            Loc = on;
            Ap1 = on;
            Ap2 = on;
            AThr = on;
            Exped = on;
            Appr = on;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is FcuLamps other) {
                result = LeftEfis.Equals(other.LeftEfis)
                    && RightEfis.Equals(other.RightEfis)
                    && Loc == other.Loc
                    && Ap1 == other.Ap1
                    && Ap2 == other.Ap2
                    && AThr == other.AThr
                    && Exped == other.Exped
                    && Appr == other.Appr;
            }

            return result;
        }

        // Just needs to be technically correct, we're not using these as keys.
        /// <inheritdoc/>
        public override int GetHashCode() => Loc ? 1 : 0;

        /// <summary>
        /// </summary>
        /// <param name="lamp"></param>
        /// <returns>
        /// True if the lamp is switched on, false if it is switched off and null if it is
        /// not supported by the device.
        /// </returns>
        public bool? GetLamp(FgcpLamp lamp)
        {
            bool? result;

            switch(lamp) {
                case FgcpLamp.Ap1:      result = Ap1; break;
                case FgcpLamp.Ap2:      result = Ap2; break;
                case FgcpLamp.Appr:     result = Appr; break;
                case FgcpLamp.AThr:     result = AThr; break;
                case FgcpLamp.Exped:    result = Exped; break;
                case FgcpLamp.Loc:      result = Loc; break;
                default:
                    result = LeftEfis.GetLamp(lamp)
                          ?? RightEfis.GetLamp(lamp);
                    break;
            }

            return result;
        }

        public void SetLamp(FgcpLamp lamp, bool on)
        {
            switch(lamp) {
                case FgcpLamp.Ap1:      Ap1 = on; break;
                case FgcpLamp.Ap2:      Ap2 = on; break;
                case FgcpLamp.Appr:     Appr = on; break;
                case FgcpLamp.AThr:     AThr = on; break;
                case FgcpLamp.Exped:    Exped = on; break;
                case FgcpLamp.Loc:      Loc = on; break;
                default:
                    LeftEfis.SetLamp(lamp, on);
                    RightEfis.SetLamp(lamp, on);
                    break;
            }
        }

        public void CopyFrom(FcuLamps other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            LeftEfis.CopyFrom(other.LeftEfis);
            RightEfis.CopyFrom(other.RightEfis);
            Loc = other.Loc;
            Ap1 = other.Ap1;
            Ap2 = other.Ap2;
            AThr = other.AThr;
            Exped = other.Exped;
            Appr = other.Appr;
        }

        public void CopyTo(FcuLamps other) => other?.CopyFrom(this);
    }
}
