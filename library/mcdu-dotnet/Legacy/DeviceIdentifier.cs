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
    [Obsolete("Retired in V2, use UsbDevice and DeviceFactory")]
    public class DeviceIdentifier
    {
        public UsbDevice UsbDevice { get; }

        public int UsbVendorId => UsbDevice.Id.VendorId;

        public int UsbProductId => UsbDevice.Id.ProductId;

        public Device Device
        {
            get {
                switch(UsbDevice.EquipmentType) {
                    case EquipmentType.Cdu:
                        switch(UsbDevice.AircraftFamily) {
                            case AircraftFamily.A32x:   return Device.WinWingMcdu;
                            case AircraftFamily.B737:   return Device.WinWingPfp3N;
                            case AircraftFamily.B777:   return Device.WinWingPfp7;
                        }
                        break;
                }
                return (Device)-1;
            }
        }

        public DeviceUser DeviceUser
        {
            get {
                switch(UsbDevice.EquipmentLocation) {
                    case EquipmentLocation.Captain:         return DeviceUser.Captain;
                    case EquipmentLocation.FirstOfficer:    return DeviceUser.FirstOfficer;
                    case EquipmentLocation.Observer:        return DeviceUser.Observer;
                }
                return DeviceUser.NotApplicable;
            }
        }

        public DeviceType DeviceType
        {
            get {
                switch(UsbDevice.EquipmentType) {
                    case EquipmentType.Cdu:
                        switch(UsbDevice.AircraftFamily) {
                            case AircraftFamily.A32x:   return DeviceType.AirbusA320Mcdu;
                            case AircraftFamily.B737:   return DeviceType.Boeing737NGPfp;
                            case AircraftFamily.B777:   return DeviceType.Boeing777Pfp;
                        }
                        break;
                }
                return DeviceType.NotSpecified;
            }
        }

        public string Description => UsbDevice.Description;

        public DeviceIdentifier(UsbDevice usbDevice)
        {
            UsbDevice = usbDevice;
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
    }
}
