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

namespace McduDotNet
{
    public class CommonAscendingAutoBrightnessSettings : CommonAutoBrightnessSettings
    {
        public int LowIntensityBelowAmbientPercent { get; set; }

        public int HighIntensityAboveAmbientPercent { get; set; }

        protected int BrightnessForAmbientPercent(int ambientPercent, int minimumIntensityPercent = 0)
        {
            var onRange = new PercentRange(
                Math.Max(minimumIntensityPercent, LowIntensityBelowAmbientPercent),
                HighIntensityAboveAmbientPercent
            );
            var intensity = new PercentRange(
                LowestIntensityPercent,
                HighestIntensityPercent
            );
            int result;
            switch(onRange.Compare(ambientPercent)) {
                case -1: // Ambient is lower than the point where we're full-off
                    result = intensity.Low;
                    break;
                case 1:  // Ambient is higher than the point where we're full-on
                    result = intensity.High;
                    break;
                default: // Ambient is between full-off and full-on
                    if(intensity.Range == 0) {
                        return intensity.High;
                    }
                    result = intensity.PowerTransformScaling(
                        onRange.PercentageOfRange(ambientPercent),
                        ScaleGamma
                    );
                    break;
            }

            return intensity.Clamp(result + PlusBrightnessPercent);
        }
    }
}
