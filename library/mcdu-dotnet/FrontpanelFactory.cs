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
using System.Linq;
using HidSharp;
using WwDevicesDotNet.WinWing.FcuAndEfis;
using WwDevicesDotNet.WinWing.Pap3;

namespace WwDevicesDotNet
{
    /// <summary>
    /// Finds USB devices and creates instances of <see cref="IFrontpanel"/> implementations
    /// for them. Supports FCU, EFIS, and other WinWing control panels.
    /// </summary>
    public static class FrontpanelFactory
    {
        //          ARE YOU HERE LOOKING FOR THE LIST OF ALL KNOWN DEVICE IDENTIFIERS?
        //                       They've moved to SupportedDevices.cs

        /// <summary>
        /// Returns a device identifier corresponding to the vendor and product IDs passed
        /// across, or null if the vendor and product ID do not correspond with a frontpanel
        /// that the library can interact with.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static DeviceIdentifier GetDeviceIdentifierForUsbIdentifiers(
            int vendorId,
            int productId
        )
        {
            return SupportedDevices
                .AllSupportedFrontpanels
                .FirstOrDefault(deviceIdentifier =>
                       deviceIdentifier.UsbVendorId == vendorId
                    && deviceIdentifier.UsbProductId == productId
                );
        }

        /// <summary>
        /// Returns a collection of all frontpanel devices that can be found on the local machine.
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<DeviceIdentifier> FindLocalDevices()
        {
            var local = DeviceList.Local;
            return local
                .GetHidDevices()
                .Select(hidDevice => GetDeviceIdentifierForUsbIdentifiers(
                    hidDevice.VendorID,
                    hidDevice.ProductID
                ))
                .Where(deviceIdentifier => deviceIdentifier != null)
                .ToList();
        }

        /// <summary>
        /// Creates an initialised connection to a frontpanel device. If the device ID is not
        /// specified then the first frontpanel found on the system is used. If the requested
        /// frontpanel cannot be found (or there are no frontpanels to default to) then null
        /// is returned.
        /// </summary>
        /// <param name="deviceId">
        /// The specific device to connect to, if null then the first device found is
        /// used. Defaults to null.
        /// </param>
        /// <param name="device">
        /// If not null then only devices for this product are considered if <paramref
        /// name="deviceId"/> is not supplied. Defaults to null.
        /// </param>
        /// <param name="deviceType">
        /// If not null then only devices for this category of frontpanel are considered if
        /// <paramref name="deviceId"/> is not supplied. Defaults to null.
        /// </param>
        /// <returns></returns>
        public static IFrontpanel ConnectLocal(
            DeviceIdentifier deviceId = null,
            Device? device = null,
            DeviceType? deviceType = null
        )
        {
            IFrontpanel result = null;

            if(deviceId == null) {
                deviceId = FindLocalDevices()
                    .Where(candidate =>
                           (device == null || candidate.Device == device)
                        && (deviceType == null || candidate.DeviceType == deviceType)
                    )
                    // The order selected here is only to make it deterministic
                    .OrderBy(candidate => candidate.UsbVendorId)
                    .ThenBy(candidate => candidate.UsbProductId)
                    .FirstOrDefault();
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
                    switch(deviceId.Device) {
                        case Device.WinWingFcu:
                        case Device.WinWingFcuLeftEfis:
                        case Device.WinWingFcuRightEfis:
                        case Device.WinWingFcuBothEfis:
                            // All FCU configurations use the same device class
                            // The device ID determines which configuration is present
                            var fcu = new FcuEfisDevice(hidDevice, deviceId);
                            fcu.Initialise();
                            result = fcu;
                            break;
                        case Device.WinWingPap3:
                            var pap3 = new Pap3Device(hidDevice, deviceId);
                            pap3.Initialise();
                            result = pap3;
                            break;
                    }
                }
            }

            return result;
        }
    }
}
