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
using HidSharp;

namespace McduDotNet.WinWing.Mcdu
{
    /// <summary>
    /// The implementation of <see cref="IMcdu"/> for the WinWing MCDU.
    /// </summary>
    class McduDevice : CommonWinWingPanel, ICduMcdu
    {
        protected override byte CommandPrefix => 0x32;

        private BinaryLampMap _BinaryLampMap = new(new BinaryLamp[] {
            new((int)CduLamp.Fail, 0x08),
            new((int)CduLamp.Fm, 0x09),
            new((int)CduLamp.Mcdu, 0x0a),
            new((int)CduLamp.Menu, 0x0b),
            new((int)CduLamp.Fm1, 0x0c),
            new((int)CduLamp.Ind, 0x0d),
            new((int)CduLamp.Rdy, 0x0e),
            new((int)CduLamp.Line, 0x0f),
            new((int)CduLamp.Fm2, 0x10),
        });
        protected override BinaryLampMap BinaryLampMap => _BinaryLampMap;

        protected override Func<Key, (int Flag, int Offset)> KeyToFlagOffsetCallback => KeyboardMap.InputReport01FlagAndOffset;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="usbDevice"></param>
        public McduDevice(HidDevice hidDevice, UsbDevice usbDevice) : base(hidDevice, usbDevice)
        {
        }

        /// <inheritdoc/>
        ~McduDevice() => Dispose(false);
    }
}
