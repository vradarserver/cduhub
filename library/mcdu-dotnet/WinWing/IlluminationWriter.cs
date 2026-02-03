// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Handles the setting of LEDs and backlight brightnesses on WinWing panels.
    /// </summary>
    class IlluminationWriter
    {
        private readonly UsbWriter _UsbWriter;
        private readonly Dictionary<Led, byte> _LedIndicatorMap;
        private Leds? _PreviousLeds;

        /// <summary>
        /// The 02 report that controls LED on/off and display brightnesses.
        /// </summary>
        private readonly byte[] _LedOrBrightnessPacket = new byte[] {
            0x02, 0x32, 0xbb, 0x00, 0x00, 0x03, 0x49,   // <-- 0x32 replaced with command prefix in ctor
            0x00, 0x00,                                 // <-- these two change during Send LED calls
            0x00, 0x00, 0x00, 0x00, 0x00
        };
        private const int _LedOrBrightnessPacketIndicatorOffset = 7;

        private const byte _SetKeyboardBacklight =  0x00;
        private const byte _SetDisplayBrightness =  0x01;
        private const byte _SetLedBrightness =      0x02;

        public IlluminationWriter(
            UsbWriter usbWriter,
            byte commandPrefix,
            Dictionary<Led, byte> ledIndicatorMap
        )
        {
            _LedOrBrightnessPacket[1] = commandPrefix;
            _UsbWriter = usbWriter;
            _LedIndicatorMap = ledIndicatorMap;
        }

        /// <summary>
        /// Sets the keyboard backlight illumination as a percentage from 0 (off) to 100 (fully on).
        /// </summary>
        /// <param name="percent"></param>
        public void SendBacklightPercent(int percent)
        {
            var byteValue = Percent.ToByte(percent);
            SendLedOrBrightnessPacket(_SetKeyboardBacklight, byteValue);
        }

        /// <summary>
        /// Sets the display backlight illumination as a percentage from 0 (off) to 100 (fully on).
        /// </summary>
        /// <param name="percent"></param>
        public void SendDisplayBrightnessPercent(int percent)
        {
            var byteValue = Percent.ToByte(percent);
            SendLedOrBrightnessPacket(_SetDisplayBrightness, byteValue);
        }

        /// <summary>
        /// Sets the LED brightness as a percentage from 0 (off) to 100 (fully on).
        /// </summary>
        /// <param name="percent"></param>
        public void SendLedBrightnessPercent(int percent)
        {
            var byteValue = Percent.ToByte(percent);
            SendLedOrBrightnessPacket(_SetLedBrightness, byteValue);
        }

        /// <summary>
        /// Copies an <see cref="Leds"/> buffer to the device.
        /// </summary>
        /// <param name="leds"></param>
        /// <param name="skipDuplicateCheck"></param>
        public void ApplyLeds(Leds leds, bool skipDuplicateCheck)
        {
            _UsbWriter.LockForOutput(() => {
                if(skipDuplicateCheck || !(_PreviousLeds?.Equals(leds) ?? false)) {
                    foreach(var kvp in _LedIndicatorMap) {
                        var led = kvp.Key;
                        var indicatorCode = kvp.Value;
                        SendLight(
                            _PreviousLeds?.GetLed(led),
                            leds.GetLed(led),
                            indicatorCode
                        );
                    }

                    if(_PreviousLeds == null) {
                        _PreviousLeds = new Leds();
                    }
                    _PreviousLeds.CopyFrom(leds);
                }
            });
        }

        private void SendLight(bool? previous, bool current, byte indicatorCode)
        {
            if(previous != current) {
                SendLedOrBrightnessPacket(indicatorCode, current ? (byte)1 : (byte)0);
            }
        }

        /// <summary>
        /// Switches an LED on or off, or sets a backlight brightness.
        /// </summary>
        /// <param name="indicatorCode"></param>
        /// <param name="value"></param>
        private void SendLedOrBrightnessPacket(byte indicatorCode, byte value)
        {
            _UsbWriter.LockForOutput(() => {
                var offset = _LedOrBrightnessPacketIndicatorOffset;
                _LedOrBrightnessPacket[offset] = indicatorCode;
                _LedOrBrightnessPacket[offset + 1] = value;
                _UsbWriter.SendPacket(_LedOrBrightnessPacket);
            });
        }
    }
}
