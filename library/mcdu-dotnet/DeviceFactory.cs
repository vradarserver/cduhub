// Copyright © 2026 onwards, Andrew Whewell
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

namespace McduDotNet
{
    /// <summary>
    /// Finds and creates instances of USB devices.
    /// </summary>
    public static class DeviceFactory
    {
        /// <summary>
        /// Returns a <see cref="UsbDevice"/> corresponding to the USB device ID passed
        /// across, or null if ID does not correspond with a device that the library can
        /// interact with.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static UsbDevice? GetDeviceForUsbDeviceId(UsbDeviceId id)
        {
            return SupportedDevices
                .AllSupportedDevices
                .Where(usbDevice => usbDevice.Id == id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns a <see cref="UsbDevice"/> corresponding to the USB device ID passed
        /// across, or null if ID does not correspond with a device that the library can
        /// interact with.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static UsbDevice? GetDeviceForUsbIdentifiers(int vendorId, int productId)
        {
            UsbDevice? result = null;

            if(   vendorId > 0 && vendorId < ushort.MaxValue
               && productId > 0 && productId < ushort.MaxValue
            ) {
                result = GetDeviceForUsbDeviceId(new((ushort)vendorId, (ushort)productId));
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of all supported devices that can be found on the local machine.
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<UsbDevice> FindLocalDevices()
        {
            var result = new List<UsbDevice>();

            var local = DeviceList.Local;
            foreach(var hidDevice in local.GetHidDevices()) {
                var usbDevice = GetDeviceForUsbIdentifiers(
                    hidDevice.VendorID,
                    hidDevice.ProductID
                );
                if(usbDevice != null) {
                    result.Add(usbDevice);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns an object that can control a CDU device connected to the local system,
        /// or null if no such device exists.
        /// </summary>
        /// <param name="usbDevice"></param>
        /// <param name="aircraftFamily"></param>
        /// <param name="aircraftManufacturer"></param>
        /// <param name="equipmentLocation"></param>
        /// <returns></returns>
        public static ICdu? ConnectLocalCdu(
            UsbDevice? usbDevice = null,
            AircraftFamily? aircraftFamily = null,
            AircraftManufacturer? aircraftManufacturer = null,
            EquipmentLocation? equipmentLocation = null
        )
        {
            ICdu? result = null;

            usbDevice = FindUsbDeviceCandidate(
                EquipmentType.Cdu,
                usbDevice,
                aircraftFamily,
                aircraftManufacturer,
                equipmentLocation
            );
            var hidDevice = OpenHidDevice(usbDevice);

            if(hidDevice != null && usbDevice != null) {
                result = SupportedDeviceFactory.CreateCdu(hidDevice, usbDevice);
            }

            return result;
        }

        /// <summary>
        /// Returns an object that can control an FGCP device (I.E. an Airbus FCU or a
        /// Boeing MCP) connected to the local system, or null if no such device exists.
        /// </summary>
        /// <param name="usbDevice"></param>
        /// <param name="aircraftFamily"></param>
        /// <param name="aircraftManufacturer"></param>
        /// <param name="equipmentLocation"></param>
        /// <returns></returns>
        public static IFgcp? ConnectLocalFgcp(
            UsbDevice? usbDevice = null,
            AircraftFamily? aircraftFamily = null,
            AircraftManufacturer? aircraftManufacturer = null,
            EquipmentLocation? equipmentLocation = null
        )
        {
            IFgcp? result = null;

            usbDevice = FindUsbDeviceCandidate(
                EquipmentType.Fgcp,
                usbDevice,
                aircraftFamily,
                aircraftManufacturer,
                equipmentLocation
            );
            var hidDevice = OpenHidDevice(usbDevice);

            if(hidDevice != null && usbDevice != null) {
                result = SupportedDeviceFactory.CreateFgcp(hidDevice, usbDevice);
            }

            return result;
        }

        private static UsbDevice? FindUsbDeviceCandidate(
            EquipmentType equipmentType,
            UsbDevice? usbDevice = null,
            AircraftFamily? aircraftFamily = null,
            AircraftManufacturer? aircraftManufacturer = null,
            EquipmentLocation? equipmentLocation = null
        )
        {
            usbDevice = FindLocalDevices()
                .Where(candidate =>
                        (candidate.EquipmentType & equipmentType) != 0
                    && (usbDevice == null || candidate == usbDevice)
                    && (aircraftFamily == null || candidate.AircraftFamily == aircraftFamily)
                    && (aircraftManufacturer == null || candidate.AircraftManufacturer == aircraftManufacturer)
                    && (equipmentLocation == null || candidate.EquipmentLocation == equipmentLocation)
                )
                // The order selected here is only to make it deterministic
                .OrderBy(candidate => candidate.Id.VendorId)
                .ThenBy(candidate => candidate.Id.ProductId)
                .FirstOrDefault();

            return usbDevice;
        }

        private static HidDevice? OpenHidDevice(UsbDevice? usbDevice)
        {
            HidDevice? result = null;

            if(usbDevice != null) {
                result = DeviceList
                    .Local
                    .GetHidDevices(
                        vendorID: usbDevice.Id.VendorId,
                        productID: usbDevice.Id.ProductId
                    )
                    .FirstOrDefault();
            }

            return result;
        }
    }
}
