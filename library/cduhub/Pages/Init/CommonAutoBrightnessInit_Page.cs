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
using Cduhub.Config;

namespace Cduhub.Pages.Init
{
    abstract class CommonAutoBrightnessInit_Page : CommonSettingsPage<BrightnessSettings>
    {
        protected override bool ApplySettingsImmediately => true;

        protected abstract string IntensityLabel { get; }

        protected abstract Func<int> LiveValueCallback { get; }

        protected CommonAutoBrightnessInit_Page(Hub hub) : base(hub)
        {
        }

        protected void SetupEditor(Func<BrightnessSettings.AscendingAutoBrightnessSettings> getSettings)
        {
            SetupIntensityEditor(getSettings);
            LeftOption("LOW",
                () => $"{getSettings().LowIntensityBelowAmbientPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => getSettings().LowIntensityBelowAmbientPercent = v,
                    min: 0, max: 100
            ));
            RightOption("HIGH",
                () => $"{getSettings().HighIntensityAboveAmbientPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => getSettings().HighIntensityAboveAmbientPercent = v,
                    min: 0, max: 100
            ));
            SetupGammaEditor(getSettings);
        }

        protected void SetupEditor(Func<BrightnessSettings.DescendingAutoBrightnessSettings> getSettings)
        {
            SetupIntensityEditor(getSettings, highFirst: true);
            LeftOption("HIGH",
                () => $"{getSettings().HighIntensityBelowAmbientPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => getSettings().HighIntensityBelowAmbientPercent = v,
                    min: 0, max: 100
            ));
            RightOption("LOW",
                () => $"{getSettings().LowIntensityAboveAmbientPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => getSettings().LowIntensityAboveAmbientPercent = v,
                    min: 0, max: 100
            ));
            SetupGammaEditor(getSettings);
        }

        private void SetupIntensityEditor(
            Func<BrightnessSettings.CommonAutoBrightnessSettings> getSettings,
            bool highFirst = false
        )
        {
            var low = LeftOption("LOW",
                () => $"{getSettings().LowestIntensityPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => getSettings().LowestIntensityPercent = v,
                    min: 0, max: 100
            ));
            var high = RightOption("HIGH",
                () => $"{getSettings().HighestIntensityPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => getSettings().HighestIntensityPercent = v,
                    min: 0, max: 100
            ));

            if(highFirst) {
                _LeftOptions.Remove(low);
                _RightOptions.Add(low);

                _RightOptions.Remove(high);
                _LeftOptions.Add(high);
            }
        }

        private void SetupGammaEditor(Func<BrightnessSettings.CommonAutoBrightnessSettings> getSettings)
        {
            LeftOption("GAMMA",
                () => $"{getSettings().ScaleGamma:N1}",
                () => _Form.DoubleFromScratchpad(Scratchpad,
                    v => getSettings().ScaleGamma = v,
                    min: 0.1, max: 20.0
            ));
        }

        protected override void AddToPage()
        {
            Output.LabelTitleLine(1).Centered($"<small>{IntensityLabel}");
            Output.LabelTitleLine(2).Centered("<small>LIGHT");
            DrawLiveValues();
            FixedBrightnessInit_Page.AddColourSamples(Output, 9);
        }

        protected virtual void DrawLiveValues()
        {
            Output
                .LabelTitleLine(4)
                .ClearRow(2)
                .LeftLabelTitle(4, "<small><green> LIGHT")
                .LeftLabel(4, $"<green>{_Hub.AmbientLightPercent}%")
                .RightLabelTitle(4, $"<small><green>{IntensityLabel} ")
                .RightLabel(4, $"<green>{LiveValueCallback()}%");
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
    }
}
