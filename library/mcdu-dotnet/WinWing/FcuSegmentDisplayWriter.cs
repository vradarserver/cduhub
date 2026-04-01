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
using System.Threading;
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
        private const int _FcuPayloadLength = 16;
        private const int _EfisPayloadLength = 5;
        private const int _NullContentSends = 1;
        private const ushort _LeftEfisId = 0x0DBF;
        private const ushort _RightEfisId = 0x0EBF;
        private const ushort _FcuId = 0x10BB;

        private readonly UsbWriter _UsbWriter;
        private readonly byte[] _FcuPacket1Buffer;
        private readonly byte[] _FcuPacket2Buffer;
        private readonly byte[] _EfisBuffer;
        private readonly byte[] _NullContentBuffer;

        private readonly bool _IsLeftEfisPresent;
        private readonly byte[] _LeftEfisPayload = new byte[_EfisPayloadLength];
        private byte[]? _LeftEfisPreviousPayload;

        private readonly bool _IsRightEfisPresent;
        private readonly byte[] _RightEfisPayload = new byte[_EfisPayloadLength];
        private byte[]? _RightEfisPreviousPayload;

        private readonly byte[] _FcuPayload = new byte[_FcuPayloadLength];
        private byte[]? _FcuPreviousPayload;

        private ushort _SequenceNumber;

        public FcuSegmentDisplayWriter(
            UsbWriter usbWriter,
            bool isLeftEfisPresent,
            bool isRightEfisPresent
        )
        {
            _UsbWriter = usbWriter;
            _IsLeftEfisPresent = isLeftEfisPresent;
            _IsRightEfisPresent = isRightEfisPresent;

            _NullContentBuffer = new byte[64];
            InitialiseBuffer(_NullContentBuffer, new byte[] { 0xf0 });

            _EfisBuffer = new byte[64];
            InitialiseBuffer(_EfisBuffer, new byte[] {
                0xf0, 0x00, 0x00, 0x2B, 0xFF, 0xFF, 0x00, 0x00,
                0x02, 0x01, 0x00, 0x00, 0x11, 0x11, 0x11, 0x00,
                0x00, 0x09,
            });

            _FcuPacket1Buffer = new byte[64];
            InitialiseBuffer(_FcuPacket1Buffer, new byte[] {
                0xf0, 0x00, 0x00, 0x31, 0xFF, 0xFF, 0x00, 0x00,
                0x02, 0x01, 0x00, 0x00, 0x49, 0x56, 0x00, 0x00,
                0x00, 0x20,
            });

            _FcuPacket2Buffer = new byte[64];
            InitialiseBuffer(_FcuPacket2Buffer, new byte[] {
                0xf0, 0x00, 0x00, 0x11, 0xFF, 0xFF, 0x00, 0x00,
                0x03, 0x01, 0x00, 0x00, 0x49, 0x56,
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
                        if(PrepareEfisPayload(
                            segmentedDisplays.LeftBaro,
                            _LeftEfisPayload,
                            ref _LeftEfisPreviousPayload
                        ) || skipDuplicateCheck) {
                            PrepareAndSendEfisPacket(_LeftEfisId, _LeftEfisPayload);
                        }
                    }

                    if(_IsRightEfisPresent && segmentedDisplays.RightBaro != null) {
                        if(PrepareEfisPayload(
                            segmentedDisplays.RightBaro,
                            _RightEfisPayload,
                            ref _RightEfisPreviousPayload
                        ) || skipDuplicateCheck) {
                            PrepareAndSendEfisPacket(_RightEfisId, _RightEfisPayload);
                        }
                    }

                    if(PrepareFcuPayload(
                        segmentedDisplays.Speed,
                        segmentedDisplays.Heading,
                        segmentedDisplays.Mode,
                        segmentedDisplays.Altitude,
                        _FcuPayload,
                        ref _FcuPreviousPayload
                    ) || skipDuplicateCheck) {
                        PrepareAndSendFcuPackets(_FcuId, _FcuPayload);
                    }
                });
            }
        }

        private void PrepareAndSendEfisPacket(ushort deviceId, byte[] payload)
        {
            SetBufferSequenceNumber(_EfisBuffer, ++_SequenceNumber);
            SetBufferDeviceId(_EfisBuffer, deviceId);
            SetBufferPayload(_EfisBuffer, payload, 0x19);

            _UsbWriter.SendPacket(_EfisBuffer);

            SendNullContent(_NullContentSends);
        }

        private void PrepareAndSendFcuPackets(ushort deviceId, byte[] payload)
        {
            SetBufferSequenceNumber(_FcuPacket1Buffer, ++_SequenceNumber);
            SetBufferDeviceId(_FcuPacket1Buffer, deviceId);
            SetBufferPayload(_FcuPacket1Buffer, payload, 0x19);

            SetBufferSequenceNumber(_FcuPacket2Buffer, ++_SequenceNumber);
            SetBufferDeviceId(_FcuPacket2Buffer, deviceId);

            _UsbWriter.SendPacket(_FcuPacket1Buffer);
            _UsbWriter.SendPacket(_FcuPacket2Buffer);

            SendNullContent(_NullContentSends);
        }

        /// <summary>
        /// Sends an empty F0 command to the device. Without this, and without any
        /// sleeps, a rapid sequence of sends "backs up" on the device and further
        /// commands can be lost. Seen both SimAppPro and Mobiflight send empty
        /// packets to the device, wondering whether it's to prevent this situation?
        /// </summary>
        /// <param name="count"></param>
        private void SendNullContent(int count)
        {
            for(var idx = 0;idx < count;++idx) {
                SetBufferSequenceNumber(_NullContentBuffer, ++_SequenceNumber);
                _UsbWriter.SendPacket(_NullContentBuffer);
            }
        }

        private void SetBufferSequenceNumber(byte[] buffer, ushort sequenceNumber)
        {
            buffer[1] = (byte)((sequenceNumber & 0xff00) >> 8);
            buffer[2] = (byte)(sequenceNumber & 0xff);
        }

        private void SetBufferDeviceId(byte[] buffer, ushort deviceId)
        {
            buffer[4] = (byte)((deviceId & 0xff00) >> 8);
            buffer[5] = (byte)(deviceId & 0xff);
        }

        private void SetBufferPayload(byte[] buffer, byte[] payload, int offset)
        {
            for(var payloadIdx = 0;payloadIdx < payload.Length;++payloadIdx) {
                buffer[offset + payloadIdx] = payload[payloadIdx];
            }
        }

        private bool PrepareEfisPayload(
            FcuBaroSegmentedDisplay segmentedDisplay,
            byte[] payload,
            ref byte[]? previousPayload
        )
        {
            Array.Clear(payload, 0, payload.Length);

            Bitmapper.SetRepeatingS7DigitCollection(
                segmentedDisplay.BaroDigits,
                BaroDisplay.DigitBitmap,
                payload,
                offset: 0
            );
            Bitmapper.SetBit(segmentedDisplay.Qfe, BaroDisplay.QfeBit, payload);
            Bitmapper.SetBit(segmentedDisplay.Qnh, BaroDisplay.QnhBit, payload);

            var result = CompareWithAndCopyToPreviousPayload(payload, previousPayload);
            return result;
        }

        private bool PrepareFcuPayload(
            FcuSpeedSegmentedDisplay speed,
            FcuHeadingSegmentedDisplay heading,
            FcuModeSegmentedDisplay mode,
            FcuAltitudeSegmentedDisplay altitude,
            byte[] payload,
            ref byte[]? previousPayload
        )
        {
            Array.Clear(payload, 0, payload.Length);

            if(speed != null) {
                SetSpeedBits(payload, speed);
            }
            if(heading != null) {
                SetHeadingBits(payload, heading);
            }
            if(mode != null) {
                SetModeBits(payload, mode);
            }

            var result = CompareWithAndCopyToPreviousPayload(payload, previousPayload);
            return result;
        }

        private void SetSpeedBits(byte[] payload, FcuSpeedSegmentedDisplay speed)
        {
            Bitmapper.SetRepeatingS7DigitCollection(
                speed.SpeedDigits,
                SpeedDisplay.DigitBitmap,
                payload,
                offset: 0
            );
            Bitmapper.SetBit(speed.Dot,  SpeedDisplay.DotBit, payload);
            Bitmapper.SetBit(speed.Mach, SpeedDisplay.MachBit, payload);
            Bitmapper.SetBit(speed.Spd,  SpeedDisplay.SpdBit, payload);
        }

        private void SetHeadingBits(byte[] payload, FcuHeadingSegmentedDisplay heading)
        {
            Bitmapper.SetRepeatingS7DigitCollection(
                heading.HeadingDigits,
                HeadingDisplay.DigitBitmap,
                payload,
                offset: 3
            );
            Bitmapper.SetBit(heading.Dot, HeadingDisplay.DotBit, payload);
            Bitmapper.SetBit(heading.Lat, HeadingDisplay.LatBit, payload);
            Bitmapper.SetBit(heading.Trk, HeadingDisplay.TrkBit, payload);
            Bitmapper.SetBit(heading.Hdg, HeadingDisplay.HdgBit, payload);
        }

        private void SetModeBits(byte[] payload, FcuModeSegmentedDisplay mode)
        {
            Bitmapper.SetBit(mode.Fpa, ModeDisplay.FpaBit, payload);
            Bitmapper.SetBit(mode.Hdg, ModeDisplay.HdgBit, payload);
            Bitmapper.SetBit(mode.Trk, ModeDisplay.TrkBit, payload);
            Bitmapper.SetBit(mode.VS, ModeDisplay.VSBit, payload);
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
