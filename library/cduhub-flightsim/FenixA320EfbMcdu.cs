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
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using McduDotNet;
using McduDotNet.FlightSim;

namespace Cduhub.FlightSim
{
    /// <summary>
    /// Manages the connection to Fenix's A320 EFB MCDUs.
    /// </summary>
    public class FenixA320EfbMcdu : SimulatedMcdus, IDisposable
    {
        private GraphQLHttpClient _GraphQLClient;
        private IObservable<GraphQLResponse<dynamic>> _PushSubscriptionStream;
        private IDisposable _PushSubscription;
        private IDisposable _ConnectionStateSubscription;

        /// <inheritdoc/>
        public override string FlightSimulatorName => FlightSimulatorNames.MSFS2020_2024;

        /// <inheritdoc/>
        public override string AircraftName => "Fenix A32x";

        /// <inheritdoc/>
        public override SimulatorMcduBuffer PilotBuffer { get; } = new SimulatorMcduBuffer();

        /// <inheritdoc/>
        public override SimulatorMcduBuffer FirstOfficerBuffer { get; } = new SimulatorMcduBuffer();

        /// <summary>
        /// The address of the machine running the Fenix A320 simulation.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// The port of the EFB exposed by the Fenix A320 simulation.
        /// </summary>
        public int Port { get; set; } = 8083;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="deviceUser"></param>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        public FenixA320EfbMcdu(DeviceUser deviceUser, Screen masterScreen, Leds masterLeds) : base(deviceUser, masterScreen, masterLeds)
        {
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FenixA320EfbMcdu() => Dispose(false);

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
                DisposeGraphQLClient();
            }
        }

        /// <summary>
        /// Connects to the Fenix EFB's MCDUs. If a connection has already been established then it is dropped
        /// and restarted.
        /// </summary>
        public override void ReconnectToSimulator()
        {
            CreateGraphQLClient();
        }

        /// <inheritdoc/>
        public override void SendKeyToSimulator(Key mcduKey, bool pressed)
        {
            var client = _GraphQLClient;
            if(client != null) {
                var key = FenixA320GraphQL.GraphQLKeyName(mcduKey, SelectedBufferDeviceUser);
                if(key != "") {
                    var request = new GraphQLRequest() {
                        Query = $@"
                            mutation ($keyName: String!) {{
                                dataRef {{
                                    writeInt(name: $keyName, value: {(pressed ? 1 : 0)})
                                    __typename
                                }}
                            }}
                        ",
                        Variables = new { keyName = $"{FenixA320GraphQL.GraphQLSystemSwitchesPrefix}.{key}" }
                    };

                    Task.Run(() => client.SendMutationAsync<object>(request));
                }
            }
        }

        private void CreateGraphQLClient()
        {
            DisposeGraphQLClient();

            var endpointUri = new Uri($"ws://{Host}:{Port}/graphql");
            var options = new GraphQLHttpClientOptions() {
                EndPoint = endpointUri,
                UseWebSocketForQueriesAndMutations = true
            };
            _GraphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            SetupConnectionStateSubscription();
            SetupPushedDataSubscription();
        }

        private void DisposeGraphQLClient()
        {
            var client = _GraphQLClient;
            var connectionStateSubscription = _ConnectionStateSubscription;
            var pushSubscription = _PushSubscription;

            RecordConnectionState(client == null
                ? ConnectionState.Disconnected
                : ConnectionState.Disconnecting
            );

            _GraphQLClient = null;
            _ConnectionStateSubscription = null;
            _PushSubscriptionStream = null;
            _PushSubscription = null;

            try {
                connectionStateSubscription?.Dispose();
            } catch {;}
            try {
                pushSubscription?.Dispose();
            } catch {;}
            try {
                client?.Dispose();
            } catch {;}

            RecordConnectionState(ConnectionState.Disconnected);
        }

        private void SetupConnectionStateSubscription()
        {
            var client = _GraphQLClient;
            if(client != null) {
                _ConnectionStateSubscription = client
                    .WebsocketConnectionState
                    .Subscribe(ConnectionStateUpdate);
            }
        }

        private void ConnectionStateUpdate(GraphQLWebsocketConnectionState state)
        {
            switch(state) {
                case GraphQLWebsocketConnectionState.Connecting:
                    RecordConnectionState(ConnectionState.Connecting);
                    break;
                case GraphQLWebsocketConnectionState.Connected:
                    RecordConnectionState(ConnectionState.Connected);
                    break;
            }
        }

        private void SetupPushedDataSubscription()
        {
            var client = _GraphQLClient;
            if(client != null) {
                var subscriptionRequest = new GraphQLRequest {
                    Query = @"
                        subscription OnDataRefChanged($names: [String!]!) {
                            dataRefs(names: $names) {
                                name
                                value
                            }
                        }
                    ",
                    Variables = new {
                        names = new[] {
                            FenixA320GraphQL.GraphQLMcdu1DisplayName,
                            FenixA320GraphQL.GraphQLMcdu2DisplayName,

                            FenixA320GraphQL.GraphQLMcdu1LedFailName,
                            FenixA320GraphQL.GraphQLMcdu1LedFmName,
                            FenixA320GraphQL.GraphQLMcdu1LedFm1Name,
                            FenixA320GraphQL.GraphQLMcdu1LedFm2Name,
                            FenixA320GraphQL.GraphQLMcdu1LedIndName,
                            FenixA320GraphQL.GraphQLMcdu1LedMcduMenuName,
                            FenixA320GraphQL.GraphQLMcdu1LedRdyName,

                            FenixA320GraphQL.GraphQLMcdu2LedFailName,
                            FenixA320GraphQL.GraphQLMcdu2LedFmName,
                            FenixA320GraphQL.GraphQLMcdu2LedFm1Name,
                            FenixA320GraphQL.GraphQLMcdu2LedFm2Name,
                            FenixA320GraphQL.GraphQLMcdu2LedIndName,
                            FenixA320GraphQL.GraphQLMcdu2LedMcduMenuName,
                            FenixA320GraphQL.GraphQLMcdu2LedRdyName,
                        }
                    }
                };

                _PushSubscriptionStream = client.CreateSubscriptionStream<dynamic>(subscriptionRequest);
                _PushSubscription = _PushSubscriptionStream.Subscribe(PushedDataUpdate);
            }
        }

        private void PushedDataUpdate(GraphQLResponse<dynamic> response)
        {
            var dataRefs = response.Data?.dataRefs;
            if(dataRefs != null) {
                var name = dataRefs.name?.ToString();
                var value = dataRefs.value?.ToString();

                Screen updateScreen = null;
                Leds updateLeds = null;
                switch(name) {
                    case FenixA320GraphQL.GraphQLMcdu1DisplayName:      updateScreen = PilotBuffer.Screen; break;
                    case FenixA320GraphQL.GraphQLMcdu2DisplayName:      updateScreen = FirstOfficerBuffer.Screen; break;

                    case FenixA320GraphQL.GraphQLMcdu1LedFailName:      updateLeds = PilotBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu1LedFmName:        updateLeds = PilotBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu1LedFm1Name:       updateLeds = PilotBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu1LedFm2Name:       updateLeds = PilotBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu1LedIndName:       updateLeds = PilotBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu1LedMcduMenuName:  updateLeds = PilotBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu1LedRdyName:       updateLeds = PilotBuffer.Leds; break;

                    case FenixA320GraphQL.GraphQLMcdu2LedFailName:      updateLeds = FirstOfficerBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu2LedFmName:        updateLeds = FirstOfficerBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu2LedFm1Name:       updateLeds = FirstOfficerBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu2LedFm2Name:       updateLeds = FirstOfficerBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu2LedIndName:       updateLeds = FirstOfficerBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu2LedMcduMenuName:  updateLeds = FirstOfficerBuffer.Leds; break;
                    case FenixA320GraphQL.GraphQLMcdu2LedRdyName:       updateLeds = FirstOfficerBuffer.Leds; break;
                }

                if(updateScreen != null) {
                    FenixA320GraphQL.ParseGraphQLMcduValueToScreen(value, updateScreen);
                    if(updateScreen == SelectedBuffer.Screen) {
                        RefreshSelectedScreen();
                    }
                }
                if(updateLeds != null) {
                    FenixA320GraphQL.ParseGraphQLIndicatorValueToLeds(name, value, updateLeds);
                    if(updateLeds == SelectedBuffer.Leds) {
                        RefreshSelectedLeds();
                    }
                }
            }
            RecordMessageReceivedFromSimulator();
        }
    }
}
