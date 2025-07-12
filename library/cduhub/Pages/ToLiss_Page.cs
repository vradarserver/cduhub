// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.FlightSim;
using McduDotNet;

namespace Cduhub.Pages
{
    class ToLiss_Page : Page
    {
        private readonly ToLissUdpMcdu _ToLissMcdu;

        public override bool DisableMenuKey => true;

        public ToLiss_Page(Hub hub) : base(hub)
        {
            _ToLissMcdu = new ToLissUdpMcdu(Screen, Leds);
            _ToLissMcdu.DisplayRefreshRequired += ToLissMcdu_DisplayRefreshRequired;
            _ToLissMcdu.LedsRefreshRequired += ToLissMcdu_LedsRefreshRequired;

            ConnectedFlightSimulators.AddFlightSimulatorMcdu(_ToLissMcdu);
            _ToLissMcdu.ReconnectToSimulator();
        }

        public override void OnKeyDown(Key key)
        {
            if(key != Key.Blank1) {
                _ToLissMcdu.SendKeyToSimulator(key, pressed: true);
            } else {
                _ToLissMcdu.AdvanceSelectedBufferProductId();
            }
        }

        public override void OnKeyUp(Key key)
        {
            if(key != Key.Blank1) {
                _ToLissMcdu.SendKeyToSimulator(key, pressed: false);
            }
        }

        public void Reconnect() => _ToLissMcdu.ReconnectToSimulator();

        private void ToLissMcdu_DisplayRefreshRequired(object sender, System.EventArgs e) => RefreshDisplay();

        private void ToLissMcdu_LedsRefreshRequired(object sender, System.EventArgs e) => RefreshLeds();
    }
}
