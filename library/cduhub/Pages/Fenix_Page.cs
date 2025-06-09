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
using System.Reactive.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using McduDotNet;

namespace Cduhub.Pages
{
    class Fenix_Page : Page
    {
        private string _EndpointHostAndPort = "localhost:8083";
        private GraphQLHttpClient _GraphQLClient;
        private IObservable<GraphQLResponse<dynamic>> _SubscriptionStream;
        private IDisposable _Subscription;
        private ProductId _DisplayProductId;
        private McduDotNet.Screen _CaptainScreen = new McduDotNet.Screen();
        private McduDotNet.Screen _FirstOfficerScreen = new McduDotNet.Screen();

        public override Key MenuKey => Key.Blank2;

        public Fenix_Page(Hub hub) : base(hub)
        {
            Reconnect();
        }

        public override void OnKeyDown(Key key)
        {
            if(key != Key.Blank1) {
                SendKeyToFenix(key, true);
            } else {
                ToggleBetweenCaptainAndFirstOfficerMcdu();
            }
        }

        public override void OnKeyUp(Key key)
        {
            if(key != Key.Blank1) {
                SendKeyToFenix(key, false);
            }
        }

        public void Reconnect()
        {
            CreateGraphQLClient();
        }

        private void CreateGraphQLClient()
        {
            DisposeGraphQLClient();

            var endpointUri = new Uri($"ws://{_EndpointHostAndPort}/graphql");
            var options = new GraphQLHttpClientOptions() {
                EndPoint = endpointUri,
                UseWebSocketForQueriesAndMutations = true
            };
            _GraphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            SetupFenixDisplayChangeEvents();
        }

        private void SetupFenixDisplayChangeEvents()
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
                            "aircraft.mcdu1.display",
                            "aircraft.mcdu2.display",
                        }
                    }
                };

                _SubscriptionStream = client.CreateSubscriptionStream<dynamic>(subscriptionRequest);
                _Subscription = _SubscriptionStream.Subscribe(result => {
                    var dataRefs = result.Data?.dataRefs;
                    if(dataRefs != null) {
                        var name = dataRefs.name?.ToString();
                        McduDotNet.Screen screen = null;
                        var isVisible = false;
                        switch(name) {
                            case FenixA320Utility.GraphGLPilotMcduDisplayName:
                                screen = _CaptainScreen;
                                isVisible = _DisplayProductId == ProductId.Captain;
                                break;
                            case FenixA320Utility.GraphGLFirstOfficerMcduDisplayName:
                                screen = _FirstOfficerScreen;
                                isVisible = _DisplayProductId == ProductId.FirstOfficer;
                                break;
                        }
                        if(screen != null) {
                            FenixA320Utility.ParseGraphQLMcduValueToScreen(
                                dataRefs.value?.ToString(),
                                screen
                            );
                            if(isVisible) {
                                RefreshVisibleDisplay();
                            }
                        }
                    }
                });
            }
        }

        private void DisposeGraphQLClient()
        {
            var client = _GraphQLClient;
            var subscription = _Subscription;

            _GraphQLClient = null;
            _SubscriptionStream = null;
            _Subscription = null;

            try {
                if(subscription != null) {
                    subscription.Dispose();
                }
            } catch {;}
            try {
                if(client != null) {
                    client.Dispose();
                }
            } catch {;}
        }

        private void RefreshVisibleDisplay()
        {
            var copyFrom = _DisplayProductId == ProductId.Captain
                ? _CaptainScreen
                : _FirstOfficerScreen;
            Screen.CopyFrom(copyFrom);
            RefreshDisplay();
        }

        private void ToggleBetweenCaptainAndFirstOfficerMcdu()
        {
            _DisplayProductId = _DisplayProductId != ProductId.Captain
                ? ProductId.Captain
                : ProductId.FirstOfficer;
            RefreshVisibleDisplay();
        }

        private void SendKeyToFenix(Key mcduKey, bool pressed)
        {
            var client = _GraphQLClient;
            if(client != null) {
                var key = FenixA320Utility.GraphQLKeyName(mcduKey, _DisplayProductId);
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
                        Variables = new { keyName = $"{FenixA320Utility.GraphGLSystemSwitchesPrefix}.{key}" }
                    };

                    Task.Run(() => client.SendMutationAsync<object>(request));
                }
            }
        }
    }
}
