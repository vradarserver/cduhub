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

namespace Cduhub.Pages
{
    class SimBridge_Page : Page
    {
        private SimBridgeA320RemoteMcdu _SimBridgeA320;

        public override bool DisableMenuKey => true;

        public SimBridge_Page(Hub hub) : base(hub)
        {
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                ShowConnecting();
                Connect();
            } else {
                Disconnect();
            }
        }

        private void Connect()
        {
            Disconnect();

            var settings = Storage.Load<SimBridgeEfbSettings>(SimBridgeEfbSettings.Name);
            var mcdu = new SimBridgeA320RemoteMcdu(Screen, Leds) {
                Host = settings.Host,
                Port = settings.Port,
            };
            _SimBridgeA320 = mcdu;
            mcdu.DisplayRefreshRequired += SimBridgeA320_DisplayRefreshRequired;
            mcdu.LedsRefreshRequired += SimBridgeA320_LedsRefreshRequired;
            mcdu.IsConnectedChanged += SimBridgeA320_IsConnectedChanged;

            ConnectedFlightSimulators.AddFlightSimulatorMcdu(mcdu);
            mcdu.ReconnectToSimulator();
        }

        private void Disconnect()
        {
            if(_SimBridgeA320 != null) {
                var reference = _SimBridgeA320;
                _SimBridgeA320 = null;

                try {
                    ConnectedFlightSimulators.RemoveFlightSimulatorMcdu(reference);
                    reference.DisplayRefreshRequired -= SimBridgeA320_DisplayRefreshRequired;
                    reference.LedsRefreshRequired -= SimBridgeA320_LedsRefreshRequired;
                    reference.IsConnectedChanged -= SimBridgeA320_IsConnectedChanged;
                    reference.Dispose();
                } catch {
                    ;
                }
            }
        }

        public override void OnKeyDown(Key key)
        {
            if(key != Key.Blank1) {
                _SimBridgeA320?.SendKeyToSimulator(key, pressed: true);
            } else {
                _SimBridgeA320?.AdvanceSelectedBufferProductId();
            }
        }

        private void ShowConnecting() => FullPageStatusMessage("<grey>CONNECTING", "<grey><small>(BLANK2 TO QUIT)");

        private void SimBridgeA320_DisplayRefreshRequired(object sender, System.EventArgs e) => RefreshDisplay();

        private void SimBridgeA320_LedsRefreshRequired(object sender, System.EventArgs e) => RefreshLeds();

        private void SimBridgeA320_IsConnectedChanged(object sender, System.EventArgs e)
        {
            if(!(_SimBridgeA320?.IsConnected ?? false)) {
                ShowConnecting();
            }
        }
    }
}
