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
using Cduhub.Config;
using McduDotNet;

namespace Cduhub.Pages.Init
{
    public abstract class CommonSettingsPage<TSettings> : Page
        where TSettings: Settings, new()
    {
        protected class OptionConfig
        {
            public string Label { get; set; }

            public Func<string> Format { get; set; }

            public Action Parse { get; set; }
        }

        protected TSettings _Settings;
        protected readonly FormHelper _Form;
        protected readonly List<OptionConfig> _LeftOptions = new List<OptionConfig>();
        protected readonly List<OptionConfig> _RightOptions = new List<OptionConfig>();

        protected abstract string Title { get; }

        protected virtual bool ShowResetToDefaults { get; } = true;

        protected virtual bool ShowReturnToParent { get; } = true;

        protected virtual bool SuppressScratchpad { get; }

        protected virtual bool ApplySettingsImmediately { get; }

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

        protected virtual void SaveSettings()
        {
            ConfigStorage.Save(_Settings);
        }

        protected OptionConfig LeftOption(
            string label,
            Func<string> format,
            Action parse
        )
        {
            var result = new OptionConfig() {
                Label = label,
                Format = format,
                Parse = parse,
            };
            _LeftOptions.Add(result);
            return result;
        }

        protected OptionConfig RightOption(
            string label,
            Func<string> format,
            Action parse
        )
        {
            var result = new OptionConfig() {
                Label = label,
                Format = format,
                Parse = parse,
            };
            _RightOptions.Add(result);
            return result;
        }

        protected virtual void DrawPage()
        {
            Output.Clear();

            if(!String.IsNullOrEmpty(Title)) {
                Output
                    .Line(0)
                    .Large()
                    .Centred($"<green>{Title}");
            }
            for(var idx = 0;idx < _LeftOptions.Count;++idx) {
                var option = _LeftOptions[idx];
                Output
                    .LeftLabelTitle(idx + 1, $"<small> {option.Label}")
                    .LeftLabel(idx + 1, $"<cyan>{SanitiseInput(option.Format())}");
            }
            for(var idx = 0;idx < _RightOptions.Count;++idx) {
                var option = _RightOptions[idx];
                Output
                    .RightLabelTitle(idx + 1, $"<small>{option.Label} ")
                    .RightLabel(idx + 1, $"<cyan>{SanitiseInput(option.Format())}");
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
            AddPageArrows();
            AddToPage();

            RefreshDisplay();
        }

        protected virtual void AddToPage()
        {
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
                    bool runOption(List<OptionConfig> options, int selectIndex)
                    {
                        var ranParse = false;
                        if(selectIndex > -1 && selectIndex <= options.Count) {
                            var option = options[selectIndex - 1];
                            option.Parse();
                            ranParse = true;
                            DoApplySettingsImmediately();
                        }
                        return ranParse;
                    }
                    var parsed = runOption(_LeftOptions, LeftLineSelectIndex(key));
                    parsed = parsed || runOption(_RightOptions, RightLineSelectIndex(key));
                    if(!parsed) {
                        parsed = CreateAndSelectPageForArrows(key);
                    }
                    if(!parsed) {
                        base.OnKeyDown(key);
                    }
                    break;
            }
        }

        private void DoApplySettingsImmediately()
        {
            if(ApplySettingsImmediately) {
                SaveSettings();
                ApplySettings();
            }
        }

        protected virtual void ResetToDefaults()
        {
            ApplyDefaults(new TSettings());
            DoApplySettingsImmediately();
            DrawPage();
        }

        protected virtual void ApplyDefaults(TSettings defaults)
        {
            _Settings = defaults;
        }
    }
}
