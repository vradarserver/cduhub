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
    public static class EquipmentTypeExtensions
    {
        /// <summary>
        /// Converts from a set of equipment type flags an an aircraft family to a <see
        /// cref="DeviceType"/>.
        /// </summary>
        /// <param name="equipmentTypeFlags"></param>
        /// <param name="aircraftFamily"></param>
        /// <returns></returns>
        public static DeviceType ToDeviceType(this EquipmentType equipmentTypeFlags, AircraftFamily aircraftFamily)
        {
            var result = DeviceType.NotSpecified;

            if((equipmentTypeFlags & EquipmentType.Cdu) != 0) {
                switch(aircraftFamily) {
                    case AircraftFamily.A32x:   result = DeviceType.AirbusA320Mcdu; break;
                    case AircraftFamily.B737:   result = DeviceType.Boeing737NGPfp; break;
                    case AircraftFamily.B777:   result = DeviceType.Boeing777Pfp; break;
                }
            }

            if((equipmentTypeFlags & EquipmentType.Fcu) != 0) {
                switch(aircraftFamily) {
                    case AircraftFamily.A32x:
                        var hasLeft =  (equipmentTypeFlags & EquipmentType.LeftEfis) != 0;
                        var hasRight = (equipmentTypeFlags & EquipmentType.RightEfis) != 0;
                        result = !hasLeft && !hasRight
                            ? DeviceType.AirbusA320Fcu
                            : hasLeft && hasRight
                                ? DeviceType.AirbusA320FcuBothEfis
                                : hasLeft
                                    ? DeviceType.AirbusA320FcuLeftEfis
                                    : DeviceType.AirbusA320FcuRightEfis;
                        break;
                    case AircraftFamily.B737:
                    case AircraftFamily.B777:
                        // I don't have the Boeing ones.
                        break;
                }
            }

            return result;
        }
    }
}
