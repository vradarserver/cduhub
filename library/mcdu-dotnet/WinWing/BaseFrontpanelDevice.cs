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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace WwDevicesDotNet.WinWing
{
    /// <summary>
    /// Base class for WinWing frontpanel devices (FCU, EFIS, PAP-3, etc.).
    /// Handles common HID communication, input loop, and event infrastructure.
    /// </summary>
    /// <typeparam name="TControl">The control enum type for this device.</typeparam>
    public abstract class BaseFrontpanelDevice<TControl> : IFrontpanel
        where TControl : struct, Enum
    {
        protected HidDevice _HidDevice;
        protected HidStream _HidStream;
        protected bool _Disposed;
        protected CancellationTokenSource _InputLoopCancellationTokenSource;
        protected Task _InputLoopTask;
        protected readonly byte[] _LastInputReport = new byte[25];

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
        /// Initializes a new instance of the <see cref="BaseFrontpanelDevice{TControl}"/> class.
        /// </summary>
        /// <param name="hidDevice">The HID device to communicate with.</param>
        /// <param name="deviceId">The device identifier.</param>
        protected BaseFrontpanelDevice(HidDevice hidDevice, DeviceIdentifier deviceId)
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

            SendInitPacket();

            DeviceList.Local.Changed += HidSharpDeviceList_Changed;

            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => RunInputLoop(_InputLoopCancellationTokenSource.Token));
        }

        /// <summary>
        /// Sends device-specific initialization packet(s).
        /// </summary>
        protected abstract void SendInitPacket();

        /// <summary>
        /// Gets the control enum value for the given input report offset and flag.
        /// </summary>
        /// <param name="offset">The byte offset in the input report.</param>
        /// <param name="flag">The bit flag within the byte.</param>
        /// <returns>The control enum value, or null if no control matches.</returns>
        protected abstract TControl? GetControl(int offset, byte flag);

        /// <inheritdoc/>
        public abstract void UpdateDisplay(IFrontpanelState state);

        /// <inheritdoc/>
        public abstract void UpdateLeds(IFrontpanelLeds leds);

        /// <inheritdoc/>
        public abstract void SetBrightness(byte panelBacklight, byte lcdBacklight, byte ledBacklight);

        /// <summary>
        /// Sends a command to the device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        protected void SendCommand(byte[] data)
        {
            if(!IsConnected)
                throw new InvalidOperationException("Device is not connected.");

            _HidStream.Write(data);
        }

        /// <summary>
        /// Sends a brightness command to the device.
        /// </summary>
        /// <param name="prefix">The device-specific command prefix.</param>
        /// <param name="variableType">The brightness variable type.</param>
        /// <param name="value">The brightness value (0-255).</param>
        protected void SendBrightnessCommand(ushort prefix, byte variableType, byte value)
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

        /// <summary>
        /// Runs the input loop on a background thread.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the loop.</param>
        protected virtual void RunInputLoop(CancellationToken cancellationToken)
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
                    // Expected when no data available - Read() blocks until data or timeout
                } catch(ObjectDisposedException) {
                    break;
                } catch(System.IO.IOException) {
                    break;
                }
            }
        }

        /// <summary>
        /// Processes an input report from the device.
        /// </summary>
        /// <param name="data">The report data.</param>
        /// <param name="length">The length of the data.</param>
        protected virtual void ProcessReport(byte[] data, int length)
        {
            if(length < 25)
                return;

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

        /// <summary>
        /// Handles device list changes to detect disconnection.
        /// </summary>
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

        /// <summary>
        /// Raises the Disconnected event.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if(_Disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by this device.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
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
