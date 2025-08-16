// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.Config;
using Cduhub.FlightSim;
using McduDotNet;

namespace Cduhub.Pages.FlightSimulator
{
    class SimBridge_Page : CommonFlightSimPage
    {
        private SimBridgeA320RemoteMcdu _SimBridgeA320;

        public override DeviceType SimulatorDeviceType => DeviceType.AirbusA320Mcdu;

        public override FontReference PageFont => SettingsFont<SimBridgeEfbSettings>(r => r.Font);

        public override Palette Palette => SettingsPalette<SimBridgeEfbSettings>(r => r.PaletteName);

        public SimBridge_Page(Hub hub) : base(hub)
        {
        }

        protected override void Connect()
        {
            Disconnect();

            var settings = ConfigStorage.Load<SimBridgeEfbSettings>();
            var mcdu = new SimBridgeA320RemoteMcdu(_Hub.ConnectedDevice.DeviceUser, Screen, Leds) {
                Host = settings.Host,
                Port = settings.Port,
            };
            _SimBridgeA320 = mcdu;
            ShowConnectionState(_SimBridgeA320?.ConnectionState);
            mcdu.DisplayRefreshRequired += SimBridgeA320_DisplayRefreshRequired;
            mcdu.LedsRefreshRequired += SimBridgeA320_LedsRefreshRequired;
            mcdu.ConnectionStateChanged += SimBridgeA320_ConnectionStateChanged;

            ConnectedFlightSimulators.AddFlightSimulatorMcdu(mcdu);
            mcdu.ReconnectToSimulator();
        }

        protected override void Disconnect()
        {
            if(_SimBridgeA320 != null) {
                var reference = _SimBridgeA320;
                _SimBridgeA320 = null;

                try {
                    reference.DisplayRefreshRequired -= SimBridgeA320_DisplayRefreshRequired;
                    reference.LedsRefreshRequired -= SimBridgeA320_LedsRefreshRequired;
                    reference.Dispose();
                    reference.ConnectionStateChanged -= SimBridgeA320_ConnectionStateChanged;
                    ConnectedFlightSimulators.RemoveFlightSimulatorMcdu(reference);
                } catch {
                    ;
                }
            }
        }

        public override void OnKeyDown(Key key)
        {
            if(key.ToCommonKey() != _Hub.InterruptKey1) {
                _SimBridgeA320?.SendKeyToSimulator(Translate(key), pressed: true);
            } else {
                _SimBridgeA320?.AdvanceSelectedBufferProductId();
            }
        }

        private void SimBridgeA320_DisplayRefreshRequired(object sender, System.EventArgs e) => RefreshDisplay();

        private void SimBridgeA320_LedsRefreshRequired(object sender, System.EventArgs e) => RefreshLeds();

        private void SimBridgeA320_ConnectionStateChanged(object sender, System.EventArgs e)
        {
            ShowConnectionState(_SimBridgeA320?.ConnectionState);
        }
    }
}
