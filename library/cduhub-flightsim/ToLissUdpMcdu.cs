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
using System.Threading;
using System.Threading.Tasks;
using Cduhub.FlightSim.XPlaneUdpModels;
using wwDevicesDotNet;
using wwDevicesDotNet.FlightSim;

namespace Cduhub.FlightSim
{
    public class ToLissUdpMcdu : SimulatedMcdus, IDisposable
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

        /// <inheritdoc/>
        public override DeviceType TargetDeviceType => DeviceType.AirbusA320Mcdu;

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

        public ToLissUdpMcdu(DeviceUser deviceUser, Screen masterScreen, Leds masterLeds) : base(deviceUser, masterScreen, masterLeds)
        {
            _XPlaneUdp.ConnectionStateChanged += (sender,args) => RecordConnectionState(_XPlaneUdp?.ConnectionState ?? ConnectionState.Disconnected);
            _XPlaneUdp.PacketReceived += (sender,args) => RecordMessageReceivedFromSimulator();
            _XPlaneUdp.DataRefRefreshIntervalTimesPerSecond = 5;
            _XPlaneUdp.DataRefUpdatesReceived = DataRefUpdatesReceived;
            _XPlaneUdp.FrameReceived = FrameReceived;

            SubscribeToDatarefs();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ToLissUdpMcdu() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Disconnect();
            }
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

        enum RowType : byte
        {
            Title,
            STitle,
            Scratchpad,
            Label,
            Cont,
            SCont,
            VertSlew
        }

        class SubscriptionTag
        {
            public RowType RowType;
            public byte McduNum;
            public string Style;
            public byte RowNumber;
            public byte CellNumber;

            private SubscriptionTag()
            {
            }

            public SubscriptionTag(RowType rowType, byte mcduNum, string style)
            {
                RowType = rowType;
                McduNum = mcduNum;
                Style = style;
            }

            public SubscriptionTag(RowType rowType, byte mcduNum, string style, byte rowNumber) : this(rowType, mcduNum, style)
            {
                RowNumber = rowNumber;
            }

            public SubscriptionTag ForCell(byte cellNumber)
            {
                return new SubscriptionTag() {
                    RowType =       RowType,
                    McduNum =       McduNum,
                    Style =         Style,
                    RowNumber =     RowNumber,
                    CellNumber =    cellNumber,
                };
            }
        }

        private void SubscribeToDatarefs()
        {
            void subscribeToEachCellInTheRow(string dataRef, SubscriptionTag tag)
            {
                for(byte idx = 0;idx < Metrics.Columns;++idx) {
                    var fullTag = tag.ForCell(idx);
                    _XPlaneUdp.AddSubscription($"{dataRef}[{idx}]", fullTag, includeInFrameEvent: true);
                }
            }

            for(byte mcduNum = 1;mcduNum < 3;++mcduNum) {
                foreach(var style in new string[] { "w", "g", "y", "b", "s", }) {
                    var tag = new SubscriptionTag(RowType.Title, mcduNum, style);
                    subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}title{style}", tag);
                }
                foreach(var style in new string[] { "w", "g", "y", "b" }) {
                    var tag = new SubscriptionTag(RowType.STitle, mcduNum, style);
                    subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}stitle{style}", tag);
                }
                foreach(var style in new string[] { "w", "a", }) {
                    var tag = new SubscriptionTag(RowType.Scratchpad, mcduNum, style);
                    subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}sp{style}", tag);
                }
                for(byte labelNum = 1;labelNum < 7;++labelNum) {
                    foreach(var style in new string[] { "w", "g", "y", "b", "a", "m", "s", "Lg", "Lw", }) {
                        var tag = new SubscriptionTag(RowType.Label, mcduNum, style, labelNum);
                        subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}label{labelNum}{style}", tag);
                    }
                }
                for(byte contNum = 1;contNum < 7;++contNum) {
                    foreach(var style in new string[] { "w", "g", "y", "b", "a", "m", "s", }) {
                        var tag = new SubscriptionTag(RowType.Cont, mcduNum, style, contNum);
                        subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}cont{contNum}{style}", tag);
                    }
                }
                for(byte scontNum = 1;scontNum < 7;++scontNum) {
                    foreach(var style in new string[] { "w", "g", "y", "b", "a", "m", "s", }) {
                        var tag = new SubscriptionTag(RowType.SCont, mcduNum, style, scontNum);
                        subscribeToEachCellInTheRow($"AirbusFBW/MCDU{mcduNum}scont{scontNum}{style}", tag);
                    }
                }
                _XPlaneUdp.AddSubscription(
                    $"AirbusFBW/MCDU{mcduNum}VertSlewKeys",
                    new SubscriptionTag(RowType.VertSlew, mcduNum, null),
                    includeInFrameEvent: true
                );
            }
        }

        private void DataRefUpdatesReceived(XPlaneDataRefValue[] dataRefValues)
        {
            foreach(var dataRefValue in dataRefValues) {
                var tag = (SubscriptionTag)dataRefValue.Subscription.Tag;
                var value = dataRefValue.Value;
                var tolissBuffer = tag.McduNum == 1
                    ? _PilotTolissBuffer
                    : _FirstOfficerToLissBuffer;

                switch(tag.RowType) {
                    case RowType.Title:
                        tolissBuffer.SetTitleCell(tag.Style, tag.CellNumber, (char)value);
                        break;
                    case RowType.STitle:
                        tolissBuffer.SetSTitleCell(tag.Style, tag.CellNumber, (char)value);
                        break;
                    case RowType.Scratchpad:
                        tolissBuffer.SetScratchPadCell(tag.Style, tag.CellNumber, (char)value);
                        break;
                    case RowType.Label:
                        tolissBuffer.SetLabelCell(tag.RowNumber, tag.Style, tag.CellNumber, (char)value);
                        break;
                    case RowType.Cont:
                        tolissBuffer.SetContCell(tag.RowNumber, tag.Style, tag.CellNumber, (char)value);
                        break;
                    case RowType.SCont:
                        tolissBuffer.SetSContCell(tag.RowNumber, tag.Style, tag.CellNumber, (char)value);
                        break;
                    case RowType.VertSlew:
                        tolissBuffer.SetVertSlewKeys((long)value);
                        break;
                }
            }
        }

        private void FrameReceived()
        {
            _PilotTolissBuffer.CopyToScreen(PilotBuffer.Screen);
            _FirstOfficerToLissBuffer.CopyToScreen(FirstOfficerBuffer.Screen);
            RefreshSelectedScreen();
        }
    }
}
