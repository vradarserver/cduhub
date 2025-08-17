// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.Config;
using McduDotNet;

namespace Cduhub.Pages.FlightSimulator
{
    class SimBridgeInit_Page : Page
    {
        private SimBridgeEfbSettings _Settings;
        private readonly FormHelper _Form;

        public SimBridgeInit_Page(Hub hub) : base(hub)
        {
            Scratchpad = new Scratchpad();
            _Form = new FormHelper(() => DrawPage());
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                LoadSettings();
            } else {
                SaveSettings();
            }
        }

        private void LoadSettings()
        {
            _Settings = ConfigStorage.Load<SimBridgeEfbSettings>();
            DrawPage();
        }

        private void DrawPage()
        {
            Output
                .Clear()
                .Centred("<green>SIMBRIDGE EFB CONFIG")
                .LeftLabelTitle(1, "<small> HOST")
                .LeftLabel(1, $"<cyan>{SanitiseInput(_Settings.Host)}")
                .LeftLabelTitle(2, "<small> PORT")
                .LeftLabel(2, $"<cyan>{_Settings.Port}")
                .LeftLabelTitle(3, "<small> FONT")
                .LeftLabel(3, $"<cyan>{_Settings.Font.FontName}")
                .LeftLabelTitle(4, "<small> FULL WIDTH")
                .LeftLabel(4, $"<cyan>{_Form.YesNo(_Settings.Font.UseFullWidth)}")
                .LeftLabelTitle(5, "<small> PALETTE")
                .LeftLabel(5, $"<cyan>{_Settings.PaletteName}")
                .LeftLabel(6, "<red><small>>BACK")
                .RightLabel(6, "<magenta>RESET<");
            CopyScratchpadIntoDisplay();
            RefreshDisplay();
        }

        private void SaveSettings()
        {
            ConfigStorage.Save(_Settings);
        }

        public override void OnCommonKeyDown(CommonKey commonKey)
        {
            switch(commonKey) {
                case CommonKey.LineSelectLeft1:
                    CopyScratchpadToHost();
                    break;
                case CommonKey.LineSelectLeft2:
                    CopyScratchpadToPort();
                    break;
                case CommonKey.LineSelectLeft3:
                    _Form.CycleFontNames(_Settings.Font.FontName, v => _Settings.Font.FontName = v, includeDefaultFontName: false);
                    break;
                case CommonKey.LineSelectLeft4:
                    _Form.ToggleBool(_Settings.Font.UseFullWidth, v => _Settings.Font.UseFullWidth = v);
                    break;
                case CommonKey.LineSelectLeft5:
                    _Form.CyclePaletteNames(_Settings.PaletteName, v => _Settings.PaletteName = v, includeDefaultPaletteName: false);
                    break;
                case CommonKey.LineSelectLeft6:
                    _Hub.ReturnToParent();
                    break;
                case CommonKey.LineSelectRight6:
                    ResetToDefaults();
                    break;
            }
        }

        private void ResetToDefaults()
        {
            var defaults = new SimBridgeEfbSettings();
            _Settings.Host = defaults.Host;
            _Settings.Port = defaults.Port;
            _Settings.Font = defaults.Font;
            _Settings.PaletteName = defaults.PaletteName;
            DrawPage();
        }

        private void CopyScratchpadToHost()
        {
            var text = Scratchpad.Text;
            if(!Validate.IsValidForWebSocketUri(host: text)) {
                Scratchpad.ShowFormatError();
            } else {
                _Settings.Host = text;
                Scratchpad.Clear();
                DrawPage();
            }
        }

        private void CopyScratchpadToPort()
        {
            var text = Scratchpad.Text;

            if(!int.TryParse(text, out var port) || !Validate.IsValidForWebSocketUri(port: port)) {
                Scratchpad.ShowFormatError();
            } else {
                _Settings.Port = port;
                Scratchpad.Clear();
                DrawPage();
            }
        }
    }
}
