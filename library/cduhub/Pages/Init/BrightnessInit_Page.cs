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
    public class BrightnessInit_Page : CommonSettingsPage<BrightnessSettings>
    {
        protected override string Title => "BRIGHTNESS CONFIG";

        protected override bool ApplySettingsImmediately => true;

        public override Func<Type> RightArrowCallback => () => _Settings.UseAutoBrightness
            ? typeof(AutoBrightnessDisplayInit_Page)
            : typeof(FixedBrightnessInit_Page);

        public BrightnessInit_Page(Hub hub) : base(hub)
        {
            LeftOption("AUTO-BRIGHTNESS",
                () => _Form.YesNo(_Settings.UseAutoBrightness),
                () => _Form.ToggleBool(_Settings.UseAutoBrightness, v => _Settings.UseAutoBrightness = v)
            );
        }

        protected override void ApplySettings() => _Hub.ReloadBrightness();

        protected override void ApplyDefaults(BrightnessSettings defaults)
        {
            _Settings.UseAutoBrightness = defaults.UseAutoBrightness;
        }
    }
}
