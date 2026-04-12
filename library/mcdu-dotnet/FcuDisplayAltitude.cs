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
    public class FcuDisplayAltitude
    {
        public bool Alt { get; set; }

        public bool AltDot { get; set; }

        public bool LvlChGroupLeft { get; set; }

        public bool LvlCh { get; set; }

        public bool LvlChGroupRight { get; set; }

        public bool VS { get; set; }

        public bool Fpa { get; set; }

        public S7DigitCollection AltitudeDigits { get; } = new(
            S7Masks.Digit,
            S7Masks.Digit,
            S7Masks.Digit,
            S7Masks.Digit,
            S7Masks.Digit
        );

        public S7DigitCollection VerticalSpeedDigits { get; } = new(
            S7Masks.Plus,
            S7Masks.Digit,
            S7Masks.DigitLeftDecimal,
            S7Masks.Digit,
            S7Masks.Digit
        );

        public void ClearDisplay()
        {
            Alt = false;
            AltDot = false;
            Fpa = false;
            LvlCh = false;
            LvlChGroupLeft = false;
            LvlChGroupRight = false;
            VS = false;

            AltitudeDigits.ClearDisplay();
            VerticalSpeedDigits.ClearDisplay();
        }

        public void CopyFrom(FcuDisplayAltitude other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            Alt = other.Alt;
            AltDot = other.AltDot;
            Fpa = other.Fpa;
            LvlCh = other.LvlCh;
            LvlChGroupLeft = other.LvlChGroupLeft;
            LvlChGroupRight = other.LvlChGroupRight;
            VS = other.VS;

            AltitudeDigits.CopyFrom(other.AltitudeDigits);
            VerticalSpeedDigits.CopyFrom(other.VerticalSpeedDigits);
        }

        public void CopyTo(FcuDisplayAltitude? other) => other?.CopyFrom(this);
    }
}
