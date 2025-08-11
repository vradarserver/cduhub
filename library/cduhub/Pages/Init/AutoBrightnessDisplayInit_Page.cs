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
using System.Text;
using Cduhub.Config;

namespace Cduhub.Pages.Init
{
    class AutoBrightnessDisplayInit_Page : CommonSettingsPage<BrightnessSettings>
    {
        protected override string Title => "DISPLAY AUTO";

        protected override bool ApplySettingsImmediately => true;

        public override Func<Type> LeftArrowCallback => () => typeof(BrightnessInit_Page);

        public AutoBrightnessDisplayInit_Page(Hub hub) : base(hub)
        {
            LeftOption("LOW",
                () => $"{_Settings.AutoBrightness.DisplayBacklight.LowestIntensityPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.AutoBrightness.DisplayBacklight.LowestIntensityPercent = v,
                    min: 0, max: 100
            ));
            RightOption("HIGH",
                () => $"{_Settings.AutoBrightness.DisplayBacklight.HighestIntensityPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.AutoBrightness.DisplayBacklight.HighestIntensityPercent = v,
                    min: 0, max: 100
            ));
            LeftOption("LOW",
                () => $"{_Settings.AutoBrightness.DisplayBacklight.LowIntensityBelowAmbientPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.AutoBrightness.DisplayBacklight.LowIntensityBelowAmbientPercent = v,
                    min: 0, max: 100
            ));
            RightOption("HIGH",
                () => $"{_Settings.AutoBrightness.DisplayBacklight.HighIntensityAboveAmbientPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.AutoBrightness.DisplayBacklight.HighIntensityAboveAmbientPercent = v,
                    min: 0, max: 100
            ));
            LeftOption("GAMMA",
                () => $"{_Settings.AutoBrightness.DisplayBacklight.ScaleGamma:N1}",
                () => _Form.DoubleFromScratchpad(Scratchpad,
                    v => _Settings.AutoBrightness.DisplayBacklight.ScaleGamma = v,
                    min: 0.1, max: 20.0
            ));
            RightOption("+/-",
                () => $"{_Settings.AutoBrightness.DisplayBacklight.AddPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.AutoBrightness.DisplayBacklight.AddPercent = v,
                    min: -100, max: 100
            ));
        }

        protected override void AddToPage()
        {
            Output.LabelTitleLine(1).Centered("<small>BRIGHTNESS");
            Output.LabelTitleLine(2).Centered("<small>LIGHT");
            DrawLiveValues();

            FixedBrightnessInit_Page.AddColourSamples(Output, 9);
        }

        private void DrawLiveValues()
        {
            Output
                .LabelTitleLine(4)
                .ClearRow(2)
                .LeftLabelTitle(4, "<small><green> LIGHT")
                .LeftLabel(4, $"<green>{_Hub.AmbientLightPercent}%")
                .RightLabelTitle(4, "<small><green>BRIGHTNESS ")
                .RightLabel(4, $"<green>{_Hub.DisplayBrightnessPercent}%");
        }

        public override void OnAmbientLightChanged(int _)
        {
            DrawLiveValues();
            RefreshDisplay();
        }

        protected override void ApplySettings()
        {
            _Hub.ReloadBrightness();
            DrawLiveValues();
            RefreshDisplay();
        }

        protected override void ApplyDefaults(BrightnessSettings defaults)
        {
            _Settings.AutoBrightness.DisplayBacklight = defaults.AutoBrightness.DisplayBacklight;
        }
    }
}
