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
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McduDotNet;
using McduDotNet.SimBridgeMcdu;
using Newtonsoft.Json;

namespace Cduhub.FlightSim
{
    public class SimBridgeA320RemoteMcdu : SimulatedMcdus, IDisposable
    {
        private CancellationTokenSource _WebSocketCancelationTokenSource;
        private Task _WebSocketTask;

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

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        public SimBridgeA320RemoteMcdu(Screen masterScreen, Leds masterLeds) : base(masterScreen, masterLeds)
        {
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~SimBridgeA320RemoteMcdu() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of, or finalises, the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                DisposeWebSocket();
            }
        }

        private void DisposeWebSocket()
        {
            var cts = _WebSocketCancelationTokenSource;
            var backgroundTask = _WebSocketTask;

            _WebSocketCancelationTokenSource = null;
            _WebSocketTask = null;

            if(cts != null) {
                try {
                    cts.Cancel();
                    backgroundTask?.Wait(5000);
                } catch {
                }
            }
        }

        /// <summary>
        /// Connects to SimBridge. If it is already connected then this drops the connection and establishes a
        /// new one.
        /// </summary>
        public void Reconnect()
        {
            DisposeWebSocket();
            _WebSocketCancelationTokenSource = new CancellationTokenSource();
            _WebSocketTask = Task.Run(() => ConnectWebSocket(_WebSocketCancelationTokenSource.Token));
        }

        /// <inheritdoc/>
        public override void SendKeyToSimulator(Key key, bool pressed)
        {
        }

        private async Task ConnectWebSocket(CancellationToken cancellationToken)
        {
            var uri = new Uri($"ws://{Host}:{Port}/interfaces/v1/mcdu");
            while(!cancellationToken.IsCancellationRequested) {
                try {
                    using(var client = new ClientWebSocket()) {
                        await client.ConnectAsync(uri, cancellationToken);
                        await SendMessage(client, "requestUpdate", cancellationToken);
                        await ReceiveLoop(client, cancellationToken);
                    }
                } catch(HttpRequestException) {
                    await Task.Delay(5000);
                } catch(WebSocketException) {
                    await Task.Delay(1000);
                } catch(OperationCanceledException) {
                    break;
                } catch {
                    throw;
                }
            }
        }

        private async Task SendMessage(ClientWebSocket client, string message, CancellationToken cancellationToken)
        {
            if(!String.IsNullOrEmpty(message) && !cancellationToken.IsCancellationRequested) {
                var utf8Buffer = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(
                    new ArraySegment<byte>(utf8Buffer),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken
                );
            }
        }

        private async Task ReceiveLoop(ClientWebSocket client, CancellationToken cancellationToken)
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
            SimBridgeUtility.ParseSimBridgeUpdateMcduToScreenAndLeds(
                mcduDisplay.Left,
                PilotBuffer.Screen,
                PilotBuffer.Leds
            );
            SimBridgeUtility.ParseSimBridgeUpdateMcduToScreenAndLeds(
                mcduDisplay.Right,
                FirstOfficerBuffer.Screen,
                FirstOfficerBuffer.Leds
            );
            RefreshSelectedScreen();
            RefreshSelectedLeds();
        }
    }
}
