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
using System.Collections.Generic;
using McduDotNet;

namespace Cduhub.FlightSim
{
    public static class DeviceTypeKeymapFactory
    {
        private readonly static object _SyncLock = new();
        private readonly static List<DeviceTypeKeymap> _DeviceKeymaps = new();

        static DeviceTypeKeymapFactory() => RegisterPredefinedKeymaps();

        private static void RegisterPredefinedKeymaps()
        {
            foreach(var keymap in PredefinedDeviceTypeKeymaps.AllPredefinedKeymaps) {
                Add(keymap);
            }
        }

        /// <summary>
        /// Returns the keymap that translates keys between two device types. If no keymap exists then
        /// this returns the empty keymap.
        /// </summary>
        /// <param name="firstDeviceType"></param>
        /// <param name="secondDeviceType"></param>
        /// <returns></returns>
        public static DeviceTypeKeymap FindFor(DeviceType firstDeviceType, DeviceType secondDeviceType)
        {
            lock(_SyncLock) {
                return FindKeymapForDeviceTypes(firstDeviceType, secondDeviceType)
                    ?? DeviceTypeKeymap.Empty;
            }
        }

        /// <summary>
        /// Sets up a new keymapping between the two device types described by <see cref="keymap"/>.
        /// If a mapping already exists between these two devices then it is overwritten.
        /// </summary>
        /// <param name="keymap"></param>
        public static void Add(DeviceTypeKeymap keymap)
        {
            if(keymap == null) {
                throw new ArgumentNullException(nameof(keymap));
            }

            // Silently ignore attempts to override the empty keymap
            if(   keymap.FirstDeviceType != DeviceType.NotSpecified
               || keymap.SecondDeviceType != DeviceType.NotSpecified
            ) {
                lock(_SyncLock) {
                    var idx = FindIndexForDeviceTypes(keymap.FirstDeviceType, keymap.SecondDeviceType);
                    if(idx != -1) {
                        _DeviceKeymaps[idx] = keymap;
                    } else {
                        _DeviceKeymaps.Add(keymap);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the keymapping between two devices.
        /// </summary>
        /// <param name="firstDeviceType"></param>
        /// <param name="secondDeviceType"></param>
        public static void Remove(DeviceType firstDeviceType, DeviceType secondDeviceType)
        {
            lock(_SyncLock) {
                var idx = FindIndexForDeviceTypes(firstDeviceType, secondDeviceType);
                if(idx != -1) {
                    _DeviceKeymaps.RemoveAt(idx);
                }
            }
        }

        private static int FindIndexForDeviceTypes(DeviceType typeA, DeviceType typeB)
        {
            var result = -1;
            for(var idx = 0;idx < _DeviceKeymaps.Count;++idx) {
                var entry = _DeviceKeymaps[idx];
                if(   (entry.FirstDeviceType == typeA && entry.SecondDeviceType == typeB)
                   || (entry.FirstDeviceType == typeB && entry.SecondDeviceType == typeA)
                ) {
                    result = idx;
                    break;
                }
            }
            return result;
        }

        private static DeviceTypeKeymap? FindKeymapForDeviceTypes(DeviceType typeA, DeviceType typeB)
        {
            var idx = FindIndexForDeviceTypes(typeA, typeB);
            return idx == -1
                ? null
                : _DeviceKeymaps[idx];
        }
    }
}
