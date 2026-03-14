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

namespace McduDotNet
{
    [Obsolete("Use CduLamps")]
    public class Leds
    {
        public CduLamps CduLamps { get; }

        public bool Fail
        {
            get => CduLamps.Fail;
            set => CduLamps.Fail = value;
        }

        public bool Fm
        {
            get => CduLamps.Fm;
            set => CduLamps.Fm = value;
        }

        public bool Mcdu
        {
            get => CduLamps.Mcdu;
            set => CduLamps.Mcdu = value;
        }

        public bool Menu
        {
            get => CduLamps.Menu;
            set => CduLamps.Menu = value;
        }

        public bool Fm1
        {
            get => CduLamps.Fm1;
            set => CduLamps.Fm1 = value;
        }

        public bool Ind
        {
            get => CduLamps.Ind;
            set => CduLamps.Ind = value;
        }

        public bool Rdy
        {
            get => CduLamps.Rdy;
            set => CduLamps.Rdy = value;
        }

        public bool Line
        {
            get => CduLamps.Line;
            set => CduLamps.Line = value;
        }

        public bool Fm2
        {
            get => CduLamps.Fm2;
            set => CduLamps.Fm2 = value;
        }

        public bool Dspy
        {
            get => CduLamps.Dspy;
            set => CduLamps.Dspy = value;
        }

        public bool Exec
        {
            get => CduLamps.Exec;
            set => CduLamps.Exec = value;
        }

        public bool Msg
        {
            get => CduLamps.Msg;
            set => CduLamps.Msg = value;
        }

        public bool Ofst
        {
            get => CduLamps.Ofst;
            set => CduLamps.Ofst = value;
        }

        public Leds() : this(new())
        {
        }

        public Leds(CduLamps cduLamps)
        {
            CduLamps = cduLamps ?? throw new ArgumentNullException(nameof(cduLamps));
        }

        public void TurnAllOn(bool on) => CduLamps.TurnAllOn(on);

        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is Leds other) {
                result = CduLamps.Equals(other.CduLamps);
            }
            return result;
        }

        public override int GetHashCode()
        {
            return CduLamps.GetHashCode();
        }

        public bool GetLed(Led led)
        {
            return CduLamps.GetLamp((CduLamp)led);
        }

        public void SetLed(Led led, bool on)
        {
            CduLamps.SetLamp((CduLamp)led, on);
        }

        public void CopyFrom(Leds other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            CduLamps.CopyFrom(other.CduLamps);
        }

        public void CopyTo(Leds other) => other?.CopyFrom(this);
    }
}
