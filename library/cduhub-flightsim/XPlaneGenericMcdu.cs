// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Cduhub.FlightSim.XPlaneRestModels;
using McduDotNet;
using McduDotNet.FlightSim.SimBridgeMcdu;

namespace Cduhub.FlightSim
{
    public class XPlaneGenericMcdu : XPlaneWebSocketDataRefsMcdu
    {
        public XPlaneGenericMcdu(HttpClient httpClient, Screen masterScreen, Leds masterLeds) : base(httpClient, masterScreen, masterLeds)
        {
        }

        /// <inheritdoc/>
        public override void SendKeyToSimulator(Key key, bool pressed)
        {
            var keyCode = key.ToXPlaneCommand();
            if(keyCode != "" && IsConnected) {
                var fms = SelectedBufferProductId == ProductId.Captain
                    ? "FMS"
                    : "FMS2";
                var command = $"sim/{fms}/{keyCode}";
                lock(_QueueLock) {
                    _SendCommandQueue.Enqueue(new KeyCommand() { Command = command, Pressed = pressed });
                }
            }
        }

        protected override IEnumerable<string> SubscribeToDatarefs()
        {
            var result = new List<string>();
            for(var idx = 0;idx < _McduLines;++idx) {
                result.Add($"sim/cockpit2/radios/indicators/fms_cdu1_text_line{idx}");
                result.Add($"sim/cockpit2/radios/indicators/fms_cdu2_text_line{idx}");
                result.Add($"sim/cockpit2/radios/indicators/fms_cdu1_style_line{idx}");
                result.Add($"sim/cockpit2/radios/indicators/fms_cdu2_style_line{idx}");
            }

            return result;
        }

        protected override void ProcessDatarefUpdateValue(DatarefInfoModel dataref, dynamic value)
        {
            ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu1_text_line", PilotBuffer.Screen, isDisplay: true);
            ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu2_text_line", FirstOfficerBuffer.Screen, isDisplay: true);
            ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu1_style_line", PilotBuffer.Screen, isDisplay: false);
            ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu2_style_line", FirstOfficerBuffer.Screen, isDisplay: false);
        }

        private void ProcessScreenUpdate(
            DatarefInfoModel dataref,
            dynamic value,
            string prefix,
            Screen screen,
            bool isDisplay
        )
        {
            if(dataref.Name.StartsWith(prefix)) {
                var rowText = dataref.Name.Substring(prefix.Length);
                if(int.TryParse(rowText, NumberStyles.None, CultureInfo.InvariantCulture, out var rowNumber)) {
                    if(isDisplay) {
                        XPlaneGenericDataRef.ParseMime64DisplayLineIntoRow(
                            screen,
                            value as string,
                            rowNumber
                        );
                    } else {
                        XPlaneGenericDataRef.ParseMime64StyleLineIntoRow(
                            screen,
                            value as string,
                            rowNumber
                        );
                    }
                }
            }
        }
    }
}
