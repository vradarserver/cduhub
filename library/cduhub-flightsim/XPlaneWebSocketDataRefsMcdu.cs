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
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cduhub.FlightSim.XPlaneWebSocketModels;
using McduDotNet;
using McduDotNet.FlightSim.SimBridgeMcdu;
using Newtonsoft.Json;

namespace Cduhub.FlightSim
{
    /// <summary>
    /// Maintains a pilot and first-officer pair of MCDU buffers for MCDU data sent from X-Plane 12+ over its web
    /// socket interface using the predefined standard MCDU data refs and commands.
    /// </summary>
    public class XPlaneWebSocketDataRefsMcdu : SimulatedMcdusOverWebSocket
    {
        class KeyCommand
        {
            public string Command;
            public bool Pressed;
        }

        private const int _McduLines = 14;      // <-- should be same as Metrics but just to ensure internal consistency...
        private Dictionary<string, DatarefInfoModel> _DatarefsByName = null;
        private Dictionary<long, DatarefInfoModel> _DatarefsById = null;
        private Dictionary<string, CommandInfoModel> _CommandsByName = null;
        private Dictionary<long, CommandInfoModel> _CommandsById = null;
        private readonly object _QueueLock = new object();
        private readonly Queue<KeyCommand> _SendCommandQueue = new Queue<KeyCommand>();

        private int _RequestId;

        /// <inheritdoc/>
        public override SimulatorMcduBuffer PilotBuffer { get; } = new SimulatorMcduBuffer();

        /// <inheritdoc/>
        public override SimulatorMcduBuffer FirstOfficerBuffer { get; } = new SimulatorMcduBuffer();

        /// <summary>
        /// Gets or sets the address of the machine running X-Plane 12+.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port that X-Plane 12+'s WebSocket server is listening to.
        /// </summary>
        public int Port { get; set; } = 8086;

        public HttpClient HttpClient { get; }

        /// <inheritdoc/>
        protected override Uri WebSocketUri => new Uri($"ws://{Host}:{Port}/api/v2");

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        public XPlaneWebSocketDataRefsMcdu(HttpClient httpClient, Screen masterScreen, Leds masterLeds) : base(masterScreen, masterLeds)
        {
            HttpClient = httpClient;
        }

        protected override void DisposeWebSocket()
        {
            base.DisposeWebSocket();

            lock(_QueueLock) {
                _SendCommandQueue.Clear();
            }
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

        protected override async Task InitialiseNewConnection(ClientWebSocket client, CancellationToken cancellationToken)
        {
            await FetchKnownDatarefs();
            await FetchKnownCommands();

            var datarefsByName = _DatarefsByName;

            var datarefs = new DatarefSubscribeValuesModel() {
                RequestId = Interlocked.Increment(ref _RequestId),
                Type =      "dataref_subscribe_values",
            };

            bool add(string name)
            {
                DatarefInfoModel datarefInfo = null;
                datarefsByName?.TryGetValue(name, out datarefInfo);
                if(datarefInfo != null) {
                    datarefs.Params.Datarefs.Add(new DatarefSubscribeModel() {
                        Id = datarefInfo.Id,
                        Name = datarefInfo.Name,
                    });
                }
                return datarefInfo != null;
            }

            var success = true;
            for(var idx = 0;idx < _McduLines;++idx) {
                success = success && add($"sim/cockpit2/radios/indicators/fms_cdu1_text_line{idx}");
                success = success && add($"sim/cockpit2/radios/indicators/fms_cdu2_text_line{idx}");
                success = success && add($"sim/cockpit2/radios/indicators/fms_cdu1_style_line{idx}");
                success = success && add($"sim/cockpit2/radios/indicators/fms_cdu2_style_line{idx}");
            }
            if(success) {
                var json = JsonConvert.SerializeObject(datarefs);
                await SendUTF8Message(client, json, cancellationToken);
            }
        }

        private async Task FetchKnownDatarefs()
        {
            var datarefsByName = new Dictionary<string, DatarefInfoModel>();
            var datarefsById = new Dictionary<long, DatarefInfoModel>();

            var uri = new Uri($"http://{Host}:{Port}/api/v2/datarefs");
            var json = await HttpClient.GetStringAsync(uri);
            var deserialised = JsonConvert.DeserializeObject<KnownDatarefsModel>(json);
            foreach(var dataref in deserialised.Data) {
                if(!datarefsByName.ContainsKey(dataref.Name)) {
                    datarefsByName.Add(dataref.Name, dataref);
                }
                if(!datarefsById.ContainsKey(dataref.Id)) {
                    datarefsById.Add(dataref.Id, dataref);
                }
            }
            _DatarefsByName = datarefsByName;
            _DatarefsById = datarefsById;
        }

        private async Task FetchKnownCommands()
        {
            var commandsByName = new Dictionary<string, CommandInfoModel>();
            var commandsById = new Dictionary<long, CommandInfoModel>();

            var uri = new Uri($"http://{Host}:{Port}/api/v2/Commands");
            var json = await HttpClient.GetStringAsync(uri);
            var deserialised = JsonConvert.DeserializeObject<KnownCommandsModel>(json);
            foreach(var command in deserialised.Data) {
                if(!commandsByName.ContainsKey(command.Name)) {
                    commandsByName.Add(command.Name, command);
                }
                if(!commandsById.ContainsKey(command.Id)) {
                    commandsById.Add(command.Id, command);
                }
            }
            _CommandsByName = commandsByName;
            _CommandsById = commandsById;
        }

        protected override async Task SendLoop(ClientWebSocket client, CancellationToken cancellationToken)
        {
            lock(_QueueLock) {
                _SendCommandQueue.Clear();
            }

            while(client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested) {
                KeyCommand command = null;
                lock(_QueueLock) {
                    if(_SendCommandQueue.Count > 0) {
                        command = _SendCommandQueue.Dequeue();
                    }
                }
                if(command == null) {
                    Thread.Sleep(1);
                } else {
                    var knownCommands = _CommandsByName;
                    if(knownCommands?.TryGetValue(command.Command, out var commandInfo) ?? false) {
                        var request = new CommandsRequestModel() {
                            RequestId = Interlocked.Increment(ref _RequestId),
                            Type = "command_set_is_active",
                        };
                        request.Params.Commands.Add(new CommandActiveModel() {
                            Id = commandInfo.Id,
                            IsActive = command.Pressed,
                        });
                        var json = JsonConvert.SerializeObject(request);
                        await SendUTF8Message(client, json, cancellationToken);
                    }
                }
            }
        }

        protected override async Task ReceiveLoop(ClientWebSocket client, CancellationToken cancellationToken)
        {
            var buffer = new byte[64 * 1024];
            while(client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested) {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if(result.MessageType == WebSocketMessageType.Text) {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if(!cancellationToken.IsCancellationRequested) {
                        ProcessMessage(msg);
                    }
                }
            }
        }

        private void ProcessMessage(string message)
        {
            if(!String.IsNullOrEmpty(message)) {
                try {
                    var updateMessage = JsonConvert.DeserializeObject<UpdateMessageModel>(message);
                    if(updateMessage?.Type == "dataref_update_values") {
                        ProcessUpdateMessage(updateMessage);
                    }
                } catch(JsonSerializationException) {
                    ;
                }
            }
        }

        private void ProcessUpdateMessage(UpdateMessageModel updateMessage)
        {
            var datarefsById = _DatarefsById;
            if(updateMessage.Data != null && datarefsById != null) {
                foreach(var kvp in updateMessage.Data) {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    if(long.TryParse(key, out var id) && datarefsById.TryGetValue(id, out var dataref)) {
                        ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu1_text_line", PilotBuffer.Screen, isDisplay: true);
                        ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu2_text_line", FirstOfficerBuffer.Screen, isDisplay: true);
                        ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu1_style_line", PilotBuffer.Screen, isDisplay: false);
                        ProcessScreenUpdate(dataref, value, "sim/cockpit2/radios/indicators/fms_cdu2_style_line", FirstOfficerBuffer.Screen, isDisplay: false);
                    }
                }
                RefreshSelectedScreen();
            }
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
                        XPlaneWebSocket.ParseWebSocketDisplayLineIntoRow(
                            screen,
                            value as string,
                            rowNumber
                        );
                    } else {
                        XPlaneWebSocket.ParseWebSocketStyleLineIntoRow(
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
