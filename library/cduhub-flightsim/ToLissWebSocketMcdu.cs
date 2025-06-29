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
using System.Net.Http;
using System.Text.RegularExpressions;
using Cduhub.FlightSim.XPlaneRestModels;
using Cduhub.FlightSim.XPlaneWebSocketModels;
using McduDotNet;
using McduDotNet.FlightSim;

namespace Cduhub.FlightSim
{
    public class ToLissWebSocketMcdu : XPlaneWebSocketDataRefsMcdu
    {
        private ToLissScreenBuffer _PilotTolissBuffer = new ToLissScreenBuffer();

        private ToLissScreenBuffer _FirstOfficerToLissBuffer = new ToLissScreenBuffer();

        /// <inheritdoc/>
        public override string FlightSimulatorName => FlightSimulatorNames.XPlane12;

        /// <inheritdoc/>
        public override string AircraftName => "ToLiss A32x";

        public ToLissWebSocketMcdu(HttpClient httpClient, Screen masterScreen, Leds masterLeds) : base(httpClient, masterScreen, masterLeds)
        {
        }

        public override void SendKeyToSimulator(Key key, bool pressed)
        {
            var mcduNumber = SelectedMcduNumber();
            var keyCode = key.ToToLissCommand(mcduNumber);
            if(keyCode != "" && IsConnected) {
                var command = $"AirbusFBW/{keyCode}";
                lock(_QueueLock) {
                    _SendCommandQueue.Enqueue(new KeyCommand() { Command = command, Pressed = pressed });
                }
            }
        }

        private int SelectedMcduNumber() => SelectedBuffer != PilotBuffer ? 2 : 1;

        protected override IEnumerable<string> SubscribeToDatarefs()
        {
            var result = new List<string>();

            for(var mcduNum = 1;mcduNum < 3;++mcduNum) {
                foreach(var style in "wgybs") {
                    result.Add($"AirbusFBW/MCDU{mcduNum}title{style}");
                }
                foreach(var style in "wgyb") {
                    result.Add($"AirbusFBW/MCDU{mcduNum}stitle{style}");
                }
                foreach(var style in "wa") {
                    result.Add($"AirbusFBW/MCDU{mcduNum}sp{style}");
                }
                for(var labelNum = 1;labelNum < 7;++labelNum) {
                    foreach(var style in new string[] { "w", "g", "y", "b", "a", "m", "s", "Lg", "Lw", }) {
                        result.Add($"AirbusFBW/MCDU{mcduNum}label{labelNum}{style}");
                    }
                }
                for(var contNum = 1;contNum < 7;++contNum) {
                    foreach(var style in "wgybams") {
                        result.Add($"AirbusFBW/MCDU{mcduNum}cont{contNum}{style}");
                    }
                }
                for(var scontNum = 1;scontNum < 7;++scontNum) {
                    foreach(var style in "wgybams") {
                        result.Add($"AirbusFBW/MCDU{mcduNum}scont{scontNum}{style}");
                    }
                }
                result.Add($"AirbusFBW/MCDU{mcduNum}VertSlewKeys");
            }

            return result;
        }

        static readonly Regex _McduNumberRegex = new Regex(@"^AirbusFBW/MCDU(?<mcduNum>1|2)", RegexOptions.Compiled);
        static readonly Regex _TitleRegex = new Regex(@"MCDU[1|2]title(?<style>w|g|y|b|s)$", RegexOptions.Compiled);
        static readonly Regex _STitleRegex = new Regex(@"MCDU[1|2]stitle(?<style>w|g|y|b)$", RegexOptions.Compiled);
        static readonly Regex _SPRegex = new Regex(@"MCDU[1|2]sp(?<style>w|a)$", RegexOptions.Compiled);
        static readonly Regex _LabelRegex = new Regex(@"MCDU[1|2]label(?<labelNum>1|2|3|4|5|6)(?<style>w|g|y|b|a|m|s|Lg|Lw)$", RegexOptions.Compiled);
        static readonly Regex _ContRegex = new Regex(@"MCDU[1|2]cont(?<contNum>1|2|3|4|5|6)(?<style>w|g|y|b|a|m|s)$", RegexOptions.Compiled);
        static readonly Regex _SContRegex = new Regex(@"MCDU[1|2]scont(?<scontNum>1|2|3|4|5|6)(?<style>w|g|y|b|a|m|s)$", RegexOptions.Compiled);
        static readonly Regex _VertSlewKeysRegex = new Regex(@"MCDU[1|2]VertSlewKeys$", RegexOptions.Compiled);

        protected override void ProcessDatarefUpdateValue(DatarefInfoModel dataref, dynamic value)
        {
            var mcduNumMatch = _McduNumberRegex.Match(dataref.Name);
            if(mcduNumMatch.Success) {
                var mcduNum = int.Parse(mcduNumMatch.Groups["mcduNum"].Value);
                var mcduBuffer = mcduNum == 1 ? PilotBuffer : FirstOfficerBuffer;
                var processed = false;
                processed |= ProcessTitleUpdate(dataref, mcduBuffer, value);
                processed |= ProcessSTitleUpdate(dataref, mcduBuffer, value);
                processed |= ProcessSPUpdate(dataref, mcduBuffer, value);
                processed |= ProcessLabelUpdate(dataref, mcduBuffer, value);
                processed |= ProcessContUpdate(dataref, mcduBuffer, value);
                processed |= ProcessSContUpdate(dataref, mcduBuffer, value);
                if(!processed) {
                    ProcessVertSlewKeysUpdate(dataref, mcduBuffer, value);
                }
            }
        }

        protected override void FinishedProcessingDatarefUpdate()
        {
            _PilotTolissBuffer.CopyToScreen(PilotBuffer.Screen);
            _FirstOfficerToLissBuffer.CopyToScreen(FirstOfficerBuffer.Screen);
            base.FinishedProcessingDatarefUpdate();
        }

        private bool ProcessTitleUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _TitleRegex.Match(dataref.Name);
            if(match.Success) {
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetTitle(style, value as string);
            }
            return match.Success;
        }

        private bool ProcessSTitleUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _STitleRegex.Match(dataref.Name);
            if(match.Success) {
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetSTitle(style, value as string);
            }
            return match.Success;
        }

        private bool ProcessSPUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _SPRegex.Match(dataref.Name);
            if(match.Success) {
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetScratchPad(style, value as string);
            }
            return match.Success;
        }

        private bool ProcessLabelUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _LabelRegex.Match(dataref.Name);
            if(match.Success) {
                var labelNum = int.Parse(match.Groups["labelNum"].Value);
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetLabel(labelNum, style, value as string);
            }
            return match.Success;
        }

        private bool ProcessContUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _ContRegex.Match(dataref.Name);
            if(match.Success) {
                var contNum = int.Parse(match.Groups["contNum"].Value);
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetCont(contNum, style, value as string);
            }
            return match.Success;
        }

        private bool ProcessSContUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _SContRegex.Match(dataref.Name);
            if(match.Success) {
                var scontNum = int.Parse(match.Groups["scontNum"].Value);
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetSCont(scontNum, style, value as string);
            }
            return match.Success;
        }

        private bool ProcessVertSlewKeysUpdate(DatarefInfoModel dataref, SimulatorMcduBuffer mcduBuffer, dynamic value)
        {
            var match = _VertSlewKeysRegex.Match(dataref.Name);
            if(match.Success) {
                if(value is long val) {
                    var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                    tolissBuffer.SetVertSlewKeys(val);
                }
            }
            return match.Success;
        }

        private ToLissScreenBuffer GetToLissScreenBuffer(SimulatorMcduBuffer mcduBuffer)
        {
            return mcduBuffer == FirstOfficerBuffer
                ? _FirstOfficerToLissBuffer
                : _PilotTolissBuffer;
        }
    }
}
