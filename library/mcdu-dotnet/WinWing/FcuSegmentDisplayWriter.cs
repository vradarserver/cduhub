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
using System.Linq;
using McduDotNet.WinWing.FcuAndEfis;

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Sends commands to a WinWing FCU device to set segments in the segmented display.
    /// </summary>
    /// <remarks>
    /// As of time of writing I only have an FCU, I don't have the MCP. When / if I get
    /// hold of an MCP this might be rejiggered and renamed to deal with both devices, but
    /// for now it hard codes IDs for the FCU and will only work with the FCU.
    /// </remarks>
    class FcuSegmentDisplayWriter
    {
        private const int _EfisPayloadLength = 5;
        private const ushort _LeftEfisId = 0x0DBF;
        private const ushort _RightEfisId = 0x0EBF;
        private const ushort _FcuId = 0x10BB;

        private readonly UsbWriter _UsbWriter;
        private readonly byte[] _EfisBuffer;
        private readonly bool _IsLeftEfisPresent;
        private readonly byte[] _LeftEfisPayload = new byte[_EfisPayloadLength];
        private byte[]? _LeftEfisPreviousPayload;

        private ushort _EfisSequence;

        public FcuSegmentDisplayWriter(
            UsbWriter usbWriter,
            bool isLeftEfisPresent
        )
        {
            _UsbWriter = usbWriter;
            _IsLeftEfisPresent = isLeftEfisPresent;

            _EfisBuffer = new byte[64];
            InitialiseBuffer(_EfisBuffer, new byte[] {
                0xf0, 0x00, 0x00, 0x2B, 0xFF, 0xFF, 0x00, 0x00,
                0x02, 0x01, 0x00, 0x00, 0x11, 0x11, 0x11, 0x00,
                0x00, 0x09
            });
        }

        private void InitialiseBuffer(byte[] buffer, byte[] prefill)
        {
            if(prefill.Length > buffer.Length) {
                throw new InvalidOperationException(
                    $"Cannot fit {prefill.Length} bytes into a buffer of length {buffer.Length}"
                );
            }

            for(var idx = 0;idx < prefill.Length;++idx) {
                buffer[idx] = prefill[idx];
            }
        }

        public void SendSegmentedDisplays(
            FcuSegmentedDisplays? segmentedDisplays,
            bool skipDuplicateCheck
        )
        {
            if(segmentedDisplays != null) {
                _UsbWriter.LockForOutput(() => {
                    if(_IsLeftEfisPresent && segmentedDisplays.LeftBaro != null) {
                        if(PrepareEfisBuffer(
                            segmentedDisplays.LeftBaro,
                            _LeftEfisPayload,
                            ref _LeftEfisPreviousPayload
                        ) || skipDuplicateCheck) {
                            PrepareAndSendEfisBuffer(_LeftEfisId, _LeftEfisPayload);
                        }
                    }
                });
            }
        }

        private void PrepareAndSendEfisBuffer(ushort efisId, byte[] payload)
        {
            ++_EfisSequence;
            _EfisBuffer[1] = (byte)((_EfisSequence & 0xff00) >> 8);
            _EfisBuffer[2] = (byte)(_EfisSequence & 0xff);
            _EfisBuffer[4] = (byte)((efisId & 0xff00) >> 8);
            _EfisBuffer[5] = (byte)(efisId & 0xff);
            for(var payloadIdx = 0;payloadIdx < payload.Length;++payloadIdx) {
                _EfisBuffer[0x19 + payloadIdx] = payload[payloadIdx];
            }

            _UsbWriter.SendPacket(_EfisBuffer);
        }

        private bool PrepareEfisBuffer(
            FcuBaroSegmentedDisplay segmentedDisplay,
            byte[] payloadBuffer,
            ref byte[]? previousPayloadBuffer
        )
        {
            Array.Clear(payloadBuffer, 0, payloadBuffer.Length);
            for(var idx = 0;idx < segmentedDisplay.BaroDigits.Count;++idx) {
                var s7 = segmentedDisplay.BaroDigits[idx];
                Bitmapper.SetS7Bits(
                    s7.Segments,
                    BaroDisplay.DigitBitmap,
                    payloadBuffer,
                    offset: idx
                );
            }
            Bitmapper.SetBit(segmentedDisplay.Qfe, BaroDisplay.QfeBit, payloadBuffer);
            Bitmapper.SetBit(segmentedDisplay.Qnh, BaroDisplay.QnhBit, payloadBuffer);

            var result = CompareWithAndCopyToPreviousPayload(payloadBuffer, previousPayloadBuffer);
            return result;
        }

        private bool CompareWithAndCopyToPreviousPayload(byte[] payload, byte[]? previousPayload)
        {
            var result = previousPayload == null || !payload.SequenceEqual(previousPayload);
            if(previousPayload == null) {
                previousPayload = new byte[payload.Length];
            }
            if(result) {
                payload.CopyTo(previousPayload, 0);
            }

            return result;
        }
    }
}
