﻿// Copyright © 2025 onwards, Andrew Whewell
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
        private IMcdu _Mcdu;
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
        private int _NormalisedBrightnessButtonSteps;

        /// <summary>
        /// Gets or sets a value indicating whether the hub should perpetually try to reconnect to the MCDU if
        /// connection is either never acquired or lost after acquisition. Defaults to true.
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// Gets or sets the brightness step.
        /// </summary>
        public int BrightnessStep { get; set; }

        /// <summary>
        /// Gets <see cref="BrightnessStep"/> expressed as a brightness percentage.
        /// </summary>
        public int BrightnessPercent
        {
            get {
                return BrightnessStep == 0
                    ? 10  // turning it off altogether makes it look broken
                    : BrightnessStep == _NormalisedBrightnessButtonSteps
                        ? 100
                        : (int)((100.0 / _NormalisedBrightnessButtonSteps) * BrightnessStep);
            }
        }

        /// <summary>
        /// True if the backlight is switched on.
        /// </summary>
        public bool IsBacklightOn { get; set; }

        /// <summary>
        /// The connected device or null if no device is connected.
        /// </summary>
        public ProductId? ConnectedDevice => _Mcdu?.ProductId;

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

            ReloadSettings();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            AutoReconnect = false;

            var timer = _ReconnectTimer;
            _ReconnectTimer = null;
            timer.Dispose();

            var settings = _Settings;
            _Mcdu?.Cleanup(
                backlightBrightnessPercent: settings?.Cleanup.BacklightBrightnessPercentOnExit ?? 0,
                displayBrightnessPercent:   settings?.Cleanup.DisplayBrightnessPercentOnExit ?? 0,
                ledBrightnessPercent:       settings?.Cleanup.DisplayBrightnessPercentOnExit ?? 0
            );
            _Mcdu?.Dispose();
            _Mcdu = null;

            PersistSettings();

            GC.SuppressFinalize(this);
        }

        public void ReloadSettings()
        {
            var settings = ConfigStorage.Load<Config.CduhubSettings>();
            _Settings = settings;

            _NormalisedBrightnessButtonSteps = Math.Max(1, Math.Min(100, settings.Brightness.ButtonSteps));
            BrightnessStep = Math.Max(1, Math.Min(100, settings.Brightness.StartAtStep));
            ApplySettingsToMcdu();
        }

        private void PersistSettings()
        {
            var settings = _Settings;
            if(settings != null) {
                var save = false;
                if(settings.Brightness.PersistBetweenSessions && settings.Brightness.StartAtStep != BrightnessStep) {
                    settings.Brightness.StartAtStep = BrightnessStep;
                    save = true;
                }

                if(save) {
                    ConfigStorage.Save(settings);
                }
            }
        }

        public void Connect()
        {
            if(_Mcdu == null && Interlocked.Exchange(ref _ConnectingCount, 1) == 0) {
                try {
                    _Mcdu = McduFactory.ConnectLocal();
                    if(_Mcdu != null) {
                        ApplySettingsToMcdu();

                        _Mcdu.KeyDown += Mcdu_KeyDown;
                        _Mcdu.KeyUp += Mcdu_KeyUp;
                        _Mcdu.Disconnected += Mcdu_Disconnected;
                        _RootPage = new Pages.Root_Page(this);
                        SetMcduBrightness();
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

        private void ApplySettingsToMcdu()
        {
            var settings = _Settings;
            var mcdu = _Mcdu;
            if(settings != null && mcdu != null) {
                mcdu.XOffset = settings.DisplayOffset.XPixels;
                mcdu.YOffset = settings.DisplayOffset.YPixels;
                SetMcduBrightness();
            }
        }

        public void Disconnect()
        {
            if(_Mcdu != null) {
                CleanupPageHistory();
                SelectPage(null);

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
                DeselectPage(_SelectedPage);
                _SelectedPage = page;

                if(page != null) {
                    UploadFont(page.PageFont);
                    page.PreparePage();
                    _PageHistory.Push(page);
                    RefreshDisplay(page);
                    RefreshLeds(page);
                    _Mcdu?.RefreshBrightnesses();
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
                var mcduFont = Fonts.LoadFontByConfigName(pageFont.FontName);
                if(mcduFont != _CurrentFont || _IsCurrentFontFullWidth != pageFont.UseFullWidth) {
                    _CurrentFont = mcduFont;
                    _IsCurrentFontFullWidth = pageFont.UseFullWidth;
                    _Mcdu.Screen.Clear();
                    _Mcdu.UseFont(_CurrentFont, _IsCurrentFontFullWidth.Value);
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

        public Page CreateAndSelectPage(Type pageType)
        {
            var result = CreatePage(pageType);
            SelectPage(result);
            return result;
        }

        public T CreateAndSelectPage<T>() where T: Page
        {
            var result = CreatePage<T>();
            SelectPage(result);
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

        private void Mcdu_Disconnected(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Mcdu_KeyDown(object sender, McduDotNet.KeyEventArgs e)
        {
            if(!_ShuttingDown) {
                var menuKey = _SelectedPage?.MenuKey ?? Key.McduMenu;
                var parentKey = _SelectedPage?.ParentKey ?? Key.Blank2;

                if(e.Key == menuKey && !(_SelectedPage?.DisableMenuKey ?? false)) {
                    ReturnToRoot();
                } else if(e.Key == parentKey && !(_SelectedPage?.DisableParentKey ?? false)) {
                    ReturnToParent();
                } else if(e.Key == Key.Brt && !(_SelectedPage?.DisableBrightnessKeys ?? false)) {
                    ChangeBrightness(true);
                } else if(e.Key == Key.Dim && !(_SelectedPage?.DisableBrightnessKeys ?? false)) {
                    ChangeBrightness(false);
                } else {
                    _SelectedPage?.OnKeyDown(e.Key);
                }
            }
        }

        private void ChangeBrightness(bool increment)
        {
            var delta = increment ? 1 : -1;
            BrightnessStep = Math.Max(1, Math.Min(BrightnessStep + delta, _NormalisedBrightnessButtonSteps));
            SetMcduBrightness();
        }

        private void SetMcduBrightness()
        {
            var mcdu = _Mcdu;
            var settings = _Settings;

            if(mcdu != null) {
                mcdu.DisplayBrightnessPercent = BrightnessPercent;
                mcdu.LedBrightnessPercent = BrightnessPercent;

                var backlightPercent = settings.Backlight.BacklightPercent;
                if(BrightnessPercent > settings.Backlight.TurnOffWhenBrightnessExceedsPercent) {
                    backlightPercent = 0;
                }
                mcdu.BacklightBrightnessPercent = backlightPercent;
            }
        }

        private void Mcdu_KeyUp(object sender, McduDotNet.KeyEventArgs e)
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
