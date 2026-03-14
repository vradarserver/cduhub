// Copyright © 2025 onwards, Andrew Whewell
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
    public static class CduLampExtensions
    {
        public static string Describe(this CduLamp lamp)
        {
            switch(lamp) {
                case CduLamp.Dspy:  return "DSPY";
                case CduLamp.Exec:  return "EXEC";
                case CduLamp.Fail:  return "FAIL";
                case CduLamp.Fm:    return "FM";
                case CduLamp.Fm1:   return "FM1";
                case CduLamp.Fm2:   return "FM2";
                case CduLamp.Ind:   return "IND";
                case CduLamp.Line:  return "LINE";
                case CduLamp.Mcdu:  return "MCDU";
                case CduLamp.Menu:  return "MENU";
                case CduLamp.Msg:   return "MSG";
                case CduLamp.Ofst:  return "OFST";
                case CduLamp.Rdy:   return "RDY";
                case (CduLamp)(-1): return "N/A";
                default:            return "";
            }
        }

        public static CommonCduLamp ToCommonLed(this CduLamp lamp)
        {
            switch(lamp) {
                case CduLamp.Fail:   return CommonCduLamp.Fail;
                case CduLamp.Line:
                case CduLamp.Exec:   return CommonCduLamp.LineOrExec;
                default:             return CommonCduLamp.DeviceSpecific;
            }
        }
    }
}
