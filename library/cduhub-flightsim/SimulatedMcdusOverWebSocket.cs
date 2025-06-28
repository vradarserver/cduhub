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

namespace Cduhub.FlightSim
{
    /// <summary>
    /// The base class for simulators that expose some kind of web socket interface. Not GraphQL though, this is
    /// just basic dumb Web Sockets.
    /// </summary>
    public abstract class SimulatedMcdusOverWebSocket : SimulatedMcdus, IDisposable
    {
        protected CancellationTokenSource _WebSocketCancelationTokenSource;
        protected Task _WebSocketTask;
        protected readonly SemaphoreSlim _SendMessageSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Gets the URL of the web socket to connect to.
        /// </summary>
        protected abstract Uri WebSocketUri { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        protected SimulatedMcdusOverWebSocket(Screen masterScreen, Leds masterLeds) : base(masterScreen, masterLeds)
        {
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~SimulatedMcdusOverWebSocket() => Dispose(false);

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

        protected virtual void DisposeWebSocket()
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
        /// Connects to the web socket. If it is already connected then this drops the connection and establishes a
        /// new one.
        /// </summary>
        public override void ReconnectToSimulator()
        {
            DisposeWebSocket();
            _WebSocketCancelationTokenSource = new CancellationTokenSource();
            _WebSocketTask = Task.Run(() => ConnectWebSocket(_WebSocketCancelationTokenSource.Token));
        }

        protected virtual async Task ConnectWebSocket(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested) {
                try {
                    RecordConnection(connected: false);
                    using(var client = new ClientWebSocket()) {
                        await client.ConnectAsync(WebSocketUri, cancellationToken);
                        await InitialiseNewConnection(client, cancellationToken);

                        var pollConnectionStateLoop = Task.Run(() => PollConnectionStateLoop(client, cancellationToken));
                        var sendLoop = Task.Run(() => SendLoop(client, cancellationToken));
                        var receiveLoop = Task.Run(() => ReceiveLoop(client, cancellationToken));
                        Task.WaitAll(pollConnectionStateLoop, sendLoop, receiveLoop);
                    }
                } catch(HttpRequestException) {
                    if(!cancellationToken.IsCancellationRequested) {
                        await Task.Delay(5000);
                    }
                } catch(WebSocketException) {
                    if(!cancellationToken.IsCancellationRequested) {
                        await Task.Delay(1000);
                    }
                } catch(OperationCanceledException) {
                    break;
                } catch {
                    throw;
                }
            }
        }

        protected virtual Task InitialiseNewConnection(ClientWebSocket client, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected async virtual Task PollConnectionStateLoop(ClientWebSocket client, CancellationToken cancellationToken)
        {
            while(client != null && !cancellationToken.IsCancellationRequested) {
                try {
                    RecordConnection(client.State == WebSocketState.Open);
                } catch {
                    // We can get all sorts of errors - client can be disposed, state is garbage etc.
                    // I think if it's erroring then we should assume that the connection is bad.
                    RecordConnection(connected: false);
                }
                await Task.Delay(100);
            }
        }

        protected virtual Task SendLoop(ClientWebSocket client, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveLoop(ClientWebSocket client, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task SendUTF8Message(
            ClientWebSocket client,
            string message,
            CancellationToken cancellationToken
        )
        {
            if(!String.IsNullOrEmpty(message) && !cancellationToken.IsCancellationRequested) {
                var utf8Buffer = Encoding.UTF8.GetBytes(message);

                var gotSemaphore = false;
                try {
                    await _SendMessageSemaphore.WaitAsync(cancellationToken);
                    gotSemaphore = true;

                    if(!cancellationToken.IsCancellationRequested) {
                        await client.SendAsync(
                            new ArraySegment<byte>(utf8Buffer),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            cancellationToken
                        );
                    }
                } finally {
                    if(gotSemaphore) {
                        _SendMessageSemaphore.Release();
                    }
                }
            }
        }
    }
}
