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
using McduDotNet.WinWingMcdu;

namespace McduDotNet
{
    /// <summary>
    /// Creates instances of <see cref="IMcdu"/>.
    /// </summary>
    public static class McduFactory
    {
        /// <summary>
        /// Returns a collection of all MCDU devices that can be found on the local machine.
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<ProductId> FindLocalMcdus()
        {
            return DeviceList
                .Local
                .GetHidDevices(vendorID: UsbIdentifiers.VendorId)
                .Where(device => UsbIdentifiers.IsMcdu(device.VendorID, device.ProductID))
                .Select(device => UsbIdentifiers.ToLibraryProductId(device.ProductID))
                .ToArray();
        }

        /// <summary>
        /// Creates an initialised connection to an MCDU. If the product ID is not specified then the first MCDU
        /// found on the system is used. If the requested MCDU cannot be found (or there are no MCDUs to default to)
        /// then null is returned.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static IMcdu ConnectLocal(ProductId? productId = null)
        {
            Mcdu result = null;

            if(productId == null) {
                productId = FindLocalMcdus().FirstOrDefault();
            }
            if(productId != null) {
                var hidDevice = DeviceList
                    .Local
                    .GetHidDevices(
                        vendorID: UsbIdentifiers.VendorId,
                        productID: UsbIdentifiers.FromLibraryProductId(productId.Value)
                    )
                    .FirstOrDefault();
                if(hidDevice != null) {
                    result = new Mcdu(hidDevice, productId.Value);
                    result.Initialise();
                }
            }

            return result;
        }
    }
}
