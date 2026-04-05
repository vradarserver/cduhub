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
    public static class CommonCduKeyExtensions
    {
        public static CduKey ToKey(this CommonCduKey commonKey, ICdu? cdu)
        {
            if(commonKey < CommonCduKey.DeviceSpecific) {
                return (CduKey)commonKey;
            } else if(commonKey < CommonCduKey.EitherOr || cdu == null) {
                return (CduKey)(-1);
            } else {
                (CduKey Choice1, CduKey Choice2) choices;
                switch(commonKey) {
                    case CommonCduKey.InitOrInitRef:            choices = (CduKey.Init, CduKey.InitRef); break;
                    case CommonCduKey.McduMenuOrMenu:           choices = (CduKey.McduMenu, CduKey.Menu); break;
                    case CommonCduKey.FPlnOrRte:                choices = (CduKey.FPln, CduKey.Rte); break;
                    case CommonCduKey.AirportOrDepArr:          choices = (CduKey.Airport, CduKey.DepArr); break;
                    case CommonCduKey.RadNavOrNavRad:           choices = (CduKey.RadNav, CduKey.NavRad); break;
                    case CommonCduKey.RightArrowOrNextPage:     choices = (CduKey.RightArrow, CduKey.NextPage); break;
                    case CommonCduKey.LeftArrowOrPrevPage:      choices = (CduKey.LeftArrow, CduKey.PrevPage); break;
                    case CommonCduKey.SecFPlnOrAltn:            choices = (CduKey.SecFPln, CduKey.Altn); break;
                    case CommonCduKey.OvfyOrDel:                choices = (CduKey.Ovfy, CduKey.Del); break;
                    case CommonCduKey.AtcCommOrFmcComm:         choices = (CduKey.AtcComm, CduKey.FmcComm); break;
                    default:                                    throw new NotImplementedException();
                }
                return cdu.IsKeySupported(choices.Choice1) ? choices.Choice1
                    : cdu.IsKeySupported(choices.Choice2) ? choices.Choice2
                    : (CduKey)(-1);
            }
        }

        public static string Describe(this CommonCduKey commonKey, ICdu? cdu)
        {
            return ToKey(commonKey, cdu).Describe();
        }
    }
}
