// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using McduDotNet;

namespace ExtractFont
{
    /// <summary>
    /// Extracts fonts from a stream of packets that had been captured while being sent to a WINWING MCDU.
    /// </summary>
    class WinwingMcduUsbExtractor
    {
        // See notes elsewhere. Basic steps to extract font glyphs are:
        //
        // Look for an f0 00 xx 2a report as the start of font marker.
        //
        // Within that packet look for a 32bb...0601 block and read the font ID from offset 11
        // 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 10 11
        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
        // 32 bb 00 00 06 01 00 00 be 90 06 00 00 19 00 00 00 05 +24 bytes
        // 05 = large font follows
        // 06 = small font follows
        //
        // Look for a 32bb...0701 block in an fc 00 xx 3c packet and skip past it:
        // 00 01 02 03 04 05
        // -- -- -- -- -- --
        // 32 bb 00 00 07 01 + 23 bytes
        //
        // Glyphs are only in f0 00 xx 3c reports (almost...).
        //
        // Each glyph starts with a 4 byte unicode codepoint (which is ignored by the device?) and then
        // 29 triplets of three bytes. The three bytes describe the 1 bpp 23 pixel row with the lsb of
        // the 3rd byte discarded by the device.
        //
        // Each glyph follows on from the next with no padding.
        //
        // The stream of f0 00 xx 3c reports is occasionally interrupted by an f0 00 xx 12 report.
        //
        // The first byte after the f0 00 xx 12 is a part of the current glyph.
        //
        // Ignore the rest of the xx 12 report, and the subsequent f0 01 xx 00 reports. I think they might
        // be padding to give the device time to process what's been sent so far?
        //
        // After either the xx 12 or the f0 01 xx 00 reports you need to start looking for another
        // 32bb...0701 and continue reading bitmaps after that. Note that you don't always get the xx 12,
        // but you do always get the f0 01 xx 00.
        //
        // Continue until you see a codepoint of 0000. The font set is complete.
        //
        // Start looking for another 32bb...0601, but this time in a fc 00 xx 3c report.
        //
        // The following 32bb...0701 might be within the same fc 00 xx 3c report as the 32bb..0601. There
        // will not be enough room for it, in which case it will continue into the next 3c report.
        //
        // Once you see the end of the 32bb...0701 report resume processing as per above.
        //
        // Once you see the second codepoint 0000 then you are done.

        enum Status
        {
            LookingForOpeningReport,
            LookingForFontStart,
            LookingFor32BB0701Head,
            LookingFor32BB0701Tail,
            ReadingCodepoint,
            ReadingBitmap,
            Finished
        }

        private Status _Status;
        private Status _StatusAfter0701;
        private int _ExtraOffsetAfter0701;
        private McduFontFile _FontFile;
        private List<McduFontGlyph> _Glyphs;
        private bool _BuildingLarge;
        private int _SkipStartLength;
        private byte[] _CodepointBytes = new byte[4];
        private int _CodepointOffset;
        private int _ReadOffset;
        private byte[] _Report;
        private bool _ReportProcessed;
        private int _CountGlyphSetsRead;
        private McduFontGlyph _Glyph;
        private string[] _GlyphBitArray;
        private readonly StringBuilder _RowBuffer = new();
        private int _RowIndex;

        /// <summary>
        /// Extracts the font glyphs from a set of USB reports wherein each array of bytes represents a single
        /// report sent to the MCDU device.
        /// </summary>
        /// <param name="usbReports"></param>
        /// <returns></returns>
        public McduFontFile ExtractFont(IEnumerable<byte[]> usbReports)
        {
            _FontFile = new() {
                GlyphWidth = 23,
                GlyphHeight = 29,
            };
            _Glyphs = [];

            _Status = Status.LookingForOpeningReport;
            _CountGlyphSetsRead = 0;

            foreach(var report in usbReports) {
                if(report?.Length > 4) {
                    _Report = report;
                    _ReadOffset = 4;
                    _ReportProcessed = false;
                    while(!_ReportProcessed) {
                        _ReportProcessed = true;

                        if(IsReportType(0xf0, 0x01, -1, 0x00)) {
                            switch(_Status) {
                                case Status.ReadingBitmap:
                                case Status.ReadingCodepoint:
                                    _ExtraOffsetAfter0701 = 17;
                                    _StatusAfter0701 = _Status;
                                    _Status = Status.LookingFor32BB0701Head;
                                    break;
                            }
                        }

                        switch(_Status) {
                            case Status.LookingForOpeningReport:
                                LookForOpeningReport();
                                break;
                            case Status.LookingForFontStart:
                                LookForFontStart();
                                break;
                            case Status.LookingFor32BB0701Head:
                                LookFor32BB0701Head();
                                break;
                            case Status.LookingFor32BB0701Tail:
                                LookFor32BB0701Tail();
                                break;
                            case Status.ReadingCodepoint:
                                ReadCodepoint();
                                break;
                            case Status.ReadingBitmap:
                                ReadBitmap();
                                break;
                        }
                    }
                }
                if(_Status == Status.Finished) {
                    break;
                }
            }

            return _FontFile;
        }

        private void LookForOpeningReport()
        {
            if(IsFullSize(_Report) && IsReportType(0xf0, 0x00, -1, 0x2a)) {
                _Status = Status.LookingForFontStart;
                _ReportProcessed = false;
            }
        }

        private void LookForFontStart()
        {
            if(IsFullSize(_Report)) {
                var idx = _Report.IndexOf(0x32, 0xbb, 0x00, 0x00, 0x06, 0x01);
                if(idx != -1 && idx + 36 <= _Report.Length) {
                    var fontId = _Report[idx + 17];
                    _BuildingLarge = fontId == 5;
                    _Status = Status.LookingFor32BB0701Head;
                    _StatusAfter0701 = Status.ReadingCodepoint;
                    _CodepointOffset = 0;
                    _ReportProcessed = false;
                }
            }
        }

        private void LookFor32BB0701Head()
        {
            if(IsFullSize(_Report) && IsReportType(0xf0, 0x00, -1, 0x3c)) {
                var idx = _Report.IndexOf(0x32, 0xbb, 0x00, 0x00, 0x07, 0x01);
                if(idx != -1) {
                    if(idx + 29 < _Report.Length) {
                        SetStatusAfter0701Read(idx + 29);
                    } else {
                        _Status = Status.LookingFor32BB0701Tail;
                        _SkipStartLength = (idx + 29) - _Report.Length;
                    }
                }
            }
        }

        private void LookFor32BB0701Tail()
        {
            if(IsFullSize(_Report) && IsReportType(0xf0, 0x00, -1, 0x3c)) {
                SetReadingCodepointStatus(4 + _SkipStartLength);
            }
        }

        private void SetStatusAfter0701Read(int offset)
        {
            _ReadOffset = offset + _ExtraOffsetAfter0701;
            _ExtraOffsetAfter0701 = 0;
            _Status = _StatusAfter0701;
            _ReportProcessed = false;
        }

        private void ReadCodepoint()
        {
            if(IsFullSize(_Report)) {
                var isInterruption = IsInterruptionReportType();
                if(isInterruption || IsReportType(0xf0, 00, -1, 0x3c)) {
                    var length = isInterruption
                        ? 1
                        : _Report.Length - _ReadOffset;
                    for(var count = 0;count < length && _ReadOffset < _Report.Length;++_ReadOffset, ++count) {
                        _CodepointBytes[_CodepointOffset++] = _Report[_ReadOffset];
                        if(_CodepointOffset == 4) {
                            CodepointHasBeenRead();
                            break;
                        }
                    }
                    if(isInterruption) {
                        SearchFor0701HeadAfterInterruption();
                    }
                }
            }
        }

        private void CodepointHasBeenRead()
        {
            _CodepointOffset = 0;
            var codepoint = (uint)(
                   _CodepointBytes[0]
                | (_CodepointBytes[1] << 8)
                | (_CodepointBytes[2] << 16)
                | (_CodepointBytes[3] << 24)
            );
            if(codepoint == 0) {
                FontHasBeenRead();
            } else {
                var characterString = char.ConvertFromUtf32((int)codepoint);
                _Glyph = new() {
                    Character = characterString[0],
                };
                _RowIndex = 0;
                _RowBuffer.Clear();
                _GlyphBitArray = new string[_FontFile.GlyphHeight];
                _Status = Status.ReadingBitmap;
                _ReportProcessed = false;
                ++_ReadOffset;
            }
        }

        private void ReadBitmap()
        {
            if(IsFullSize(_Report)) {
                var isInterruption = IsInterruptionReportType();
                if(isInterruption || IsReportType(0xf0, 00, -1, 0x3c)) {
                    var length = isInterruption
                        ? 1
                        : _Report.Length - _ReadOffset;
                    for(var count = 0;count < length && _ReadOffset < _Report.Length;++count, ++_ReadOffset) {
                        var b = _Report[_ReadOffset];
                        for(var bit = 0x80;bit > 0;bit >>= 1) {
                            var isolated = (b & bit) != 0 ? 'X' : '.';
                            _RowBuffer.Append(isolated);
                        }
                        if(_RowBuffer.Length >= _FontFile.GlyphWidth) {
                            if(!RowHasBeenRead()) {
                                break;
                            }
                        }
                    }
                    if(isInterruption) {
                        SearchFor0701HeadAfterInterruption();
                    }
                }
            }
        }

        private bool RowHasBeenRead()
        {
            _GlyphBitArray[_RowIndex++] = _RowBuffer.ToString();
            _RowBuffer.Clear();

            if(_RowIndex == _FontFile.GlyphHeight) {
                _Glyph.BitArray = _GlyphBitArray;
                _Glyphs.Add(_Glyph);
                SetReadingCodepointStatus(_ReadOffset + 1);
            }

            return _Status == Status.ReadingBitmap;
        }

        private void FontHasBeenRead()
        {
            if(_BuildingLarge) {
                _FontFile.LargeGlyphs = [.._Glyphs];
            } else {
                _FontFile.SmallGlyphs = [.._Glyphs];
            }
            _Glyphs.Clear();

            _Status = ++_CountGlyphSetsRead == 1
                ? Status.LookingForFontStart
                : Status.Finished;
        }

        private void SetReadingCodepointStatus(int offset)
        {
            _ReadOffset = offset;
            _ReportProcessed = false;
            _Status = Status.ReadingCodepoint;
            _CodepointOffset = 0;
        }

        private bool IsInterruptionReportType() => IsReportType(0xf0, 0x00, -1, 0x12);

        private void SearchFor0701HeadAfterInterruption()
        {
            _StatusAfter0701 = _Status;
            _Status = Status.LookingFor32BB0701Head;
        }

        private static bool IsFullSize(byte[] report) => report?.Length == 64;

        private bool IsReportType(params int[] signature)
        {
            var result = _Report.Length >= signature.Length;
            for(var idx = 0;result && idx < signature.Length;++idx) {
                var expected = signature[idx];
                result = expected < 0 || expected == _Report[idx];
            }

            return result;
        }
    }
}
