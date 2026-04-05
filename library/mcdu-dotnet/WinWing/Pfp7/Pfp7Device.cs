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

namespace McduDotNet.WinWing.Pfp7
{
    /// <summary>
    /// Implements <see cref="ICdu"/> for a WinWing PFP-7.
    /// </summary>
    class Pfp7Device : CommonWinWingPanel, ICduPfp7
    {
        protected override byte CommandPrefix => 0x33;

        private BinaryLampMap _BinaryLampMap = new(new BinaryLamp[] {
            new((int)CduLamp.Dspy, 0x03),
            new((int)CduLamp.Fail, 0x04),
            new((int)CduLamp.Msg, 0x05),
            new((int)CduLamp.Ofst, 0x06),
            new((int)CduLamp.Exec, 0x07),
        });
        protected override BinaryLampMap BinaryLampMap => _BinaryLampMap;

        protected override Func<CduKey, (int Flag, int Offset)> KeyToFlagOffsetCallback => KeyboardMap.InputReport01FlagAndOffset;

        public Pfp7Device(HidDevice hidDevice, UsbDevice usbDevice) : base(hidDevice, usbDevice)
        {
        }

        /// <inheritdoc/>
        ~Pfp7Device() => Dispose(false);
    }
}
