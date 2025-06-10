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
    /// <summary>
    /// Describes the state of the LEDs.
    /// </summary>
    public class Leds
    {
        /// <summary>
        /// Gets or sets the brightness of the LEDs as a value from 0 to 1.
        /// </summary>
        public double Brightness { get; set; } = 0.75;

        /// <summary>
        /// Gets or sets the lit state of the FAIL LED.
        /// </summary>
        public bool Fail { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM LED.
        /// </summary>
        public bool Fm { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MCDU LED.
        /// </summary>
        public bool Mcdu { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MENU LED.
        /// </summary>
        public bool Menu { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM1 LED.
        /// </summary>
        public bool Fm1 { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the IND LED.
        /// </summary>
        public bool Ind { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the RDY LED.
        /// </summary>
        public bool Rdy { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the horizontal line LED.
        /// </summary>
        public bool Line { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM2 LED.
        /// </summary>
        public bool Fm2 { get; set; }

        /// <summary>
        /// Switches all of the LEDs on or off.
        /// </summary>
        /// <param name="on"></param>
        public void TurnAllOn(bool on)
        {
            Fail =  on;
            Fm =    on;
            Fm1 =   on;
            Fm2 =   on;
            Ind =   on;
            Line =  on;
            Mcdu =  on;
            Menu =  on;
            Rdy =   on;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is Leds other) {
                result = Brightness == other.Brightness
                      && Fail == other.Fail
                      && Fm == other.Fm
                      && Fm1 == other.Fm1
                      && Fm2 == other.Fm2
                      && Ind == other.Ind
                      && Line == other.Line
                      && Mcdu == other.Mcdu
                      && Menu == other.Menu
                      && Rdy == other.Rdy;
            }
            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Brightness.GetHashCode();
        }

        public void CopyFrom(Leds other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            Brightness = other.Brightness;
            Fail = other.Fail;
            Fm = other.Fm;
            Fm1 = other.Fm1;
            Fm2 = other.Fm2;
            Ind = other.Ind;
            Line = other.Line;
            Mcdu = other.Mcdu;
            Menu = other.Menu;
            Rdy = other.Rdy;
        }

        public void CopyTo(Leds other) => other?.CopyFrom(this);
    }
}
