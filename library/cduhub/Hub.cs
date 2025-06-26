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
using System.Net.Http;
using System.Threading;
using McduDotNet;

namespace Cduhub
{
    /// <summary>
    /// The root hub object.
    /// </summary>
    public class Hub : IDisposable
    {
        private IMcdu _Mcdu;
        private Page _SelectedPage;
        private Page _RootPage;
        private int _ConnectingCount;
        private System.Timers.Timer _ReconnectTimer;
        private bool _WaitingForConnect = true;

        /// <summary>
        /// Gets or sets a value indicating whether the hub should perpetually try to reconnect to the MCDU if
        /// connection is either never acquired or lost after acquisition. Defaults to true.
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// The connected device or null if no device is connected.
        /// </summary>
        public ProductId? ConnectedDevice => _Mcdu?.ProductId;

        /// <summary>
        /// The HttpClient for pages to use.
        /// </summary>
        public HttpClient HttpClient { get; } = new HttpClient();

        /// <summary>
        /// Raised when the hub wants the parent application to close.
        /// </summary>
        public event EventHandler CloseApplication;

        /// <summary>
        /// Raises <see cref="CloseApplication"/>.
        /// </summary>
        protected virtual void OnCloseApplication()
        {
            CloseApplication?.Invoke(this, EventArgs. Empty);
        }

        /// <summary>
        /// Raised when <see cref="ConnectedDevice"/> changes.
        /// </summary>
        public event EventHandler ConnectedDeviceChanged;

        /// <summary>
        /// Raises <see cref="ConnectedDeviceChanged"/>.
        /// </summary>
        protected virtual void OnConnectedDeviceChanged()
        {
            ConnectedDeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Hub()
        {
            _ReconnectTimer = new System.Timers.Timer() {
                AutoReset = false,
                Interval = 1000,
            };
            _ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            _ReconnectTimer.Start();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            AutoReconnect = false;

            var timer = _ReconnectTimer;
            _ReconnectTimer = null;
            timer.Dispose();

            _Mcdu?.Cleanup();
            _Mcdu?.Dispose();
            _Mcdu = null;

            GC.SuppressFinalize(this);
        }

        public void Connect()
        {
            if(_Mcdu == null && Interlocked.Exchange(ref _ConnectingCount, 1) == 0) {
                try {
                    _Mcdu = McduFactory.ConnectLocal();
                    if(_Mcdu != null) {
                        _Mcdu.KeyDown += Mcdu_KeyDown;
                        _Mcdu.KeyUp += Mcdu_KeyUp;
                        _Mcdu.Disconnected += Mcdu_Disconnected;
                        _RootPage = new Pages.Root_Page(this);
                        SelectPage(_RootPage);
                        OnConnectedDeviceChanged();
                    }
                } finally {
                    Interlocked.Exchange(ref _ConnectingCount, 0);
                    _WaitingForConnect = false;
                }
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
                OnConnectedDeviceChanged();
            }
        }

        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        private void PerformAutoReconnect()
        {
            if(AutoReconnect && !_WaitingForConnect) {
                Connect();
            }
        }

        public void SelectPage(Page page)
        {
            if(page != _SelectedPage) {
                _SelectedPage?.OnSelected(false);
                page?.OnPrepareScreen();
                _SelectedPage = page;

                if(page != null) {
                    RefreshDisplay(page);
                    RefreshLeds(page);

                    _SelectedPage.OnSelected(true);
                }
            }
        }

        public void RefreshDisplay(Page page)
        {
            if(page == _SelectedPage) {
                _Mcdu.Screen.CopyFrom(page.Screen);
                _Mcdu.RefreshDisplay();
            }
        }

        public void RefreshLeds(Page page)
        {
            if(page == _SelectedPage) {
                _Mcdu.Leds.CopyFrom(page.Leds);
                _Mcdu.RefreshLeds();
            }
        }

        public void Shutdown() => OnCloseApplication();

        private void Mcdu_Disconnected(object sender, EventArgs e)
        {
            Disconnect();
        }

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

        private void ReconnectTimer_Elapsed(object sender, EventArgs e)
        {
            try {
                PerformAutoReconnect();
            } finally {
                var timer = _ReconnectTimer;
                timer?.Start();
            }
        }
    }
}
