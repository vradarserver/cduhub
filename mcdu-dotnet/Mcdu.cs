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
using HidSharp;

namespace McduDotNet
{
    /// <summary>
    /// The internal representation of an MCDU.
    /// </summary>
    class Mcdu : IMcdu
    {
        private HidDevice _HidDevice;
        private HidStream _HidStream;
        private readonly byte[] _DisplayPacket = new byte[64];
        private readonly char[] _DisplayCharacterBuffer = new char[1];
        private int _DisplayPacketOffset = 0;
        private string _DisplayDuplicateCheckString;

        /// <inheritdoc/>
        public ProductId ProductId { get; }

        /// <inheritdoc/>
        public Screen Screen { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="productId"></param>
        public Mcdu(HidDevice hidDevice, ProductId productId)
        {
            _HidDevice = hidDevice;
            ProductId = productId;
            Screen = new Screen();
        }

        /// <inheritdoc/>
        ~Mcdu()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_HidStream != null) {
                    _HidStream.Dispose();
                    _HidStream = null;
                }
            }
        }

        public void Initialise()
        {
            var maxOutputReportLength = _HidDevice.GetMaxOutputReportLength();
            if(maxOutputReportLength < 64) {
                throw new McduException(
                    $"HID device {_HidDevice} reported an invalid max output report length of {maxOutputReportLength}"
                );
            }
            if(!_HidDevice.TryOpen(out _HidStream)) {
                throw new McduException($"Could not open a stream to {_HidDevice}");
            }
            UseMobiFlightInitialisationSequence();
        }

        private void UseMobiFlightInitialisationSequence()
        {
            var packets = new string[] {
                "f000013832bb00001e0100005f633100000000000032bb0000180100005f6331000008000000340018000e00180032bb0000190100005f633100000e00000000",
                "f0000238000000010005000000020000000000000032bb0000190100005f633100000e000000010006000000030000000000000032bb00001901000000000000",
                "f00003385f633100000e0000000200000000ff040000000000000032bb0000190100005f633100000e000000020000a5ffff050000000000000032bb00000000",
                "f00004380000190100005f633100000e0000000200ffffffff060000000000000032bb0000190100005f633100000e0000000200ffff00ff0700000000000000",
                "f00005380000000032bb0000190100005f633100000e00000002003dff00ff080000000000000032bb0000190100005f633100000e0000000200ff6300000000",
                "f0000638ffff090000000000000032bb0000190100005f633100000e00000002000000ffff0a0000000000000032bb0000190100005f633100000e0000000000",
                "f00007380000020000ffffff0b0000000000000032bb0000190100005f633100000e0000000200425c61ff0c0000000000000032bb0000190100005f00000000",
                "f0000838633100000e0000000200777777ff0d0000000000000032bb0000190100005f633100000e00000002005e7379ff0e0000000000000032bb0000000000",
                "f000093800190100005f633100000e0000000300000000ff0f0000000000000032bb0000190100005f633100000e000000030000a5ffff100000000000000000",
                "f0000a3800000032bb0000190100005f633100000e0000000300ffffffff110000000000000032bb0000190100005f633100000e0000000300ffff0000000000",
                "f0000b38ff120000000000000032bb0000190100005f633100000e00000003003dff00ff130000000000000032bb0000190100005f633100000e000000000000",
                "f0000c38000300ff63ffff140000000000000032bb0000190100005f633100000e00000003000000ffff150000000000000032bb0000190100005f6300000000",
                "f0000d383100000e000000030000ffffff160000000000000032bb0000190100005f633100000e0000000300425c61ff170000000000000032bb000000000000",
                "f0000e38190100005f633100000e0000000300777777ff180000000000000032bb0000190100005f633100000e00000003005e7379ff19000000000000000000",
                "f0000f38000032bb0000190100005f633100000e0000000400000000001a0000000000000032bb0000190100005f633100000e00000004000100000000000000",
                "f00010381b0000000000000032bb0000190100005f633100000e0000000400020000001c0000000000000032bb00001a0100005f633100000100000000000000",
                "f00011120232bb00001c0100005f6331000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                "0232bb0000034900cc0000000000",
                "0232bb0000034901ff0000000000",
            };
            foreach(var packet in packets) {
                SendStringPacket(packet);
            }
        }

        /// <inheritdoc/>
        public void RefreshDisplay(bool skipDuplicateCheck = false)
        {
            var duplicateCheckString = Screen.BuildDuplicateCheckString();
            if(skipDuplicateCheck || _DisplayDuplicateCheckString != duplicateCheckString) {
                InitialiseDisplayPacket();
                for(var rowIdx = 0;rowIdx < Screen.Rows.Length;++rowIdx) {
                    var row = Screen.Rows[rowIdx];
                    for(var cellIdx = 0;cellIdx < row.Cells.Length;++cellIdx) {
                        var cell = row.Cells[cellIdx];
                        AddCellToDisplayPacket(
                            cell,
                            isFirstCell: rowIdx == 0 && cellIdx == 0,
                            isLastCell:  rowIdx + 1 == Screen.Rows.Length && cellIdx + 1 == row.Cells.Length
                        );
                    }
                }
                PadAndSendDisplayPacket();
                _DisplayDuplicateCheckString = duplicateCheckString;
            }
        }

        private void InitialiseDisplayPacket()
        {
            _DisplayPacket[0] = 0xF2;
            _DisplayPacketOffset = 1;
        }

        private void AddCellToDisplayPacket(Cell cell, bool isFirstCell, bool isLastCell)
        {
            _DisplayCharacterBuffer[0] = cell.Character;
            var utf8Bytes = Encoding.UTF8.GetBytes(_DisplayCharacterBuffer);
            if(utf8Bytes.Length + 2 + _DisplayPacketOffset > _DisplayPacket.Length) {
                PadAndSendDisplayPacket();
            }
            AddColourAndBytesToDisplayPacket(cell.Colour, cell.Small, isFirstCell, isLastCell);
            for(var chIdx = 0;chIdx < utf8Bytes.Length;++chIdx) {
                _DisplayPacket[_DisplayPacketOffset++] = utf8Bytes[chIdx];
            }
        }

        private void AddColourAndBytesToDisplayPacket(Colour colour, bool isSmallFont, bool isFirstCell, bool isLastCell)
        {
            (var b1, var b2) = colour.ToUsbColourAndFontCode(isSmallFont);
            if(isFirstCell) {
                b1 += 1;
            } else if(isLastCell) {
                b1 += 2;
            }
            _DisplayPacket[_DisplayPacketOffset++] = b1;
            _DisplayPacket[_DisplayPacketOffset++] = b2;
        }

        private void PadAndSendDisplayPacket()
        {
            if(_DisplayPacketOffset > 1) {
                for(var idx = _DisplayPacketOffset;idx < _DisplayPacket.Length;++idx) {
                    _DisplayPacket[idx] = 0;
                }
                SendPacket(_DisplayPacket);
                InitialiseDisplayPacket();
            }
        }

        private void SendStringPacket(string packet)
        {
            var bytes = new byte[packet.Length / 2];
            for(var idx = 0;idx < packet.Length / 2;++idx) {
                bytes[idx] = (byte)Convert.ToInt32(packet.Substring(idx * 2, 2), 16);
            }
            SendPacket(bytes);
        }

        private void SendPacket(byte[] bytes)
        {
            var stream = _HidStream;
            if(stream != null) {
                stream.Write(bytes);
            }
        }
    }
}
