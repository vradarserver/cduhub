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
using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// Identifies a USB CDU device.
    /// </summary>
    public class DeviceIdentifier
    {
        /// <summary>
        /// Gets the USB Vendor ID returned by the device.
        /// </summary>
        public int UsbVendorId { get; }

        /// <summary>
        /// Gets the USB Product ID returned by the device.
        /// </summary>
        public int UsbProductId { get; }

        /// <summary>
        /// Gets the MCDU.NET device that corresponds to this vendor and product ID.
        /// </summary>
        public Device Device { get; }

        /// <summary>
        /// Gets the MCDU.NET device position (pilot, co-pilot etc.) that corresponds to
        /// this vendor and product ID.
        /// </summary>
        public DeviceUser DeviceUser { get; }

        /// <summary>
        /// Gets the broad category of device (Airbus A320 MCDU, Boeing 777 PFP) that this
        /// device replicates.
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// Gets a terse description of the device represented by this identifier.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        /// <param name="device"></param>
        /// <param name="deviceUser"></param>
        /// <param name="deviceType"></param>
        public DeviceIdentifier(
            string description,
            int vendorId,
            int productId,
            Device device,
            DeviceUser deviceUser,
            DeviceType deviceType
        )
        {
            Description = description;
            UsbVendorId = vendorId;
            UsbProductId = productId;
            Device = device;
            DeviceUser = deviceUser;
            DeviceType = deviceType;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = new StringBuilder(Device.ToString());
            if(DeviceUser != DeviceUser.NotApplicable) {
                result.Append(' ');
                result.Append(DeviceUser.ToString());
            }
            result.Append($" Vendor 0x{UsbVendorId:X4}");
            result.Append($" Product 0x{UsbProductId:X4}");

            return result.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is DeviceIdentifier other) {
                result = UsbVendorId == other.UsbVendorId
                      && UsbProductId == other.UsbProductId
                      && Device == other.Device
                      && DeviceUser == other.DeviceUser
                      && DeviceType == other.DeviceType;
            }

            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => UsbProductId.GetHashCode();

        /// <summary>
        /// For backwards compatibility.
        /// </summary>
        /// <returns></returns>
        internal ProductId GetLegacyProductId()
        {
            switch(DeviceUser) {
                case DeviceUser.FirstOfficer:   return ProductId.FirstOfficer;
                case DeviceUser.Observer:       return ProductId.Observer;
                default:                        return ProductId.Captain;
            }
        }
    }
}
