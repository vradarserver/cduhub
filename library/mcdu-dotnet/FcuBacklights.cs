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
using System.Collections.Generic;
using System.Text;

namespace McduDotNet
{
    public class FcuBacklights
    {
        public FcuBacklightSet LeftEfis { get; } = new();

        public FcuBacklightFcuSet Fcu { get; } = new();

        public FcuBacklightSet RightEfis { get; } = new();

        public int PanelPercent
        {
            get => Fcu.PanelPercent;
            set => Fcu.PanelPercent = LeftEfis.PanelPercent = RightEfis.PanelPercent = value;
        }

        public int DisplayPercent
        {
            get => Fcu.DisplayPercent;
            set => Fcu.DisplayPercent = LeftEfis.DisplayPercent = RightEfis.DisplayPercent = value;
        }

        public int LedPercent
        {
            get => Fcu.GreenLedPercent;
            set => Fcu.GreenLedPercent = LeftEfis.GreenLedPercent = RightEfis.GreenLedPercent = value;
        }

        public int ExpedPercent
        {
            get => Fcu.ExpedPercent;
            set => Fcu.ExpedPercent = value;
        }

        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is FcuBacklights other) {
                result = LeftEfis.Equals(other.LeftEfis)
                      && Fcu.Equals(other.Fcu)
                      && RightEfis.Equals(other.RightEfis);
            }
            return result;
        }

        // Do not use these as keys.
        public override int GetHashCode() => Fcu.DisplayPercent;

        public void CopyFrom(FcuBacklights other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            LeftEfis.CopyFrom(other.LeftEfis);
            Fcu.CopyFrom(other.Fcu);
            RightEfis.CopyFrom(other.RightEfis);
        }

        public void CopyTo(FcuBacklights? to) => to?.CopyFrom(this);
    }
}
