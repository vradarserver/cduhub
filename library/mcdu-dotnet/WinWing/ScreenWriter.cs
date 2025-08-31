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
using System.Threading;
using System.Threading.Tasks;

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Manages the sending of an entire screen buffer to the device.
    /// </summary>
    class ScreenWriter
    {
        private UsbWriter _UsbWriter;
        private DisplayBuffer _DisplayBuffer;
        private readonly int _ProcessingPauseMilliseconds = 40;
        private readonly byte[] _DisplayPacket = new byte[64];
        private readonly char[] _DisplayCharacterBuffer = new char[1];
        private int _DisplayPacketOffset = 0;

        public Action<DisplayBuffer> UpdatingDisplayCallback { get; set; }

        public ScreenWriter(UsbWriter usbWriter)
        {
            _UsbWriter = usbWriter;
            _DisplayPacket[0] = 0xF2;
        }

        /// <summary>
        /// Sends a screen buffer to the device.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="skipDuplicateCheck"></param>
        /// <param name="suppressUpdatingDisplayCallback"></param>
        public void SendScreenToDisplay(Screen screen, bool skipDuplicateCheck, bool suppressUpdatingDisplayCallback)
        {
            _UsbWriter.LockForOutput(() => {
                if(_DisplayBuffer == null) {
                    _DisplayBuffer = new DisplayBuffer(screen.Rows.Length, screen.Rows[0].Cells.Length);
                }
                var hasChanged = _DisplayBuffer.CopyFrom(screen);
                if(skipDuplicateCheck || hasChanged) {
                    if(UpdatingDisplayCallback != null && !suppressUpdatingDisplayCallback) {
                        var clone = _DisplayBuffer.Clone();
                        Task.Run(() => UpdatingDisplayCallback?.Invoke(clone));
                    }

                    InitialiseDisplayPacket();
                    for(var rowIdx = 0;rowIdx < _DisplayBuffer.CountRows;++rowIdx) {
                        for(var cellIdx = 0;cellIdx < _DisplayBuffer.CountCells;++cellIdx) {
                            AddCellToDisplayPacket(
                                _DisplayBuffer.Characters[rowIdx, cellIdx],
                                _DisplayBuffer.FontsAndColours[rowIdx, cellIdx],
                                rowIdx == 0 && cellIdx == 0,
                                rowIdx == _DisplayBuffer.CountRows - 1 && cellIdx == _DisplayBuffer.CountCells - 1
                            );
                        }
                    }
                    PadAndSendDisplayPacket();

                    if(_ProcessingPauseMilliseconds > 0) {
                        // If we send packets too quickly then the device can freak out.
                        // This forces a pause so that a set of very fast updates won't
                        // corrupt the display.
                        Thread.Sleep(_ProcessingPauseMilliseconds);
                    }
                }
            });
        }

        private void InitialiseDisplayPacket()
        {
            _DisplayPacketOffset = 1;
        }

        private void AddCellToDisplayPacket(
            char character,
            DisplayBufferFontAndColour fontAndColour,
            bool isFirstCell,
            bool isLastCell
        )
        {
            _DisplayCharacterBuffer[0] = character;
            var utf8Bytes = Encoding.UTF8.GetBytes(_DisplayCharacterBuffer);
            AddColourIndexAndBytesToDisplayPacket(
                fontAndColour,
                isFirstCell,
                isLastCell
            );
            for(var chIdx = 0;chIdx < utf8Bytes.Length;++chIdx) {
                AddToDisplayPacketSendWhenFull(utf8Bytes[chIdx]);
            }
        }

        private void AddColourIndexAndBytesToDisplayPacket(
            DisplayBufferFontAndColour fontAndColour,
            bool isFirstCell,
            bool isLastCell
        )
        {
            (var b1, var b2) = fontAndColour.ToWinWingUsbColourAndFontCode(isFirstCell, isLastCell);
            AddToDisplayPacketSendWhenFull(b1);
            AddToDisplayPacketSendWhenFull(b2);
        }

        private void AddToDisplayPacketSendWhenFull(byte value)
        {
            _DisplayPacket[_DisplayPacketOffset++] = value;
            if(_DisplayPacketOffset == _DisplayPacket.Length) {
                _UsbWriter.SendPacket(_DisplayPacket);
                InitialiseDisplayPacket();
            }
        }

        private void PadAndSendDisplayPacket()
        {
            if(_DisplayPacketOffset > 1) {
                for(var idx = _DisplayPacketOffset;idx < _DisplayPacket.Length;++idx) {
                    _DisplayPacket[idx] = 0;
                }
                _UsbWriter.SendPacket(_DisplayPacket);
                InitialiseDisplayPacket();
            }
        }
    }
}
