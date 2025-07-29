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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Cduhub.FlightSim;

namespace Cduhub.WindowsGui
{
    public partial class MainForm : Form
    {
        private ListViewHelper<IFlightSimulatorMcdu, object> _ConnectedFlightSimulatorsListView;
        private bool _HookedConnectedFlightSimulators;
        private int _ConnectedFlightSimulatorsChanged = 0;
        private bool _HubShutdownRequired = true;

        public Hub Hub => Program.Hub;

        public string StateText
        {
            get => _Label_UsbDeviceState.Text;
            set {
                if(StateText != value) {
                    _Label_UsbDeviceState.Text = value;
                }
            }
        }

        public MainForm()
        {
            InitializeComponent();

            _ConnectedFlightSimulatorsListView = new(
                _ListView_ConnectedFlightSimulators,
                flightsim => new string[] {
                    flightsim.FlightSimulatorName,
                    flightsim.AircraftName,
                    flightsim.ConnectionState.ToString(),
                    flightsim.LastMessageTimeUtc == default
                        ? ""
                        : flightsim.LastMessageTimeUtc.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"),
                    flightsim.CountMessagesFromSimulator.ToString("N0")
                },
                extractID: flightsim => flightsim,
                autoResizeToContent: true
            );
        }

        private void UpdateStateDisplay()
        {
            var device = Hub.ConnectedDevice;
            StateText = device == null
                ? "Not connected to an MCDU"
                : $"Connected to a {device} MCDU";
        }

        private void UpdateConnectedFlightSimulatorsDisplay()
        {
            _ConnectedFlightSimulatorsListView.RefreshList(
                ConnectedFlightSimulators.GetFlightSimulatorMcdus()
            );
        }

        private void OpenConfigStorageFolder()
        {
            if(!Directory.Exists(ConfigStorage.Folder)) {
                MessageBox.Show(
                    $"Could not create {ConfigStorage.Folder}",
                    "No Working Folder"
                );
            } else {
                Process.Start(new ProcessStartInfo() {
                    FileName = "explorer.exe",
                    Arguments = ConfigStorage.Folder,
                    UseShellExecute = true,
                });
            }
        }

        private void ShowAboutForm()
        {
            using(var form = new AboutForm()) {
                form.ShowDialog(this);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if(!e.Cancel && _HubShutdownRequired) {
                _HubShutdownRequired = false;
                Hub.Shutdown();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) {
                Text = $"{Text} {Program.Version}";

                _LinkLabel_ConfigFolder.Text = ConfigStorage.Folder;
                Hub.ConnectedDeviceChanged += Hub_ConnectedDeviceChanged;
                Hub.CloseApplication += Hub_CloseApplication;
                UpdateStateDisplay();
                UpdateConnectedFlightSimulatorsDisplay();

                ConnectedFlightSimulators.FlightSimulatorStateChanged += ConnectedFlightSimulators_FlightSimulatorStateChanged;
                _HookedConnectedFlightSimulators = true;
                _ConnectedFlightSimulatorsListView.SendResizeAllColumns();
            }
        }

        private void UnhookConnectedFlightSimulators()
        {
            if(_HookedConnectedFlightSimulators) {
                _HookedConnectedFlightSimulators = false;
                ConnectedFlightSimulators.FlightSimulatorStateChanged -= ConnectedFlightSimulators_FlightSimulatorStateChanged;
            }
        }

        private void ConnectedFlightSimulators_FlightSimulatorStateChanged(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref _ConnectedFlightSimulatorsChanged, 1);
        }

        private void Hub_CloseApplication(object sender, EventArgs e)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => Hub_CloseApplication(sender, e)));
            } else {
                _HubShutdownRequired = false;
                Close();
            }
        }

        private void Hub_ConnectedDeviceChanged(object sender, EventArgs e)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => Hub_ConnectedDeviceChanged(sender, e)));
            } else {
                UpdateStateDisplay();
            }
        }

        private void LinkLabel_About_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowAboutForm();
        }

        private void LinkLabel_ConfigFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenConfigStorageFolder();
        }

        private void RefreshDisplayTimer_Tick(object sender, EventArgs e)
        {
            if(Interlocked.Exchange(ref _ConnectedFlightSimulatorsChanged, 0) != 0) {
                UpdateConnectedFlightSimulatorsDisplay();
            }
        }
    }
}
