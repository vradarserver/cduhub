// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet
{
    /// <summary>
    /// A device identifier.
    /// </summary>
    public readonly struct UsbDeviceId
    {
        /// <summary>
        /// Gets the USB Vendor ID returned by the device.
        /// </summary>
        public ushort VendorId { get; }

        /// <summary>
        /// Gets the USB Product ID returned by the device.
        /// </summary>
        public ushort ProductId { get; }

        /// <inheritdoc/>
        public static bool operator==(UsbDeviceId lhs, UsbDeviceId rhs)
        {
            return lhs.VendorId == rhs.VendorId
                && lhs.ProductId == rhs.ProductId;
        }

        /// <inheritdoc/>
        public static bool operator!=(UsbDeviceId lhs, UsbDeviceId rhs) => !(lhs == rhs);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        public UsbDeviceId(ushort vendorId, ushort productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            var result = false;
            if(obj is UsbDeviceId rhs) {
                result = this == rhs;
            }
            return result;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode() => ((int)VendorId << 16) | ProductId;
    }
}
