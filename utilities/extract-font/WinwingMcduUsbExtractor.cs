// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Linq;
using System.Text;
using McduDotNet;

namespace ExtractFont
{
    /// <summary>
    /// Extracts fonts from a stream of packets that had been captured while being sent to a WINWING MCDU.
    /// </summary>
    class WinWingMcduUsbExtractor
    {
        enum Status
        {
            LookingForOpeningReport,
            LookingForFontStart,
            LookingForCommand0701Head,
            LookingForCommand0701Tail,
            LookingForBrightness,
            LookingForCommand1801,
            ReadingCodepoint,
            ReadingBitmap,
            Finished
        }

        // Font parser state
        private byte[] _CommandPrefix;
        private Status _Status;
        private Status _StatusAfter0701;
        private McduFontFile _FontFile;
        private List<McduFontGlyph> _Glyphs;
        private int _GlyphChunkIndex;
        private const int _GlyphChunkSize = 512;
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

        // Font mapper state
        private int _PacketOffset;
        private List<string> _MapPackets;
        private int[] _CodepointMap;
        private int[] _GlyphMap;
        private int _GlyphMapIndex;
        private List<McduFontGlyphOffsets> _GlyphMaps;
        private List<int> _GlyphWidthMapOffsets = [];
        private List<int> _GlyphHeightMapOffsets = [];

        /// <summary>
        /// Gets the font packet map as built during the last call to <see cref="ExtractFont"/>.
        /// </summary>
        public McduFontPacketMap FontPacketMap { get; private set; }

        /// <summary>
        /// Extracts the font glyphs from a set of USB reports wherein each array of bytes represents a single
        /// report sent to the MCDU device.
        /// </summary>
        /// <param name="commandPrefix"></param>
        /// <param name="usbReports"></param>
        /// <returns></returns>
        public McduFontFile ExtractFont(byte[] commandPrefix, IEnumerable<byte[]> usbReports)
        {
            if(commandPrefix?.Length != 2) {
                throw new InvalidOperationException($"Command prefix must be two bytes");
            }
            _CommandPrefix = commandPrefix;
            _FontFile = new();
            _Glyphs = [];

            _Status = Status.LookingForOpeningReport;
            _CountGlyphSetsRead = 0;
            _GlyphChunkIndex = 0;
            _BuildingLarge = false;

            FontPacketMap = new();
            _GlyphMaps = [];
            _MapPackets = [];
            _GlyphMap = null;
            _CodepointMap = new int[_CodepointBytes.Length];
            _GlyphHeightMapOffsets.Clear();
            _GlyphWidthMapOffsets.Clear();
            _PacketOffset = 0;

            foreach(var report in usbReports) {
                _MapPackets.Add(
                    String.Join("", report.Select(b => b.ToString("x2")))
                );

                if(report.Length > 4 && _Status != Status.Finished) {
                    _Report = report;
                    _ReadOffset = 4;
                    _ReportProcessed = false;
                    while(!_ReportProcessed) {
                        _ReportProcessed = true;

                        switch(_Status) {
                            case Status.LookingForOpeningReport:
                                LookForOpeningReport();
                                break;
                            case Status.LookingForFontStart:
                                LookForFontStart();
                                break;
                            case Status.LookingForCommand0701Head:
                                LookForCommand0701Head();
                                break;
                            case Status.LookingForCommand0701Tail:
                                LookForCommand0701Tail();
                                break;
                            case Status.LookingForBrightness:
                                LookForBrightness();
                                break;
                            case Status.LookingForCommand1801:
                                LookForCommand1801();
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

                _PacketOffset += _Report.Length;
            }

            ReplaceProbablyCommandPrefixesInMapPackets();

            FontPacketMap.Packets = [.._MapPackets];
            FontPacketMap.GlyphWidthOffsets = [.._GlyphWidthMapOffsets];
            FontPacketMap.GlyphHeightOffsets = [.._GlyphHeightMapOffsets];

            return _FontFile;
        }

        private void ReplaceProbablyCommandPrefixesInMapPackets()
        {
            // This is so dodgy... but then again the glyph bytes have been replaced with
            // underscores, other variables with letters. Even so, this is so dodgy...

            var commandPrefix = $"{_CommandPrefix[0]:x2}{_CommandPrefix[1]:x2}";
            var searchFor = $"{commandPrefix}0000";
            var replaceWith = "{CP}0000";

            for(var idx = 0;idx < _MapPackets.Count;++idx) {
                var packet = _MapPackets[idx];
                var subbed = packet.Replace(searchFor, replaceWith);
                _MapPackets[idx] = subbed;
            }
        }

        private void EstablishFontSize(int width, int height)
        {
            _FontFile.GlyphWidth = width;
            _FontFile.GlyphHeight = height;

            FontPacketMap.GlyphWidth = width;
            FontPacketMap.GlyphHeight = height;

            var rowBytes = (_FontFile.GlyphWidth / 8)
                         + (_FontFile.GlyphWidth % 8 != 0 ? 1 : 0);
            _GlyphMap = new int[rowBytes * FontPacketMap.GlyphHeight];
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
                var idx = _Report.IndexOf(_CommandPrefix[0], _CommandPrefix[1], 0x00, 0x00, 0x06, 0x01);
                if(idx != -1 && idx + 36 <= _Report.Length) {
                    const int fontIdOffset = 17;
                    const int widthOffset = 21;
                    const int heightOffset = 23;

                    var fontId = _Report[idx + fontIdOffset];
                    //_BuildingLarge = fontId == 5;
                    // On the MCDU the large font is ID 5, small font ID 6. But on the PFP-7
                    // the large font seems to be 1. So now we just assume first font is large,
                    // next is small.
                    _BuildingLarge = !_BuildingLarge;
                    _Status = Status.LookingForCommand0701Head;
                    _StatusAfter0701 = Status.ReadingCodepoint;
                    _CodepointOffset = 0;
                    _GlyphChunkIndex = 0;
                    _ReportProcessed = false;

                    ReplacePacketMapCommandPrefix(idx);

                    var wOffset = idx + widthOffset;
                    var hOffset = idx + heightOffset;
                    var width = _Report[wOffset];
                    var height = _Report[hOffset];
                    EstablishFontSize(width, height);

                    _GlyphWidthMapOffsets.Add(_PacketOffset + wOffset);
                    _GlyphHeightMapOffsets.Add(_PacketOffset + hOffset);
                    ReplacePacketMapBytes(wOffset, 'W');
                    ReplacePacketMapBytes(hOffset, 'H');
                }
            }
        }

        private void LookForCommand0701Head()
        {
            if(IsFullSize(_Report) && IsReportType(0xf0, 0x00, -1, 0x3c)) {
                var idx = _Report.IndexOf(_CommandPrefix[0], _CommandPrefix[1], 0x00, 0x00, 0x07, 0x01);
                if(idx != -1) {
                    ReplacePacketMapCommandPrefix(idx);
                    if(idx + 29 < _Report.Length) {
                        SetStatusAfter0701Read(idx + 29);
                    } else {
                        _Status = Status.LookingForCommand0701Tail;
                        _SkipStartLength = (idx + 29) - _Report.Length;
                    }
                }
            }
        }

        private void LookForCommand0701Tail()
        {
            if(IsFullSize(_Report) && IsReportType(0xf0, 0x00, -1, 0x3c)) {
                SetReadingCodepointStatus(4 + _SkipStartLength);
            }
        }

        private void SetStatusAfter0701Read(int offset)
        {
            _ReadOffset = offset;
            _GlyphChunkIndex = 0;
            _Status = _StatusAfter0701;
            _ReportProcessed = false;
        }

        private void ReadCodepoint()
        {
            if(IsGlyphDataReportType()) {
                for(;_ReadOffset < _Report.Length;++_ReadOffset) {
                    if(!IsWithinGlyphChunk()) {
                        SearchFor0701HeadAfterGlyphChunk();
                        break;
                    }

                    _CodepointBytes[_CodepointOffset] = _Report[_ReadOffset];
                    _CodepointMap[_CodepointOffset] = _PacketOffset + _ReadOffset;

                    if(++_CodepointOffset == 4) {
                        CodepointHasBeenRead();
                        break;
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
                _GlyphMapIndex = 0;
                _Status = Status.ReadingBitmap;
                _ReportProcessed = false;
                ++_ReadOffset;
            }
        }

        private void ReadBitmap()
        {
            if(IsGlyphDataReportType()) {
                for(;_ReadOffset < _Report.Length;++_ReadOffset) {
                    if(!IsWithinGlyphChunk()) {
                        SearchFor0701HeadAfterGlyphChunk();
                        break;
                    }

                    var b = _Report[_ReadOffset];
                    _GlyphMap[_GlyphMapIndex++] = _PacketOffset + _ReadOffset;
                    ReplacePacketMapBytes(_ReadOffset, '_');

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
            }
        }

        private void ReplacePacketMapCommandPrefix(int byteOffset)
        {
            if(_MapPackets.Count > 0) {
                var buffer = new StringBuilder(_MapPackets[^1]);
                var textOffset = byteOffset * 2;
                if(textOffset + 4 <= buffer.Length) {
                    buffer[textOffset] = '{';
                    buffer[textOffset + 1] = 'C';
                    buffer[textOffset + 2] = 'P';
                    buffer[textOffset + 3] = '}';
                }
                _MapPackets[^1] = buffer.ToString();
            }
        }

        private void ReplacePacketMapBytes(int byteOffset, char replaceWith)
        {
            if(_MapPackets.Count > 0) {
                var buffer = new StringBuilder(_MapPackets[^1]);
                var textOffset = byteOffset * 2;
                if(textOffset + 2 <= buffer.Length) {
                    buffer[textOffset] = replaceWith;
                    buffer[textOffset + 1] = replaceWith;
                }
                _MapPackets[^1] = buffer.ToString();
            }
        }

        private bool RowHasBeenRead()
        {
            _GlyphBitArray[_RowIndex++] = _RowBuffer.ToString();
            _RowBuffer.Clear();

            if(_RowIndex == _FontFile.GlyphHeight) {
                _Glyph.BitArray = _GlyphBitArray;
                _Glyphs.Add(_Glyph);

                var offsets = new McduFontGlyphOffsets() {
                    Character = _Glyph.Character,
                    CodepointMap = McduFontGlyphOffsets.CompressOffsetMap(_CodepointMap),
                    GlyphMap = McduFontGlyphOffsets.CompressOffsetMap(_GlyphMap),
                };
                _GlyphMaps.Add(offsets);
                _GlyphMapIndex = 0;

                SetReadingCodepointStatus(_ReadOffset + 1);
            }

            return _Status == Status.ReadingBitmap;
        }

        private void FontHasBeenRead()
        {
            if(_BuildingLarge) {
                _FontFile.LargeGlyphs = [.._Glyphs];
                FontPacketMap.LargeGlyphOffsets = [.._GlyphMaps];
            } else {
                _FontFile.SmallGlyphs = [.._Glyphs];
                FontPacketMap.SmallGlyphOffsets = [.._GlyphMaps];
            }
            _Glyphs.Clear();
            _GlyphMaps.Clear();

            _Status = ++_CountGlyphSetsRead == 1
                ? Status.LookingForFontStart
                : Status.LookingForBrightness;
        }

        private void LookForBrightness()
        {
            var idx = _Report.IndexOf(0x02, _CommandPrefix[0], _CommandPrefix[1], 0x00, 0x00, 0x03, 0x49, 0x01);
            if(idx != -1 && idx + 8 < _Report.Length) {
                var brightnessOffset = idx + 8;
                ReplacePacketMapBytes(brightnessOffset, 'L');
                FontPacketMap.DisplayBrightnessOffset = _PacketOffset + brightnessOffset;

                _Status = Status.LookingForCommand1801;
            }
        }

        private void LookForCommand1801()
        {
            const int xOffset = 17;
            const int yOffset = 19;

            if(IsFullSize(_Report)) {
                var idx = _Report.IndexOf(_CommandPrefix[0], _CommandPrefix[1], 0x00, 0x00, 0x18, 0x01);
                if(idx != -1 && idx + 24 <= _Report.Length) {
                    ReplacePacketMapCommandPrefix(idx);
                    var x = idx + xOffset;
                    var y = idx + yOffset;
                    ReplacePacketMapBytes(x, 'X');
                    ReplacePacketMapBytes(y, 'Y');
                    FontPacketMap.XOffsetOffset = _PacketOffset + x;
                    FontPacketMap.YOffsetOffset = _PacketOffset + y;
                }
            }
        }

        private void SetReadingCodepointStatus(int offset)
        {
            _ReadOffset = offset;
            _ReportProcessed = false;
            _Status = Status.ReadingCodepoint;
            _CodepointOffset = 0;
        }

        private bool IsGlyphDataReportType()
        {
            return IsFullSize(_Report)
                && (IsReportType(0xf0, 0x00, -1, 0x3c) || IsReportType(0xf0, 0x00, -1, 0x12));
        }

        private bool IsWithinGlyphChunk() => _GlyphChunkIndex++ < _GlyphChunkSize;

        private void SearchFor0701HeadAfterGlyphChunk()
        {
            _StatusAfter0701 = _Status;
            _Status = Status.LookingForCommand0701Head;
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
