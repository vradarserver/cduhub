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
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McduDotNet;
using McduDotNet.FlightSim;
using McduDotNet.FlightSim.SimBridgeMcdu;
using Newtonsoft.Json;

namespace Cduhub.FlightSim
{
    public class SimBridgeA320RemoteMcdu : SimulatedMcdusOverWebSocket
    {
        private readonly object _QueueLock = new object();
        private readonly Queue<string> _SendEventQueue = new Queue<string>();

        /// <inheritdoc/>
        public override SimulatorMcduBuffer PilotBuffer { get; } = new SimulatorMcduBuffer();

        /// <inheritdoc/>
        public override SimulatorMcduBuffer FirstOfficerBuffer { get; } = new SimulatorMcduBuffer();

        /// <summary>
        /// The address of the machine running SimBridge.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// The port that SimBridge is listening to.
        /// </summary>
        public int Port { get; set; } = 8380;

        /// <inheritdoc/>
        protected override Uri WebSocketUri => new Uri($"ws://{Host}:{Port}/interfaces/v1/mcdu");

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        public SimBridgeA320RemoteMcdu(Screen masterScreen, Leds masterLeds) : base(masterScreen, masterLeds)
        {
        }

        protected override void DisposeWebSocket()
        {
            base.DisposeWebSocket();

            lock(_QueueLock) {
                _SendEventQueue.Clear();
            }
        }

        /// <inheritdoc/>
        public override void SendKeyToSimulator(Key key, bool pressed)
        {
            var keyCode = key.ToSimBridgeRemoteMcduKeyName();
            if(pressed && keyCode != "" && IsConnected) {
                var leftRight = SelectedBufferProductId == ProductId.Captain
                    ? "left"
                    : "right";
                var eventCode = $"event:{leftRight}:{keyCode}";
                lock(_QueueLock) {
                    _SendEventQueue.Enqueue(eventCode);
                }
            }
        }

        protected override async Task InitialiseNewConnection(ClientWebSocket client, CancellationToken cancellationToken)
        {
            await SendUTF8Message(client, "requestUpdate", cancellationToken);
        }

        protected override async Task SendLoop(ClientWebSocket client, CancellationToken cancellationToken)
        {
            lock(_QueueLock) {
                _SendEventQueue.Clear();
            }

            while(client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested) {
                string eventCode = null;
                lock(_QueueLock) {
                    if(_SendEventQueue.Count > 0) {
                        eventCode = _SendEventQueue.Dequeue();
                    }
                }
                if(eventCode == null) {
                    Thread.Sleep(1);
                } else {
                    await SendUTF8Message(client, eventCode, cancellationToken);
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
                if(message.StartsWith("update:")) {
                    ProcessUpdateMessage(message);
                }
            }
        }

        private void ProcessUpdateMessage(string message)
        {
            var json = message.Substring("update:".Length);
            var mcduDisplay = JsonConvert.DeserializeObject<McduDisplay>(json);
            SimBridgeWebSocket.ParseSimBridgeUpdateMcduToScreenAndLeds(
                mcduDisplay.Left,
                PilotBuffer.Screen,
                PilotBuffer.Leds
            );
            SimBridgeWebSocket.ParseSimBridgeUpdateMcduToScreenAndLeds(
                mcduDisplay.Right,
                FirstOfficerBuffer.Screen,
                FirstOfficerBuffer.Leds
            );
            RefreshSelectedScreen();
            RefreshSelectedLeds();
        }
    }
}
