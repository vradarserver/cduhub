// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using McduDotNet;

namespace Cduhub.DesktopGui.Controls
{
    /// <summary>
    /// A control that mirrors the content of an <see cref="ICdu"/> display.
    /// </summary>
    public class CduDisplayControl : Control
    {
        private const int _BytesPerPixel = 4;

        private static readonly DisplayColour _FallbackDisplayColour = new() { PackedValue = 0xffffffff, };

        /// <summary>
        /// The bitmap that the display buffer is composed into. It is sized to the
        /// device's screen and recreated whenever the font or geometry changes.
        /// </summary>
        private WriteableBitmap? _ComposedBitmap;

        /// <summary>
        /// Set when the next compose must recreate <see cref="_ComposedBitmap"/> because
        /// the font (and therefore the screen geometry) has changed.
        /// </summary>
        private bool _BitmapGeometryStale;

        private DisplayBuffer? _DisplayBuffer;

        private DisplayFont? _DisplayFont;

        private DisplayPalette? _DisplayPalette;

        private int _XOffset = 5;

        private int _YOffset = 5;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CduDisplayControl()
        {
            RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
        }

        /// <summary>
        /// Copies the content of the display buffer into the control.
        /// </summary>
        /// <param name="displayBuffer"></param>
        public void CopyFromDisplayBuffer(DisplayBuffer? displayBuffer)
        {
            OnUIThread(() => {
                _DisplayBuffer = displayBuffer;
                RecomposeAndInvalidate();
            });
        }

        /// <summary>
        /// Copies the font (and optionally the X and Y offsets uploaded with the font)
        /// into the control.
        /// </summary>
        /// <param name="displayFont"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        public void CopyFromDisplayFont(DisplayFont? displayFont, int xOffset = int.MinValue, int yOffset = int.MinValue)
        {
            OnUIThread(() => {
                _XOffset = xOffset == int.MinValue ? _XOffset : xOffset - 0x24;
                _YOffset = yOffset == int.MinValue ? _YOffset : yOffset - 0x17;
                _DisplayFont = displayFont;
                _BitmapGeometryStale = true;
                RecomposeAndInvalidate();
            });
        }

        /// <summary>
        /// Copies the palette into the control.
        /// </summary>
        /// <param name="displayPalette"></param>
        public void CopyFromDisplayPalette(DisplayPalette? displayPalette)
        {
            OnUIThread(() => {
                _DisplayPalette = displayPalette;
                RecomposeAndInvalidate();
            });
        }

        private static void OnUIThread(Action action)
        {
            if(Dispatcher.UIThread.CheckAccess()) {
                action();
            } else {
                Dispatcher.UIThread.Post(action);
            }
        }

        private void RecomposeAndInvalidate()
        {
            RecomposeBitmap();
            InvalidateVisual();
        }

        private void RecomposeBitmap()
        {
            var displayBuffer = _DisplayBuffer;
            var displayFont = _DisplayFont;

            var width = displayBuffer != null && displayFont != null
                ? (_XOffset * 2) + (displayBuffer.CountCells * displayFont.PixelWidth)
                : 0;
            var height = displayBuffer != null && displayFont != null
                ? (_YOffset * 2) + (displayBuffer.CountRows * displayFont.PixelHeight)
                : 0;

            if(displayBuffer == null || displayFont == null || width < 1 || height < 1) {
                _ComposedBitmap?.Dispose();
                _ComposedBitmap = null;
            } else {
                var pixelSize = new PixelSize(width, height);
                if(_ComposedBitmap == null || _BitmapGeometryStale || _ComposedBitmap.PixelSize != pixelSize) {
                    _ComposedBitmap?.Dispose();
                    _ComposedBitmap = new WriteableBitmap(
                        pixelSize,
                        new Vector(96, 96),
                        PixelFormat.Bgra8888,
                        AlphaFormat.Opaque
                    );
                }
                _BitmapGeometryStale = false;

                using(var frameBuffer = _ComposedBitmap.Lock()) {
                    unsafe {
                        var basePtr = (byte*)frameBuffer.Address;
                        var rowBytes = frameBuffer.RowBytes;

                        ClearBuffer(basePtr, rowBytes, width, height);

                        for(var rowIdx = 0;rowIdx < displayBuffer.CountRows;++rowIdx) {
                            for(var cellIdx = 0;cellIdx < displayBuffer.CountCells;++cellIdx) {
                                var ch = displayBuffer.Characters[rowIdx, cellIdx];
                                var fontAndColour = displayBuffer.FontsAndColours[rowIdx, cellIdx];
                                DrawCharacterUsingDisplayFontAt(
                                    basePtr,
                                    rowBytes,
                                    height,
                                    displayFont,
                                    rowIdx,
                                    cellIdx,
                                    ch,
                                    fontAndColour
                                );
                            }
                        }
                    }
                }
            }
        }

        private unsafe void ClearBuffer(byte* basePtr, int rowBytes, int width, int height)
        {
            var background = _DisplayPalette != null && _DisplayPalette.CountColours > 0
                ? _DisplayPalette.Colours[0]
                : new DisplayColour { PackedValue = 0x000000ff, };
            var packed = (uint)(
                  (background.A << 24)
                | (background.R << 16)
                | (background.G << 8)
                | background.B
            );

            for(var y = 0;y < height;++y) {
                var rowPtr = (uint*)(basePtr + (y * rowBytes));
                for(var x = 0;x < width;++x) {
                    rowPtr[x] = packed;
                }
            }
        }

        private unsafe void DrawCharacterUsingDisplayFontAt(
            byte* basePtr,
            int rowBytes,
            int bufferHeight,
            DisplayFont displayFont,
            int screenRowIdx,
            int screenCellIdx,
            char ch,
            DisplayBufferFontAndColour fontAndColour
        )
        {
            var topLeftAddress = basePtr;
            var bottomRightAddress = basePtr + (rowBytes * bufferHeight);
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
                                pixelAddress += rowBytes * _YOffset;
                                pixelAddress += rowBytes * (screenRowIdx * displayFont.PixelHeight);
                                pixelAddress += rowBytes * glyphRowIdx;
                                pixelAddress += (_XOffset + glyphBitCount) * _BytesPerPixel;
                                pixelAddress += screenCellIdx * displayFont.PixelWidth * _BytesPerPixel;
                                if(pixelAddress >= topLeftAddress && pixelAddress <= bottomRightAddress - _BytesPerPixel) {
                                    pixelAddress[0] = colour.B;
                                    pixelAddress[1] = colour.G;
                                    pixelAddress[2] = colour.R;
                                    pixelAddress[3] = 0xff;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(Bounds.Size);
            context.FillRectangle(Brushes.Black, bounds);

            var bitmap = _ComposedBitmap;
            if(bitmap != null && bounds.Width > 0 && bounds.Height > 0) {
                var source = new Rect(bitmap.Size);
                if(source.Width > 0 && source.Height > 0) {
                    var scale = Math.Min(bounds.Width / source.Width, bounds.Height / source.Height);
                    if(scale > 0 && !double.IsInfinity(scale) && !double.IsNaN(scale)) {
                        var destWidth = source.Width * scale;
                        var destHeight = source.Height * scale;
                        var destination = new Rect(
                            (bounds.Width - destWidth) / 2,
                            (bounds.Height - destHeight) / 2,
                            destWidth,
                            destHeight
                        );

                        context.DrawImage(bitmap, source, destination);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _ComposedBitmap?.Dispose();
            _ComposedBitmap = null;
        }
    }
}
