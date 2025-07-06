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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cduhub.FlightSim.XPlaneUdpModels;

namespace Cduhub.FlightSim
{
    /// <summary>
    /// The base for classes that need to communicate with X-Plane over UDP.
    /// </summary>
    public class XPlaneUdp : IDisposable
    {
        private object _UdpClientLock = new object();
        private UdpClient _UdpClient;
        private IPEndPoint _XPlaneSendEndpoint;
        private DateTime _IdleReceiveTimeoutFromUtc;
        private readonly List<string> _Subscriptions = new List<string>();

        /// <summary>
        /// Gets or sets the address of the machine running X-Plane.
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets the port that X-Plane is listening to for UDP packets.
        /// </summary>
        public int Port { get; set; } = 49000;

        /// <summary>
        /// The address to listen to. Leave at loopback to listen on all interfaces.
        /// </summary>
        public IPAddress ListenToAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// The port to listen to. Leave at zero to let the operating system decide.
        /// </summary>
        public int ListenToPort { get; set; } = 0;

        /// <summary>
        /// The duration of radio silence from X-Plane that have to elapse before the class retries the
        /// connection.
        /// </summary>
        public int IdleReceiveReconnectMilliseconds { get; set; } = 5000;

        /// <summary>
        /// The number of times per second to receive data.
        /// </summary>
        public int DataRefRefreshIntervalTimesPerSecond { get; set; } = 1;

        /// <summary>
        /// The number of subscription requests to make before we pause to let X-Plane catch up.
        /// </summary>
        public int CountSubscriptionsBetweenPauses { get; set; } = 100;

        /// <summary>
        /// How long to wait during subscriptions while we let X-Plane catch up.
        /// </summary>
        public int MillisecondPauseDuringSubscriptions { get; set; } = 100;

        /// <summary>
        /// True if the UDP client exists. It might not actually be "connected" to anything, UDP doesn't do
        /// persistent connections.
        /// </summary>
        public bool IsConnected => _UdpClient != null;

        /// <summary>
        /// All of the dataref subscriptions.
        /// </summary>
        public IReadOnlyList<string> DataRefSubscriptions => _Subscriptions;

        /// <summary>
        /// Called when an RREF or RREFO packet is received.
        /// </summary>
        public Action<XPlaneDataRefValue[]> DataRefUpdatesReceived { get; set; }

        /// <summary>
        /// Raised when <see cref="IsConnected"/> changes.
        /// </summary>
        public event EventHandler IsConnectedChanged;

        /// <summary>
        /// Raises <see cref="IsConnectedChanged"/>.
        /// </summary>
        protected virtual void OnIsConnectedChanged() => IsConnectedChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised when a packet has been received, regardless of whether we could parse it.
        /// </summary>
        public event EventHandler PacketReceived;

        /// <summary>
        /// Raises <see cref="PacketReceived"/>.
        /// </summary>
        protected virtual void OnPacketReceived() => PacketReceived?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~XPlaneUdp() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                DisposeUdpClient();
            }
        }

        /// <summary>
        /// Cleans up and disposes of the UDP client.
        /// </summary>
        protected virtual void DisposeUdpClient()
        {
            var udpClient = _UdpClient;
            var xplaneEndpoint = _XPlaneSendEndpoint;
            var subscriptions = _Subscriptions.ToArray();
            _UdpClient = null;

            if(udpClient != null) {
                try {
                    SendSubscriptions(udpClient, xplaneEndpoint, subscriptions, enable: false);
                } catch { }
                try {
                    udpClient.Close();
                } catch { }
                try {
                    udpClient.Dispose();
                } catch { }

                OnIsConnectedChanged();
            }
        }

        /// <summary>
        /// Adds a dataref subscription.
        /// </summary>
        /// <param name="dataRef"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddSubscription(string dataRef)
        {
            if(IsConnected) {
                throw new InvalidOperationException(
                    "You cannot set a subscription after the connection to X-Plane has been established"
                );
            }
            if(!_Subscriptions.Contains(dataRef)) {
                _Subscriptions.Add(dataRef);
            }
        }

        /// <summary>
        /// Adds many dataref subscriptions.
        /// </summary>
        /// <param name="dataRefs"></param>
        public void AddSubscriptions(string[] dataRefs)
        {
            foreach(var dataRef in dataRefs) {
                AddSubscription(dataRef);
            }
        }

        /// <summary>
        /// Connects to XPlane and starts communicating with it.
        /// </summary>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var subscriptions = _Subscriptions.ToArray();

            // UdpClient in .NET Standard 2.0 does not support passing cancellation tokens to the send and receive
            // async methods - so instead what we will do is dispose of the client when the task is cancelled and
            // then handle the resulting exceptions...
            using(cancellationToken.Register(() => DisposeUdpClient())) {
                while(!cancellationToken.IsCancellationRequested) {
                    DisposeUdpClient();

                    using(var receiveTimeoutCancellationTokenSource = new CancellationTokenSource()) {
                        using(receiveTimeoutCancellationTokenSource.Token.Register(() => DisposeUdpClient())) {
                            var mergedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                                receiveTimeoutCancellationTokenSource.Token,
                                cancellationToken
                            );
                            var mergedToken = mergedCancellationTokenSource.Token;

                            var listenToEndpoint = new IPEndPoint(ListenToAddress, ListenToPort);
                            var client = new UdpClient(listenToEndpoint);

                            var remoteAddress = IPAddress.Parse(Host);
                            var remoteEndpoint = new IPEndPoint(remoteAddress, Port);
                            _XPlaneSendEndpoint = remoteEndpoint;

                            _UdpClient = client;
                            try {
                                OnIsConnectedChanged();

                                SendSubscriptions(client, remoteEndpoint, subscriptions, enable: true);

                                _IdleReceiveTimeoutFromUtc = DateTime.UtcNow;
                                var receiveLoopTask = ReceiveLoop(client, subscriptions, mergedToken);
                                var idleReconnectTask = IdleReconnectLoop(mergedToken, receiveTimeoutCancellationTokenSource);

                                await Task.WhenAll(
                                    receiveLoopTask,
                                    idleReconnectTask
                                );
                            } catch(ObjectDisposedException) {
                                // We're going to see this one a LOT.
                                ;
                            } catch(SocketException) {
                                await Task.Delay(5000);
                            }
                        }
                    }
                }
            }
        }

        private void SendSubscriptions(UdpClient client, IPEndPoint xplaneEndpoint, string[] subscriptions, bool enable)
        {
            var buffer = new byte[413];
            var dataRefIdx = 0;

            void addToBuffer(byte[] bytes)
            {
                bytes.CopyTo(buffer, dataRefIdx);
                dataRefIdx += bytes.Length;
            }

            addToBuffer(Encoding.ASCII.GetBytes("RREF\0"));
            addToBuffer(LittleEndian.GetBytes(enable ? DataRefRefreshIntervalTimesPerSecond : 0));

            var startDataRefIdx = dataRefIdx;
            void sendBuffer(int countSent)
            {
                Array.Clear(buffer, dataRefIdx, buffer.Length - dataRefIdx);
                lock(_UdpClientLock) {
                    client.Send(buffer, buffer.Length, xplaneEndpoint);
                }
                dataRefIdx = startDataRefIdx;

                // I've had this run too quickly for X-Plane to process and it ends up missing some, which can
                // cause some very freaky effects. We have no way of knowing whether it was received because
                // they're using UDP instead of anything reliable / knowable. So I'm going to pause between
                // every-so-many to give it a chance to catch up. Fingers crossed!
                _IdleReceiveTimeoutFromUtc = DateTime.UtcNow;
                if(countSent % CountSubscriptionsBetweenPauses == 0) {
                    Thread.Sleep(MillisecondPauseDuringSubscriptions);
                }
            }

            for(var idx = 0;idx < subscriptions.Length;++idx) {
                var dataRef = subscriptions[idx];
                addToBuffer(LittleEndian.GetBytes(idx + 1));
                addToBuffer(Encoding.ASCII.GetBytes($"{dataRef}\0"));
                sendBuffer(idx + 1);
            }
        }

        protected async virtual Task ReceiveLoop(UdpClient client, string[] subscriptions, CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested) {
                var response = await client.ReceiveAsync();
                _IdleReceiveTimeoutFromUtc = DateTime.UtcNow;
                if(response.Buffer?.Length > 0) {
                    var buffer = new byte[response.Buffer.Length];
                    Array.Copy(response.Buffer, buffer, buffer.Length);
                    await ProcessUdpResponse(buffer, subscriptions);
                }
            }
        }

        protected Task ProcessUdpResponse(byte[] buffer, string[] subscriptions)
        {
            OnPacketReceived();
            if(buffer.Length > 4) {
                var type = Encoding.ASCII.GetString(buffer, 0, 4);
                if(type == "RREF") {
                    var dataRefValues = ProcessRREF(buffer, subscriptions, 5);
                    if(dataRefValues.Length > 0) {
                        DataRefUpdatesReceived?.Invoke(dataRefValues);
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected virtual XPlaneDataRefValue[] ProcessRREF(byte[] buffer, string[] subscriptions, int startOffset)
        {
            var result = new List<XPlaneDataRefValue>();

            for(var idx = startOffset;idx + 8 <= buffer.Length;idx += 8) {
                var dataRefIdx = LittleEndian.ToInt32(buffer, idx);
                var value = LittleEndian.ToSingle(buffer, idx + 4);
                ReceivedFloat(result, dataRefIdx, value, subscriptions);
            }

            return result.ToArray();
        }

        private void ReceivedFloat(List<XPlaneDataRefValue> values, int dataRefIdx, float value, string[] subscriptions)
        {
            var subscription = GetDataRefName(subscriptions, dataRefIdx);
            if(subscription != null) {
                values.Add(new XPlaneDataRefValue(subscription, value));
            }
        }

        private static string GetDataRefName(string[] subscriptions, int index)
        {
            var result = index > 0 && index <= subscriptions.Length
                ? subscriptions[index - 1]
                : null;
            return result;
        }

        protected async virtual Task IdleReconnectLoop(
            CancellationToken cancellationToken,
            CancellationTokenSource receiveTimeoutCancellationTokenSource
        )
        {
            while(!cancellationToken.IsCancellationRequested) {
                var threshold = _IdleReceiveTimeoutFromUtc.AddMilliseconds(IdleReceiveReconnectMilliseconds);
                if(DateTime.UtcNow >= threshold) {
                    receiveTimeoutCancellationTokenSource.Cancel();
                }
                await Task.Delay(100);
            }
        }

        public void SendCommand(string command)
        {
            var client = _UdpClient;
            var endpoint = _XPlaneSendEndpoint;

            if(client != null) {
                var buffer = new byte[509];
                Array.Clear(buffer, 0, buffer.Length);
                Encoding.ASCII.GetBytes("CMND").CopyTo(buffer, 0);
                Encoding.ASCII.GetBytes(command).CopyTo(buffer, 5);
                lock(_UdpClientLock) {
                    try {
                        client.Send(buffer, buffer.Length, endpoint);
                    } catch { };
                }
            }
        }
    }
}
