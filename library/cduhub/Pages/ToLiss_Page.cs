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
    class ToLiss_Page : Page
    {
        private ToLissUdpMcdu _ToLissMcdu;

        public override bool DisableMenuKey => true;

        public override FontReference PageFont => LoadFromSettings<ToLissUdpSettings>(r => r.Font);

        public ToLiss_Page(Hub hub) : base(hub)
        {
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                Connect();
            } else {
                Disconnect();
            }
        }

        private void Connect()
        {
            Disconnect();

            var settings = ConfigStorage.Load<ToLissUdpSettings>();
            var mcdu = new ToLissUdpMcdu(Screen, Leds) {
                Host = settings.Host,
                Port = settings.Port,
            };
            _ToLissMcdu = mcdu;
            ShowConnectionState(_ToLissMcdu?.ConnectionState);
            mcdu.DisplayRefreshRequired += ToLissMcdu_DisplayRefreshRequired;
            mcdu.LedsRefreshRequired += ToLissMcdu_LedsRefreshRequired;
            mcdu.ConnectionStateChanged += ToLissMcdu_ConnectionStateChanged;

            ConnectedFlightSimulators.AddFlightSimulatorMcdu(mcdu);
            mcdu.ReconnectToSimulator();
        }

        private void Disconnect()
        {
            if(_ToLissMcdu != null) {
                var reference = _ToLissMcdu;
                _ToLissMcdu = null;

                try {
                    reference.DisplayRefreshRequired -= ToLissMcdu_DisplayRefreshRequired;
                    reference.LedsRefreshRequired -= ToLissMcdu_LedsRefreshRequired;
                    reference.Dispose();
                    reference.ConnectionStateChanged -= ToLissMcdu_ConnectionStateChanged;
                    ConnectedFlightSimulators.RemoveFlightSimulatorMcdu(reference);
                } catch {
                    ;
                }
            }
        }

        public override void OnKeyDown(Key key)
        {
            if(key != Key.Blank1) {
                _ToLissMcdu?.SendKeyToSimulator(key, pressed: true);
            } else {
                _ToLissMcdu?.AdvanceSelectedBufferProductId();
            }
        }

        public override void OnKeyUp(Key key)
        {
            if(key != Key.Blank1) {
                _ToLissMcdu?.SendKeyToSimulator(key, pressed: false);
            }
        }

        private void ToLissMcdu_DisplayRefreshRequired(object sender, System.EventArgs e) => RefreshDisplay();

        private void ToLissMcdu_LedsRefreshRequired(object sender, System.EventArgs e) => RefreshLeds();

        private void ToLissMcdu_ConnectionStateChanged(object sender, System.EventArgs e)
        {
            ShowConnectionState(_ToLissMcdu?.ConnectionState);
        }
    }
}
