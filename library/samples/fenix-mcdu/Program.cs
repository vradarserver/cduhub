// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using McduDotNet;
using McduDotNet.FlightSim;

namespace FenixMcdu
{
    class Program
    {
        private static ICdu _Mcdu;
        private static GraphQLHttpClient _FenixEfbGraphQLClient;
        private static DeviceUser _DeviceUser;
        private static Screen _CaptainScreen = new();
        private static Screen _FirstOfficerScreen = new();

        static void Main(string[] _)
        {
            using(var mcdu = CduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.DeviceId}");
                _Mcdu = mcdu;
                _DeviceUser = mcdu.DeviceId.DeviceUser;

                var endpointHostAndPort = "localhost:8083";
                var endpointUri = new Uri($"ws://{endpointHostAndPort}/graphql");

                Console.WriteLine($"Opening connection to Fenix EFB at {endpointHostAndPort}");
                var graphQLOptions = new GraphQLHttpClientOptions() {
                    EndPoint = endpointUri,
                    UseWebSocketForQueriesAndMutations = true
                };

                using(var graphQLClient = new GraphQLHttpClient(graphQLOptions, new NewtonsoftJsonSerializer())) {
                    _FenixEfbGraphQLClient = graphQLClient;
                    SetupFenixDisplayChangeEvents(mcdu, graphQLClient);
                    mcdu.KeyDown += Mcdu_KeyEvent;
                    mcdu.KeyUp   += Mcdu_KeyEvent;

                    Console.WriteLine($"Press Q to quit");
                    while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);
                }

                mcdu.Cleanup();
            }
        }

        private static void SetupFenixDisplayChangeEvents(ICdu mcdu, GraphQLHttpClient graphQLClient)
        {
            var mcduDisplay = mcdu.DeviceId.DeviceUser == DeviceUser.Captain
                ? 1
                : 2;
            var mcduDisplayName = $"aircraft.mcdu{mcduDisplay}.display";

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
                    }
                }
            };

            var subscriptionStream = graphQLClient.CreateSubscriptionStream<dynamic>(subscriptionRequest);
            _ = subscriptionStream.Subscribe(result => {
                var dataRefs = result.Data?.dataRefs;
                if(dataRefs != null) {
                    var name = dataRefs.name?.ToString();
                    Screen screen = null;
                    var isVisible = false;
                    switch(name) {
                        case FenixA320GraphQL.GraphQLMcdu1DisplayName:
                            screen = _CaptainScreen;
                            isVisible = _DeviceUser == DeviceUser.Captain;
                            break;
                        case FenixA320GraphQL.GraphQLMcdu2DisplayName:
                            screen = _FirstOfficerScreen;
                            isVisible = _DeviceUser == DeviceUser.FirstOfficer;
                            break;
                    }
                    if(screen != null) {
                        FenixA320GraphQL.ParseGraphQLMcduValueToScreen(
                            dataRefs.value?.ToString(),
                            screen
                        );
                        if(isVisible) {
                            RefreshVisibleDisplay();
                            mcdu.RefreshDisplay();
                        }
                    }
                }
            });
        }

        private static void RefreshVisibleDisplay()
        {
            if(_Mcdu != null) {
                var copyFrom = _DeviceUser == DeviceUser.Captain
                    ? _CaptainScreen
                    : _FirstOfficerScreen;
                _Mcdu.Screen.CopyFrom(copyFrom);
                _Mcdu.RefreshDisplay();
            }
        }

        private static void ToggleBetweenCaptainAndFirstOfficerMcdu()
        {
            _DeviceUser = _DeviceUser != DeviceUser.Captain
                ? DeviceUser.Captain
                : DeviceUser.FirstOfficer;
            RefreshVisibleDisplay();
        }

        private static void SendKeyToFenix(Key mcduKey, bool pressed)
        {
            var client = _FenixEfbGraphQLClient;
            if(client != null) {
                var key = FenixA320GraphQL.GraphQLKeyName(mcduKey, _DeviceUser);
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

        private static void Mcdu_KeyEvent(object sender, KeyEventArgs args)
        {
            if(args.Key != Key.Blank1) {
                SendKeyToFenix(args.Key, args.Pressed);
            } else if(args.Pressed) {
                ToggleBetweenCaptainAndFirstOfficerMcdu();
            }
        }
    }
}
