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
using System.Threading;
using Cduhub.Config;
using McduDotNet;

namespace Cduhub
{
    /// <summary>
    /// The root hub object.
    /// </summary>
    public class Hub : IDisposable
    {
        private bool _ShuttingDown;
        private ICdu _Cdu;
        private Page _SelectedPage;
        private Page _RootPage;
        private int _ConnectingCount;
        private System.Timers.Timer _ReconnectTimer;
        private bool _WaitingForConnect = true;
        private Stack<Page> _PageHistory = new Stack<Page>();
        private Dictionary<Type, Page> _PageTypeMap = new Dictionary<Type, Page>();
        private McduFontFile _CurrentFont;
        private bool? _IsCurrentFontFullWidth;
        private CduhubSettings _Settings;
        private BrightnessSettings _BrightnessSettings;

        /// <summary>
        /// Gets or sets a value indicating whether the hub should perpetually try to reconnect to the MCDU if
        /// connection is either never acquired or lost after acquisition. Defaults to true.
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// The connected device or null if no device is connected.
        /// </summary>
        public DeviceIdentifier ConnectedDevice => _Cdu?.DeviceId;

        /// <summary>
        /// The HttpClient for pages to use.
        /// </summary>
        public HttpClient HttpClient { get; } = new HttpClient();

        /// <summary>
        /// The default font reference. This is user configurable.
        /// </summary>
        public FontReference DefaultFontReference
        {
            get {
                return _Settings.Font ?? new FontReference();
            }
        }

        /// <summary>
        /// The default palette. This is user configurable.
        /// </summary>
        public Palette DefaultPalette
        {
            get {
                return Palettes.LoadByConfigName(_Settings.PaletteName);
            }
        }

        /// <summary>
        /// The ambient light expressed as a percentage.
        /// </summary>
        public int AmbientLightPercent => _Cdu?.AmbientLightPercent ?? 0;

        /// <summary>
        /// The CDU's current display brightness percent value.
        /// </summary>
        public int DisplayBrightnessPercent => _Cdu?.DisplayBrightnessPercent ?? 0;

        /// <summary>
        /// The CDU's current LED intensity percent value.
        /// </summary>
        public int LedBrightnessPercent => _Cdu?.LedBrightnessPercent ?? 0;

        /// <summary>
        /// The CDU's current keyboard backlight brightness percent value.
        /// </summary>
        public int BacklightBrightnessPercent => _Cdu?.BacklightBrightnessPercent ?? 0;

        public CommonKey InterruptKey1 { get; set; } = CommonKey.Brt;

        public CommonKey InterruptKey2 { get; set; } = CommonKey.Dim;

        public CommonKey MenuKey { get; set; } = CommonKey.McduMenuOrMenu;

        public CommonKey InitKey { get; set; } = CommonKey.InitOrInitRef;

        public string InterruptKey1Name => InterruptKey1.Describe(_Cdu);

        public string InterruptKey2Name => InterruptKey2.Describe(_Cdu);

        public string MenuKeyName => MenuKey.Describe(_Cdu);

        public string InitKeyName => InitKey.Describe(_Cdu);

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

            LoadSettings();
            ApplySettingsToDevice();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            AutoReconnect = false;

            var timer = _ReconnectTimer;
            _ReconnectTimer = null;
            timer.Dispose();

            var settings = _Settings;
            _Cdu?.Cleanup(
                backlightBrightnessPercent: settings?.Cleanup.BacklightBrightnessPercentOnExit ?? 0,
                displayBrightnessPercent:   settings?.Cleanup.DisplayBrightnessPercentOnExit ?? 0,
                ledBrightnessPercent:       settings?.Cleanup.DisplayBrightnessPercentOnExit ?? 0
            );
            _Cdu?.Dispose();
            _Cdu = null;

            PersistSettings();

            GC.SuppressFinalize(this);
        }

        private void LoadSettings()
        {
            var settings = ConfigStorage.Load<Config.CduhubSettings>();
            _Settings = settings;

            var brightnessSettings = ConfigStorage.Load<Config.BrightnessSettings>();
            _BrightnessSettings = brightnessSettings;
        }

        public void ReloadSettings()
        {
            LoadSettings();
            ApplySettingsToDevice();

            _CurrentFont = null;
            if(_SelectedPage != null) {
                UploadFont(_SelectedPage.PageFont);
                RefreshPalette(_SelectedPage, forceRefresh: true);
                RefreshLeds(_SelectedPage);
                _Cdu?.RefreshBrightnesses();
            }
        }

        public void ReloadBrightness()
        {
            LoadSettings();
            ApplySettingsToDevice();
            _Cdu?.ApplyAutoBrightness();
            _Cdu?.RefreshBrightnesses();
        }

        private void PersistSettings()
        {
            var settings = _Settings;
            if(settings != null) {
                var save = false;

                // This used to have stuff to save brightness values between sessions. That's
                // gone away, and if you can see this comment then there's nothing that needs
                // persisting any more. However I've kept the function for when, inevitably,
                // something new needs persisting in the future.
                // if(condition) {
                //   set up settings
                //   save = true;
                // }

                if(save) {
                    ConfigStorage.Save(settings);
                }
            }
        }

        public void Connect()
        {
            if(_Cdu == null && Interlocked.Exchange(ref _ConnectingCount, 1) == 0) {
                try {
                    _Cdu = CduFactory.ConnectLocal();
                    if(_Cdu != null) {
                        ApplySettingsToDevice();

                        _Cdu.KeyDown += Cdu_KeyDown;
                        _Cdu.KeyUp += Cdu_KeyUp;
                        _Cdu.Disconnected += Cdu_Disconnected;
                        _Cdu.AmbientLightChanged += Cdu_AmbientLightChanged;
                        _RootPage = new Pages.Root_Page(this);
                        _CurrentFont = null;
                        _IsCurrentFontFullWidth = null;

                        SelectPage(_RootPage);
                        OnConnectedDeviceChanged();
                    }
                } finally {
                    Interlocked.Exchange(ref _ConnectingCount, 0);
                    _WaitingForConnect = false;
                }
            }
        }

        private void ApplySettingsToDevice()
        {
            var settings = _Settings;
            var cdu = _Cdu;
            if(settings != null && cdu != null) {
                cdu.XOffset = settings.DisplayOffset.XPixels;
                cdu.YOffset = settings.DisplayOffset.YPixels;
            }

            var brightnessSettings = _BrightnessSettings;
            if(brightnessSettings != null && cdu != null) {
                brightnessSettings.CopyToCdu(cdu);
                cdu.ApplyAutoBrightness();
            }
        }

        public void Disconnect()
        {
            if(_Cdu != null) {
                CleanupPageHistory();
                SelectPage(null);

                _Cdu.KeyDown -= Cdu_KeyDown;
                _Cdu.KeyUp -= Cdu_KeyUp;
                _Cdu.Cleanup();
                _Cdu.Dispose();
                _Cdu = null;
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

        public void SelectPage(Page page, bool replaceCurrentInHistory = false)
        {
            if(page != _SelectedPage) {
                DeselectPage(_SelectedPage);
                _SelectedPage = page;

                if(page != null) {
                    RefreshPalette(page);
                    UploadFont(page.PageFont);
                    page.PreparePage();
                    if(replaceCurrentInHistory && _PageHistory.Count > 0) {
                        _PageHistory.Pop();
                    }
                    _PageHistory.Push(page);
                    RefreshDisplay(page);
                    RefreshLeds(page);
                    _Cdu?.RefreshBrightnesses();
                    _SelectedPage.OnSelected(true);
                }
            }
        }

        private void UploadFont(FontReference pageFont)
        {
            if(pageFont == null) {
                _CurrentFont = null;
                _IsCurrentFontFullWidth = null;
            } else {
                var font = Fonts.LoadFontByConfigName(pageFont.FontName);
                if(font != _CurrentFont || _IsCurrentFontFullWidth != pageFont.UseFullWidth) {
                    _CurrentFont = font;
                    _IsCurrentFontFullWidth = pageFont.UseFullWidth;
                    _Cdu.Screen.Clear();
                    _Cdu.UseFont(_CurrentFont, _IsCurrentFontFullWidth.Value);
                }
            }
        }

        private void DeselectPage(Page page)
        {
            page?.OnSelected(false);
        }

        public void ReturnToRoot()
        {
            CleanupPageHistory();
            SelectPage(_RootPage);
        }

        public void ReturnToParent()
        {
            if(_PageHistory.Count > 1) {
                var currentPage = _PageHistory.Pop();
                DeselectPage(currentPage);
                _SelectedPage = null;

                var parent = _PageHistory.Pop();
                SelectPage(parent);
            }
        }

        public void RefreshDisplay(Page page)
        {
            if(page == _SelectedPage) {
                _Cdu.Screen.CopyFrom(page.Screen);
                _Cdu.RefreshDisplay();
            }
        }

        public void RefreshLeds(Page page)
        {
            if(page == _SelectedPage) {
                _Cdu.Leds.CopyFrom(page.Leds);
                _Cdu.RefreshLeds();
            }
        }

        public void RefreshPalette(Page page, bool forceRefresh = false)
        {
            if(page == _SelectedPage) {
                _Cdu.Palette.CopyFrom(page.Palette);
                _Cdu.RefreshPalette(skipDuplicateCheck: forceRefresh);
            }
        }

        public void Shutdown()
        {
            try {
                _ShuttingDown = true;
                _SelectedPage = null;
                CleanupPageHistory();
            } finally {
                OnCloseApplication();
            }
        }

        public Page CreatePage(Type pageType)
        {
            lock(_PageTypeMap) {
                if(!_PageTypeMap.TryGetValue(pageType, out var result)) {
                    result = (Page)Activator.CreateInstance(pageType, new object[] { this });
                    _PageTypeMap.Add(pageType, result);
                }
                return result;
            }
        }

        public T CreatePage<T>() where T: Page
        {
            return (T)CreatePage(typeof(T));
        }

        public Page CreateAndSelectPage(Type pageType, bool replaceCurrentInHistory = false)
        {
            var result = CreatePage(pageType);
            SelectPage(result, replaceCurrentInHistory);
            return result;
        }

        public T CreateAndSelectPage<T>(bool replaceCurrentInHistory = false) where T: Page
        {
            var result = CreatePage<T>();
            SelectPage(result, replaceCurrentInHistory);
            return result;
        }

        private void CleanupPageHistory()
        {
            _SelectedPage = null;
            while(_PageHistory.Count > 0) {
                var page = _PageHistory.Pop();
                DeselectPage(page);
            }
        }

        private void Cdu_Disconnected(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Cdu_AmbientLightChanged(object sender, EventArgs e)
        {
            _SelectedPage?.OnAmbientLightChanged(_Cdu.AmbientLightPercent);
        }

        private void Cdu_KeyDown(object sender, McduDotNet.KeyEventArgs e)
        {
            if(!_ShuttingDown) {
                var cdu = sender as ICdu;
                var initCommonKey = InitKey;
                var menuCommonKey = _SelectedPage?.MenuKey ?? MenuKey;
                var parentCommonKey = _SelectedPage?.ParentKey ?? InterruptKey2;

                var initKey = initCommonKey.ToKey(cdu);
                var menuKey = menuCommonKey.ToKey(cdu);
                var parentKey = parentCommonKey.ToKey(cdu);

                if(e.Key == menuKey && !(_SelectedPage?.DisableMenuKey ?? false)) {
                    ReturnToRoot();
                } else if(e.Key == parentKey && !(_SelectedPage?.DisableParentKey ?? false)) {
                    ReturnToParent();
                } else if(e.Key == initKey && !(_SelectedPage?.DisableInitKey ?? false)) {
                    CreateAndSelectPage<Pages.Init.InitMenu_Page>();
                } else {
                    _SelectedPage?.OnKeyDown(e.Key);
                    if(e.CommonKey != CommonKey.DeviceSpecific) {
                        _SelectedPage?.OnCommonKeyDown(e.CommonKey);
                    }
                }
            }
        }

        private void Cdu_KeyUp(object sender, McduDotNet.KeyEventArgs e)
        {
            if(!_ShuttingDown) {
                _SelectedPage?.OnKeyUp(e.Key);
            }
        }

        private void ReconnectTimer_Elapsed(object sender, EventArgs e)
        {
            try {
                if(!_ShuttingDown) {
                    PerformAutoReconnect();
                }
            } finally {
                if(!_ShuttingDown) {
                    var timer = _ReconnectTimer;
                    timer?.Start();
                }
            }
        }
    }
}
