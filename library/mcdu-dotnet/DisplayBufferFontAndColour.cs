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
    /// Describes a combination of font and colour. Used by internal representations of
    /// the display buffer.
    /// </summary>
    public struct DisplayBufferFontAndColour
    {
        /// <summary>
        /// The encoded value that the other properties read and write. The type of this
        /// property might change in the future if new devices support more colours, do
        /// not assume that it will always be a byte.
        /// </summary>
        public ushort PackedValue { get; set; }

        /// <summary>
        /// Gets or sets the foreground colour index number. See also <see
        /// cref="ForegroundColour"/>.
        /// </summary>
        public int ForegroundColourIndex
        {
            get => PackedValue & 0x0f;
            set => PackedValue = (ushort)((PackedValue & 0xfff0) | (value & 0x0f));
        }

        /// <summary>
        /// Gets or sets the background colour index number. See also <see
        /// cref="BackgroundColour"/>.
        /// </summary>
        public int BackgroundColourIndex
        {
            get => (PackedValue >> 4) & 0x0f;
            set => PackedValue = (ushort)((PackedValue & 0xff0f) | ((value & 0x0f) << 4));
        }

        /// <summary>
        /// Gets or sets the foreground colour.
        /// </summary>
        public Colour ForegroundColour
        {
            get => ColourExtensions.FromDisplayBufferColourIndex(ForegroundColourIndex);
            set => ForegroundColourIndex = value.ToDisplayBufferColourIndex();
        }

        /// <summary>
        /// Gets or sets the background colour.
        /// </summary>
        public Colour BackgroundColour
        {
            get => ColourExtensions.FromDisplayBufferColourIndex(BackgroundColourIndex);
            set => BackgroundColourIndex = value.ToDisplayBufferColourIndex();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the character is rendered in the small
        /// or large font.
        /// </summary>
        public bool IsSmallFont
        {
            get => (PackedValue & 0x8000) == 0x8000;
            set => PackedValue = (ushort)((PackedValue & 0x7fff) | (value ? 0x8000 : 0x0000));
        }

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="packedValue"></param>
        public DisplayBufferFontAndColour(ushort packedValue)
        {
            PackedValue = packedValue;
        }

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="colourIndex"></param>
        /// <param name="isSmallFont"></param>
        public DisplayBufferFontAndColour(int colourIndex, bool isSmallFont)
            : this((ushort)((isSmallFont ? 0x8000 : 0x0000) | (colourIndex & 0x0f)))
        {
        }

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="isSmallFont"></param>
        public DisplayBufferFontAndColour(Colour colour, bool isSmallFont)
            : this(colour.ToDisplayBufferColourIndex(), isSmallFont)
        {
        }

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="foregroundColour"></param>
        /// <param name="backgroundColour"></param>
        /// <param name="isSmallFont"></param>
        public DisplayBufferFontAndColour(Colour foregroundColour, Colour backgroundColour, bool isSmallFont)
            : this((ushort)(
                (isSmallFont ? 0x8000 : 0x0000) | 
                (foregroundColour.ToDisplayBufferColourIndex() & 0x0f) |
                ((backgroundColour.ToDisplayBufferColourIndex() & 0x0f) << 4)
            ))
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"{ForegroundColour}[{(IsSmallFont ? 's' : 'L')}] on {BackgroundColour}";

        /// <summary>
        /// Sets the properties from the cell passed across.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>True if this changed one or both of the values.</returns>
        public bool CopyFrom(Cell cell)
        {
            if(cell == null) {
                throw new ArgumentNullException(nameof(cell));
            }
            var originalPackedValue = PackedValue;

            ForegroundColour = cell.Colour;
            BackgroundColour = cell.BackgroundColour;
            IsSmallFont = cell.Small;

            return PackedValue != originalPackedValue;
        }

        /// <summary>
        /// Changes the cell to match the font and colour values.
        /// </summary>
        /// <param name="cell"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void CopyTo(Cell cell)
        {
            if(cell == null) {
                throw new ArgumentNullException(nameof(cell));
            }
            cell.Colour = ForegroundColour;
            cell.BackgroundColour = BackgroundColour;
            cell.Small = IsSmallFont;
        }

        /// <summary>
        /// Copies another font and colour.
        /// </summary>
        /// <param name="other"></param>
        public void CopyFrom(DisplayBufferFontAndColour other) => PackedValue = other.PackedValue;

        /// <summary>
        /// Copies our font and colour into another.
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(DisplayBufferFontAndColour other) => other.PackedValue = PackedValue;

        /// <summary>
        /// Converts the font and colour code into the two bytes that describe the font and colour in
        /// WinWing panel display packets.
        /// </summary>
        /// <param name="isFirstCell"></param>
        /// <param name="isLastCell"></param>
        /// <returns></returns>
        public (byte,byte) ToWinWingUsbColourAndFontCode(bool isFirstCell, bool isLastCell)
        {
            // Use the lookup table based on foreground and background color ordinals
            var fgOrdinal = ForegroundColourIndex;
            var bgOrdinal = BackgroundColourIndex;
            
            // Calculate code from the table pattern
            int code = (fgOrdinal * 0x21) + (bgOrdinal * 0x03);
            
            if(IsSmallFont) {
                code += 0x16B;
            }
            if(isFirstCell) {
                code += 1;
            } else if(isLastCell) {
                code += 2;
            }
            
            return ((byte)(code & 0xff), (byte)((code & 0xff00) >> 8));
        }
    }
}
