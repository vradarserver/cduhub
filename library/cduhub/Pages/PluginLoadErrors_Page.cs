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
using System.IO;
using System.Linq;
using Cduhub.Plugin;
using wwDevicesDotNet;

namespace Cduhub.Pages
{
    class PluginLoadErrors_Page : Page
    {
        private int _ErrorIndex;
        private IReadOnlyList<(string PluginFolder, string[] Error)> _Errors;

        public PluginLoadErrors_Page(Hub hub) : base(hub)
        {
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                _Errors = PluginLoader.LoadErrors;
                DrawPage();
            }
        }

        private void DrawPage()
        {
            _ErrorIndex = _Errors.Count == 0
                ? -1
                : Math.Max(0, Math.Min(_ErrorIndex, _Errors.Count - 1));
            var page = _ErrorIndex == -1
                ? 1
                : _ErrorIndex + 1;
            var countPages = Math.Min(1, _Errors.Count);

            Output
                .Clear()
                .Centred("<green>PLUGIN ERROR")
                .RightToLeft()
                .Write($"<white><small>{page}/{countPages}")
                .LeftToRight()
                .LeftLabel(6, $"<red><small>BACK");

            if(_ErrorIndex > -1) {
                (var pluginFolder, var errors) = _Errors[_ErrorIndex];
                Output
                    .LeftLabelTitle(1, " <small>PLUGIN FOLDER")
                    .LeftLabel(1, $"<cyan>{SanitiseInput(Path.GetFileName(pluginFolder))}")
                    .LeftLabelTitle(2, " <small>ERROR")
                    .Large().Cyan()
                    .LabelLine(2)
                    .WrapText(String.Join("\n", errors), 8, clearLines: true);
            }

            RefreshDisplay();
        }

        public override void OnCommonKeyDown(CommonKey commonKey)
        {
            switch(commonKey) {
                case CommonKey.LeftArrowOrPrevPage:
                    --_ErrorIndex;
                    DrawPage();
                    break;
                case CommonKey.RightArrowOrNextPage:
                    ++_ErrorIndex;
                    DrawPage();
                    break;
                case CommonKey.LineSelectLeft6:
                    _Hub.ReturnToParent();
                    break;
            }
        }
    }
}
