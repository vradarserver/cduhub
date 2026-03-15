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

namespace McduDotNet
{
    /// <summary>
    /// Creates instances of interfaces from USB devices.
    /// </summary>
    static class SupportedDeviceFactory
    {
        /// <summary>
        /// Creates an instance of an object that implements <see cref="ICdu"/> for the
        /// HID and USB devices passed across.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="usbDevice"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="usbDevice"/> does not represent a supported device.
        /// </exception>
        public static ICdu CreateCdu(HidDevice hidDevice, UsbDevice usbDevice)
        {
            var id = usbDevice.Id;

            WinWing.CommonWinWingPanel? result = null;

            if(   id == SupportedDevices.WinWingMcduCaptain.Id
               || id == SupportedDevices.WinWingMcduFirstOfficer.Id
               || id == SupportedDevices.WinWingMcduObserver.Id
            ) {
                result = new WinWing.Mcdu.McduDevice(hidDevice, usbDevice);
            } else if(id == SupportedDevices.WinWingPfp3NCaptain.Id
                   || id == SupportedDevices.WinWingPfp3NFirstOfficer.Id
                   || id == SupportedDevices.WinWingPfp3NObserver.Id
            ) {
                result = new WinWing.Pfp3N.Pfp3NDevice(hidDevice, usbDevice);
            } else if(id == SupportedDevices.WinWingPfp7Captain.Id
                   || id == SupportedDevices.WinWingPfp7FirstOfficer.Id
                   || id == SupportedDevices.WinWingPfp7Observer.Id
            ) {
                result = new WinWing.Pfp7.Pfp7Device(hidDevice, usbDevice);
            }

            if(result == null) {
                throw new InvalidOperationException($"{usbDevice} does not represent a supported CDU device");
            }

            result.Initialise();

            return result;
        }

        /// <summary>
        /// Creates an instance of an object that implements <see cref="IFgcp"/> for the
        /// HID and USB devices passed across.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="usbDevice"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="usbDevice"/> does not represent a supported device.
        /// </exception>
        public static IFgcp CreateFgcp(HidDevice device, UsbDevice usbDevice)
        {
            var id = usbDevice.Id;

            WinWing.FcuAndEfis.FcuDevice? result = null;

            if(   id == SupportedDevices.WinWingFcu.Id
               || id == SupportedDevices.WinWingFcuBothEfis.Id
               || id == SupportedDevices.WinWingFcuLeftEfis.Id
               || id == SupportedDevices.WinWingFcuRightEfis.Id
            ) {
                result = new WinWing.FcuAndEfis.FcuDevice(device, usbDevice);
            }

            if(result == null) {
                throw new InvalidOperationException($"{usbDevice} does not represent a supported FGCP device");
            }

            result.Initialise();

            return result;
        }
    }
}
