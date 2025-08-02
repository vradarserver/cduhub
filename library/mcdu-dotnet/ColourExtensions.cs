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
    static class ColourExtensions
    {
        public static (byte,byte) ToUsbColourAndFontCode(this Colour colour, bool isSmallFont)
        {
            var code = ToWinWingColourOrdinal(colour) * 0x21;
            if(isSmallFont) {
                code += 0x16B;
            }

            return ((byte)(code & 0xff), (byte)((code & 0xff00) >> 8));
        }

        /// <summary>
        /// Returns the order in which the colour appears in WinWing's 32bb...1901...0002
        /// and 32bb...1901...0003 packets. This feeds into the value to send for the colour
        /// when setting foreground (and background?).
        /// </summary>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static int ToWinWingColourOrdinal(this Colour colour)
        {
            switch(colour) {
                case Colour.Black:      return 0;
                case Colour.Amber:      return 1;
                case Colour.White:      return 2;
                case Colour.Cyan:       return 3;
                case Colour.Green:      return 4;
                case Colour.Magenta:    return 5;
                case Colour.Red:        return 6;
                case Colour.Yellow:     return 7;
                case Colour.Brown:      return 8;
                case Colour.Grey:       return 9;
                case Colour.Khaki:      return 10;
                default:                throw new NotImplementedException();
            }
        }

        public static char ToDuplicateCheckCode(this Colour colour, bool isSmallFont)
        {
            switch(colour) {
                case Colour.Black:      return isSmallFont ? 'l' : 'L';
                case Colour.Amber:      return isSmallFont ? 'a' : 'A';
                case Colour.Brown:      return isSmallFont ? 'b' : 'B';
                case Colour.Cyan:       return isSmallFont ? 'c' : 'C';
                case Colour.Green:      return isSmallFont ? 'g' : 'G';
                case Colour.Grey:       return isSmallFont ? 'e' : 'E';
                case Colour.Khaki:      return isSmallFont ? 'k' : 'K';
                case Colour.Magenta:    return isSmallFont ? 'm' : 'M';
                case Colour.Red:        return isSmallFont ? 'r' : 'R';
                case Colour.White:      return isSmallFont ? 'w' : 'W';
                case Colour.Yellow:     return isSmallFont ? 'y' : 'Y';
                default:                throw new NotImplementedException();
            }
        }
    }
}
