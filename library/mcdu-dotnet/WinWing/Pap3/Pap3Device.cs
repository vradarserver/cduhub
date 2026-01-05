// Copyright © 2025 onwards Laurent Andre
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace WwDevicesDotNet.WinWing.Pap3
{
    /// <summary>
    /// Represents a WinWing PAP-3 Primary Autopilot Panel device.
    /// Handles communication with the physical PAP-3 hardware via HID protocol.
    /// </summary>
    public class Pap3Device : IFrontpanel
    {
        // Command prefix for PAP-3 panel (verified from hardware testing)
        const ushort _Pap3DisplayPrefix = 0x0FBF;
        const ushort _Pap3LedPrefix = 0x0100;

        // Brightness command types (verified from hardware testing)
        const byte _BrightnessPanelBacklight = 0x00;
        const byte _BrightnessDigitalTube = 0x01;
        const byte _BrightnessMarkerLight = 0x02;

        // Seven-segment display digit values.
        // Bit order matches `_SegmentBits`: G, F, E, D, C, B, A, DP
        static readonly byte[] _DigitValues = new byte[] {
            0x7E, // 0: A B C D E F
            0x0C, // 1: B C
            0xB6, // 2: A B D E G
            0x9E, // 3: A B C D G
            0xCC, // 4: B C F G
            0xDA, // 5: A C D F G
            0xFA, // 6: A C D E F G
            0x0E, // 7: A B C
            0xFE, // 8: A B C D E F G
            0xDE  // 9: A B C D F G
        };

        // Segment bit mapping for 7-segment displays
        // These map to the bits in `_DigitValues`.
        //
        // IMPORTANT: The PAP-3 digit byte encoding uses a non-standard segment order that matches
        // the segment offsets used in the digit mappings (7 offsets per digit):
        //   G (middle), F (top left), E (bottom left), D (bottom), C (bottom right), B (top right), A (top)
        // Decimal point is bit 0.
        static readonly byte[] _SegmentBits = new byte[] {
            0x80, // Bit 7: Segment G (middle)
            0x40, // Bit 6: Segment F (top left)
            0x20, // Bit 5: Segment E (bottom left)
            0x10, // Bit 4: Segment D (bottom)
            0x08, // Bit 3: Segment C (bottom right)
            0x04, // Bit 2: Segment B (top right)
            0x02, // Bit 1: Segment A (top)
            0x01  // Bit 0: Decimal point
        };


        // Display mapping structure: defines which bit mask and offsets to use for each digit
        class DigitMapping
        {
            public byte BitMask { get; set; }
            public int[] SegmentOffsets { get; set; }
        }

        // PLT Course (Speed) - 3 digits
        static readonly DigitMapping[] _PltCourseMapping = new DigitMapping[]
        {
            new DigitMapping { BitMask = 0x80, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }, // Hundreds
            new DigitMapping { BitMask = 0x40, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }, // Tens
            new DigitMapping { BitMask = 0x20, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }  // Ones
        };

        // CPL Course - 3 digits (offset +4 from PLT Course pattern)
        static readonly DigitMapping[] _CplCourseMapping = new DigitMapping[]
        {
            new DigitMapping { BitMask = 0x40, SegmentOffsets = new int[] { 0x20, 0x24, 0x28, 0x2C, 0x30, 0x34, 0x38 } }, // Hundreds
            new DigitMapping { BitMask = 0x20, SegmentOffsets = new int[] { 0x20, 0x24, 0x28, 0x2C, 0x30, 0x34, 0x38 } }, // Tens
            new DigitMapping { BitMask = 0x10, SegmentOffsets = new int[] { 0x20, 0x24, 0x28, 0x2C, 0x30, 0x34, 0x38 } }  // Ones
            // todo add Decimal point if needed 1C 10 
        };

        // Speed - 4 digits physical window
        static readonly DigitMapping[] _Speed4Mapping = new DigitMapping[]
        {
            new DigitMapping { BitMask = 0x08, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }, // Digit 1 (leftmost)
            new DigitMapping { BitMask = 0x04, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }, // Digit 2
            new DigitMapping { BitMask = 0x02, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }, // Digit 3
            new DigitMapping { BitMask = 0x01, SegmentOffsets = new int[] { 0x1D, 0x21, 0x25, 0x29, 0x2D, 0x31, 0x35 } }  // Digit 4 (rightmost)
        };

        // Convenience mapping to address digits 2-4 (rightmost three)
        static readonly DigitMapping[] _Speed3RightAlignedMapping = new DigitMapping[]
        {
            _Speed4Mapping[1],
            _Speed4Mapping[2],
            _Speed4Mapping[3]
        };

        // Heading (HDG) - 3 digits
        static readonly DigitMapping[] _HeadingMapping = new DigitMapping[]
        {
            new DigitMapping { BitMask = 0x40, SegmentOffsets = new int[] { 0x1E, 0x22, 0x26, 0x2A, 0x2E, 0x32, 0x36 } }, // Hundreds
            new DigitMapping { BitMask = 0x20, SegmentOffsets = new int[] { 0x1E, 0x22, 0x26, 0x2A, 0x2E, 0x32, 0x36 } }, // Tens
            new DigitMapping { BitMask = 0x10, SegmentOffsets = new int[] { 0x1E, 0x22, 0x26, 0x2A, 0x2E, 0x32, 0x36 } }  // Ones
            // Todo Decimal point 26 08 
        };

        // Altitude - 5 digits 
        static readonly DigitMapping[] _AltitudeMapping = new DigitMapping[]
        {
            new DigitMapping { BitMask = 0x04, SegmentOffsets = new int[] { 0x1E, 0x22, 0x26, 0x2A, 0x2E, 0x32, 0x36 } }, // Ten-thousands
            new DigitMapping { BitMask = 0x02, SegmentOffsets = new int[] { 0x1E, 0x22, 0x26, 0x2A, 0x2E, 0x32, 0x36 } }, // Thousands
            new DigitMapping { BitMask = 0x01, SegmentOffsets = new int[] { 0x1E, 0x22, 0x26, 0x2A, 0x2E, 0x32, 0x36 } }, // Hundreds
            // Todo decilmal point 1A 01
            new DigitMapping { BitMask = 0x80, SegmentOffsets = new int[] { 0x1F, 0x23, 0x27, 0x2B, 0x2F, 0x33, 0x37 } }, // Tens
            new DigitMapping { BitMask = 0x40, SegmentOffsets = new int[] { 0x1F, 0x23, 0x27, 0x2B, 0x2F, 0x33, 0x37 } }  // Ones
        };

        // Vertical Speed - 4 digits
        static readonly DigitMapping[] _VerticalSpeedMapping = new DigitMapping[]
        {
            new DigitMapping { BitMask = 0x08, SegmentOffsets = new int[] { 0x1F, 0x23, 0x27, 0x2B, 0x2F, 0x33, 0x37 } }, // Thousands
            new DigitMapping { BitMask = 0x04, SegmentOffsets = new int[] { 0x1F, 0x23, 0x27, 0x2B, 0x2F, 0x33, 0x37 } }, // Hundreds
            // Todo decimal point 1B 04
            new DigitMapping { BitMask = 0x02, SegmentOffsets = new int[] { 0x1F, 0x23, 0x27, 0x2B, 0x2F, 0x33, 0x37 } }, // Tens
            new DigitMapping { BitMask = 0x01, SegmentOffsets = new int[] { 0x1F, 0x23, 0x27, 0x2B, 0x2F, 0x33, 0x37 } }  // Ones
            
        };


        HidDevice _HidDevice;
        HidStream _HidStream;
        bool _Disposed;
        CancellationTokenSource _InputLoopCancellationTokenSource;
        Task _InputLoopTask;
        readonly byte[] _LastInputReport = new byte[25];
        ushort _SequenceNumber = 0; // Track sequence number for display packets

        /// <inheritdoc/>
        public DeviceIdentifier DeviceId { get; }

        /// <inheritdoc/>
        public bool IsConnected => _HidStream != null;

        /// <inheritdoc/>
        public event EventHandler<FrontpanelEventArgs> ControlActivated;

        /// <inheritdoc/>
        public event EventHandler<FrontpanelEventArgs> ControlDeactivated;

        /// <inheritdoc/>
        public event EventHandler Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pap3Device"/> class.
        /// </summary>
        /// <param name="hidDevice">The HID device to communicate with.</param>
        /// <param name="deviceId">The device identifier.</param>
        public Pap3Device(HidDevice hidDevice, DeviceIdentifier deviceId)
        {
            _HidDevice = hidDevice;
            DeviceId = deviceId;
        }

        /// <summary>
        /// Initializes the device connection and starts reading input.
        /// </summary>
        public void Initialise()
        {
            var maxOutputReportLength = _HidDevice.GetMaxOutputReportLength();
            if(maxOutputReportLength < 64) {
                throw new WwDeviceException(
                    $"HID device {_HidDevice} reported an invalid max output report length of {maxOutputReportLength}"
                );
            }

            if(!_HidDevice.TryOpen(out _HidStream)) {
                throw new WwDeviceException($"Could not open a stream to {_HidDevice}");
            }

            // Send initialization packet (verified from hardware capture)
            SendInitPacket();

            // Subscribe to device list changes for disconnect detection
            DeviceList.Local.Changed += HidSharpDeviceList_Changed;

            // Start reading input reports on a background task
            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => RunInputLoop(_InputLoopCancellationTokenSource.Token));
        }

        void SendInitPacket()
        {
            // Initialization packet: F0 02 00... (all zeros)
            var initPacket = new byte[64];
            initPacket[0] = 0xF0;
            initPacket[1] = 0x02;
            // Rest is already 0x00
            
            SendCommand(initPacket);
        }

        /// <inheritdoc/>
        public void UpdateDisplay(IFrontpanelState state)
        {
            if(!IsConnected)
                return;
        
            var commands = BuildDisplayCommands((Pap3State)state);
            foreach(var data in commands) {
                SendCommand(data);
            }
        }

        /// <inheritdoc/>
        public void UpdateLeds(IFrontpanelLeds leds)
        {
            if(!IsConnected)
                return;

            if(leds is Pap3Leds pap3Leds) {
                var commands = BuildLedCommands(pap3Leds);
                foreach(var data in commands) {
                    SendCommand(data);
                }
            }
        }

        /// <inheritdoc/>
        public void SetBrightness(byte panelBacklight, byte lcdBacklight, byte ledBacklight)
        {
            if(!IsConnected)
                return;

            SendBrightnessCommand(_Pap3LedPrefix, _BrightnessPanelBacklight, panelBacklight);
            SendBrightnessCommand(_Pap3LedPrefix, _BrightnessDigitalTube, lcdBacklight);
            SendBrightnessCommand(_Pap3LedPrefix, _BrightnessMarkerLight, ledBacklight);
        }

        void SendBrightnessCommand(ushort prefix, byte variableType, byte value)
        {
            var packet = new byte[14];
            packet[0] = 0x02;
            packet[1] = (byte)((prefix >> 8) & 0xFF);
            packet[2] = (byte)(prefix & 0xFF);
            packet[3] = 0x00;
            packet[4] = 0x00;
            packet[5] = 0x03;
            packet[6] = 0x49;
            packet[7] = variableType;
            packet[8] = value;

            SendCommand(packet);
        }

        void SendCommand(byte[] data)
        {
            if(!IsConnected)
                throw new InvalidOperationException("Device is not connected.");

            _HidStream.Write(data);
        }

        void RunInputLoop(CancellationToken cancellationToken)
        {
            var readBuffer = new byte[25];
            _HidStream.ReadTimeout = 100; // Reduced from 1000ms to 100ms for faster response

            while(!cancellationToken.IsCancellationRequested) {
                try {
                    if(_HidStream != null && _HidStream.CanRead) {
                        var bytesRead = _HidStream.Read(readBuffer, 0, readBuffer.Length);
                        if(bytesRead > 0 && readBuffer[0] == 0x01) {
                            ProcessReport(readBuffer, bytesRead);
                        }
                    }
                } catch(TimeoutException) {
                    // Expected when no data available - don't sleep, just continue
                } catch(ObjectDisposedException) {
                    // Stream was disposed
                    break;
                } catch(System.IO.IOException) {
                    // Device disconnected
                    break;
                }
                
                // Removed Thread.Sleep(1) - let the HID read timeout handle the delay
            }
        }

        void ProcessReport(byte[] data, int length)
        {
            if(length < 25)
                return;

            // Compare with last report to detect changes
            for(var i = 1; i < 13; i++) {
                var currentByte = data[i];
                var lastByte = _LastInputReport[i];
                
                if(currentByte != lastByte) {
                    var changed = (byte)(currentByte ^ lastByte);
                    
                    for(byte bit = 0; bit < 8; bit++) {
                        var mask = (byte)(1 << bit);
                        if((changed & mask) != 0) {
                            var pressed = (currentByte & mask) != 0;
                            var control = GetControl(i, mask);
                            
                            if(control.HasValue) {
                                var controlId = control.Value.ToString();
                                if(pressed) {
                                    ControlActivated?.Invoke(this, new FrontpanelEventArgs(controlId, data));
                                } else {
                                    ControlDeactivated?.Invoke(this, new FrontpanelEventArgs(controlId, data));
                                }
                            }
                        }
                    }
                }
            }

            Array.Copy(data, _LastInputReport, length);
        }

        Control? GetControl(int offset, byte flag)
        {
            foreach (Control control in Enum.GetValues(typeof(Control))) {
                var (mapFlag, mapOffset) = ControlMap.InputReport01FlagAndOffset(control);
                if(mapOffset == offset && mapFlag == flag) {
                    return control;
                }
            }
            return null;
        }

        List<byte[]> BuildDisplayCommands(Pap3State state)
        {
            var commands = new List<byte[]>();

            commands.AddRange(BuildPap3DisplayCommands(state));
            return commands;
        }

        List<byte[]> BuildPap3DisplayCommands(Pap3State state)
        {
            var commands = new List<byte[]>();
            var payload = new byte[64];
            var followup = new byte[64];

            // Increment sequence number for each display update
            _SequenceNumber++;
            if(_SequenceNumber > 255) _SequenceNumber = 1;

            // Pattern from hardware capture (see README packet analysis):
            // Packet 1: 38 command with display data to unit 0F BF
            // Packet 2: 38 command empty (to unit 00 00 - no device)
            // Packet 3: 38 command empty (to unit 00 00 - no device)
            // Packet 4: 2A acknowledgment packet
            
            // Packet 1: Main display data (38 command)
            payload[0] = 0xF0;
            payload[1] = 0x00;
            payload[2] = (byte)_SequenceNumber;
            payload[3] = 0x38; // Command to device ?
            payload[4] = (byte)((_Pap3DisplayPrefix >> 8) & 0xFF); // 0x0F
            payload[5] = (byte)(_Pap3DisplayPrefix & 0xFF);        // 0xBF
            payload[6] = 0x00; // Checksum bytes (set to 00 00 for now)
            payload[7] = 0x00;
            
            // Fixed sequence from hardware capture (bytes 08-1E)
            payload[8] = 0x02;
            payload[9] = 0x01;
            payload[10] = 0x00;
            payload[11] = 0x00;
            // Bytes 12-15 appear to be a checksum/identifier that gets echoed in 2A packet
            // For now, use captured values
            payload[12] = 0xC3;
            payload[13] = 0x29;
            payload[14] = 0x20;
            payload[15] = 0x00;
            payload[16] = 0x00;
            payload[17] = 0xB0;
            
            
            EncodePap3Displays(payload, state);

            if (state.Speed.HasValue)
            {
                if (state.SpeedIsMach)
                {
                    payload[0x2E] |= 0x80;
                    payload[0x32] |= 0x80;  
                    payload[0x19] |= 0x04;
                }
                else
                {
                    payload[0x36] |= 0x80; 
                    payload[0x1A] |= 0x80;  
                }
            }

            if (state.Heading.HasValue)
            {
                if (state.HeadingIsTrack)
                {
                    payload[0x2A] |= 0x08;  
                    payload[0x2E] |= 0x08;  
                }
                else
                {
                    payload[0x32] |= 0x08;  
                    payload[0x36] |= 0x08;  
                }
            }

            if (state.VerticalSpeed.HasValue)
            {
                payload[0x1F] |= 0x10;
                if ((state.VerticalSpeed.Value > 0))
                {
                    payload[0x23] |= 0x10;
                    payload[0x2C] |= 0x80;
                    payload[0x28] |= 0x80;
                }

                if (state.VsIsFpa)
                {
                    payload[0x30] |= 0x80;  
                    payload[0x34] |= 0x80;
                    payload[0x1B] |= 0x04;
                }
                else
                {
                    payload[0x1C] |= 0x80;  
                    payload[0x38] |= 0x80;  
                }
                
            }

            commands.Add(payload);

            // Packet 2: Empty 38 packet (to unit 00 00)
            var empty2 = new byte[64];
            empty2[0] = 0xF0;
            empty2[1] = 0x00;
            empty2[2] = (byte)((_SequenceNumber + 1) % 256);
            empty2[3] = 0x38;
            empty2[4] = 0x00; // Unit ID 00 00 (no device)
            empty2[5] = 0x00;
            commands.Add(empty2);

            // Packet 3: Empty 38 packet (to unit 00 00)
            var empty3 = new byte[64];
            empty3[0] = 0xF0;
            empty3[1] = 0x00;
            empty3[2] = (byte)((_SequenceNumber + 2) % 256);
            empty3[3] = 0x38;
            empty3[4] = 0x00; // Unit ID 00 00 (no device)
            empty3[5] = 0x00;
            commands.Add(empty3);

            // Packet 2: Acknowledgment (2A packet)
            followup[0] = 0xF0;
            followup[1] = 0x00;
            followup[2] = (byte)((_SequenceNumber + 3) % 256);
            followup[3] = 0x2A; // Acknowledgment command
            // Bytes 04-1C are zeros
            followup[29] = (byte)((_Pap3DisplayPrefix >> 8) & 0xFF); // 0x0F (Unit ID being acknowledged)
            followup[30] = (byte)(_Pap3DisplayPrefix & 0xFF);        // 0xBF
            followup[31] = 0x00;
            followup[32] = 0x00;
            followup[33] = 0x03;
            followup[34] = 0x01;
            followup[35] = 0x00;
            followup[36] = 0x00;
            // Bytes 37-40 echo the checksum from the 38 packet (bytes 12-15)
            followup[37] = payload[12]; // 0xC3
            followup[38] = payload[13]; // 0x29
            followup[39] = payload[14]; // 0x20
            followup[40] = 0x00;
            // Remaining bytes are zeros

            commands.Add(followup);

            // Update sequence number to account for all 4 packets
            _SequenceNumber = (ushort)((_SequenceNumber + 4) % 256);

            return commands;
        }

        void EncodePap3Displays(byte[] buffer, Pap3State state)
        {
            // Clear the display area first to avoid stale data
            for (int i = 0x19; i <= 0x38; i++)
            {
                buffer[i] = 0x00;
            }

            if (state.Speed.HasValue) {
                var speed = Math.Max(0, Math.Min(9999, state.Speed.Value));

                if(state.SpeedIsMach) {
                    // Mach speeds come in as "82" for Mach 0.82, or e.g. "105" for Mach 1.05.
                    // Show as 0.xx / 1.xx on the RIGHTMOST 3 digits.
                    var leading = speed >= 100 ? 1 : 0;
                    var lastTwo = speed % 100;

                    EncodeDigitWithMapping(buffer, leading, _Speed3RightAlignedMapping[0]);
                    EncodeDigitWithMapping(buffer, (lastTwo / 10) % 10, _Speed3RightAlignedMapping[1]);
                    EncodeDigitWithMapping(buffer, lastTwo % 10, _Speed3RightAlignedMapping[2]);
                } else {
                    // IAS: support full 0-9999. For traditional 3-digit IAS (0-999), right-align to digits 2-4.
                    if(speed <= 999) {
                        EncodeMultiDigitValue(buffer, speed, 3, _Speed3RightAlignedMapping);
                    } else {
                        EncodeMultiDigitValue(buffer, speed, 4, _Speed4Mapping);
                    }
                }
            }
            
            if (state.PltCourse.HasValue) {
                var course = Math.Max(0, Math.Min(999, state.PltCourse.Value));
                EncodeMultiDigitValue(buffer, course, 3, _PltCourseMapping);
            }
            if (state.CplCourse.HasValue) {
                var course = Math.Max(0, Math.Min(999, state.CplCourse.Value));
                EncodeMultiDigitValue(buffer, course, 3, _CplCourseMapping);
            }

            // Encode Heading display (if present)
            if (state.Heading.HasValue) {
                var heading = Math.Max(0, Math.Min(999, state.Heading.Value));
                EncodeMultiDigitValue(buffer, heading, 3, _HeadingMapping);
            }

            // Encode Altitude display (if present)
            if (state.Altitude.HasValue) {
                var altitude = Math.Max(0, Math.Min(99999, state.Altitude.Value));
                EncodeMultiDigitValue(buffer, altitude, 5, _AltitudeMapping);
            }

            // Encode Vertical Speed display (if present)
            if (state.VerticalSpeed.HasValue) {
                var vs = Math.Abs(state.VerticalSpeed.Value);
                vs = Math.Max(0, Math.Min(9999, vs));
                EncodeMultiDigitValue(buffer, vs, 4, _VerticalSpeedMapping);
            }
        }

        void EncodeMultiDigitValue(byte[] buffer, int value, int numDigits, DigitMapping[] mapping)
        {
            var valueStr = value.ToString().PadLeft(numDigits, '0');
            
            for (int i = 0; i < numDigits && i < mapping.Length; i++)
            {
                int digit = valueStr[i] - '0';
                EncodeDigitWithMapping(buffer, digit, mapping[i]);
            }
        }

        void EncodeDigitWithMapping(byte[] buffer, int digit, DigitMapping mapping)
        {
            if (digit < 0 || digit > 9)
                return;

            var segmentValue = _DigitValues[digit];
            
            // For each segment bit in the 7-segment encoding
            for (int segIdx = 0; segIdx < _SegmentBits.Length && segIdx < mapping.SegmentOffsets.Length; segIdx++)
            {
                // Check if this segment should be lit for this digit
                if ((segmentValue & _SegmentBits[segIdx]) != 0)
                {
                    // Set the bit at the corresponding offset
                    buffer[mapping.SegmentOffsets[segIdx]] |= mapping.BitMask;
                }
            }
        }
                
        List<byte[]> BuildLedCommands(Pap3Leds leds)
        {
            var commands = new List<byte[]>();

            // LED command codes verified from hardware testing
            // Format: 02 01 00 00 00 03 49 [code] [value] 00 00 00 00 00
            // LED commands use prefix 0x0100
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x03, leds.N1));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x04, leds.Speed));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x05, leds.Vnav));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x06, leds.LvlChg));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x07, leds.HdgSel));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x08, leds.Lnav));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x09, leds.VorLoc));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x0A, leds.App));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x0B, leds.AltHold));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x0C, leds.Vs));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x0D, leds.CmdA));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x0E, leds.CwsA));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x0F, leds.CmdB));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x10, leds.CwsB));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x11, leds.AtArm));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x12, leds.FdL));
            commands.Add(BuildLedCommand(_Pap3LedPrefix, 0x13, leds.FdR));

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

        void HidSharpDeviceList_Changed(object sender, DeviceListChangedEventArgs e)
        {
            if(_HidDevice != null) {
                var devicePresent = DeviceList.Local
                    .GetHidDevices()
                    .Any(device => device.DevicePath == _HidDevice.DevicePath);
                
                if(!devicePresent) {
                    OnDisconnected();
                }
            }
        }

        void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if(_Disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if(disposing) {
                DeviceList.Local.Changed -= HidSharpDeviceList_Changed;

                _InputLoopCancellationTokenSource?.Cancel();
                _InputLoopTask?.Wait(5000);
                _InputLoopTask = null;

                var hidStream = _HidStream;
                _HidStream = null;
                try {
                    hidStream?.Dispose();
                } catch {
                    ;
                }

                _HidDevice = null;
            }

            _Disposed = true;
        }
    }
}
