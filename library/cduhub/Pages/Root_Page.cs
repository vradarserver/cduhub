// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.Plugin;
using WwDevicesDotNet;

namespace Cduhub.Pages
{
    class Root_Page : Page
    {
        private bool _HookedServices;

        public Root_Page(Hub hub) : base(hub)
        {
        }

        public override void OnSelected(bool selected)
        {
            if(!selected) {
                UnhookServices();
            } else {
                DrawPage();
                HookServices();
            }
        }

        private void DrawPage()
        {
            var updateAvailable = CduhubVersions.IsLatest
                ? ""
                : "*";
            Output
                .Clear()
                .Centred("<green>CDU <small>HUB")
                .LeftLabel(1, ">CLOCK")
                .LeftLabel(2, ">WEATHER")
                .LeftLabel(6, $">ABOUT{updateAvailable}")
                .RightLabel(1, "FLIGHT SIMS<")
                .RightLabel(6, "<red>QUIT<");
            Leds.Mcdu = Leds.Menu = true;

            if(RegisteredPlugins.EntryPointHasPlugins(EntryPointPage.Root)) {
                Output.RightLabel(2, "PLUGINS<");
            }

            RefreshDisplay();
            RefreshLeds();
        }

        private void HookServices()
        {
            if(!_HookedServices) {
                _HookedServices = true;
                RegisteredPlugins.RegisteredPluginsChanged += RegisteredPlugins_RegisteredPluginsChanged;
                GithubUpdateChecker.DefaultInstance.UpdateInfoChanged += VersionChecker_UpdateInfoChanged;
            }
        }

        private void UnhookServices()
        {
            if(_HookedServices) {
                _HookedServices = false;
                RegisteredPlugins.RegisteredPluginsChanged -= RegisteredPlugins_RegisteredPluginsChanged;
                GithubUpdateChecker.DefaultInstance.UpdateInfoChanged -= VersionChecker_UpdateInfoChanged;
            }
        }

        public override void OnCommonKeyDown(CommonKey key)
        {
            switch(key) {
                case CommonKey.LineSelectLeft1:   _Hub.CreateAndSelectPage<Clock_Page>(); break;
                case CommonKey.LineSelectLeft2:   _Hub.CreateAndSelectPage<WeatherMenu_Page>(); break;
                case CommonKey.LineSelectLeft6:   _Hub.CreateAndSelectPage<About_Page>(); break;
                case CommonKey.LineSelectRight1:  _Hub.CreateAndSelectPage<FlightSimulator.FlightSimMenu_Page>(); break;
                case CommonKey.LineSelectRight2:  _Hub.ShowPluginMenuFor(EntryPointPage.Root); break;
                case CommonKey.LineSelectRight6:  _Hub.Shutdown(); break;
            }
        }

        private void RegisteredPlugins_RegisteredPluginsChanged(object sender, System.EventArgs e)
        {
            DrawPage();
        }

        private void VersionChecker_UpdateInfoChanged(object sender, System.EventArgs e)
        {
            DrawPage();
        }
    }
}
