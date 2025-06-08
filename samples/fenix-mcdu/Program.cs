using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using McduDotNet;

namespace FenixMcdu
{
    class Program
    {
        private static IMcdu _Mcdu;
        private static GraphQLHttpClient _FenixEfbGraphQLClient;
        private static ProductId _DisplayProductId;
        private static Screen _CaptainScreen = new();
        private static Screen _FirstOfficerScreen = new();

        static void Main(string[] _)
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");
                _Mcdu = mcdu;
                _DisplayProductId = mcdu.ProductId;

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

        private static void SetupFenixDisplayChangeEvents(IMcdu mcdu, GraphQLHttpClient graphQLClient)
        {
            var mcduDisplay = mcdu.ProductId == ProductId.Captain
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
                        "aircraft.mcdu1.display",
                        "aircraft.mcdu2.display",
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
                            mcdu.RefreshDisplay();
                        }
                    }
                }
            });
        }

        private static void RefreshVisibleDisplay()
        {
            if(_Mcdu != null) {
                var copyFrom = _DisplayProductId == ProductId.Captain
                    ? _CaptainScreen
                    : _FirstOfficerScreen;
                _Mcdu.Screen.CopyFrom(copyFrom);
                _Mcdu.RefreshDisplay();
            }
        }

        private static void ToggleBetweenCaptainAndFirstOfficerMcdu()
        {
            _DisplayProductId = _DisplayProductId != ProductId.Captain
                ? ProductId.Captain
                : ProductId.FirstOfficer;
            RefreshVisibleDisplay();
        }

        private static void SendKeyToFenix(Key mcduKey, bool pressed)
        {
            var client = _FenixEfbGraphQLClient;
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
