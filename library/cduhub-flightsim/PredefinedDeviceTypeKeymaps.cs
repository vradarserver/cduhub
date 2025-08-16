﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using McduDotNet;

namespace Cduhub.FlightSim
{
    static class PredefinedDeviceTypeKeymaps
    {
        public static DeviceTypeKeymap BetweenA320McduAndPfp7 = new DeviceTypeKeymap(
            DeviceType.AirbusA320Mcdu, DeviceType.Boeing777Pfp, new (Key,Key)[] {
                (Key.AtcComm, Key.FmcComm),
                (Key.Init, Key.InitRef),
                (Key.Data, Key.Exec),
                (Key.FPln, Key.Rte),
                (Key.RadNav, Key.NavRad),
                (Key.McduMenu, Key.Menu),
                (Key.Dir, Key.Fix),
                (Key.LeftArrow, Key.PrevPage),
                (Key.RightArrow, Key.NextPage),
                (Key.Ovfy, Key.Del),
            }
        );

        public static DeviceTypeKeymap BetweenA320McduAndPfp3N = new DeviceTypeKeymap(
            DeviceType.AirbusA320Mcdu, DeviceType.Boeing737NGPfp, new (Key,Key)[] {
                (Key.Init, Key.InitRef),
                (Key.Data, Key.Exec),
                (Key.FPln, Key.Rte),
                (Key.RadNav, Key.NavRad),
                (Key.McduMenu, Key.Menu),
                (Key.Dir, Key.Fix),
                (Key.LeftArrow, Key.PrevPage),
                (Key.RightArrow, Key.NextPage),
                (Key.Ovfy, Key.Del),
            }
        );

        public static DeviceTypeKeymap BetweenPfp3NAndPfp7 = new DeviceTypeKeymap(
            DeviceType.Boeing777Pfp, DeviceType.Boeing737NGPfp, new (Key,Key)[] {
                // TBD
            }
        );

        private static readonly DeviceTypeKeymap[] _AllPredefinedKeymaps = new DeviceTypeKeymap[] {
            BetweenA320McduAndPfp3N,
            BetweenA320McduAndPfp7,
            BetweenPfp3NAndPfp7,
        };
        /// <summary>
        /// A R/O collection of all predefined device keymaps that <see cref="DeviceTypeKeymapFactory"/>
        /// sets itself up with.
        /// </summary>
        public static IReadOnlyCollection<DeviceTypeKeymap> AllPredefinedKeymaps => _AllPredefinedKeymaps;
    }
}
