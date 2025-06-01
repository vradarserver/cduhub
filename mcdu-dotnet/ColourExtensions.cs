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
            if(isSmallFont) {
                switch(colour) {
                    case Colour.Amber:      return (0x8C, 0x01);
                    case Colour.Brown:      return (0x73, 0x02);
                    case Colour.Cyan:       return (0xCE, 0x01);
                    case Colour.Green:      return (0xEF, 0x01);
                    case Colour.Grey:       return (0x94, 0x02);
                    case Colour.Khaki:      return (0xB5, 0x02);
                    case Colour.Magenta:    return (0x10, 0x02);
                    case Colour.Red:        return (0x31, 0x02);
                    case Colour.White:      return (0xAD, 0x01);
                    case Colour.Yellow:     return (0x52, 0x02);
                    default:                throw new NotImplementedException();
                }
            } else {
                switch(colour) {
                    case Colour.Amber:      return (0x21, 0x00);
                    case Colour.Brown:      return (0x08, 0x01);
                    case Colour.Cyan:       return (0x63, 0x00);
                    case Colour.Green:      return (0x84, 0x00);
                    case Colour.Grey:       return (0x29, 0x01);
                    case Colour.Khaki:      return (0x4A, 0x01);
                    case Colour.Magenta:    return (0xA5, 0x00);
                    case Colour.Red:        return (0xC6, 0x00);
                    case Colour.White:      return (0x42, 0x00);
                    case Colour.Yellow:     return (0xE7, 0x00);
                    default:                throw new NotImplementedException();
                }
            }
        }
    }
}
