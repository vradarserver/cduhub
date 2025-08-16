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

namespace McduDotNet
{
    /// <summary>
    /// An enumeration of all supported devices.
    /// </summary>
    public static class SupportedDevices
    {
        /// <summary>
        /// The identifier for a WinWing MCDU device set to the left-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingMcduCaptainDevice = new DeviceIdentifier(
            "Winwing MCDU (Captain)", 0x4098, 0xBB36, Device.WinWingMcdu, DeviceUser.Captain, DeviceType.AirbusA320Mcdu
        );

        /// <summary>
        /// The identifier for a WinWing MCDU device set to the right-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingMcduFirstOfficerDevice = new DeviceIdentifier(
            "Winwing MCDU (F/O)", 0x4098, 0xBB3E, Device.WinWingMcdu, DeviceUser.FirstOfficer, DeviceType.AirbusA320Mcdu
        );

        /// <summary>
        /// The identifier for a WinWing MCDU device set to the observer seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingMcduObserverDevice = new DeviceIdentifier(
            "Winwing MCDU (Observer)", 0x4098, 0xBB3A, Device.WinWingMcdu, DeviceUser.Observer, DeviceType.AirbusA320Mcdu
        );

        /// <summary>
        /// The identifier for a WinWing PFP-3N device set to the left-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp3NCaptainDevice = new DeviceIdentifier(
            "Winwing PFP-3N (Captain)", 0x4098, 0xBB35, Device.WinWingPfp3N, DeviceUser.Captain, DeviceType.Boeing737NGPfp
        );

        /// <summary>
        /// The identifier for a WinWing PFP-3N device set to the right-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp3NFirstOfficerDevice = new DeviceIdentifier(
            "Winwing PFP-3N (F/O)", 0x4098, 0xBB3D, Device.WinWingPfp3N, DeviceUser.FirstOfficer, DeviceType.Boeing737NGPfp
        );

        /// <summary>
        /// The identifier for a WinWing PFP-3N device set to the observer seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp3NObserverDevice = new DeviceIdentifier(
            "Winwing PFP-3N (Observer)", 0x4098, 0xBB39, Device.WinWingPfp3N, DeviceUser.Observer, DeviceType.Boeing737NGPfp
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the left-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp7CaptainDevice = new DeviceIdentifier(
            "Winwing PFP-7 (Captain)", 0x4098, 0xBB37, Device.WinWingPfp7, DeviceUser.Captain, DeviceType.Boeing777Pfp
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the right-hand seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp7FirstOfficerDevice = new DeviceIdentifier(
            "Winwing PFP-7 (F/O)", 0x4098, 0xBB3F, Device.WinWingPfp7, DeviceUser.FirstOfficer, DeviceType.Boeing777Pfp
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the observer seat position.
        /// </summary>
        public static readonly DeviceIdentifier WinWingPfp7ObserverDevice = new DeviceIdentifier(
            "Winwing PFP-7 (Observer)", 0x4098, 0xBB3B, Device.WinWingPfp7, DeviceUser.Observer, DeviceType.Boeing777Pfp
        );

        private static readonly DeviceIdentifier[] _AllSupportedDevices = new DeviceIdentifier[] {
            WinWingMcduCaptainDevice,
            WinWingMcduFirstOfficerDevice,
            WinWingMcduObserverDevice,

            WinWingPfp3NCaptainDevice,
            WinWingPfp3NFirstOfficerDevice,
            WinWingPfp3NObserverDevice,

            WinWingPfp7CaptainDevice,
            WinWingPfp7FirstOfficerDevice,
            WinWingPfp7ObserverDevice,
        };

        /// <summary>
        /// A collection of device identifiers for all supported devices.
        /// </summary>
        public static IReadOnlyList<DeviceIdentifier> AllSupportedDevices => _AllSupportedDevices;
    }
}
