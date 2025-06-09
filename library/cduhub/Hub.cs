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
using System.Net.Http;
using System.Text;
using McduDotNet;

namespace Cduhub
{
    public class Hub : IDisposable
    {
        private IMcdu _Mcdu;
        private Page _SelectedPage;
        private Page _RootPage;

        public ProductId? ConnectedDevice => _Mcdu?.ProductId;

        public HttpClient HttpClient { get; } = new HttpClient();

        public event EventHandler CloseApplication;

        protected virtual void OnCloseApplication() => CloseApplication?.Invoke(this, EventArgs. Empty);

        public void Dispose()
        {
            _Mcdu?.Cleanup();
            _Mcdu?.Dispose();
            _Mcdu = null;
            GC.SuppressFinalize(this);
        }

        public void Connect()
        {
            if(_Mcdu == null) {
                _Mcdu = McduFactory.ConnectLocal();
                _Mcdu.KeyDown += Mcdu_KeyDown;
                _Mcdu.KeyUp += Mcdu_KeyUp;
                _RootPage = new Pages.Root_Page(this);
                SelectPage(_RootPage);
            }
        }

        public void Disconnect()
        {
            if(_Mcdu != null) {
                SelectPage(null);
                _RootPage?.OnDisconnecting();

                _Mcdu.KeyDown -= Mcdu_KeyDown;
                _Mcdu.KeyUp -= Mcdu_KeyUp;
                _Mcdu.Cleanup();
                _Mcdu.Dispose();
                _Mcdu = null;
            }
        }

        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        public void SelectPage(Page page)
        {
            if(page != _SelectedPage) {
                _SelectedPage?.OnSelected(false);
                page.OnPrepareScreen();
                _SelectedPage = page;

                _Mcdu.Screen.CopyFrom(page.Screen);
                _Mcdu.RefreshDisplay();

                _SelectedPage.OnSelected(true);
            }
        }

        public void RefreshDisplay(Page page)
        {
            if(page == _SelectedPage) {
                _Mcdu.Screen.CopyFrom(page.Screen);
                _Mcdu.RefreshDisplay();
            }
        }

        public void Shutdown() => OnCloseApplication();

        private void Mcdu_KeyDown(object sender, McduDotNet.KeyEventArgs e)
        {
            var menuKey = _SelectedPage?.MenuKey ?? Key.McduMenu;

            if(e.Key == menuKey) {
                SelectPage(_RootPage);
            } else {
                _SelectedPage?.OnKeyDown(e.Key);
            }
        }

        private void Mcdu_KeyUp(object sender, McduDotNet.KeyEventArgs e)
        {
            _SelectedPage?.OnKeyUp(e.Key);
        }
    }
}
