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
using wwDevicesDotNet;

namespace Cduhub.Pages.Init
{
    class FixedBrightnessInit_Page : CommonSettingsPage<BrightnessSettings>
    {
        protected override string Title => "FIXED BRIGHTNESS";

        protected override bool ApplySettingsImmediately => true;

        public override Func<Type> LeftArrowCallback => () => typeof(BrightnessInit_Page);

        public FixedBrightnessInit_Page(Hub hub) : base(hub)
        {
            Leds.TurnAllOn(true);

            LeftOption("DISPLAY",
                () => $"{_Settings.FixedBrightness.DisplayBacklightPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.FixedBrightness.DisplayBacklightPercent = Math.Max(5, v),
                    min: 0, max: 100
                )
            );
            RightOption("LED",
                () => $"{_Settings.FixedBrightness.LedIntensityPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.FixedBrightness.LedIntensityPercent = Math.Max(5, v),
                    min: 0, max: 100
                )
            );
            LeftOption("KEYBOARD",
                () => $"{_Settings.FixedBrightness.KeyboardBacklightPercent}%",
                () => _Form.IntegerFromScratchpad(Scratchpad,
                    v => _Settings.FixedBrightness.KeyboardBacklightPercent = v,
                    min: 0, max: 100
                )
            );
        }

        protected override void ApplySettings() => _Hub.ReloadBrightness();

        protected override void ApplyDefaults(BrightnessSettings defaults)
        {
            _Settings.FixedBrightness = defaults.FixedBrightness;
        }

        protected override void AddToPage()
        {
            AddColourSamples(Output, 7);
        }

        internal static void AddColourSamples(Compositor output, int line)
        {
            output
                .Line(line++)
                .Large()
                .Centred("<amber>XX <white>XX <cyan>XX <green>XX <magenta>XX")
                .Line(line++)
                .Centred("<red>XX <yellow>XX <brown>XX <grey>XX <khaki>XX");
        }
    }
}
