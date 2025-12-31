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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace wwDevicesDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// Represents a WinWing FCU (Flight Control Unit) device with optional EFIS panels.
    /// Handles communication with the physical FCU hardware via HID protocol.
    /// </summary>
    public class FcuEfisDevice : IFrontpanel
    {
        // Command prefixes for different panels
        const ushort _LeftEfisPrefix = 0x0DBF;
        const ushort _FcuPrefix = 0x10BB;
        const ushort _RightEfisPrefix = 0x0EBF;

        // Seven-segment display digit values for FCU displays (complex encoding)
        static readonly byte[] _EfisDigitValues = new byte[] {
            0xFA, // 0 - matches Python
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

        // Seven-segment display digit values for EFIS baro display (simple direct mapping)
        // Standard 7-segment layout: DP G F E D C B A
        // Segment A = top, B = top-right, C = bottom-right, D = bottom
        // E = bottom-left, F = top-left, G = middle, DP = decimal point
        static readonly byte[] _EfisBaroDigitValues = new byte[] {
            0x3F, // 0 = A B C D E F (all except G and DP)
            0x06, // 1 = B C
            0x5B, // 2 = A B D E G
            0x4F, // 3 = A B C D G
            0x66, // 4 = B C F G
            0x6D, // 5 = A C D F G
            0x7D, // 6 = A C D E F G
            0x07, // 7 = A B C
            0x7F, // 8 = all segments
            0x6F  // 9 = A B C D F G
        };

        HidDevice _HidDevice;
        HidStream _HidStream;
        bool _Disposed;
        CancellationTokenSource _InputLoopCancellationTokenSource;
        Task _InputLoopTask;
        ushort _SequenceNumber;
        readonly byte[] _LastInputReport = new byte[25];
        readonly object _SequenceLock = new object();

        /// <inheritdoc/>
        public DeviceIdentifier DeviceId { get; }

        /// <inheritdoc/>
        public bool IsConnected => _HidStream != null;

        /// <inheritdoc/>
        public event EventHandler<FrontpanelEventArgs> ControlActivated;

        /// <inheritdoc/>
        public event EventHandler<FrontpanelEventArgs> ControlDeactivated;

        /// <inheritdoc/>
        public event EventHandler<FrontpanelRotaryEventArgs> RotaryChanged;

        /// <inheritdoc/>
        public event EventHandler Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="FcuEfisDevice"/> class.
        /// </summary>
        /// <param name="hidDevice">The HID device to communicate with.</param>
        /// <param name="deviceId">The device identifier.</param>
        public FcuEfisDevice(HidDevice hidDevice, DeviceIdentifier deviceId)
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
                throw new McduException(
                    $"HID device {_HidDevice} reported an invalid max output report length of {maxOutputReportLength}"
                );
            }

            if(!_HidDevice.TryOpen(out _HidStream)) {
                throw new McduException($"Could not open a stream to {_HidDevice}");
            }

            // Subscribe to device list changes for disconnect detection
            DeviceList.Local.Changed += HidSharpDeviceList_Changed;

            // Start reading input reports on a background task
            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => RunInputLoop(_InputLoopCancellationTokenSource.Token));
        }

        /// <inheritdoc/>
        public void UpdateDisplay(IFrontpanelState state)
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
        public void UpdateLeds(IFrontpanelLeds leds)
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
        public void SetBrightness(byte panelBacklight, byte lcdBacklight, byte ledBacklight)
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

        void SendBrightnessCommand(ushort prefix, byte variableType, byte value)
        {
            var packet = new byte[14];
            packet[0] = 0x02;
            packet[1] = (byte)((prefix >> 8) & 0xFF);  // High byte first
            packet[2] = (byte)(prefix & 0xFF);          // Low byte second
            packet[3] = 0x00;
            packet[4] = 0x00;
            packet[5] = 0x03;
            packet[6] = 0x49;
            packet[7] = variableType;
            packet[8] = value;
            // Remaining bytes are already 0x00

            SendCommand(packet);
        }

        /// <summary>
        /// Sends a command to the FCU device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        void SendCommand(byte[] data)
        {
            if(!IsConnected)
                throw new InvalidOperationException("Device is not connected.");

            // Log the command being sent for protocol debugging
            var hex = BitConverter.ToString(data).Replace("-", " ");
            System.Diagnostics.Debug.WriteLine($"[FCU] Sending {data.Length} bytes: {hex}");

            _HidStream.Write(data);
        }

        void RunInputLoop(CancellationToken cancellationToken)
        {
            var readBuffer = new byte[25]; // FCU/EFIS reports are 25 bytes
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
                    // Stream was disposed
                    break;
                } catch(System.IO.IOException) {
                    // Device disconnected
                    break;
                }

                // Yield to prevent busy-waiting
                Thread.Sleep(1);
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
                            var controlId = GetControlId(i, mask);
                            
                            if(!string.IsNullOrEmpty(controlId)) {
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

            // Copy current report for next comparison
            Array.Copy(data, _LastInputReport, length);
        }

        string GetControlId(int offset, byte flag)
        {
            // FCU controls
            if(offset == 1) {
                switch(flag) {
                    case 0x01: return "FcuSpdMach";
                    case 0x02: return "FcuLoc";
                    case 0x04: return "FcuHdgTrkVsFpa";
                    case 0x08: return "FcuAp1";
                    case 0x10: return "FcuAp2";
                    case 0x20: return "FcuAThr";
                    case 0x40: return "FcuExped";
                    case 0x80: return "FcuMetricAlt";
                }
            } else if(offset == 2) {
                switch(flag) {
                    case 0x01: return "FcuAppr";
                    case 0x02: return "FcuSpdDec";
                    case 0x04: return "FcuSpdInc";
                    case 0x08: return "FcuSpdPush";
                    case 0x10: return "FcuSpdPull";
                    case 0x20: return "FcuHdgDec";
                    case 0x40: return "FcuHdgInc";
                    case 0x80: return "FcuHdgPush";
                }
            } else if(offset == 3) {
                switch(flag) {
                    case 0x01: return "FcuHdgPull";
                    case 0x02: return "FcuAltDec";
                    case 0x04: return "FcuAltInc";
                    case 0x08: return "FcuAltPush";
                    case 0x10: return "FcuAltPull";
                    case 0x20: return "FcuVsDec";
                    case 0x40: return "FcuVsInc";
                    case 0x80: return "FcuVsPush";
                }
            } else if(offset == 4) {
                switch(flag) {
                    case 0x01: return "FcuVsPull";
                    case 0x02: return "FcuAlt100";
                    case 0x04: return "FcuAlt1000";
                }
            }
            // Left EFIS controls
            else if(offset == 5) {
                switch(flag) {
                    case 0x01: return "LeftFd";
                    case 0x02: return "LeftLs";
                    case 0x04: return "LeftCstr";
                    case 0x08: return "LeftWpt";
                    case 0x10: return "LeftVorD";
                    case 0x20: return "LeftNdb";
                    case 0x40: return "LeftArpt";
                    case 0x80: return "LeftBaroPush";
                }
            } else if(offset == 6) {
                switch(flag) {
                    case 0x01: return "LeftBaroPull";
                    case 0x02: return "LeftBaroDec";
                    case 0x04: return "LeftBaroInc";
                    case 0x08: return "LeftInHg";
                    case 0x10: return "LeftHPa";
                    case 0x20: return "LeftModeLs";
                    case 0x40: return "LeftModeVor";
                    case 0x80: return "LeftModeNav";
                }
            } else if(offset == 7) {
                switch(flag) {
                    case 0x01: return "LeftModeArc";
                    case 0x02: return "LeftModePlan";
                    case 0x04: return "LeftRange10";
                    case 0x08: return "LeftRange20";
                    case 0x10: return "LeftRange40";
                    case 0x20: return "LeftRange80";
                    case 0x40: return "LeftRange160";
                    case 0x80: return "LeftRange320";
                }
            } else if(offset == 8) {
                switch(flag) {
                    case 0x01: return "LeftNeedle1Adf";
                    case 0x02: return "LeftNeedle1Off";
                    case 0x04: return "LeftNeedle1Vor";
                    case 0x08: return "LeftNeedle2Adf";
                    case 0x10: return "LeftNeedle2Off";
                    case 0x20: return "LeftNeedle2Vor";
                }
            }
            // Right EFIS controls
            else if(offset == 9) {
                switch(flag) {
                    case 0x01: return "RightFd";
                    case 0x02: return "RightLs";
                    case 0x04: return "RightCstr";
                    case 0x08: return "RightWpt";
                    case 0x10: return "RightVorD";
                    case 0x20: return "RightNdb";
                    case 0x40: return "RightArpt";
                    case 0x80: return "RightBaroPush";
                }
            } else if(offset == 10) {
                switch(flag) {
                    case 0x01: return "RightBaroPull";
                    case 0x02: return "RightBaroDec";
                    case 0x04: return "RightBaroInc";
                    case 0x08: return "RightInHg";
                    case 0x10: return "RightHPa";
                    case 0x20: return "RightModeLs";
                    case 0x40: return "RightModeVor";
                    case 0x80: return "RightModeNav";
                }
            } else if(offset == 11) {
                switch(flag) {
                    case 0x01: return "RightModeArc";
                    case 0x02: return "RightModePlan";
                    case 0x04: return "RightRange10";
                    case 0x08: return "RightRange20";
                    case 0x10: return "RightRange40";
                    case 0x20: return "RightRange80";
                    case 0x40: return "RightRange160";
                    case 0x80: return "RightRange320";
                }
            } else if(offset == 12) {
                switch(flag) {
                    case 0x01: return "RightNeedle1Vor";
                    case 0x02: return "RightNeedle1Off";
                    case 0x04: return "RightNeedle1Adf";
                    case 0x08: return "RightNeedle2Vor";
                    case 0x10: return "RightNeedle2Off";
                    case 0x20: return "RightNeedle2Adf";
                }
            }

            return null;
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

            // Use fixed package number 1 like working Python implementation
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
            
            // Set mode indicator flags based on documentation image
            
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
            // Python line 93: ("lat", Flag('hdg-trk-lat_lat', Byte.H0, 0x20))
            if(state.LatIndicator) {
                payload[0x1F] |= 0x20;  // LAT indicator
            }
            
            // Middle section indicators at A5 position (0x20 = a[5])
            // From Python reference implementation:
            // ("vshdg", Flag('hdg-v/s_hdg', Byte.A5, 0x08))
            // ("vs", Flag('hdg-v/s_v/s', Byte.A5, 0x04))
            // ("ftrk", Flag('trk-fpa_trk', Byte.A5, 0x02))
            // ("ffpa", Flag('trk-fpa_fpa', Byte.A5, 0x01))
            
            // The middle section has TWO independent groups:
            // 1. HDG/TRK (bits 0x08 for HDG, 0x02 for TRK)
            // 2. V/S/FPA (bits 0x04 for V/S, 0x01 for FPA)
            
            // TRK indicator in middle section
            if(state.TrkMiddleIndicator) {
                payload[0x20] |= 0x02;  // TRK indicator in middle
            }
            // HDG indicator in middle section - shows when NOT in TRK mode
            // Note: We don't require V/S or FPA to be selected for HDG to show
            // The UI controls this independently now
            else {
                // Show HDG when TRK is not active
                // The vshdg flag (0x08) represents HDG mode in the middle section
                payload[0x20] |= 0x08;  // HDG indicator in middle
            }
            
            // V/S indicator in middle section
            if(state.VsMiddleIndicator) {
                payload[0x20] |= 0x04;  // V/S indicator in middle
            }
            
            // FPA indicator in middle section
            if(state.FpaMiddleIndicator) {
                payload[0x20] |= 0x01;  // FPA indicator in middle
            }
            
            // Altitude managed mode indicator
            // From Python line 105: ("alt_managed", Flag('alt managed', Byte.V1, 0x10))
            if(state.AltitudeManaged) {
                payload[0x28] |= 0x10;  // ALT managed indicator at v[1] position
            }
            
            // LVL/CH indicators (brackets around a0 in doc)
            // Python line 106: ("vs_horz", Flag('v/s plus horizontal', Byte.A0, 0x10))
            if(state.VsHorzIndicator) {
                payload[0x25] |= 0x20;  // Horizontal indicator (different bit to avoid minus)
            }
            
            // Python line 108: ("lvl", Flag('lvl change', Byte.A2, 0x10))
            if(state.LvlIndicator) {
                payload[0x23] |= 0x10;  // Level change indicator
            }
            
            // Python line 109: ("lvl_left", Flag('lvl change left', Byte.A3, 0x10))
            // This is the LEFT BRACKET
            if(state.LvlLeftBracket) {
                payload[0x22] |= 0x10;  // Left bracket
            }
            
            // Python line 110: ("lvl_right", Flag('lvl change right', Byte.A1, 0x10))
            // This is the RIGHT BRACKET
            if(state.LvlRightBracket) {
                payload[0x24] |= 0x10;  // Right bracket
            }
            
            // V/S/FPA indicators above V/S digits (black in doc, right side)
            // Python line 113: ("fvs", Flag('v/s-fpa_v/s', Byte.V0, 0x40))
            // Python line 114: ("ffpa2", Flag('v/s-fpa_fpa', Byte.V0, 0x80))
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

            // Python line 318-333 shows EXACT buffer write order starting at 0x19 (byte 25):
            // s[2], s[1] | bl[Byte.S1.value], s[0], h[3] | bl[Byte.H3.value], h[2], h[1], h[0] | bl[Byte.H0.value],
            // a[5] | bl[Byte.A5.value], a[4] | bl[Byte.A4.value], a[3] | bl[Byte.A3.value],
            // a[2] | bl[Byte.A2.value], a[1] | bl[Byte.A1.value], a[0] | v[4] | bl[Byte.A0.value],
            // v[3] | bl[Byte.V3.value], v[2] | bl[Byte.V2.value], v[1] | bl[Byte.V1.value], v[0] | bl[Byte.V0.value]

            // This means the buffer layout is:
            // 0x19: s[2]
            // 0x1A: s[1] (+ speed flags)
            // 0x1B: s[0]
            // 0x1C: h[3] (+ heading flags)
            // 0x1D: h[2]
            // 0x1E: h[1]
            // 0x1F: h[0] (+ heading flags)
            // 0x20: a[5] (+ altitude flags)
            // 0x21: a[4] (+ altitude flags)
            // 0x22: a[3] (+ altitude flags)
            // 0x23: a[2] (+ altitude flags)
            // 0x24: a[1] (+ altitude flags)
            // 0x25: a[0] - shares with v[4]
            // 0x26: v[3] (+ v/s flags)
            // 0x27: v[2] (+ v/s flags)
            // 0x28: v[1] (+ v/s flags)
            // 0x29: v[0] (+ v/s flags)

            // SPD display - direct encoding
            if(state.Speed.HasValue) {
                var speed = Math.Max(0, Math.Min(999, state.Speed.Value));
                buffer[0x19] = _EfisDigitValues[(speed / 100) % 10];   // s[2]
                buffer[0x1A] = _EfisDigitValues[(speed / 10) % 10];    // s[1]
                buffer[0x1B] = _EfisDigitValues[speed % 10];           // s[0]
                
                // MACH decimal point - only in MACH mode OR if explicitly requested
                if(state.SpeedIsMach || state.SpeedDot) {
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
                
                // HDG decimal point if requested
                if(state.HeadingDot) {
                    buffer[0x1D] |= 0x01;  // Decimal in heading display
                }
            }

            // ALT display - nibble swapped encoding
            if(state.Altitude.HasValue) {
                var altitude = Math.Max(0, Math.Min(99999, state.Altitude.Value));
                var a = DataFromStringSwapped(5, altitude);
                buffer[0x20] |= a[5];  // a[5]
                buffer[0x21] |= a[4];  // a[4]
                buffer[0x22] |= a[3];  // a[3]
                buffer[0x23] |= a[2];  // a[2]
                buffer[0x24] |= a[1];  // a[1]
                buffer[0x25] |= a[0];  // a[0] - shares with v[4]
                
                // ALT decimal point if requested
                if(state.AltitudeDot) {
                    buffer[0x22] |= 0x01;  // Decimal in altitude display
                }
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
                
                // Set plus/minus indicator using two bits:
                // Python line 106: ("vs_horz", Flag('v/s plus horizontal', Byte.A0, 0x10, True))  - at 0x25
                // Python line 107: ("vs_vert", Flag('v/s plus vertical', Byte.V2, 0x10))        - at 0x27
                //
                // The hardware has a plus-shaped segment:
                // - vs_horz (bit at 0x25) controls horizontal bar (default ON in Python)
                // - vs_vert (bit at 0x27) controls vertical bar
                // 
                // Horizontal only (vs_horz=1, vs_vert=0) = minus sign (-)
                // Both bars (vs_horz=1, vs_vert=1) = plus sign (+)
                //
                // IMPORTANT: vs_horz defaults to True in Python, meaning horizontal bar is always on
                // We need to explicitly set this bit to show the horizontal bar
                
                // Always show horizontal bar (this is the base minus sign)
                buffer[0x25] |= 0x10;  // vs_horz - horizontal bar always on
                
                // For POSITIVE values, also show vertical bar (completes the plus sign)
                if(vs >= 0) {
                    buffer[0x27] |= 0x10;  // vs_vert - add vertical bar for positive = plus sign
                }
                // For NEGATIVE values, DON'T set vertical bar - only horizontal shows = minus sign
                
                // V/S decimal point if requested
                if(state.VsDot) {
                    buffer[0x27] |= 0x01;  // Decimal in V/S display
                }
            }
        }

        // Implements Python's data_from_string_swapped function
        // Python lines 134-142
        byte[] DataFromStringSwapped(int numDigits, int value)
        {
            // Step 1: Create array and populate with digits (Python data_from_string, line 125-130)
            var d = new byte[numDigits];
            
            // Convert value to string and pad with zeros
            var str = value.ToString().PadLeft(numDigits, '0');
            
            System.Diagnostics.Debug.WriteLine($"[FCU] DataFromStringSwapped({numDigits}, {value}) -> string: '{str}'");
            
            // Store in REVERSE order: d[l-1-i] = representations[string[i]]
            // This means: for "320", d[2]='3', d[1]='2', d[0]='0'
            for(int i = 0; i < numDigits; i++) {
                int digit = str[i] - '0';
                d[numDigits - 1 - i] = _EfisDigitValues[digit];
            }
            
            System.Diagnostics.Debug.WriteLine($"[FCU]   After reverse store: {BitConverter.ToString(d)}");
            
            // Step 2: Append 0 (Python line 135: d.append(0))
            var result = new byte[numDigits + 1];
            Array.Copy(d, result, numDigits);
            result[numDigits] = 0;
            
            System.Diagnostics.Debug.WriteLine($"[FCU]   After append 0: {BitConverter.ToString(result)}");
            
            // Step 3: Apply swap_nibbles to each element (Python line 137-138)
            for(int i = 0; i < result.Length; i++) {
                result[i] = (byte)(((result[i] & 0x0F) << 4) | ((result[i] & 0xF0) >> 4));
            }
            
            System.Diagnostics.Debug.WriteLine($"[FCU]   After swap nibbles: {BitConverter.ToString(result)}");
            
            // Step 4: Apply complex redistribution (Python lines 139-141)
            // for i in range(0, len(d) - 1):
            //     d[l-i] = (d[l-i] & 0x0f) | (d[l-1-i] & 0xf0)
            //     d[l-1-i] = d[l-1-i] & 0x0f
            int l = numDigits;
            for(int i = 0; i < l; i++) {
                var before_high = result[l - i];
                var before_low = result[l - 1 - i];
                result[l - i] = (byte)((result[l - i] & 0x0F) | (result[l - 1 - i] & 0xF0));
                result[l - 1 - i] = (byte)(result[l - 1 - i] & 0x0F);
                System.Diagnostics.Debug.WriteLine($"[FCU]   Redistrib i={i}: result[{l-i}]=0x{before_high:X2}->0x{result[l-i]:X2}, result[{l-1-i}]=0x{before_low:X2}->0x{result[l-1-i]:X2}");
            }
            
            System.Diagnostics.Debug.WriteLine($"[FCU]   Final result: {BitConverter.ToString(result)}");
            
            return result;
        }

        void EncodeSevenSegmentByte(byte[] buffer, int offset, int digit)
        {
            if(digit >= 0 && digit <= 9) {
                buffer[offset] = _EfisDigitValues[digit];
            }
        }

        void EncodeSevenSegmentNibbles(byte[] buffer, int highOffset, int lowOffset, int digit)
        {
            if(digit >= 0 && digit <= 9) {
                var value = _EfisDigitValues[digit];
                // Step 1: Swap nibbles (matches Python swap_nibbles)
                var swapped = (byte)(((value & 0x0F) << 4) | ((value & 0xF0) >> 4));
                
                // Step 2: Distribute to buffer
                // The "wired mapping" in Python: high byte gets low nibble, low byte gets high nibble
                buffer[highOffset] |= (byte)(swapped & 0x0F);        // Low nibble to high offset
                buffer[lowOffset] |= (byte)(swapped & 0xF0);         // High nibble to low offset
            }
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

            // EFIS display uses data_from_string_swapped_efis encoding
            // Python: data_from_string_swapped_efis(4, pressure)
            var pressureStr = pressure.ToString().PadLeft(4, '0');
            
            // Detect inHg mode: values >= 2000 are inHg (displayed as XX.XX)
            // hPa range: 870-1085, inHg range: 2570-3200 (representing 25.70-32.00)
            bool isInHg = pressure >= 2000;
            
            // Encode using EFIS-specific bit remapping
            var encoded = DataFromStringSwappedEfis(4, pressureStr);
            
            packet[0x19] = encoded[0];  // leftmost digit (thousands)
            packet[0x1A] = encoded[1];  // hundreds
            packet[0x1B] = encoded[2];  // tens
            packet[0x1C] = encoded[3];  // ones (rightmost)

            // Add decimal point for inHg mode (between tens and ones: after tens digit)
            // The decimal point is typically bit 0x01 of the digit display byte
            if(isInHg) {
                packet[0x1B] |= 0x01;  // Add decimal point after tens digit (29.92)
            }

            System.Diagnostics.Debug.WriteLine($"[EFIS] Pressure {pressure} ({(isInHg ? "inHg" : "hPa")}) -> '{pressureStr}' -> bytes: [0x19]=0x{packet[0x19]:X2} [0x1A]=0x{packet[0x1A]:X2} [0x1B]=0x{packet[0x1B]:X2} [0x1C]=0x{packet[0x1C]:X2}");

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

        // Implements Python's data_from_string_swapped_efis function
        // This applies EFIS-specific bit remapping to the standard segment values
        byte[] DataFromStringSwappedEfis(int numDigits, string str)
        {
            // Step 1: Get base segment values using data_from_string
            // (which just converts digits to _EfisDigitValues)
            var d = new byte[numDigits];
            for(int i = 0; i < numDigits; i++) {
                int digit = str[i] - '0';
                d[i] = _EfisDigitValues[digit];
            }
            
            System.Diagnostics.Debug.WriteLine($"[EFIS] DataFromStringSwappedEfis('{str}') -> base values: {BitConverter.ToString(d)}");
            
            // Step 2: Apply EFIS wired segment mapping (bit remapping)
            // Python code:
            // n[i] |= 0x01 if d[i] & 0x08 else 0
            // n[i] |= 0x02 if d[i] & 0x04 else 0
            // n[i] |= 0x04 if d[i] & 0x02 else 0
            // n[i] |= 0x08 if d[i] & 0x10 else 0
            // n[i] |= 0x10 if d[i] & 0x80 else 0
            // n[i] |= 0x20 if d[i] & 0x40 else 0
            // n[i] |= 0x40 if d[i] & 0x20 else 0
            // n[i] |= 0x80 if d[i] & 0x01 else 0
            
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
            
            System.Diagnostics.Debug.WriteLine($"[EFIS] After bit remapping: {BitConverter.ToString(n)}");
            
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

    public class FcuEfisState : IFrontpanelState
    {
        public int? Speed { get; set; }
        public int? Heading { get; set; }
        public int? Altitude { get; set; }
        public int? VerticalSpeed { get; set; }

        public bool SpeedIsMach { get; set; } = false;
        public bool HeadingIsTrack { get; set; } = false;
        public bool VsIsFpa { get; set; } = false;

        public bool SpeedManaged { get; set; } = false;
        public bool HeadingManaged { get; set; } = false;
        public bool AltitudeManaged { get; set; } = false;

        public bool SpeedDot { get; set; } = false;
        public bool HeadingDot { get; set; } = false;
        public bool AltitudeDot { get; set; } = false;
        public bool VsDot { get; set; } = false;

        public bool LatIndicator { get; set; } = false;
        public bool VsMiddleIndicator { get; set; } = false;
        public bool TrkMiddleIndicator { get; set; } = false;
        public bool FpaMiddleIndicator { get; set; } = false;
        
        public bool LvlIndicator { get; set; } = false;
        public bool LvlLeftBracket { get; set; } = false;
        public bool LvlRightBracket { get; set; } = false;
        public bool VsHorzIndicator { get; set; } = false;

        public int? LeftBaroPressure { get; set; }
        public bool LeftBaroQnh { get; set; }
        public bool LeftBaroQfe { get; set; }

        public int? RightBaroPressure { get; set; }
        public bool RightBaroQnh { get; set; }
        public bool RightBaroQfe { get; set; }
    }

    public class FcuEfisLeds : IFrontpanelLeds
    {
        public bool Loc { get; set; }
        public bool Ap1 { get; set; }
        public bool Ap2 { get; set; }
        public bool AThr { get; set; }
        public bool Exped { get; set; }
        public byte ExpedYellowBrightness { get; set; } = 0;
        public bool Appr { get; set; }

        public bool LeftFd { get; set; }
        public bool LeftLs { get; set; }
        public bool LeftCstr { get; set; }
        public bool LeftWpt { get; set; }
        public bool LeftVorD { get; set; }
        public bool LeftNdb { get; set; }
        public bool LeftArpt { get; set; }

        public bool RightFd { get; set; }
        public bool RightLs { get; set; }
        public bool RightCstr { get; set; }
        public bool RightWpt { get; set; }
        public bool RightVorD { get; set; }
        public bool RightNdb { get; set; }
        public bool RightArpt { get; set; }
    }
}
