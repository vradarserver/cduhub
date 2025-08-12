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
using System.Text;
using HidSharp;
using McduDotNet.WinWing.Mcdu;

namespace McduDotNet
{
    /// <summary>
    /// Finds USB devices and creates instances of <see cref="ICdu"/> implementations
    /// for them.
    /// </summary>
    public static class CduFactory
    {
        /// <summary>
        /// The identifier for a WinWing MCDU device set to the left-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingMcduCaptainDevice = new DeviceIdentifier(
            "Winwing MCDU (Captain)", 0x4098, 0xBB36, Device.WinWingMcdu, DeviceUser.Captain
        );

        /// <summary>
        /// The identifier for a WinWing MCDU device set to the right-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingMcduFirstOfficerDevice = new DeviceIdentifier(
            "Winwing MCDU (F/O)", 0x4098, 0xBB3E, Device.WinWingMcdu, DeviceUser.FirstOfficer
        );

        /// <summary>
        /// The identifier for a WinWing MCDU device set to the observer seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingMcduObserverDevice = new DeviceIdentifier(
            "Winwing MCDU (Observer)", 0x4098, 0xBB3A, Device.WinWingMcdu, DeviceUser.Observer
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the left-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp7CaptainDevice = new DeviceIdentifier(
            "Winwing PFP-7 (Captain)", 0x4098, 0xBB37, Device.WinWingPfp7, DeviceUser.Captain
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the right-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp7FirstOfficerDevice = new DeviceIdentifier(
            "Winwing PFP-7 (F/O)", 0x4098, 0xBB3F, Device.WinWingPfp7, DeviceUser.FirstOfficer
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the observer seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp7ObserverDevice = new DeviceIdentifier(
            "Winwing PFP-7 (Observer)", 0x4098, 0xBB3B, Device.WinWingPfp7, DeviceUser.Observer
        );

        /// <summary>
        /// A collection of device identifiers for all supported devices.
        /// </summary>
        public static readonly IReadOnlyList<DeviceIdentifier> AllKnownDevices = new DeviceIdentifier[] {
            WinWingMcduCaptainDevice,
            WinWingMcduFirstOfficerDevice,
            WinWingMcduObserverDevice,

            WinWingPfp7CaptainDevice,
            WinWingPfp7FirstOfficerDevice,
            WinWingPfp7ObserverDevice,
        };

        /// <summary>
        /// Returns a device identifier corresponding to the vendor and product IDs passed
        /// across, or null if the vendor and product ID do not correspond with a CDU that
        /// the library can interact with.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static DeviceIdentifier GetDeviceIdentifierForUsbIdentifiers(
            int vendorId,
            int productId
        )
        {
            return AllKnownDevices.FirstOrDefault(deviceIdentifier =>
                   deviceIdentifier.UsbVendorId == vendorId
                && deviceIdentifier.UsbProductId == productId
            );
        }

        /// <summary>
        /// Returns a collection of all MCDU devices that can be found on the local machine.
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<DeviceIdentifier> FindLocalDevices()
        {
            var result = new List<DeviceIdentifier>();

            var local = DeviceList.Local;
            foreach(var hidDevice in local.GetHidDevices()) {
                var deviceIdentifier = GetDeviceIdentifierForUsbIdentifiers(
                    hidDevice.VendorID,
                    hidDevice.ProductID
                );
                if(deviceIdentifier != null) {
                    result.Add(deviceIdentifier);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an initialised connection to a CDU device. If the device ID is not
        /// specified then the first CDU found on the system is used. If the requested CDU
        /// cannot be found (or there are no CDUs to default to) then null is returned.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static ICdu ConnectLocal(DeviceIdentifier deviceId = null)
        {
            ICdu result = null;

            if(deviceId == null) {
                deviceId = FindLocalDevices().FirstOrDefault();
            }

            if(deviceId != null) {
                var hidDevice = DeviceList
                    .Local
                    .GetHidDevices(
                        vendorID: deviceId.UsbVendorId,
                        productID: deviceId.UsbProductId
                    )
                    .FirstOrDefault();
                if(hidDevice != null) {
                    var mcdu = new McduDevice(hidDevice, deviceId);
                    mcdu.Initialise();
                    result = mcdu;
                }
            }

            return result;
        }
    }
}
