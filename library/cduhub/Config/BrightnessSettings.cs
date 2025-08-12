// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Cduhub.Config
{
    public class BrightnessSettings : Settings
    {
        public override int GetCurrentVersion() => 1;

        public override string GetName() => "brightness-settings";

        public class FixedBrightnessSettings
        {
            public int KeyboardBacklightPercent { get; set; } = 50;

            public int DisplayBacklightPercent { get; set; } = 80;

            public int LedIntensityPercent { get; set; } = 80;
        }

        public class CommonAutoBrightnessSettings
        {
            public int LowestIntensityPercent { get; set; } = 0;

            public int HighestIntensityPercent { get; set; } = 100;

            public double ScaleGamma { get; set; } = 1.0;
        }

        public class AscendingAutoBrightnessSettings : CommonAutoBrightnessSettings
        {
            public int LowIntensityBelowAmbientPercent { get; set; } = 1;

            public int HighIntensityAboveAmbientPercent { get; set; } = 99;
        }

        public class DescendingAutoBrightnessSettings : CommonAutoBrightnessSettings
        {
            public int LowIntensityAboveAmbientPercent { get; set; } = 99;

            public int HighIntensityBelowAmbientPercent { get; set; } = 1;
        }

        public class AutoBrightnessSettings
        {
            public AscendingAutoBrightnessSettings DisplayBacklight { get; set; } = new AscendingAutoBrightnessSettings();

            public AscendingAutoBrightnessSettings LedIntensity { get; set; } = new AscendingAutoBrightnessSettings();

            public DescendingAutoBrightnessSettings KeyboardBacklight { get; set; } = new DescendingAutoBrightnessSettings();
        }

        public bool UseAutoBrightness { get; set; } = true;

        public FixedBrightnessSettings FixedBrightness { get; set; } = new FixedBrightnessSettings();

        public AutoBrightnessSettings AutoBrightness { get; set; } = new AutoBrightnessSettings();

        public BrightnessSettings()
        {
            McduToHub(new McduDotNet.AutoBrightnessSettings());
        }

        public void CopyToCdu(McduDotNet.ICdu cdu)
        {
            if(cdu != null) {
                cdu.AutoBrightness.Enabled = UseAutoBrightness;
                HubToMcdu(cdu.AutoBrightness);
                if(!UseAutoBrightness) {
                    cdu.BacklightBrightnessPercent = FixedBrightness.KeyboardBacklightPercent;
                    cdu.DisplayBrightnessPercent = FixedBrightness.DisplayBacklightPercent;
                    cdu.LedBrightnessPercent = FixedBrightness.LedIntensityPercent;
                }
            }
        }

        public void McduToHub(McduDotNet.AutoBrightnessSettings mcduSettings)
        {
            McduToHub(mcduSettings.DisplayBacklight, AutoBrightness.DisplayBacklight);
            McduToHub(mcduSettings.LedIntensity, AutoBrightness.LedIntensity);
            McduToHub(mcduSettings.KeyboardBacklight, AutoBrightness.KeyboardBacklight);
        }

        private static void McduCommonToHub(McduDotNet.CommonAutoBrightnessSettings mcdu, CommonAutoBrightnessSettings hub)
        {
            hub.HighestIntensityPercent = mcdu.HighestIntensityPercent;
            hub.LowestIntensityPercent = mcdu.LowestIntensityPercent;
            hub.ScaleGamma = mcdu.ScaleGamma;
        }

        private static void McduToHub(McduDotNet.CommonAscendingAutoBrightnessSettings mcdu, AscendingAutoBrightnessSettings hub)
        {
            McduCommonToHub(mcdu, hub);
            hub.HighIntensityAboveAmbientPercent = mcdu.HighIntensityAboveAmbientPercent;
            hub.LowIntensityBelowAmbientPercent = mcdu.LowIntensityBelowAmbientPercent;
        }

        private static void McduToHub(McduDotNet.CommonDescendingAutoBrightnessSettings mcdu, DescendingAutoBrightnessSettings hub)
        {
            McduCommonToHub(mcdu, hub);
            hub.HighIntensityBelowAmbientPercent = mcdu.HighIntensityBelowAmbientPercent;
            hub.LowIntensityAboveAmbientPercent = mcdu.LowIntensityAboveAmbientPercent;
        }

        public void HubToMcdu(McduDotNet.AutoBrightnessSettings mcduSettings)
        {
            HubToMcdu(mcduSettings.DisplayBacklight, AutoBrightness.DisplayBacklight);
            HubToMcdu(mcduSettings.LedIntensity, AutoBrightness.LedIntensity);
            HubToMcdu(mcduSettings.KeyboardBacklight, AutoBrightness.KeyboardBacklight);
        }

        private static void HubCommonToMcdu(McduDotNet.CommonAutoBrightnessSettings mcdu, CommonAutoBrightnessSettings hub)
        {
            mcdu.HighestIntensityPercent = hub.HighestIntensityPercent;
            mcdu.LowestIntensityPercent = hub.LowestIntensityPercent;
            mcdu.ScaleGamma = hub.ScaleGamma;
        }

        private static void HubToMcdu(McduDotNet.CommonAscendingAutoBrightnessSettings mcdu, AscendingAutoBrightnessSettings hub)
        {
            HubCommonToMcdu(mcdu, hub);
            mcdu.HighIntensityAboveAmbientPercent = hub.HighIntensityAboveAmbientPercent;
            mcdu.LowIntensityBelowAmbientPercent = hub.LowIntensityBelowAmbientPercent;
        }

        private static void HubToMcdu(McduDotNet.CommonDescendingAutoBrightnessSettings mcdu, DescendingAutoBrightnessSettings hub)
        {
            HubCommonToMcdu(mcdu, hub);
            mcdu.HighIntensityBelowAmbientPercent = hub.HighIntensityBelowAmbientPercent;
            mcdu.LowIntensityAboveAmbientPercent = hub.LowIntensityAboveAmbientPercent;
        }
    }
}
