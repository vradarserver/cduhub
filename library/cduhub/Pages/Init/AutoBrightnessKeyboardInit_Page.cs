using System;
using System.Collections.Generic;
using System.Text;
using Cduhub.Config;

namespace Cduhub.Pages.Init
{
    class AutoBrightnessKeyboardInit_Page : CommonAutoBrightnessInit_Page
    {
        protected override string Title => "KEYBOARD AUTO";

        protected override string IntensityLabel => "BRIGHTNESS";

        protected override Func<int> LiveValueCallback => () => _Hub.BacklightBrightnessPercent;

        public override Func<Type> LeftArrowCallback => () => typeof(AutoBrightnessLedInit_Page);

        public AutoBrightnessKeyboardInit_Page(Hub hub) : base(hub)
        {
            SetupEditor(() => _Settings.AutoBrightness.KeyboardBacklight);
        }

        protected override void ApplyDefaults(BrightnessSettings defaults)
        {
            _Settings.AutoBrightness.KeyboardBacklight = defaults.AutoBrightness.KeyboardBacklight;
        }
    }
}
