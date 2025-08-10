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
using McduDotNet;

namespace Cduhub.Pages.Init
{
    public abstract class CommonSettingsPage<TSettings> : Page
        where TSettings: Settings, new()
    {
        protected TSettings _Settings;
        protected readonly FormHelper _Form;

        protected abstract string Title { get; }

        protected virtual bool ShowResetToDefaults { get; } = true;

        protected virtual bool ShowReturnToParent { get; } = true;

        protected virtual bool SuppressScratchpad { get; }

        public CommonSettingsPage(Hub hub) : base(hub)
        {
            if(!SuppressScratchpad) {
                Scratchpad = new Scratchpad();
            }
            _Form = new FormHelper(() => DrawPage());
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                LoadSettings();
            } else {
                SaveSettings();
                ApplySettings();
            }
        }

        protected virtual void ApplySettings()
        {
        }

        protected virtual void LoadSettings()
        {
            _Settings = ConfigStorage.Load<TSettings>();
            DrawPage();
        }

        private void SaveSettings()
        {
            ConfigStorage.Save(_Settings);
        }

        protected virtual void DrawPage()
        {
            if(!String.IsNullOrEmpty(Title)) {
                Output
                    .Line(0)
                    .Large()
                    .Centred($"<green>{Title}");
            }

            if(ShowResetToDefaults) {
                Output.RightLabel(6, "<magenta>RESET<");
            }
            if(ShowReturnToParent) {
                Output.LeftLabel(6, "<red><small>>BACK");
            }
            if(!SuppressScratchpad) {
                CopyScratchpadIntoDisplay();
            }

            RefreshDisplay();
        }

        public override void OnKeyDown(Key key)
        {
            switch(key) {
                case Key.LineSelectLeft6:
                    if(ShowReturnToParent) {
                        _Hub.ReturnToParent();
                    } else {
                        base.OnKeyDown(key);
                    }
                    break;
                case Key.LineSelectRight6:
                    if(ShowResetToDefaults) {
                        ResetToDefaults();
                    } else {
                        base.OnKeyDown(key);
                    }
                    break;
                default:
                    base.OnKeyDown(key);
                    break;
            }
        }

        protected virtual void ResetToDefaults()
        {
            _Settings = new TSettings();
            DrawPage();
        }
    }
}
