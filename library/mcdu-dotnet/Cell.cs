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
using System.Text;

namespace WwDevicesDotNet
{
    /// <summary>
    /// Describes a single character cell in the MCDU display.
    /// </summary>
    public class Cell
    {
        public char Character { get; set; }

        public Colour Colour { get; set; }

        public Colour BackgroundColour { get; set; } = Colour.Black;

        public bool Small { get; set; }

        public Cell() : this(' ', Colour.White, false)
        {
        }

        public Cell(char character, Colour colour, bool small)
        {
            Character = character;
            Colour = colour;
            Small = small;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(Character);
            return result.ToString();
        }

        public void Clear()
        {
            Character = ' ';
            Colour = Colour.White;
            BackgroundColour = Colour.Black;
            Small = false;
        }

        public void Set(char character, Colour colour, bool small, Colour? backgroundColor = null)
        {
            Character = character;
            Colour = colour;
            Small = small;
            BackgroundColour = backgroundColor ?? Colour.Black;
        }

        public void CopyFrom(Cell other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            Set(other.Character, other.Colour, other.Small, other.BackgroundColour);
        }

        public void CopyTo(Cell other)  => other?.CopyFrom(this);
    }
}
