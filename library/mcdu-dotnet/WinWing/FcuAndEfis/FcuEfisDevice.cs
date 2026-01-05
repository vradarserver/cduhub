// Copyright © 2025 onwards, Laurent Andre
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using HidSharp;

namespace WwDevicesDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// Represents a WinWing FCU (Flight Control Unit) device with optional EFIS panels.
    /// Handles communication with the physical FCU hardware via HID protocol.
    /// </summary>
    public class FcuEfisDevice : BaseFrontpanelDevice<Control>
    {
        // Command prefixes for different panels
        const ushort _LeftEfisPrefix = 0x0DBF;
        const ushort _FcuPrefix = 0x10BB;
        const ushort _RightEfisPrefix = 0x0EBF;

        // Seven-segment display digit values for FCU displays (complex encoding)
        static readonly byte[] _EfisDigitValues = new byte[] {
            0xFA, // 0
            0x60, // 1
            0xD6, // 2
            0xF4, // 3
            0x6C, // 4
            0xBC, // 5
            0xBE, // 6
            0xE0, // 7
            0xFE, // 8
            0xFC  // 9
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FcuEfisDevice"/> class.
        /// </summary>
        /// <param name="hidDevice">The HID device to communicate with.</param>
        /// <param name="deviceId">The device identifier.</param>
        public FcuEfisDevice(HidDevice hidDevice, DeviceIdentifier deviceId)
            : base(hidDevice, deviceId)
        {
        }

        /// <inheritdoc/>
        protected override void SendInitPacket()
        {
            // FCU doesn't require an explicit initialization packet
        }

        /// <inheritdoc/>
        protected override Control? GetControl(int offset, byte flag)
        {
            foreach(Control control in Enum.GetValues(typeof(Control))) {
                var (mapFlag, mapOffset) = ControlMap.InputReport01FlagAndOffset(control);
                if(mapOffset == offset && mapFlag == flag) {
                    return control;
                }
            }
            return null;
        }

        /// <inheritdoc/>
        protected override void RunInputLoop(System.Threading.CancellationToken cancellationToken)
        {
            var readBuffer = new byte[25];
            _HidStream.ReadTimeout = 1000;

            while(!cancellationToken.IsCancellationRequested) {
                try {
                    if(_HidStream != null && _HidStream.CanRead) {
                        var bytesRead = _HidStream.Read(readBuffer, 0, readBuffer.Length);
                        if(bytesRead > 0 && readBuffer[0] == 0x01) {
                            ProcessReport(readBuffer, bytesRead);
                        }
                    }
                } catch(TimeoutException) {
                    // Expected when no data available
                } catch(ObjectDisposedException) {
                    break;
                } catch(System.IO.IOException) {
                    break;
                }

                // Yield to prevent busy-waiting
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <inheritdoc/>
        public override void UpdateDisplay(IFrontpanelState state)
        {
            if(!IsConnected)
                return;

            if(state is FcuEfisState fcuState) {
                var commands = BuildDisplayCommands(fcuState);
                foreach(var data in commands) {
                    SendCommand(data);
                }
            }
        }

        /// <inheritdoc/>
        public override void UpdateLeds(IFrontpanelLeds leds)
        {
            if(!IsConnected)
                return;

            if(leds is FcuEfisLeds fcuLeds) {
                var commands = BuildLedCommands(fcuLeds);
                foreach(var data in commands) {
                    SendCommand(data);
                }
            }
        }

        /// <inheritdoc/>
        public override void SetBrightness(byte panelBacklight, byte lcdBacklight, byte ledBacklight)
        {
            if(!IsConnected)
                return;

            // Send brightness commands for FCU
            SendBrightnessCommand(_FcuPrefix, 0x00, panelBacklight);
            SendBrightnessCommand(_FcuPrefix, 0x01, lcdBacklight);
            SendBrightnessCommand(_FcuPrefix, 0x11, ledBacklight);

            // Send brightness commands for left EFIS if present
            if(HasLeftEfis()) {
                SendBrightnessCommand(_LeftEfisPrefix, 0x00, panelBacklight);
                SendBrightnessCommand(_LeftEfisPrefix, 0x01, lcdBacklight);
                SendBrightnessCommand(_LeftEfisPrefix, 0x11, ledBacklight);
            }

            // Send brightness commands for right EFIS if present
            if(HasRightEfis()) {
                SendBrightnessCommand(_RightEfisPrefix, 0x00, panelBacklight);
                SendBrightnessCommand(_RightEfisPrefix, 0x01, lcdBacklight);
                SendBrightnessCommand(_RightEfisPrefix, 0x11, ledBacklight);
            }
        }

        List<byte[]> BuildDisplayCommands(FcuEfisState state)
        {
            var commands = new List<byte[]>();

            // Build FCU display commands
            commands.AddRange(BuildFcuDisplayCommands(state));

            // Build EFIS display commands if panels are present
            if(HasLeftEfis() && state.LeftBaroPressure.HasValue) {
                commands.Add(BuildEfisDisplayCommand(_LeftEfisPrefix, state.LeftBaroPressure.Value, state.LeftBaroQnh, state.LeftBaroQfe));
            }

            if(HasRightEfis() && state.RightBaroPressure.HasValue) {
                commands.Add(BuildEfisDisplayCommand(_RightEfisPrefix, state.RightBaroPressure.Value, state.RightBaroQnh, state.RightBaroQfe));
            }

            return commands;
        }

        List<byte[]> BuildFcuDisplayCommands(FcuEfisState state)
        {
            var commands = new List<byte[]>();
            var payload = new byte[64];
            var followup = new byte[64];

            ushort seqNum = 1;

            // First packet (payload)
            payload[0] = 0xF0;
            payload[1] = 0x00;
            payload[2] = (byte)seqNum;
            payload[3] = 0x31;
            payload[4] = (byte)((_FcuPrefix >> 8) & 0xFF);
            payload[5] = (byte)(_FcuPrefix & 0xFF);
            payload[6] = 0x00;
            payload[7] = 0x00;
            payload[8] = 0x02;
            payload[9] = 0x01;
            payload[10] = 0x00;
            payload[11] = 0x00;
            payload[12] = 0xFF;  
            payload[13] = 0xFF;  
            payload[14] = 0x02;  
            payload[15] = 0x00;
            payload[16] = 0x00;
            payload[17] = 0x20;
            // Bytes 18-24 are 0x00

            // Encode displays starting at offset 0x19 (25)
            EncodeFcuDisplays(payload, state);
            
            // SPD/MACH indicators at H3 position (0x1C)
            if(state.SpeedIsMach) {
                payload[0x1C] |= 0x04;  // MACH indicator
            } else {
                payload[0x1C] |= 0x08;  // SPD indicator
            }
            
            // Speed managed mode indicator (round dot after speed)
            if(state.SpeedManaged) {
                payload[0x1C] |= 0x02;  // SPD managed indicator
            }
            
            // HDG/TRK indicators at h0 position (0x1F)
            if(state.HeadingIsTrack) {
                payload[0x1F] |= 0x40;  // TRK indicator
            } else {
                payload[0x1F] |= 0x80;  // HDG indicator
            }
            
            // Heading managed mode indicator (round dot after heading)
            if(state.HeadingManaged) {
                payload[0x1F] |= 0x10;  // HDG managed indicator
            }
            
            // LAT indicator (purple in doc, between HDG and ALT)
            if(state.LatIndicator) {
                payload[0x1F] |= 0x20;  // LAT indicator
            }
            
            // Middle section indicators at A5 position (0x20 = a[5])
            // The middle section has TWO independent groups:
            // 1. HDG/TRK (bits 0x08 for HDG, 0x02 for TRK)
            // 2. V/S/FPA (bits 0x04 for V/S, 0x01 for FPA)
            
            // TRK indicator in middle section
            if(state.HeadingIsTrack) {
                payload[0x20] |= 0x02;  // TRK indicator in middle
            }
            // HDG indicator in middle section - shows when NOT in TRK mode
            // Note: We don't require V/S or FPA to be selected for HDG to show
            else {
                // Show HDG when TRK is not active
                // The vshdg flag (0x08) represents HDG mode in the middle section
                payload[0x20] |= 0x08;  // HDG indicator in middle
            }
            
            // V/S indicator in middle section
            if(!state.VsIsFpa) {
                payload[0x20] |= 0x04;  // V/S indicator in middle
            }
            
            // FPA indicator in middle section
            if(state.VsIsFpa) {
                payload[0x20] |= 0x01;  // FPA indicator in middle
            }
            
            if(state.AltitudeManaged) {
                payload[0x28] |= 0x10;  // ALT managed indicator at v[1] position
            }
            
            if(state.VsHorzIndicator) {
                payload[0x25] |= 0x20;  // Horizontal indicator (different bit to avoid minus)
            }
            
            if(state.LvlIndicator) {
                payload[0x23] |= 0x10;  // Level change indicator
            }
            
            if(state.LvlLeftBracket) {
                payload[0x22] |= 0x10;  // Left bracket
            }
            
            if(state.LvlRightBracket) {
                payload[0x24] |= 0x10;  // Right bracket
            }
            
            if(state.VsIsFpa) {
                payload[0x29] |= 0x80;  // FPA indicator (above V/S digits)
            } else {
                payload[0x29] |= 0x40;  // V/S indicator (above V/S digits)
            }

            commands.Add(payload);

            // Second packet (followup)
            followup[0] = 0xF0;
            followup[1] = 0x00;
            followup[2] = (byte)seqNum;
            followup[3] = 0x11;
            followup[4] = (byte)((_FcuPrefix >> 8) & 0xFF);
            followup[5] = (byte)(_FcuPrefix & 0xFF);
            followup[6] = 0x00;
            followup[7] = 0x00;
            followup[8] = 0x03;
            followup[9] = 0x01;
            followup[10] = 0x00;
            followup[11] = 0x00;
            followup[12] = 0xFF;  
            followup[13] = 0xFF;  
            followup[14] = 0x02;  
            // Remaining bytes are 0x00

            commands.Add(followup);

            return commands;
        }

        void EncodeFcuDisplays(byte[] buffer, FcuEfisState state)
        {
            // Clear the display area first to avoid stale data
            for(int i = 0x19; i <= 0x29; i++) {
                buffer[i] = 0x00;
            }

            // SPD display - direct encoding
            if(state.Speed.HasValue) {
                var speed = Math.Max(0, Math.Min(999, state.Speed.Value));
                buffer[0x19] = _EfisDigitValues[(speed / 100) % 10];   // s[2]
                buffer[0x1A] = _EfisDigitValues[(speed / 10) % 10];    // s[1]
                buffer[0x1B] = _EfisDigitValues[speed % 10];           // s[0]
                
                // MACH decimal point - only in MACH mode OR if explicitly requested
                if(state.SpeedIsMach) {
                    buffer[0x1A] |= 0x01;  // Decimal point for MACH mode (e.g., 0.78)
                }
            }

            // HDG display - nibble swapped encoding
            if(state.Heading.HasValue) {
                var heading = Math.Max(0, Math.Min(359, state.Heading.Value));
                var h = DataFromStringSwapped(3, heading);
                buffer[0x1C] |= h[3];  // h[3]
                buffer[0x1D] |= h[2];  // h[2]
                buffer[0x1E] |= h[1];  // h[1]
                buffer[0x1F] |= h[0];  // h[0]
            }

            // ALT display - nibble swapped encoding
            if(state.Altitude.HasValue) {
                var altitude = Math.Max(0, Math.Min(99999, state.Altitude.Value));
                byte[] a;
                
                if(state.AltitudeIsFlightLevel) {
                    // Flight Level mode: display "FL" + flight level (e.g., altitude 10000 → FL100)
                    var flightLevel = altitude / 100;
                    
                    // Create custom encoding for "FL" + 3-digit number
                    // We need to manually encode F, L, and three digits into the 5-position display
                    // F = segments for letter F, L = segments for letter L
                    // Then encode the 3-digit flight level
                    
                    // Encode FL as a 5-character string where first 2 chars are letters
                    // Use special encoding for letters F and L
                    var flString = $"FL{flightLevel:D3}";
                    a = EncodeAlphanumericSwapped(flString);
                } else {
                    // Standard altitude mode: display without leading zeros
                    var altString = altitude.ToString();
                    
                    // Right-align in 5-digit space but don't pad with zeros
                    if(altString.Length < 5) {
                        altString = altString.PadLeft(5, ' ');
                    }
                    
                    a = EncodeAlphanumericSwapped(altString);
                }
                
                buffer[0x20] |= a[5];  // a[5]
                buffer[0x21] |= a[4];  // a[4]
                buffer[0x22] |= a[3];  // a[3]
                buffer[0x23] |= a[2];  // a[2]
                buffer[0x24] |= a[1];  // a[1]
                buffer[0x25] |= a[0];  // a[0] - shares with v[4]
            }

            // V/S display - nibble swapped encoding
            if(state.VerticalSpeed.HasValue) {
                var vs = state.VerticalSpeed.Value;
                var absVs = Math.Abs(vs);
                absVs = Math.Min(9999, absVs);
                
                var v = DataFromStringSwapped(4, absVs);
                buffer[0x25] |= v[4];  // v[4] - shares with a[0]
                buffer[0x26] |= v[3];  // v[3]
                buffer[0x27] |= v[2];  // v[2]
                buffer[0x28] |= v[1];  // v[1]
                buffer[0x29] |= v[0];  // v[0]
                
                // Always show horizontal bar (this is the base minus sign)
                buffer[0x25] |= 0x10;  // vs_horz - horizontal bar always on
                
                // For POSITIVE values, also show vertical bar (completes the plus sign)
                if(vs >= 0) {
                    buffer[0x27] |= 0x10;  // vs_vert - add vertical bar for positive = plus sign
                }
            }
        }

        byte[] DataFromStringSwapped(int numDigits, int value)
        {
            
            var d = new byte[numDigits];

            var str = value.ToString().PadLeft(numDigits, '0');
            for(int i = 0; i < numDigits; i++) {
                int digit = str[i] - '0';
                d[numDigits - 1 - i] = _EfisDigitValues[digit];
            }
            
            var result = new byte[numDigits + 1];
            Array.Copy(d, result, numDigits);
            result[numDigits] = 0;

            for(int i = 0; i < result.Length; i++) {
                result[i] = (byte)(((result[i] & 0x0F) << 4) | ((result[i] & 0xF0) >> 4));
            }

            
            int l = numDigits;
            for(int i = 0; i < l; i++) {
                var before_high = result[l - i];
                var before_low = result[l - 1 - i];
                result[l - i] = (byte)((result[l - i] & 0x0F) | (result[l - 1 - i] & 0xF0));
                result[l - 1 - i] = (byte)(result[l - 1 - i] & 0x0F);
            }
            
            return result;
        }

        byte[] BuildEfisDisplayCommand(ushort prefix, int pressure, bool qnh, bool qfe)
        {
            var packet = new byte[64];
            
            // Use fixed package number 1
            ushort seqNum = 1;

            packet[0] = 0xF0;
            packet[1] = 0x00;
            packet[2] = (byte)seqNum;
            packet[3] = 0x1A;  
            packet[4] = (byte)((prefix >> 8) & 0xFF);
            packet[5] = (byte)(prefix & 0xFF);
            packet[6] = 0x00;
            packet[7] = 0x00;
            packet[8] = 0x02;
            packet[9] = 0x01;
            packet[10] = 0x00;
            packet[11] = 0x00;
            packet[12] = 0xFF;  
            packet[13] = 0xFF;  
            packet[14] = 0x1D;  
            packet[15] = 0x00;
            packet[16] = 0x00;
            packet[17] = 0x09;
            // Bytes 18-24 are 0x00

            var pressureStr = pressure.ToString().PadLeft(4, '0');
            
            // Detect inHg mode: values >= 2000 are inHg (displayed as XX.XX)
            // hPa range: 870-1085, inHg range: 2570-3200 (representing 25.70-32.00)
            bool isInHg = pressure >= 2000;
            
            var encoded = DataFromStringSwappedEfis(4, pressureStr);
            
            packet[0x19] = encoded[0];  // leftmost digit (thousands)
            packet[0x1A] = encoded[1];  // hundreds
            packet[0x1B] = encoded[2];  // tens
            packet[0x1C] = encoded[3];  // ones (rightmost)

            // Add decimal point for inHg mode (after 2nd digit, not 3rd)
            // For 2992: display as 29.92 (decimal after hundreds digit)
            // The decimal point in EFIS encoding is bit 0x80 (not 0x01 like in FCU displays)
            // This is because DataFromStringSwappedEfis remaps: 0x01 → 0x80
            if(isInHg) {
                packet[0x1A] |= 0x80;  // Add decimal point after hundreds digit (29.92)
            }

            // QNH/QFE indicators at offset 0x1D (29)
            if(qfe) {
                packet[0x1D] = 0x01;
            } else if(qnh) {
                packet[0x1D] = 0x02;
            }

            // Add followup packet bytes (Python sends both in one packet)
            packet[0x1E] = (byte)((prefix >> 8) & 0xFF);
            packet[0x1F] = (byte)(prefix & 0xFF);
            packet[0x20] = 0x00;
            packet[0x21] = 0x00;
            packet[0x22] = 0x03;
            packet[0x23] = 0x01;
            packet[0x24] = 0x00;
            packet[0x25] = 0x00;
            packet[0x26] = 0x4C;
            packet[0x27] = 0x0C;
            packet[0x28] = 0x1D;

            return packet;
        }

        byte[] DataFromStringSwappedEfis(int numDigits, string str)
        {
            var d = new byte[numDigits];
            for(int i = 0; i < numDigits; i++) {
                int digit = str[i] - '0';
                d[i] = _EfisDigitValues[digit];
            }
            
            var n = new byte[numDigits];
            for(int i = 0; i < numDigits; i++) {
                n[i] = 0;
                if((d[i] & 0x08) != 0) n[i] |= 0x01;
                if((d[i] & 0x04) != 0) n[i] |= 0x02;
                if((d[i] & 0x02) != 0) n[i] |= 0x04;
                if((d[i] & 0x10) != 0) n[i] |= 0x08;
                if((d[i] & 0x80) != 0) n[i] |= 0x10;
                if((d[i] & 0x40) != 0) n[i] |= 0x20;
                if((d[i] & 0x20) != 0) n[i] |= 0x40;
                if((d[i] & 0x01) != 0) n[i] |= 0x80;
            }
            
            return n;
        }

        List<byte[]> BuildLedCommands(FcuEfisLeds leds)
        {
            var commands = new List<byte[]>();

            // FCU LEDs
            commands.Add(BuildLedCommand(_FcuPrefix, 0x03, leds.Loc));
            commands.Add(BuildLedCommand(_FcuPrefix, 0x05, leds.Ap1));
            commands.Add(BuildLedCommand(_FcuPrefix, 0x07, leds.Ap2));
            commands.Add(BuildLedCommand(_FcuPrefix, 0x09, leds.AThr));
            commands.Add(BuildLedCommand(_FcuPrefix, 0x0B, leds.Exped));
            commands.Add(BuildLedBrightnessCommand(_FcuPrefix, 0x1E, leds.ExpedYellowBrightness));
            commands.Add(BuildLedCommand(_FcuPrefix, 0x0D, leds.Appr));

            // Left EFIS LEDs
            if(HasLeftEfis()) {
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x03, leds.LeftFd));
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x04, leds.LeftLs));
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x05, leds.LeftCstr));
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x06, leds.LeftWpt));
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x07, leds.LeftVorD));
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x08, leds.LeftNdb));
                commands.Add(BuildLedCommand(_LeftEfisPrefix, 0x09, leds.LeftArpt));
            }

            // Right EFIS LEDs
            if(HasRightEfis()) {
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x03, leds.RightFd));
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x04, leds.RightLs));
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x05, leds.RightCstr));
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x06, leds.RightWpt));
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x07, leds.RightVorD));
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x08, leds.RightNdb));
                commands.Add(BuildLedCommand(_RightEfisPrefix, 0x09, leds.RightArpt));
            }

            return commands;
        }

        byte[] BuildLedCommand(ushort prefix, byte ledCode, bool on)
        {
            var packet = new byte[14];
            packet[0] = 0x02;
            packet[1] = (byte)((prefix >> 8) & 0xFF);
            packet[2] = (byte)(prefix & 0xFF);
            packet[3] = 0x00;
            packet[4] = 0x00;
            packet[5] = 0x03;
            packet[6] = 0x49;
            packet[7] = ledCode;
            packet[8] = (byte)(on ? 0x01 : 0x00);

            return packet;
        }

        byte[] BuildLedBrightnessCommand(ushort prefix, byte ledCode, byte brightness)
        {
            var packet = new byte[14];
            packet[0] = 0x02;
            packet[1] = (byte)((prefix >> 8) & 0xFF);
            packet[2] = (byte)(prefix & 0xFF);
            packet[3] = 0x00;
            packet[4] = 0x00;
            packet[5] = 0x03;
            packet[6] = 0x49;
            packet[7] = ledCode;
            packet[8] = brightness;

            return packet;
        }

        bool HasLeftEfis()
        {
            return DeviceId.Device == Device.WinWingFcuLeftEfis
                || DeviceId.Device == Device.WinWingFcuBothEfis;
        }

        bool HasRightEfis()
        {
            return DeviceId.Device == Device.WinWingFcuRightEfis
                || DeviceId.Device == Device.WinWingFcuBothEfis;
        }

        byte[] EncodeAlphanumericSwapped(string text)
        {
            var numChars = text.Length;
            var d = new byte[numChars];

            for(int i = 0; i < numChars; i++) {
                char c = text[i];
                if(c >= '0' && c <= '9') {
                    int digit = c - '0';
                    d[numChars - 1 - i] = _EfisDigitValues[digit];
                } else if(c == 'F') {
                    // F: segments A,E,F,G = 0x8E
                    d[numChars - 1 - i] = 0x8E;
                } else if(c == 'L') {
                    // L: segments D,E,F (bottom, bottom-left, top-left) = 0x1A
                    d[numChars - 1 - i] = 0x1A;
                } else if(c == ' ') {
                    // Space: no segments
                    d[numChars - 1 - i] = 0x00;
                } else {
                    // Unknown character: blank
                    d[numChars - 1 - i] = 0x00;
                }
            }
            
            var result = new byte[numChars + 1];
            Array.Copy(d, result, numChars);
            result[numChars] = 0;

            // Nibble swap
            for(int i = 0; i < result.Length; i++) {
                result[i] = (byte)(((result[i] & 0x0F) << 4) | ((result[i] & 0xF0) >> 4));
            }

            // Position swap (same logic as DataFromStringSwapped)
            int l = numChars;
            for(int i = 0; i < l; i++) {
                result[l - i] = (byte)((result[l - i] & 0x0F) | (result[l - 1 - i] & 0xF0));
                result[l - 1 - i] = (byte)(result[l - 1 - i] & 0x0F);
            }
            
            return result;
        }
    }
}
