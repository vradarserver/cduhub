// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet
{
    /// <summary>
    /// Describes the state of the LEDs.
    /// </summary>
    public interface ILeds
    {
        /// <summary>
        /// Gets or sets the lit state of the FAIL LED.
        /// </summary>
        bool Fail { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM LED.
        /// </summary>
        bool Fm { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MCDU LED.
        /// </summary>
        bool Mcdu { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the MENU LED.
        /// </summary>
        bool Menu { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM1 LED.
        /// </summary>
        bool Fm1 { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the IND LED.
        /// </summary>
        bool Ind { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the RDY LED.
        /// </summary>
        bool Rdy { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the horizontal line LED.
        /// </summary>
        bool Line { get; set; }

        /// <summary>
        /// Gets or sets the lit state of the FM2 LED.
        /// </summary>
        bool Fm2 { get; set; }

        /// <summary>
        /// Gets or sets the brightness of the LEDs as a value from 0 to 1.
        /// </summary>
        double Brightness { get; set; }

        /// <summary>
        /// Switches all of the LEDs on or off.
        /// </summary>
        /// <param name="on"></param>
        void TurnAllOn(bool on);
    }
}
