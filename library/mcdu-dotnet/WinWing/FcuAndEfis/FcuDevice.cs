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
using HidSharp;

namespace McduDotNet.WinWing.FcuAndEfis
{
    class FcuDevice : IFgcp
    {
        protected HidDevice _HidDevice;
        protected HidStream? _HidStream;

        public UsbDevice UsbDevice { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="usbDevice"></param>
        public FcuDevice(HidDevice hidDevice, UsbDevice usbDevice)
        {
            _HidDevice = hidDevice;
            UsbDevice = usbDevice;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                var hidStream = _HidStream;
                _HidStream = null;
                try {
                    hidStream?.Dispose();
                } catch {
                    ;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() => UsbDevice.ToString();

        /// <summary>
        /// Initialises the USB device to a known state.
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
        }
    }
}
