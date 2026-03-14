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
        public static readonly UsbDevice WinWingMcduCaptain = new(
            new(0x4098, 0xBB36), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Cdu, EquipmentLocation.Captain, "Winwing MCDU (Captain)"
        );

        /// <summary>
        /// The identifier for a WinWing MCDU device set to the right-hand seat position.
        /// </summary>
        public static readonly UsbDevice WinWingMcduFirstOfficer = new(
            new(0x4098, 0xBB3E), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Cdu, EquipmentLocation.FirstOfficer, "Winwing MCDU (F/O)"
        );

        /// <summary>
        /// The identifier for a WinWing MCDU device set to the observer seat position.
        /// </summary>
        public static readonly UsbDevice WinWingMcduObserver = new(
            new(0x4098, 0xBB3A), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Cdu, EquipmentLocation.Observer, "Winwing MCDU (Observer)"
        );

        /// <summary>
        /// The identifier for a WinWing PFP-3N device set to the left-hand seat position.
        /// </summary>
        public static readonly UsbDevice WinWingPfp3NCaptain = new(
            new(0x4098, 0xBB35), AircraftManufacturer.Boeing, AircraftFamily.B737, EquipmentType.Cdu, EquipmentLocation.Captain, "Winwing PFP-3N (Captain)"
        );

        /// <summary>
        /// The identifier for a WinWing PFP-3N device set to the right-hand seat position.
        /// </summary>
        public static readonly UsbDevice WinWingPfp3NFirstOfficer = new(
            new(0x4098, 0xBB3D), AircraftManufacturer.Boeing, AircraftFamily.B737, EquipmentType.Cdu, EquipmentLocation.FirstOfficer, "Winwing PFP-3N (F/O)"
        );

        /// <summary>
        /// The identifier for a WinWing PFP-3N device set to the observer seat position.
        /// </summary>
        public static readonly UsbDevice WinWingPfp3NObserver = new(
            new(0x4098, 0xBB39), AircraftManufacturer.Boeing, AircraftFamily.B737, EquipmentType.Cdu, EquipmentLocation.Observer, "Winwing PFP-3N (Observer)"
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the left-hand seat position.
        /// </summary>
        public static readonly UsbDevice WinWingPfp7Captain = new(
            new(0x4098, 0xBB37), AircraftManufacturer.Boeing, AircraftFamily.B777, EquipmentType.Cdu, EquipmentLocation.Captain, "Winwing PFP-7 (Captain)"
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the right-hand seat position.
        /// </summary>
        public static readonly UsbDevice WinWingPfp7FirstOfficer = new(
            new(0x4098, 0xBB3F), AircraftManufacturer.Boeing, AircraftFamily.B777, EquipmentType.Cdu, EquipmentLocation.FirstOfficer, "Winwing PFP-7 (F/O)"
        );

        /// <summary>
        /// The identifier for a WinWing PFP-7 device set to the observer seat position.
        /// </summary>
        public static readonly UsbDevice WinWingPfp7Observer = new(
            new(0x4098, 0xBB3B), AircraftManufacturer.Boeing, AircraftFamily.B777, EquipmentType.Cdu, EquipmentLocation.Observer, "Winwing PFP-7 (Observer)"
        );

        public static readonly UsbDevice WinWingFcu = new(
            new(0x4098, 0xBB10), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Fcu, EquipmentLocation.NotApplicable, "Winwing FCU"
        );

        public static readonly UsbDevice WinWingFcuLeftEfis = new(
            new(0x4098, 0xBC1D), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Fcu | EquipmentType.LeftEfis, EquipmentLocation.NotApplicable, "Winwing FCU + Left EFIS"
        );

        public static readonly UsbDevice WinWingFcuRightEfis = new(
            new(0x4098, 0xBC1E), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Fcu | EquipmentType.RightEfis, EquipmentLocation.NotApplicable, "Winwing FCU + Right EFIS"
        );

        public static readonly UsbDevice WinWingFcuBothEfis = new(
            new(0x4098, 0xBA01), AircraftManufacturer.Airbus, AircraftFamily.A32x, EquipmentType.Fcu | EquipmentType.LeftEfis | EquipmentType.RightEfis, EquipmentLocation.NotApplicable, "Winwing FCU + Both EFIS"
        );

        private static readonly UsbDevice[] _AllSupportedDevices = new UsbDevice[] {
            WinWingMcduCaptain,
            WinWingMcduFirstOfficer,
            WinWingMcduObserver,

            WinWingPfp3NCaptain,
            WinWingPfp3NFirstOfficer,
            WinWingPfp3NObserver,

            WinWingPfp7Captain,
            WinWingPfp7FirstOfficer,
            WinWingPfp7Observer,

            WinWingFcu,
            WinWingFcuLeftEfis,
            WinWingFcuRightEfis,
            WinWingFcuBothEfis,
        };

        /// <summary>
        /// A collection of device identifiers for all supported devices.
        /// </summary>
        public static IReadOnlyList<UsbDevice> AllSupportedDevices => _AllSupportedDevices;
    }
}
