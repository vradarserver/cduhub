// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Avalonia.Controls;
using McduDotNet;

namespace Cduhub.DesktopGui
{
    public partial class MainWindow : Window
    {
        private readonly Hub? _Hub;
        private bool _HubEventsHooked;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(Hub hub) : this()
        {
            _Hub = hub;
        }

        /// <inheritdoc/>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if(_Hub != null) {
                HookHub();

                _CduDisplay.CopyFromDisplayFont(_Hub.CurrentDisplayFont, _Hub.CurrentXOffset, _Hub.CurrentYOffset);
                _CduDisplay.CopyFromDisplayPalette(_Hub.CurrentDisplayPalette);
                _CduDisplay.CopyFromDisplayBuffer(_Hub.CurrentDisplayBuffer);
            }
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            UnhookHub();
        }

        private void HookHub()
        {
            if(_Hub != null && !_HubEventsHooked) {
                _HubEventsHooked = true;
                _Hub.DisplayChanging += Hub_DisplayChanging;
                _Hub.FontChanging += Hub_FontChanging;
                _Hub.PaletteChanging += Hub_PaletteChanging;
            }
        }

        private void UnhookHub()
        {
            if(_Hub != null && _HubEventsHooked) {
                _HubEventsHooked = false;
                _Hub.DisplayChanging -= Hub_DisplayChanging;
                _Hub.FontChanging -= Hub_FontChanging;
                _Hub.PaletteChanging -= Hub_PaletteChanging;
            }
        }

        private void Hub_DisplayChanging(object? sender, DisplayChangingEventArgs e)
        {
            _CduDisplay.CopyFromDisplayBuffer(e.DisplayBuffer);
        }

        private void Hub_FontChanging(object? sender, FontChangingEventArgs e)
        {
            _CduDisplay.CopyFromDisplayFont(e.DisplayFont, e.XOffset, e.YOffset);
        }

        private void Hub_PaletteChanging(object? sender, PaletteChangingEventArgs e)
        {
            _CduDisplay.CopyFromDisplayPalette(e.DisplayPalette);
        }
    }
}
