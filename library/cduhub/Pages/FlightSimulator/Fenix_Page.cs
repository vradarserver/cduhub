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
using wwDevicesDotNet;

namespace Cduhub.Pages.FlightSimulator
{
    class Fenix_Page : CommonFlightSimPage
    {
        private FenixA320EfbMcdu _FenixA320;

        public override DeviceType SimulatorDeviceType => DeviceType.AirbusA320Mcdu;

        public override FontReference PageFont => SettingsFont<FenixEfbSettings>(r => r.Font);

        public override Palette Palette => SettingsPalette<FenixEfbSettings>(r => r.PaletteName);

        public Fenix_Page(Hub hub) : base(hub)
        {
        }

        protected override void Connect()
        {
            Disconnect();

            var settings = ConfigStorage.Load<FenixEfbSettings>();
            var mcdu = new FenixA320EfbMcdu(_Hub.ConnectedDevice.DeviceUser, Screen, Leds) {
                Host = settings.Host,
                Port = settings.Port,
            };
            _FenixA320 = mcdu;
            ShowConnectionState(_FenixA320?.ConnectionState);
            mcdu.DisplayRefreshRequired += FenixA320_DisplayRefreshRequired;
            mcdu.LedsRefreshRequired += FenixA320_LedsRefreshRequired;
            mcdu.ConnectionStateChanged += FenixA320_ConnectionStateChanged;

            ConnectedFlightSimulators.AddFlightSimulatorMcdu(mcdu);
            mcdu.ReconnectToSimulator();
        }

        protected override void Disconnect()
        {
            if(_FenixA320 != null) {
                var reference = _FenixA320;
                _FenixA320 = null;

                try {
                    reference.DisplayRefreshRequired -= FenixA320_DisplayRefreshRequired;
                    reference.LedsRefreshRequired -= FenixA320_LedsRefreshRequired;
                    reference.Dispose();
                    reference.ConnectionStateChanged -= FenixA320_ConnectionStateChanged;
                    ConnectedFlightSimulators.RemoveFlightSimulatorMcdu(reference);
                } catch {
                    ;
                }
            }
        }

        public override void OnKeyDown(Key key)
        {
            if(key.ToCommonKey() != _Hub.InterruptKey1) {
                _FenixA320?.SendKeyToSimulator(Translate(key), pressed: true);
            } else {
                _FenixA320?.AdvanceSelectedBufferProductId();
            }
        }

        public override void OnKeyUp(Key key)
        {
            if(key.ToCommonKey() != _Hub.InterruptKey1) {
                _FenixA320?.SendKeyToSimulator(Translate(key), pressed: false);
            }
        }

        private void FenixA320_DisplayRefreshRequired(object sender, System.EventArgs e) => RefreshDisplay();

        private void FenixA320_LedsRefreshRequired(object sender, System.EventArgs e) => RefreshLeds();

        private void FenixA320_ConnectionStateChanged(object sender, System.EventArgs e)
        {
            ShowConnectionState(_FenixA320?.ConnectionState);
        }
    }
}
