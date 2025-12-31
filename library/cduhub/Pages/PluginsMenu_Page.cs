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
using Cduhub.Plugin;
using wwDevicesDotNet;

namespace Cduhub.Pages
{
    class PluginsMenu_Page : Page
    {
        private const int _PluginsPerPage = 10;

        private List<RegisteredPlugin> _LeftPlugins = new List<RegisteredPlugin>();

        private List<RegisteredPlugin> _RightPlugins = new List<RegisteredPlugin>();

        private int _PageCount;

        public EntryPointPage EntryPointPage { get; set; }

        public int PageNumber { get; set; } = 1;

        public PluginsMenu_Page(Hub hub) : base(hub)
        {
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                BuildPluginsMenu();
                DrawPage();
            }
        }

        private void BuildPluginsMenu()
        {
            var plugins = Plugin.RegisteredPlugins.AllForEntryPoint(EntryPointPage);
            _PageCount = plugins.Count == 0 ? 1 : ((plugins.Count - 1) / _PluginsPerPage) + 1;
            PageNumber = Math.Max(1, Math.Min(PageNumber, _PageCount));

            _LeftPlugins.Clear();
            _RightPlugins.Clear();
            for(var count = 0;count < _PluginsPerPage;++count) {
                var pluginIdx = count + ((PageNumber - 1) * _PluginsPerPage);
                if(pluginIdx < plugins.Count) {
                    var list = count < _PluginsPerPage / 2
                        ? _LeftPlugins
                        : _RightPlugins;
                    list.Add(plugins[pluginIdx]);
                }
            }
        }

        private void DrawPage()
        {
            Output
                .Clear()
                .Centred("<green>PLUGINS")
                .RightToLeft()
                .Write($"<small>{PageNumber}/{_PageCount}")
                .LeftToRight()
                .LeftLabel(6, "<red><small>BACK");

            for(var idx = 0;idx < _PluginsPerPage / 2;++idx) {
                var leftPlugin = idx < _LeftPlugins.Count
                    ? _LeftPlugins[idx]
                    : null;
                var rightPlugin = idx < _RightPlugins.Count
                    ? _RightPlugins[idx]
                    : null;
                if(leftPlugin != null) {
                    Output.LeftLabel(idx + 1, $">{SanitiseInput(leftPlugin.Label)}");
                }
                if(rightPlugin != null) {
                    Output.RightLabel(idx + 1, $"{SanitiseInput(rightPlugin.Label)}<");
                }
            }

            RefreshDisplay();
        }

        public override void OnCommonKeyDown(CommonKey commonKey)
        {
            RegisteredPlugin plugin = null;
            var leftIdx = LeftLineSelectIndex(commonKey);
            var rightIdx = RightLineSelectIndex(commonKey);

            if(rightIdx > -1) {
                plugin = _RightPlugins[rightIdx - 1];
            } else if(leftIdx > -1) {
                if(leftIdx == 6) {
                    _Hub.ReturnToParent();
                } else {
                    plugin = _LeftPlugins[leftIdx - 1];
                }
            }

            if(plugin != null) {
                _Hub.ShowPageForPlugin(plugin);
            }
        }
    }
}
