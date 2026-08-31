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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using McduDotNet;

namespace Cduhub.DesktopGui
{
    public partial class MainWindow : Window
    {
        private readonly Hub? _Hub;
        private bool _HubEventsHooked;

        private readonly ObservableCollection<FlightSimRow> _FlightSimRows = new();
        private readonly DispatcherTimer _RefreshTimer;
        private bool _FlightSimulatorsHooked;
        private int _FlightSimulatorsChanged;

        public MainWindow()
        {
            InitializeComponent();

            _RefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250), };
            _RefreshTimer.Tick += RefreshTimer_Tick;
        }

        public MainWindow(Hub hub) : this()
        {
            _Hub = hub;
        }

        /// <inheritdoc/>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            Title = $"CDU Hub {CduhubVersions.LibraryVersion}";
            _Link_ConfigFolder.Content = ConfigStorage.Folder;

            _Grid_FlightSims.ItemsSource = _FlightSimRows;
            HookFlightSimulators();
            RefreshFlightSimulators();
            _RefreshTimer.Start();

            if(_Hub != null) {
                HookHub();
                UpdateStateDisplay();

                _CduDisplay.CopyFromDisplayFont(_Hub.CurrentDisplayFont, _Hub.CurrentXOffset, _Hub.CurrentYOffset);
                _CduDisplay.CopyFromDisplayPalette(_Hub.CurrentDisplayPalette);
                _CduDisplay.CopyFromDisplayBuffer(_Hub.CurrentDisplayBuffer);
            }
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _RefreshTimer.Stop();
            UnhookFlightSimulators();
            UnhookHub();
        }

        private void HookHub()
        {
            if(_Hub != null && !_HubEventsHooked) {
                _HubEventsHooked = true;
                _Hub.ConnectedDeviceChanged += Hub_ConnectedDeviceChanged;
                _Hub.DisplayChanging += Hub_DisplayChanging;
                _Hub.FontChanging += Hub_FontChanging;
                _Hub.PaletteChanging += Hub_PaletteChanging;
            }
        }

        private void UnhookHub()
        {
            if(_Hub != null && _HubEventsHooked) {
                _HubEventsHooked = false;
                _Hub.ConnectedDeviceChanged -= Hub_ConnectedDeviceChanged;
                _Hub.DisplayChanging -= Hub_DisplayChanging;
                _Hub.FontChanging -= Hub_FontChanging;
                _Hub.PaletteChanging -= Hub_PaletteChanging;
            }
        }

        private void HookFlightSimulators()
        {
            if(!_FlightSimulatorsHooked) {
                _FlightSimulatorsHooked = true;
                ConnectedFlightSimulators.FlightSimulatorStateChanged += FlightSimulators_StateChanged;
            }
        }

        private void UnhookFlightSimulators()
        {
            if(_FlightSimulatorsHooked) {
                _FlightSimulatorsHooked = false;
                ConnectedFlightSimulators.FlightSimulatorStateChanged -= FlightSimulators_StateChanged;
            }
        }

        private void RefreshFlightSimulators()
        {
            _FlightSimRows.Clear();
            foreach(var mcdu in ConnectedFlightSimulators.GetFlightSimulatorMcdus()) {
                _FlightSimRows.Add(new FlightSimRow(mcdu));
            }
        }

        private void UpdateStateDisplay()
        {
            var device = _Hub?.ConnectedDevice;
            _Label_UsbDeviceState.Text = device == null
                ? "Not connected"
                : $"{device.Description} connected";
        }

        private static void OpenFolder(string path)
        {
            if(Directory.Exists(path)) {
                Shell.Open(path);
            }
        }

        private void About_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new AboutWindow();
            dialog.ShowDialog(this);
        }

        private void ConfigFolder_Click(object? sender, RoutedEventArgs e)
        {
            OpenFolder(ConfigStorage.Folder);
        }

        private void FlightSimulators_StateChanged(object? sender, EventArgs e)
        {
            Interlocked.Exchange(ref _FlightSimulatorsChanged, 1);
        }

        private void Hub_ConnectedDeviceChanged(object? sender, EventArgs e)
        {
            if(Dispatcher.UIThread.CheckAccess()) {
                UpdateStateDisplay();
            } else {
                Dispatcher.UIThread.Post(UpdateStateDisplay);
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

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            if(Interlocked.Exchange(ref _FlightSimulatorsChanged, 0) != 0) {
                RefreshFlightSimulators();
            }
        }
    }
}
