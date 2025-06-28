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
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cduhub.FlightSim.XPlaneWebSocketModels;
using McduDotNet;
using Newtonsoft.Json;

namespace Cduhub.FlightSim
{
    /// <summary>
    /// Maintains a pilot and first-officer pair of MCDU buffers for MCDU data sent from X-Plane 12+ over its web
    /// socket interface using the predefined standard MCDU data refs and commands.
    /// </summary>
    public abstract class XPlaneWebSocketDataRefsMcdu : SimulatedMcdusOverWebSocket
    {
        protected class KeyCommand
        {
            public string Command { get; set; }

            public bool Pressed { get; set; }
        }

        protected const int _McduLines = 14;      // <-- should be same as Metrics but just to ensure internal consistency...
        protected Dictionary<string, DatarefInfoModel> _DatarefsByName = null;
        protected Dictionary<long, DatarefInfoModel> _DatarefsById = null;
        protected Dictionary<string, CommandInfoModel> _CommandsByName = null;
        protected Dictionary<long, CommandInfoModel> _CommandsById = null;
        protected readonly object _QueueLock = new object();
        protected readonly Queue<KeyCommand> _SendCommandQueue = new Queue<KeyCommand>();

        protected int _RequestId;

        /// <inheritdoc/>
        public override string FlightSimulatorName => FlightSimulatorNames.XPlane12;

        /// <inheritdoc/>
        public override string AircraftName => "Generic";

        /// <inheritdoc/>
        public override SimulatorMcduBuffer PilotBuffer { get; } = new SimulatorMcduBuffer();

        /// <inheritdoc/>
        public override SimulatorMcduBuffer FirstOfficerBuffer { get; } = new SimulatorMcduBuffer();

        /// <summary>
        /// Gets or sets the address of the machine running X-Plane.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port that X-Plane's WebSocket server is listening to.
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

        protected override async Task InitialiseNewConnection(ClientWebSocket client, CancellationToken cancellationToken)
        {
            await FetchKnownDatarefs();
            await FetchKnownCommands();

            var datarefsByName = _DatarefsByName;
            var subscribeToDatarefNames = SubscribeToDatarefs().ToArray();

            if(datarefsByName != null && subscribeToDatarefNames.Length > 0) {
                var datarefs = new DatarefSubscribeValuesModel() {
                    RequestId = Interlocked.Increment(ref _RequestId),
                    Type =      "dataref_subscribe_values",
                };

                var success = true;
                foreach(var name in subscribeToDatarefNames) {
                    datarefsByName.TryGetValue(name, out var datarefInfo);
                    if(datarefInfo == null) {
                        success = false;
                        break;
                    }
                    datarefs.Params.Datarefs.Add(new DatarefSubscribeModel() {
                        Id = datarefInfo.Id,
                        Name = datarefInfo.Name,
                    });
                }
                if(success) {
                    var json = JsonConvert.SerializeObject(datarefs);
                    await SendUTF8Message(client, json, cancellationToken);
                }
            }
        }

        protected abstract IEnumerable<string> SubscribeToDatarefs();

        protected virtual async Task FetchKnownDatarefs()
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

        protected virtual async Task FetchKnownCommands()
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
                RecordMessageReceivedFromSimulator();
            }
        }

        protected virtual void ProcessMessage(string message)
        {
            if(!String.IsNullOrEmpty(message)) {
                try {
                    var updateMessage = JsonConvert.DeserializeObject<UpdateMessageModel>(message);
                    if(updateMessage?.Type == "dataref_update_values") {
                        ProcessDatarefUpdateValuesMessage(updateMessage);
                    }
                } catch(JsonSerializationException) {
                    ;
                }
            }
        }

        protected virtual void ProcessDatarefUpdateValuesMessage(UpdateMessageModel updateMessage)
        {
            var datarefsById = _DatarefsById;
            if(updateMessage.Data != null && datarefsById != null) {
                foreach(var kvp in updateMessage.Data) {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    if(long.TryParse(key, out var id) && datarefsById.TryGetValue(id, out var dataref)) {
                        ProcessDatarefUpdateValue(dataref, value);
                    }
                }
                FinishedProcessingDatarefUpdate();
                RefreshSelectedScreen();
            }
        }

        protected abstract void ProcessDatarefUpdateValue(DatarefInfoModel dataref, dynamic value);

        protected virtual void FinishedProcessingDatarefUpdate()
        {
        }
    }
}
