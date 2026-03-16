// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace McduDotNet
{
    /// <summary>
    /// Flags that identify each segment in a seven segment (+1 decimal point) digit
    /// display.
    /// </summary>
    [Flags]
    public enum S7 : byte
    {
        /// <summary>
        /// Top segment (a).
        /// </summary>
        TT = 0x01,

        /// <summary>
        /// Top-right segment (b).
        /// </summary>
        TR = 0x02,

        /// <summary>
        /// Bottom-right segment (c).
        /// </summary>
        BR = 0x04,

        /// <summary>
        /// Bottom segment (d).
        /// </summary>
        BB = 0x08,

        /// <summary>
        /// Bottom-left segment (e).
        /// </summary>
        BL = 0x10,

        /// <summary>
        /// Top-left segment (f).
        /// </summary>
        TL = 0x20,

        /// <summary>
        /// Middle segment (g).
        /// </summary>
        MM = 0x40,

        /// <summary>
        /// Optional decimal point segment (h).
        /// </summary>
        DP = 0x80,
    }
}
