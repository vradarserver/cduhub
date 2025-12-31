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

namespace WwDevicesDotNet.WinWing
{
    /// <summary>
    /// Holds the content of an input report sent by a WinWing panel.
    /// </summary>
    class InputReport
    {
        public const int ReportCode = 1;
        public const int PacketLength = 25;
        private const int _LeftAmbientLightSensorOffset = 17;
        private const int _RightAmbientLightSensorOffset = 19;

        private readonly byte[] _Packet = new byte[PacketLength];

        public void CopyFrom(byte[] buffer, int offset, int length)
        {
            if(length > 0) {
                if(buffer[offset] != ReportCode) {
                    throw new WwDeviceException($"Unexpected report code {buffer[offset]} for a report code 1 buffer");
                }
                length = Math.Min(PacketLength, length);
                Array.ConstrainedCopy(buffer, offset, _Packet, 0, length);
                for(var idx = length;idx < _Packet.Length;++idx) {
                    _Packet[idx] = 0;
                }
            }
        }

        public void CopyFrom(InputReport other) => CopyFrom(other._Packet, 0, other._Packet.Length);

        public (UInt64, UInt64, UInt64) ToDigest()
        {
            return (
                BitConverter.ToUInt64(_Packet, 1),
                BitConverter.ToUInt64(_Packet, 9),
                BitConverter.ToUInt64(_Packet, 17)
            );
        }

        public bool IsKeyPressed(int flag, int offset) => (_Packet[offset] & flag) != 0;

        public (UInt16 LeftSensor, UInt16 RightSensor) AmbientLightSensorValues()
        {
            var leftSensorValue = ReadSensorValue(_LeftAmbientLightSensorOffset);
            var rightSensorValue = ReadSensorValue(_RightAmbientLightSensorOffset);
            return (leftSensorValue, rightSensorValue);
        }

        private UInt16 ReadSensorValue(int offset)
        {
            UInt16 result = _Packet[offset];
            result |= (UInt16)(_Packet[offset + 1] << 8);
            return result;
        }
    }
}
