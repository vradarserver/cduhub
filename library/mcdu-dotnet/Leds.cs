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

namespace wwDevicesDotNet
{
    /// <summary>
    /// Describes the state of the LEDs.
    /// </summary>
    public class Leds
    {
        /// <summary>
        /// Gets or sets the lit state of the FAIL LED. Supported on all panels.
        /// </summary>
        public bool Fail { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM LED. Only supported on the MCDU.
        /// </summary>
        public bool Fm { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MCDU LED. Only supported on the MCDU.
        /// </summary>
        public bool Mcdu { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MENU LED. Only supported on the MCDU.
        /// </summary>
        public bool Menu { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM1 LED. Only supported on the MCDU.
        /// </summary>
        public bool Fm1 { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the IND LED. Only supported on the MCDU.
        /// </summary>
        public bool Ind { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the RDY LED. Only supported on the MCDU.
        /// </summary>
        public bool Rdy { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the horizontal line LED. Only supported on the MCDU
        /// (see <see cref="Exec"/> for the Boeing line LED).
        /// </summary>
        public bool Line { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM2 LED. Only supported on the MCDU.
        /// </summary>
        public bool Fm2 { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the DSPY LED. Only supported on the PFP.
        /// </summary>
        public bool Dspy { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the EXEC LED. Only supported on the PFP.
        /// </summary>
        public bool Exec { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MSG LED. Only supported on the PFP.
        /// </summary>
        public bool Msg { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the OFST LED. Only supported on the PFP.
        /// </summary>
        public bool Ofst { get; set; }

        /// <summary>
        /// Switches all of the LEDs on or off.
        /// </summary>
        /// <param name="on"></param>
        public void TurnAllOn(bool on)
        {
            Dspy =  on;
            Exec =  on;
            Fail =  on;
            Fm =    on;
            Fm1 =   on;
            Fm2 =   on;
            Ind =   on;
            Line =  on;
            Mcdu =  on;
            Menu =  on;
            Msg =   on;
            Ofst =  on;
            Rdy =   on;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is Leds other) {
                result = Dspy == other.Dspy
                      && Exec == other.Exec
                      && Fail == other.Fail
                      && Fm == other.Fm
                      && Fm1 == other.Fm1
                      && Fm2 == other.Fm2
                      && Ind == other.Ind
                      && Line == other.Line
                      && Mcdu == other.Mcdu
                      && Menu == other.Menu
                      && Msg == other.Msg
                      && Ofst == other.Ofst
                      && Rdy == other.Rdy;
            }
            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Just needs to be technically correct to shut the compiler up.
            return Fail.GetHashCode();
        }

        public bool GetLed(Led led)
        {
            switch(led) {
                case Led.Dspy:  return Dspy;
                case Led.Exec:  return Exec;
                case Led.Fail:  return Fail;
                case Led.Fm:    return Fm;
                case Led.Fm1:   return Fm1;
                case Led.Fm2:   return Fm2;
                case Led.Ind:   return Ind;
                case Led.Line:  return Line;
                case Led.Mcdu:  return Mcdu;
                case Led.Menu:  return Menu;
                case Led.Msg:   return Msg;
                case Led.Ofst:  return Ofst;
                case Led.Rdy:   return Rdy;
                default:        throw new NotImplementedException();
            }
        }

        public void SetLed(Led led, bool on)
        {
            switch(led) {
                case Led.Dspy:  Dspy = on; break;
                case Led.Exec:  Exec = on; break;
                case Led.Fail:  Fail = on; break;
                case Led.Fm:    Fm = on; break;
                case Led.Fm1:   Fm1 = on; break;
                case Led.Fm2:   Fm2 = on; break;
                case Led.Ind:   Ind = on; break;
                case Led.Line:  Line = on; break;
                case Led.Mcdu:  Mcdu = on; break;
                case Led.Menu:  Menu = on; break;
                case Led.Msg:   Msg = on; break;
                case Led.Ofst:  Ofst = on; break;
                case Led.Rdy:   Rdy = on; break;
                default:        throw new NotImplementedException();
            }
        }

        public void CopyFrom(Leds other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            Dspy = other.Dspy;
            Exec = other.Exec;
            Fail = other.Fail;
            Fm = other.Fm;
            Fm1 = other.Fm1;
            Fm2 = other.Fm2;
            Ind = other.Ind;
            Line = other.Line;
            Mcdu = other.Mcdu;
            Menu = other.Menu;
            Msg = other.Msg;
            Ofst = other.Ofst;
            Rdy = other.Rdy;
        }

        public void CopyTo(Leds other) => other?.CopyFrom(this);
    }
}
