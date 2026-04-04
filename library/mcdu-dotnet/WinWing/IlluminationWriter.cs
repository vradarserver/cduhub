// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Handles the setting of LEDs and backlight brightnesses on WinWing panels.
    /// </summary>
    class IlluminationWriter
    {
        private readonly UsbWriter _UsbWriter;
        private BinaryLampMap? _PreviousLamps;

        /// <summary>
        /// The 02 report that controls LED on/off and display brightnesses.
        /// </summary>
        private readonly byte[] _IlluminationPacket = new byte[] {
            0x02, 0xFF, 0xFF, 0x00, 0x00, 0x03, 0x49,   // <-- 0xFF replaced with device ID in ctor
            0x00, 0x00,                                 // <-- these two change during send calls
            0x00, 0x00, 0x00, 0x00, 0x00
        };
        private const int _IlluminationPacketTypeIndicatorOffset = 7;

        public IlluminationWriter(
            UsbWriter usbWriter,
            ushort winwingDeviceId
        )
        {
            _IlluminationPacket[1] = (byte)((winwingDeviceId & 0xff00) >> 8);
            _IlluminationPacket[2] = (byte)(winwingDeviceId & 0x00ff);
            _UsbWriter = usbWriter;
        }

        /// <summary>
        /// Sets a backlight or LED brightness intensity.
        /// </summary>
        /// <param name="adjustableElementId">
        /// The code for the device-specific element with an adjustable intensity.
        /// </param>
        /// <param name="percent"></param>
        public void SetIntensity(byte adjustableElementId, int percent)
        {
            var byteValue = Percent.ToByte(percent);
            SendIlluminationSettingPacket(adjustableElementId, byteValue);
        }

        /// <summary>
        /// Turns lamps on and off in accordance with the map passed across.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="skipDuplicateCheck"></param>
        public void SetLamps(BinaryLampMap map, bool skipDuplicateCheck)
        {
            _UsbWriter.LockForOutput(() => {
                if(skipDuplicateCheck || !(_PreviousLamps?.Equals(map) ?? false)) {
                    for(var idx = 0;idx < map.BinaryLamps.Count;++idx) {
                        var lamp = map.BinaryLamps[idx];
                        bool? previous = _PreviousLamps == null || _PreviousLamps.BinaryLamps.Count <= idx
                            ? null
                            : _PreviousLamps.BinaryLamps[idx].On;
                        SendLight(
                            previous,
                            lamp.On,
                            lamp.LedId
                        );
                    }

                    if(_PreviousLamps == null) {
                        _PreviousLamps = new BinaryLampMap(map);
                    } else {
                        _PreviousLamps.CopyFrom(map);
                    }
                }
            });
        }

        private void SendLight(bool? previous, bool current, byte indicatorCode)
        {
            if(previous != current) {
                SendIlluminationSettingPacket(indicatorCode, current ? (byte)1 : (byte)0);
            }
        }

        /// <summary>
        /// Switches an LED on or off, or sets a backlight brightness.
        /// </summary>
        /// <param name="indicatorCode"></param>
        /// <param name="value"></param>
        private void SendIlluminationSettingPacket(byte indicatorCode, byte value)
        {
            _UsbWriter.LockForOutput(() => {
                var offset = _IlluminationPacketTypeIndicatorOffset;
                _IlluminationPacket[offset] = indicatorCode;
                _IlluminationPacket[offset + 1] = value;
                _UsbWriter.SendPacket(_IlluminationPacket);
            });
        }
    }
}
