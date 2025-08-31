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
        /// <summary>
        /// A bitmap that's the same dimensions as the device's screen.
        /// </summary>
        private Bitmap _PixelBuffer = new(580, 480, PixelFormat.Format32bppArgb);

        /// <summary>
        /// The bitmap that is shown in the picture box.
        /// </summary>
        private Bitmap _PictureBoxImage = new(580, 480, PixelFormat.Format32bppArgb);

        /// <summary>
        /// The colours associated with each display colour index.
        /// </summary>
        private Color[] _Colours = new Color[] {
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0xFF, 0xA5, 0x00),
            Color.FromArgb(0xFF, 0xFF, 0xFF),
            Color.FromArgb(0x00, 0xFF, 0xFF),
            Color.FromArgb(0x00, 0xFF, 0x3D),
            Color.FromArgb(0xFF, 0x63, 0xFF),
            Color.FromArgb(0xFF, 0x00, 0x00),
            Color.FromArgb(0xFF, 0xFF, 0x00),
            Color.FromArgb(0x61, 0x5C, 0x42),
            Color.FromArgb(0x77, 0x77, 0x77),
            Color.FromArgb(0x79, 0x73, 0x5E),
        };

        private Brush[] _ColourBrushes;

        /// <summary>
        /// The font that the display is using.
        /// </summary>
        private McduFontFile _CurrentFont = null;

        /// <summary>
        /// The font used for large font output when <see cref="CurrentFont"/> is null.
        /// </summary>
        private readonly Font _FallbackLargeFont = new Font(FontFamily.GenericMonospace, 20);

        /// <summary>
        /// The font used for small font output when <see cref="CurrentFont"/> is null.
        /// </summary>
        private readonly Font _FallbackSmallFont = new Font(FontFamily.GenericMonospace, 16);

        private const int _FallbackFontPixelWidth = 23;

        private const int _FallbackFontPixelHeight = 31;

        private DisplayBuffer _DisplayBuffer;

        private int _XOffset;

        private int _YOffset;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CduDisplayControl()
        {
            InitializeComponent();
            BuildColourBrushes();
            _PictureBox.Image = _PictureBoxImage;
        }

        private void BuildColourBrushes()
        {
            if(_ColourBrushes != null) {
                DisposeOfColourBrushes();
            }
            _ColourBrushes = new Brush[_Colours.Length];
            for(var idx = 0;idx < _Colours.Length;++idx) {
                _ColourBrushes[idx] = new SolidBrush(_Colours[idx]);
            }
        }

        private void DisposeOfColourBrushes()
        {
            foreach(var brush in _ColourBrushes) {
                brush.Dispose();
            }
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

        public void SetXYOffsets(int xOffset, int yOffset)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => SetXYOffsets(xOffset, yOffset)));
            } else {
                _XOffset = xOffset;
                _YOffset = yOffset;
                CopyDisplayBufferToPixelBuffer();
                RefreshPictureBox();
            }
        }

        private void CopyDisplayBufferToPixelBuffer()
        {
            var displayBuffer = _DisplayBuffer;
            if(displayBuffer != null) {
                using(var graphics = Graphics.FromImage(_PixelBuffer)) {
                    graphics.Clear(_Colours[0]);

                    for(var rowIdx = 0;rowIdx < displayBuffer.CountRows;++rowIdx) {
                        for(var cellIdx = 0;cellIdx < displayBuffer.CountCells;++cellIdx) {
                            var ch = displayBuffer.Characters[rowIdx, cellIdx];
                            var fontAndColour = displayBuffer.FontsAndColours[rowIdx, cellIdx];
                            DrawCharacterAt(graphics, rowIdx, cellIdx, ch, fontAndColour);
                        }
                    }
                }
            }
        }

        private void DrawCharacterAt(
            Graphics graphics,
            int rowIdx,
            int cellIdx,
            char ch,
            DisplayBufferFontAndColour fontAndColour
        )
        {
            if(_CurrentFont == null) {
                DrawCharacterUsingFallbackFontAt(graphics, rowIdx, cellIdx, ch, fontAndColour);
            } else {
                ;
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
                _ColourBrushes[fontAndColour.ForegroundColourIndex],
                _XOffset + (cellIdx * _FallbackFontPixelWidth),
                _YOffset + (rowIdx * _FallbackFontPixelHeight)
            );
        }

        private void RefreshPictureBox()
        {
            using(var graphics = Graphics.FromImage(_PictureBoxImage)) {
                graphics.DrawImageUnscaled(_PixelBuffer, Point.Empty);
            }
            _PictureBox.Refresh();
        }
    }
}
