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

namespace WwDevicesDotNet
{
    /// <summary>
    /// Describes a copy of the palette held by the device. This is similar to <see
    /// cref="Palette"/> but it is at a lower level. It is not intended as a random write
    /// buffer, it is intended to be populated with an entire palette buffer in one go and
    /// to represent that buffer's contents reasonably efficiently.
    /// </summary>
    public class DisplayPalette
    {
        /// <summary>
        /// An array of colours, one for each colour index (as used by <see
        /// cref="DisplayBufferFontAndColour"/>).
        /// </summary>
        public DisplayColour[] Colours { get; }

        /// <summary>
        /// The number of colours in the palette.
        /// </summary>
        public int CountColours => Colours.Length;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="countColours"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DisplayPalette(int countColours)
        {
            if(countColours < 1) {
                throw new ArgumentOutOfRangeException(nameof(countColours));
            }
            Colours = new DisplayColour[countColours];
        }

        public bool CopyFrom(PaletteColour[] colourArray)
        {
            if(colourArray == null) {
                throw new ArgumentNullException(nameof(colourArray));
            }
            if(colourArray.Length != CountColours) {
                throw new ArgumentOutOfRangeException(nameof(colourArray));
            }

            var result = false;
            for(var idx = 0;idx < CountColours;++idx) {
                result = Colours[idx].CopyFrom(colourArray[idx]) || result;
            }

            return result;
        }

        public void CopyFrom(DisplayPalette other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            if(other.CountColours != CountColours) {
                throw new ArgumentOutOfRangeException(nameof(other));
            }
            for(var idx = 0;idx < CountColours;++idx) {
                Colours[idx].CopyFrom(other.Colours[idx]);
            }
        }

        public void CopyTo(DisplayPalette other) => other?.CopyTo(this);

        public DisplayPalette Clone()
        {
            var result = new DisplayPalette(CountColours);
            result.CopyFrom(this);
            return result;
        }
    }
}
