// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// The identifiers for FCU and EFIS LEDs.
    /// </summary>
    static class LampMaps
    {
        public static BinaryLampMap CreateLeftEfisMap()
        {
            return new(new BinaryLamp[] {
                new((int)FgcpLamp.Left_FD,      0x03),
                new((int)FgcpLamp.Left_LS,      0x04),
                new((int)FgcpLamp.Left_Cstr,    0x05),
                new((int)FgcpLamp.Left_Wpt,     0x06),
                new((int)FgcpLamp.Left_VorD,    0x07),
                new((int)FgcpLamp.Left_Ndb,     0x08),
                new((int)FgcpLamp.Left_Arpt,    0x09),
            });
        }

        public static BinaryLampMap CreateRightEfisMap()
        {
            return new(new BinaryLamp[] {
                new((int)FgcpLamp.Right_FD,     0x03),
                new((int)FgcpLamp.Right_LS,     0x04),
                new((int)FgcpLamp.Right_Cstr,   0x05),
                new((int)FgcpLamp.Right_Wpt,    0x06),
                new((int)FgcpLamp.Right_VorD,   0x07),
                new((int)FgcpLamp.Right_Ndb,    0x08),
                new((int)FgcpLamp.Right_Arpt,   0x09),
            });
        }

        public static BinaryLampMap CreateFcuMap()
        {
            return new(new BinaryLamp[] {
                new((int)FgcpLamp.Loc,          0x03),
                new((int)FgcpLamp.Ap1,          0x05),
                new((int)FgcpLamp.Ap2,          0x07),
                new((int)FgcpLamp.AThr,         0x09),
                new((int)FgcpLamp.Exped,        0x0b),
                new((int)FgcpLamp.Appr,         0x0d),
            });
        }
    }
}
