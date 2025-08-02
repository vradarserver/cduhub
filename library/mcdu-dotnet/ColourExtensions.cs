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
            int foreground;
            switch(colour) {
                case Colour.Amber:      foreground = 0x0021; break;
                case Colour.Brown:      foreground = 0x0108; break;
                case Colour.Cyan:       foreground = 0x0063; break;
                case Colour.Green:      foreground = 0x0084; break;
                case Colour.Grey:       foreground = 0x0129; break;
                case Colour.Khaki:      foreground = 0x014A; break;
                case Colour.Magenta:    foreground = 0x00A5; break;
                case Colour.Red:        foreground = 0x00C6; break;
                case Colour.White:      foreground = 0x0042; break;
                case Colour.Yellow:     foreground = 0x00E7; break;
                default:                throw new NotImplementedException();
            }
            if(isSmallFont) {
                foreground += 0x16B;
            }

            return ((byte)(foreground & 0xff), (byte)((foreground >> 8) & 0xff));
        }

        public static char ToDuplicateCheckCode(this Colour colour, bool isSmallFont)
        {
            switch(colour) {
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
