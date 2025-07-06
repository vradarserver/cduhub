// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cduhub.FlightSim.XPlaneUdpModels;
using McduDotNet;
using McduDotNet.FlightSim;

namespace Cduhub.FlightSim
{
    public class ToLissUdpMcdu : SimulatedMcdus
    {
        private XPlaneUdp _XPlaneUdp = new XPlaneUdp();
        private ToLissScreenBuffer _PilotTolissBuffer = new ToLissScreenBuffer();
        private ToLissScreenBuffer _FirstOfficerToLissBuffer = new ToLissScreenBuffer();
        private CancellationTokenSource _XPlaneUdpCancellationTokenSource;
        private Task _XPlaneUdpTask;

        /// <inheritdoc/>
        public override string FlightSimulatorName => FlightSimulatorNames.XPlane;

        /// <inheritdoc/>
        public override string AircraftName => "ToLiss";

        /// <summary>
        /// Gets or sets the address of the machine running X-Plane.
        /// </summary>
        public string Host
        {
            get => _XPlaneUdp.Host;
            set => _XPlaneUdp.Host = value;
        }

        /// <summary>
        /// Gets or sets the port that X-Plane is listening to for UDP connections.
        /// </summary>
        public int Port
        {
            get => _XPlaneUdp.Port;
            set => _XPlaneUdp.Port = value;
        }

        public override SimulatorMcduBuffer PilotBuffer { get; } = new SimulatorMcduBuffer();

        public override SimulatorMcduBuffer FirstOfficerBuffer { get; } = new SimulatorMcduBuffer();

        public ToLissUdpMcdu(Screen masterScreen, Leds masterLeds) : base(masterScreen, masterLeds)
        {
            _XPlaneUdp.IsConnectedChanged += (sender,args) => RecordConnection(_XPlaneUdp.IsConnected);
            _XPlaneUdp.PacketReceived += (sender,args) => RecordMessageReceivedFromSimulator();
            _XPlaneUdp.DataRefRefreshIntervalTimesPerSecond = 5;
            _XPlaneUdp.DataRefUpdatesReceived = DataRefUpdatesReceived;

            SubscribeToDatarefs();
        }

        public override void ReconnectToSimulator()
        {
            Disconnect();
            _XPlaneUdpCancellationTokenSource = new CancellationTokenSource();
            _XPlaneUdpTask = Task.Run(() => _XPlaneUdp.ConnectAsync(_XPlaneUdpCancellationTokenSource.Token));
        }

        private void Disconnect()
        {
            var cts = _XPlaneUdpCancellationTokenSource;
            var task = _XPlaneUdpTask;

            _XPlaneUdpCancellationTokenSource = null;
            _XPlaneUdpTask = null;

            if(cts != null) {
                try {
                    cts.Cancel();
                    Task.WaitAll(new Task[] { task }, 5000);
                } catch {
                }
                try {
                    cts.Dispose();
                } catch {
                }
            }
        }

        public override void SendKeyToSimulator(Key key, bool pressed)
        {
            if(pressed) {
                var mcduNumber = SelectedMcduNumber();
                var keyCode = key.ToToLissCommand(mcduNumber);
                if(keyCode != "" && IsConnected) {
                    var command = $"AirbusFBW/{keyCode}";
                    _XPlaneUdp.SendCommand(command);
                }
            }
        }

        private int SelectedMcduNumber() => SelectedBuffer != PilotBuffer ? 2 : 1;

        private void SubscribeToDatarefs()
        {
            void subscribeToEachCellInTheRow(string dataRef)
            {
                for(var idx = 0;idx < Metrics.Columns;++idx) {
                    _XPlaneUdp.AddSubscription($"{dataRef}[{idx}]");
                }
            }

            for(var mcduNum = 1;mcduNum < 3;++mcduNum) {
                foreach(var style in "wgybs") {
                    subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}title{style}");
                }
                foreach(var style in "wgyb") {
                    subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}stitle{style}");
                }
                foreach(var style in "wa") {
                    subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}sp{style}");
                }
                for(var labelNum = 1;labelNum < 7;++labelNum) {
                    foreach(var style in new string[] { "w", "g", "y", "b", "a", "m", "s", "Lg", "Lw", }) {
                        subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}label{labelNum}{style}");
                    }
                }
                for(var contNum = 1;contNum < 7;++contNum) {
                    foreach(var style in "wgybams") {
                        subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}cont{contNum}{style}");
                    }
                }
                for(var scontNum = 1;scontNum < 7;++scontNum) {
                    foreach(var style in "wgybams") {
                        subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}scont{scontNum}{style}");
                    }
                }
                _XPlaneUdp.AddSubscription($"AirbusFBW/MCDU{mcduNum}VertSlewKeys");
            }
        }

        static readonly Regex _McduNumberRegex = new Regex(@"^AirbusFBW/MCDU(?<mcduNum>1|2)", RegexOptions.Compiled);
        static readonly Regex _TitleRegex = new Regex(@"MCDU[1|2]title(?<style>w|g|y|b|s)$", RegexOptions.Compiled);
        static readonly Regex _STitleRegex = new Regex(@"MCDU[1|2]stitle(?<style>w|g|y|b)$", RegexOptions.Compiled);
        static readonly Regex _SPRegex = new Regex(@"MCDU[1|2]sp(?<style>w|a)$", RegexOptions.Compiled);
        static readonly Regex _LabelRegex = new Regex(@"MCDU[1|2]label(?<labelNum>1|2|3|4|5|6)(?<style>w|g|y|b|a|m|s|Lg|Lw)$", RegexOptions.Compiled);
        static readonly Regex _ContRegex = new Regex(@"MCDU[1|2]cont(?<contNum>1|2|3|4|5|6)(?<style>w|g|y|b|a|m|s)$", RegexOptions.Compiled);
        static readonly Regex _SContRegex = new Regex(@"MCDU[1|2]scont(?<scontNum>1|2|3|4|5|6)(?<style>w|g|y|b|a|m|s)$", RegexOptions.Compiled);
        static readonly Regex _VertSlewKeysRegex = new Regex(@"MCDU[1|2]VertSlewKeys$", RegexOptions.Compiled);

        private void DataRefUpdatesReceived(XPlaneDataRefValue[] dataRefValues)
        {
            foreach(var dataRefValue in dataRefValues) {
                (var dataRefName, var dataRefIndex) = dataRefValue.ParseArrayDataRef();

                var mcduNumMatch = _McduNumberRegex.Match(dataRefName);
                if(mcduNumMatch.Success) {
                    var mcduNum = int.Parse(mcduNumMatch.Groups["mcduNum"].Value);
                    var mcduBuffer = mcduNum == 1 ? PilotBuffer : FirstOfficerBuffer;
                    var processed = false;
                    var character = (char)dataRefValue.Value;
                    processed |= ProcessTitleUpdate(dataRefName, mcduBuffer, dataRefIndex, character);
                    processed |= ProcessSTitleUpdate(dataRefName, mcduBuffer, dataRefIndex, character);
                    processed |= ProcessSPUpdate(dataRefName, mcduBuffer, dataRefIndex, character);
                    processed |= ProcessLabelUpdate(dataRefName, mcduBuffer, dataRefIndex, character);
                    processed |= ProcessContUpdate(dataRefName, mcduBuffer, dataRefIndex, character);
                    processed |= ProcessSContUpdate(dataRefName, mcduBuffer, dataRefIndex, character);
                    if(!processed) {
                        ProcessVertSlewKeysUpdate(dataRefName, mcduBuffer, (int)dataRefValue.Value);
                    }
                }
            }

            _PilotTolissBuffer.CopyToScreen(PilotBuffer.Screen);
            _FirstOfficerToLissBuffer.CopyToScreen(FirstOfficerBuffer.Screen);
            RefreshSelectedScreen();
        }

        private bool ProcessTitleUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int chIndex, char ch)
        {
            var match = _TitleRegex.Match(dataRefName);
            if(match.Success) {
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetTitleCell(style, chIndex, ch);
            }
            return match.Success;
        }

        private bool ProcessSTitleUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int chIndex, char ch)
        {
            var match = _STitleRegex.Match(dataRefName);
            if(match.Success) {
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetSTitleCell(style, chIndex, ch);
            }
            return match.Success;
        }

        private bool ProcessSPUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int chIndex, char ch)
        {
            var match = _SPRegex.Match(dataRefName);
            if(match.Success) {
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetScratchPadCell(style, chIndex, ch);
            }
            return match.Success;
        }

        private bool ProcessLabelUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int chIndex, char ch)
        {
            var match = _LabelRegex.Match(dataRefName);
            if(match.Success) {
                var labelNum = int.Parse(match.Groups["labelNum"].Value);
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetLabelCell(labelNum, style, chIndex, ch);
            }
            return match.Success;
        }

        private bool ProcessContUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int chIndex, char ch)
        {
            var match = _ContRegex.Match(dataRefName);
            if(match.Success) {
                var contNum = int.Parse(match.Groups["contNum"].Value);
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetContCell(contNum, style, chIndex, ch);
            }
            return match.Success;
        }

        private bool ProcessSContUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int chIndex, char ch)
        {
            var match = _SContRegex.Match(dataRefName);
            if(match.Success) {
                var scontNum = int.Parse(match.Groups["scontNum"].Value);
                var style = match.Groups["style"].Value;
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetSContCell(scontNum, style, chIndex, ch);
            }
            return match.Success;
        }

        private bool ProcessVertSlewKeysUpdate(string dataRefName, SimulatorMcduBuffer mcduBuffer, int value)
        {
            var match = _VertSlewKeysRegex.Match(dataRefName);
            if(match.Success) {
                var tolissBuffer = GetToLissScreenBuffer(mcduBuffer);
                tolissBuffer.SetVertSlewKeys(value);
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
