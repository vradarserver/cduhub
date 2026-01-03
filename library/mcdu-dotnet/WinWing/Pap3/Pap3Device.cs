// Copyright © 2025 onwards, Andrew Whewell, Laurent Andre
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
        // Prefix is 0x0100 for LED commands
        const ushort _Pap3Prefix = 0x0100;

        // Seven-segment display digit values (inherited from FCU/EFIS encoding)
        static readonly byte[] _DigitValues = new byte[] {
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

        HidDevice _HidDevice;
        HidStream _HidStream;
        bool _Disposed;
        CancellationTokenSource _InputLoopCancellationTokenSource;
        Task _InputLoopTask;
        readonly byte[] _LastInputReport = new byte[25];

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

            if(state is Pap3State pap3State) {
                var commands = BuildDisplayCommands(pap3State);
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

            // Send brightness commands for PAP-3
            // Verified from hardware testing:
            // - 0x00: Panel backlight
            // - 0x01: Digital Tube Backlight (LCD) - Working correctly
            // - 0x02: Marker Light (LEDs) - Verified working
            SendBrightnessCommand(_Pap3Prefix, 0x00, panelBacklight);
            SendBrightnessCommand(_Pap3Prefix, 0x01, lcdBacklight);
            SendBrightnessCommand(_Pap3Prefix, 0x02, ledBacklight);
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
            
            // TODO: Implement display command building based on actual hardware protocol
            // This is a placeholder that follows the FCU pattern
            var payload = new byte[64];

            payload[0] = 0xF0;
            payload[1] = 0x00;
            payload[2] = 0x01; // Sequence number
            payload[3] = 0x31; // TODO: Verify this value for PAP-3
            payload[4] = (byte)((_Pap3Prefix >> 8) & 0xFF);
            payload[5] = (byte)(_Pap3Prefix & 0xFF);
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

            // Encode displays
            EncodeDisplays(payload, state);

            commands.Add(payload);

            return commands;
        }

        void EncodeDisplays(byte[] buffer, Pap3State state)
        {
            // TODO: Implement actual display encoding based on PAP-3 hardware protocol
            // This is a placeholder implementation
            // The actual offsets and encoding will need to be determined through hardware testing
        }

        List<byte[]> BuildLedCommands(Pap3Leds leds)
        {
            var commands = new List<byte[]>();

            // LED command codes verified from hardware testing
            // Format: 02 01 00 00 00 03 49 [code] [value] 00 00 00 00 00
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x03, leds.N1));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x04, leds.Speed));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x05, leds.Vnav));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x06, leds.LvlChg));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x07, leds.HdgSel));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x08, leds.Lnav));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x09, leds.VorLoc));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x0A, leds.App));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x0B, leds.AltHold));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x0C, leds.Vs));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x0D, leds.CmdA));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x0E, leds.CwsA));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x0F, leds.CmdB));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x10, leds.CwsB));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x11, leds.AtArm));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x12, leds.FdL));
            commands.Add(BuildLedCommand(_Pap3Prefix, 0x13, leds.FdR));

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
