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
using McduDotNet;

namespace Cduhub.FlightSim
{
    public class DeviceTypeKeymap
    {
        /// <summary>
        /// A pre-defined keymap that has no entries.
        /// </summary>
        public static readonly DeviceTypeKeymap Empty = new(
            DeviceType.NotSpecified, DeviceType.NotSpecified, new (Key,Key)[] { }
        );

        public DeviceType FirstDeviceType { get; }

        public DeviceType SecondDeviceType { get; }

        private readonly Dictionary<Key, Key> _FirstToSecondKeymap = new();
        /// <summary>
        /// A lookup table of key mappings when the key was pressed on a device of type
        /// <see cref="FirstDeviceType"/>.
        /// </summary>
        public IReadOnlyDictionary<Key, Key> FirstToSecondKeymap => _FirstToSecondKeymap;

        private readonly Dictionary<Key, Key> _SecondToFirstKeymap = new();
        /// <summary>
        /// A lookup table of key mappings when the key was pressed on a device of type
        /// <see cref="SecondDeviceType"/>.
        /// </summary>
        public IReadOnlyDictionary<Key, Key> SecondToFirstKeymap => _SecondToFirstKeymap;

        public DeviceTypeKeymap(
            DeviceType firstDeviceType,
            DeviceType secondDeviceType,
            IEnumerable<(Key From, Key To)> keyMap
        )
        {
            FirstDeviceType = firstDeviceType;
            SecondDeviceType = secondDeviceType;
            foreach(var (from, to) in keyMap) {
                _FirstToSecondKeymap[from] = to;
                _SecondToFirstKeymap[to] = from;
            }
        }

        /// <summary>
        /// Translates an input key to an output key.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fromDeviceType"></param>
        /// <returns></returns>
        public Key Translate(Key input, DeviceType fromDeviceType)
        {
            var result = input;

            var lookupTable = fromDeviceType == FirstDeviceType
                ? FirstToSecondKeymap
                : SecondToFirstKeymap;
            if(lookupTable.TryGetValue(input, out var translation)) {
                result = translation;
            }

            return result;
        }
    }
}
