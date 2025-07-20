// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ConvertFont
{
    static class FontConverter
    {
        /// <summary>
        /// Creates a font using the parameters passed across. The font must be disposed.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="fontStyle"></param>
        /// <param name="pointSize"></param>
        /// <returns></returns>
        public static Font CreateFont(FontFamily fontFamily, FontStyle fontStyle, float pointSize)
        {
            return new Font(fontFamily, pointSize, fontStyle, GraphicsUnit.Point);
        }

        /// <summary>
        /// Returns a bitma
        /// </summary>
        /// <param name="font"></param>
        /// <param name="character"></param>
        /// <param name="baseX"></param>
        /// <param name="baseY"></param>
        /// <param name="brightnessThreshold"></param>
        /// <param name="oneChar"></param>
        /// <param name="zeroChar"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string[] CreateBitmap(
            Font font,
            char character,
            int baseX,
            int baseY,
            float brightnessThreshold,
            char oneChar = 'X',
            char zeroChar = '.',
            int width = 23,
            int height = 29
        )
        {
            var result = new List<string>();

            using(var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb)) {
                using(var graphics = Graphics.FromImage(bitmap)) {
                    graphics.Clear(Color.White);
                    graphics.DrawString(
                        new String(character, 1),
                        font,
                        System.Drawing.Brushes.Black,
                        new Point(baseX, baseY)
                    );
                }

                var buffer = new StringBuilder();

                for(var row = 0;row < height;++row) {
                    for(var col = 0;col < width;++col) {
                        var pixel = bitmap.GetPixel(col, row);
                        var ch = zeroChar;
                        if(pixel.GetBrightness() < brightnessThreshold) {
                            ch = oneChar;
                        }
                        buffer.Append(ch);
                    }
                    result.Add(buffer.ToString());
                    buffer.Clear();
                }
            }

            return [..result];
        }
    }
}
