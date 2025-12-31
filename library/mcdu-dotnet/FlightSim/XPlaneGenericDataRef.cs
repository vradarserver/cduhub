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

namespace WwDevicesDotNet.FlightSim.SimBridgeMcdu
{
    /// <summary>
    /// Utility methods that can help when translating between the X-Plane data refs world and the MCDU-DOTNET
    /// world.
    /// </summary>
    public static class XPlaneGenericDataRef
    {
        public static void ParseMime64DisplayLineIntoRow(Screen screen, string displayLine, int rowNumber)
        {
            if(!String.IsNullOrEmpty(displayLine)) {
                var lineBytes = Convert.FromBase64String(displayLine);
                ParseDisplayLineBytesIntoRow(screen, lineBytes, rowNumber);
            }
        }

        public static void ParseDisplayLineBytesIntoRow(Screen screen, byte[] lineBytes, int rowNumber)
        {
            if(screen != null && lineBytes?.Length > 0 && rowNumber >= 0 && rowNumber < screen.Rows.Length) {
                var row = screen.Rows[rowNumber];
                var text = Encoding.UTF8.GetString(lineBytes);
                var colLimit = Math.Min(row.Cells.Length, text.Length);
                for(var idx = 0;idx < colLimit;++idx) {
                    row.Cells[idx].Character = text[idx];
                }
            }
        }

        public static void ParseDisplayCellValueIntoRow(Screen screen, int value, int rowNumber, int colNumber)
        {
            if(screen != null && rowNumber >= 0 && rowNumber < Metrics.Lines && colNumber >= 0 && colNumber < Metrics.Columns) {
                screen
                    .Rows[rowNumber]
                    .Cells[colNumber]
                    .Character = (char)value;
            }
        }

        public static void ParseMime64StyleLineIntoRow(Screen screen, string styleLine, int rowNumber)
        {
            if(!String.IsNullOrEmpty(styleLine)) {
                var styleBytes = Convert.FromBase64String(styleLine);
                ParseStyleLineBytesIntoRow(screen, styleBytes, rowNumber);
            }
        }

        public static void ParseStyleLineBytesIntoRow(Screen screen, byte[] styleBytes, int rowNumber)
        {
            if(screen != null && styleBytes?.Length > 0 && rowNumber >= 0 && rowNumber < screen.Rows.Length) {
                var row = screen.Rows[rowNumber];
                var colLimit = Math.Min(row.Cells.Length, styleBytes.Length);
                for(var idx = 0;idx < colLimit;++idx) {
                    var styleByte = styleBytes[idx];
                    var cell = row.Cells[idx];
                    SetCellStyle(cell, styleByte);
                }
            }
        }

        public static void ParseStyleCellValueIntoRow(Screen screen, int value, int rowNumber, int colNumber)
        {
            if(screen != null && rowNumber >= 0 && rowNumber < Metrics.Lines && colNumber >= 0 && colNumber < Metrics.Columns) {
                if(value >= 0 && value < 256) {
                    var styleByte = (byte)value;
                    var cell = screen.Rows[rowNumber].Cells[colNumber];
                    SetCellStyle(cell, styleByte);
                }
            }
        }

        private static void SetCellStyle(Cell cell, byte styleByte)
        {
            cell.Small = (styleByte & 0x80) == 0;
            var xplaneColour = (styleByte & 0x0f);
            switch(xplaneColour) {
                case 0: break;      // <-- black - XPlane supports inverted text, WinWings doesn't (AFAIK)
                case 1: cell.Colour = Colour.Cyan; break;
                case 2: cell.Colour = Colour.Red; break;
                case 3: cell.Colour = Colour.Yellow; break;
                case 4: cell.Colour = Colour.Green; break;
                case 5: cell.Colour = Colour.Magenta; break;
                case 6: cell.Colour = Colour.Amber; break;
                case 7: cell.Colour = Colour.White; break;
            }
        }
    }
}
