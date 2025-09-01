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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using McduDotNet;

namespace Cduhub.WindowsGui
{
    /// <summary>
    /// A control that mirrors the content of an <see cref="ICdu"/> display.
    /// </summary>
    public partial class CduDisplayControl : UserControl
    {
        private const int _BytesPerPixel = 4;
        private const PixelFormat _PixelFormat = PixelFormat.Format32bppArgb;
        private const int _FallbackPixelBufferPixelWidth = 580;
        private const int _FallbackPixelBufferPixelHeight = 480;

        /// <summary>
        /// A bitmap that's the same dimensions as the device's screen.
        /// </summary>
        private Bitmap _PixelBuffer = new(_FallbackPixelBufferPixelWidth, _FallbackPixelBufferPixelHeight, _PixelFormat);

        /// <summary>
        /// The bitmap that is shown in the picture box.
        /// </summary>
        private Bitmap _PictureBoxImage = new(_FallbackPixelBufferPixelWidth, _FallbackPixelBufferPixelHeight, _PixelFormat);

        /// <summary>
        /// The colours associated with each display colour index.
        /// </summary>
        private Color[] _Colours;

        /// <summary>
        /// Exactly the same colours as per <see cref="_Colours"/> except in brush form.
        /// </summary>
        private Brush[] _ColourBrushes;

        /// <summary>
        /// The font used for large font output when <see cref="CurrentFont"/> is null.
        /// </summary>
        private readonly Font _FallbackLargeFont = new Font(FontFamily.GenericMonospace, 20);

        /// <summary>
        /// The font used for small font output when <see cref="CurrentFont"/> is null.
        /// </summary>
        private readonly Font _FallbackSmallFont = new Font(FontFamily.GenericMonospace, 16);

        /// <summary>
        /// The brush used when no palettes have been established.
        /// </summary>
        private readonly SolidBrush _FallbackColourBrush = new SolidBrush(Color.White);

        private const int _FallbackFontPixelWidth = 23;

        private const int _FallbackFontPixelHeight = 31;

        private DisplayColour _FallbackDisplayColour = new DisplayColour() { PackedValue = 0xffffffff, };

        private DisplayBuffer _DisplayBuffer;

        private DisplayFont _DisplayFont;

        private DisplayPalette _DisplayPalette;

        private int _XOffset = 5;

        private int _YOffset = 5;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CduDisplayControl()
        {
            InitializeComponent();
            _PictureBox.Image = _PictureBoxImage;
        }

        private void DisposeOfColourBrushes()
        {
            if(_ColourBrushes != null) {
                foreach(var brush in _ColourBrushes) {
                    brush.Dispose();
                }
            }
            _Colours = null;
            _ColourBrushes = null;
        }

        /// <summary>
        /// Copies the content of the display buffer into the control.
        /// </summary>
        /// <param name="displayBuffer"></param>
        public void CopyFromDisplayBuffer(DisplayBuffer displayBuffer)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => CopyFromDisplayBuffer(displayBuffer)));
            } else {
                _DisplayBuffer = displayBuffer;
                CopyDisplayBufferToPixelBuffer();
                RefreshPictureBox();
            }
        }

        /// <summary>
        /// Copies the font (and optionally the X and Y offsets uploaded with the font) into the control.
        /// </summary>
        /// <param name="displayFont"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        public void CopyFromDisplayFont(DisplayFont displayFont, int xOffset = int.MinValue, int yOffset = int.MinValue)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => CopyFromDisplayFont(displayFont, xOffset, yOffset)));
            } else {
                _XOffset = xOffset == int.MinValue ? _XOffset : xOffset - 0x24;
                _YOffset = yOffset == int.MinValue ? _YOffset : yOffset - 0x17;

                _DisplayFont = displayFont;

                if(_PixelBuffer != null) {
                    _PixelBuffer.Dispose();
                    _PixelBuffer = null;
                }

                if(_DisplayBuffer != null) {
                    CopyDisplayBufferToPixelBuffer();
                    RefreshPictureBox();
                }
            }
        }

        /// <summary>
        /// Copies the palette into the control.
        /// </summary>
        /// <param name="displayPalette"></param>
        public void CopyFromDisplayPalette(DisplayPalette displayPalette)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => CopyFromDisplayPalette(displayPalette)));
            } else {
                _DisplayPalette = displayPalette;
                CopyDisplayPaletteToControl();
                if(_DisplayBuffer != null) {
                    CopyDisplayBufferToPixelBuffer();
                    RefreshPictureBox();
                }
            }
        }

        private void CopyDisplayPaletteToControl()
        {
            DisposeOfColourBrushes();
            if(_DisplayPalette != null) {
                _Colours = new Color[_DisplayPalette.CountColours];
                _ColourBrushes = new Brush[_DisplayPalette.CountColours];
                for(var idx = 0;idx < _DisplayPalette.CountColours;++idx) {
                    var displayColour = _DisplayPalette.Colours[idx];
                    _Colours[idx] = Color.FromArgb(
                        displayColour.A,
                        displayColour.R,
                        displayColour.G,
                        displayColour.B
                    );
                    _ColourBrushes[idx] = new SolidBrush(_Colours[idx]);
                }
            }
        }

        private void CopyDisplayBufferToPixelBuffer()
        {
            var displayBuffer = _DisplayBuffer;
            var displayFont = _DisplayFont;

            if(displayBuffer != null) {
                if(_PixelBuffer == null) {
                    CreatePixelBuffer(displayBuffer, displayFont);
                }
                using(var graphics = Graphics.FromImage(_PixelBuffer)) {
                    graphics.Clear(_Colours == null
                        ? Color.Black
                        : _Colours[0]
                    );

                    if(displayFont == null) {
                        DrawCharactersWithFallbackFont(graphics, displayBuffer);
                    }
                }

                if(displayFont != null) {
                    DrawCharactersWithDisplayFont(displayFont, displayBuffer);
                }
            }
        }

        private void CreatePixelBuffer(DisplayBuffer displayBuffer, DisplayFont displayFont)
        {
            var width = displayBuffer == null || displayFont == null
                ? _FallbackFontPixelWidth
                : (_XOffset * 2) + (displayBuffer.CountCells * displayFont.PixelWidth);
            var height = displayBuffer == null || displayFont == null
                ? _FallbackFontPixelHeight
                : (_YOffset * 2) + (displayBuffer.CountRows * displayFont.PixelHeight);

            var pictureBoxBuffer = new Bitmap(width, height, _PixelFormat);
            using(var graphic = Graphics.FromImage(pictureBoxBuffer)) {
                graphic.Clear(Color.Black);
            }
            _PictureBox.Image = pictureBoxBuffer;
            _PictureBoxImage.Dispose();
            _PictureBoxImage = pictureBoxBuffer;

            _PixelBuffer = new Bitmap(width, height, _PixelFormat);
        }

        private void DrawCharactersWithFallbackFont(Graphics graphics, DisplayBuffer displayBuffer)
        {
            for(var rowIdx = 0;rowIdx < displayBuffer.CountRows;++rowIdx) {
                for(var cellIdx = 0;cellIdx < displayBuffer.CountCells;++cellIdx) {
                    var ch = displayBuffer.Characters[rowIdx, cellIdx];
                    var fontAndColour = displayBuffer.FontsAndColours[rowIdx, cellIdx];
                    DrawCharacterUsingFallbackFontAt(graphics, rowIdx, cellIdx, ch, fontAndColour);
                }
            }
        }

        private StringBuilder _FallbackFontDrawTextBuffer;

        private void DrawCharacterUsingFallbackFontAt(
            Graphics graphics,
            int rowIdx,
            int cellIdx,
            char ch,
            DisplayBufferFontAndColour fontAndColour
        )
        {
            if(_FallbackFontDrawTextBuffer != null) {
                _FallbackFontDrawTextBuffer[0] = ch;
            } else {
                _FallbackFontDrawTextBuffer = new StringBuilder();
                _FallbackFontDrawTextBuffer.Append(ch);
            }
            var text = _FallbackFontDrawTextBuffer.ToString();
            graphics.DrawString(
                text,
                fontAndColour.IsSmallFont ? _FallbackSmallFont : _FallbackLargeFont,
                _ColourBrushes == null
                    ? _FallbackColourBrush
                    : _ColourBrushes[fontAndColour.ForegroundColourIndex],
                _XOffset + (cellIdx * _FallbackFontPixelWidth),
                _YOffset + (rowIdx * _FallbackFontPixelHeight)
            );
        }

        private void DrawCharactersWithDisplayFont(DisplayFont displayFont, DisplayBuffer displayBuffer)
        {
            unsafe {
                var bitmapData = _PixelBuffer.LockBits(
                    new Rectangle(0, 0, _PixelBuffer.Width, _PixelBuffer.Height),
                    ImageLockMode.ReadWrite,
                    _PixelBuffer.PixelFormat
                );
                try {
                    for(var rowIdx = 0;rowIdx < displayBuffer.CountRows;++rowIdx) {
                        for(var cellIdx = 0;cellIdx < displayBuffer.CountCells;++cellIdx) {
                            var ch = displayBuffer.Characters[rowIdx, cellIdx];
                            var fontAndColour = displayBuffer.FontsAndColours[rowIdx, cellIdx];
                            DrawCharacterUsingDisplayFontAt(bitmapData, displayFont, rowIdx, cellIdx, ch, fontAndColour);
                        }
                    }
                } finally {
                    _PixelBuffer.UnlockBits(bitmapData);
                }
            }
        }

        private unsafe void DrawCharacterUsingDisplayFontAt(
            BitmapData bitmapData,
            DisplayFont displayFont,
            int screenRowIdx,
            int screenCellIdx,
            char ch,
            DisplayBufferFontAndColour fontAndColour
        )
        {
            var scanWidth = bitmapData.Stride;
            var topLeftAddress = (byte*)bitmapData.Scan0;
            var bottomRightAddress = (byte*)bitmapData.Scan0 + (scanWidth * bitmapData.Height);
            var colourIdx = fontAndColour.ForegroundColourIndex;
            var colour = colourIdx < _DisplayPalette?.CountColours
                ? _DisplayPalette.Colours[colourIdx]
                : _FallbackDisplayColour;

            var glyphs = fontAndColour.IsSmallFont
                ? displayFont.SmallGlyphs
                : displayFont.LargeGlyphs;
            if(glyphs.TryGetValue(ch, out var glyphBitmap)) {
                for(var glyphRowIdx = 0;glyphRowIdx < glyphBitmap.GetLength(0);++glyphRowIdx) {
                    var glyphBitCount = 0;
                    for(var glyphByteIdx = 0;glyphByteIdx < glyphBitmap.GetLength(1);++glyphByteIdx) {
                        var glyphByte = glyphBitmap[glyphRowIdx, glyphByteIdx];
                        for(var byteBit = 0x80;glyphBitCount < displayFont.PixelWidth && byteBit != 0;byteBit >>= 1, ++glyphBitCount) {
                            var pixelLit = (glyphByte & byteBit) == byteBit;
                            if(pixelLit) {
                                var pixelAddress = topLeftAddress;
                                pixelAddress += scanWidth * _YOffset;
                                pixelAddress += scanWidth * (screenRowIdx * displayFont.PixelHeight);
                                pixelAddress += scanWidth * glyphRowIdx;
                                pixelAddress += (_XOffset + glyphBitCount) * _BytesPerPixel;
                                pixelAddress += screenCellIdx * displayFont.PixelWidth * _BytesPerPixel;
                                if(pixelAddress >= topLeftAddress && pixelAddress <= bottomRightAddress - _BytesPerPixel) {
                                    pixelAddress[0] = colour.B;
                                    pixelAddress[1] = colour.G;
                                    pixelAddress[2] = colour.R;
                                    pixelAddress[3] = colour.A;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RefreshPictureBox()
        {
            using(var graphics = Graphics.FromImage(_PictureBoxImage)) {
                graphics.DrawImageUnscaled(_PixelBuffer, Point.Empty);
            }
            _PictureBox.Refresh();
        }

        private void CopyToClipboard()
        {
            try {
                using(var pictureBoxView = new Bitmap(_PictureBox.Width, _PictureBox.Height)) {
                    _PictureBox.DrawToBitmap(
                        pictureBoxView,
                        new Rectangle(0, 0, pictureBoxView.Width, pictureBoxView.Height)
                    );
                    Clipboard.SetImage(pictureBoxView);
                }
            } catch {
                ;
            }
        }

        private void ContextMenuItem_CopyToClipoard_Clicked(object sender, EventArgs e)
        {
            CopyToClipboard();
        }
    }
}
