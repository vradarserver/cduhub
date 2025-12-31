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
using System.Threading.Tasks;
using WwDevicesDotNet;
using WwDevicesDotNet.FlightSim.SimBridgeMcdu;

namespace Cduhub.FlightSim
{
    public class XPlaneGenericMcdu : XPlaneRestMcdus
    {
        const int _ScratchpadDownloadIntervalMilliseconds = 100;
        const int _VisibleScreenDownloadIntervalMilliseconds = 750;
        const int _OtherScreenDownloadIntervalMilliseconds = 30000;
        private DateTime _LastScratchpadLineDownloadUtc;
        private DateTime _LastVisibleScreenDownloadUtc;
        private DateTime _LastOtherScreenDownloadUtc;

        /// <inheritdoc/>
        public override string AircraftName => "Generic";

        /// <inheritdoc/>
        public override DeviceType TargetDeviceType => DeviceType.NotSpecified;

        public XPlaneGenericMcdu(HttpClient httpClient, DeviceUser deviceUser, Screen masterScreen, Leds masterLeds) : base(httpClient, deviceUser, masterScreen, masterLeds)
        {
        }

        /// <inheritdoc/>
        public override void SendKeyToSimulator(Key key, bool pressed)
        {
            if(pressed) {
                var keyCode = key.ToXPlaneCommand();
                if(keyCode != "" && IsConnected) {
                    var fms = SelectedBufferDeviceUser == DeviceUser.Captain
                        ? "FMS"
                        : "FMS2";
                    var command = $"sim/{fms}/{keyCode}";
                    Task.Run(() => ActivateCommandOrReconnect(command));
                }
            }
        }

        protected override void DownloadMcduContent()
        {
            var now = DateTime.UtcNow;
            if(_LastScratchpadLineDownloadUtc.AddMilliseconds(_ScratchpadDownloadIntervalMilliseconds) <= now) {
                DownloadVisibleScratchpad();
            }
            if(_LastVisibleScreenDownloadUtc.AddMilliseconds(_VisibleScreenDownloadIntervalMilliseconds) <= now) {
                _LastVisibleScreenDownloadUtc = DownloadScreen(SelectedBuffer.Screen);
                RefreshSelectedScreen();
            }
            if(_LastOtherScreenDownloadUtc.AddMilliseconds(_OtherScreenDownloadIntervalMilliseconds) < now) {
                _LastOtherScreenDownloadUtc = DownloadScreen(SelectedBuffer == PilotBuffer
                    ? FirstOfficerBuffer.Screen
                    : PilotBuffer.Screen
                );
            }
        }

        private void DownloadVisibleScratchpad()
        {
            DownloadLineForScreen(SelectedBuffer.Screen, 13);
            _LastScratchpadLineDownloadUtc = DateTime.UtcNow;
            RefreshSelectedScreen();
        }

        private DateTime DownloadScreen(Screen screen)
        {
            for(var lineIdx = 0;lineIdx < Metrics.Lines;++lineIdx) {
                DownloadLineForScreen(screen, lineIdx);
            }
            return DateTime.UtcNow;
        }

        private void DownloadLineForScreen(Screen screen, int lineNumber)
        {
            var cduNumber = ScreenToCduNumber(screen);

            var displayLine = GetDataRef($"sim/cockpit2/radios/indicators/fms_cdu{cduNumber}_text_line{lineNumber}");
            var styleLine = GetDataRef($"sim/cockpit2/radios/indicators/fms_cdu{cduNumber}_style_line{lineNumber}");
            XPlaneGenericDataRef.ParseMime64DisplayLineIntoRow(screen, displayLine, lineNumber);
            XPlaneGenericDataRef.ParseMime64StyleLineIntoRow(screen, styleLine, lineNumber);
        }

        private int ScreenToCduNumber(Screen screen) => screen == PilotBuffer.Screen ? 1 : 2;
    }
}
