// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.FlightSim;

namespace Cduhub.CommandLineInterface
{
    class Program
    {
        private static readonly object _SyncLock = new();
        private static readonly List<FlightSim.IFlightSimulatorMcdu> _HookedFlightSimulatorStates = [];

        static void Main(string[] _)
        {
            var exitCode = 0;

            Hub hub = null;
            var cancelSource = new CancellationTokenSource();
            var hasBeenConnected = false;

            try {
                hub = new();
                hub.CloseApplication += (_,_) => {
                    OutputTimestamped("Quit selected on the CDU");
                    cancelSource.Cancel();
                };
                hub.ConnectedDeviceChanged += (_,_) => {
                    var connectedDevice = hub.ConnectedDevice;
                    if(connectedDevice == null && hasBeenConnected) {
                        OutputTimestamped("Disconnected from CDU");
                    } else {
                        OutputTimestamped($"Connected to {connectedDevice} CDU");
                        hasBeenConnected = true;
                    }
                };
                ConnectedFlightSimulators.FlightSimulatorStateChanged += (_,_) => {
                    HookAndUnhookFlightSimulatorStates(ConnectedFlightSimulators.GetFlightSimulatorMcdus());
                };
            } catch(Exception ex) {
                Console.WriteLine("Caught exception when instantiating the CDU Hub");
                Console.WriteLine(ex);
                exitCode = 2;
            }

            if(exitCode == 0 && hub != null) {
                Console.WriteLine("Press Q to quit");
                try {
                    hub.Connect();
                    while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                        if(cancelSource.Token.IsCancellationRequested) {
                            break;
                        }
                        Thread.Sleep(1);
                    }
                } catch(Exception ex) {
                    OutputTimestamped("Caught exception while running the CDU Hub");
                    Console.WriteLine(ex);
                    exitCode = 2;
                }
            }

            UnhookFlightSimulatorStates();

            if(hub != null) {
                try {
                    hub.Dispose();
                } catch {
                    ;
                }
            }

            Environment.Exit(exitCode);
        }

        private static void OutputTimestamped(string note)
        {
            if(String.IsNullOrEmpty(note)) {
                Console.WriteLine();
            } else {
                var localNow = DateTime.Now;
                Console.WriteLine($"[{localNow.ToString("dd-MMM").ToUpperInvariant()} {localNow:HH:mm:ss}] {note}");
            }
        }

        private static void HookAndUnhookFlightSimulatorStates(IFlightSimulatorMcdu[] flightSimulatorMcdus)
        {
            lock(_SyncLock) {
                var newFlightSimulators = flightSimulatorMcdus
                    .Except(_HookedFlightSimulatorStates)
                    .ToArray();

                var oldFlightSimulators = _HookedFlightSimulatorStates
                    .Except(flightSimulatorMcdus)
                    .ToArray();

                foreach(var newFlightSim in newFlightSimulators) {
                    OutputTimestamped($"{newFlightSim.AircraftName} CDU mirror started");
                    HookFlightSim(newFlightSim);
                    _HookedFlightSimulatorStates.Add(newFlightSim);
                }

                foreach(var oldFlightSim in oldFlightSimulators) {
                    OutputTimestamped($"{oldFlightSim.AircraftName} CDU mirror stopped");
                    UnhookFlightSim(oldFlightSim);
                    _HookedFlightSimulatorStates.Remove(oldFlightSim);
                }
            }
        }

        private static void UnhookFlightSimulatorStates()
        {
            lock(_SyncLock) {
                foreach(var flightSim in _HookedFlightSimulatorStates) {
                    UnhookFlightSim(flightSim);
                }
                _HookedFlightSimulatorStates.Clear();
            }
        }

        private static void HookFlightSim(IFlightSimulatorMcdu flightSim)
        {
            flightSim.ConnectionStateChanged += FlightSimState_ConnectionStateChanged;
        }

        private static void UnhookFlightSim(IFlightSimulatorMcdu flightSim)
        {
            flightSim.ConnectionStateChanged -= FlightSimState_ConnectionStateChanged;
        }

        private static void FlightSimState_ConnectionStateChanged(object sender, EventArgs _)
        {
            var flightSim = sender as IFlightSimulatorMcdu;
            OutputTimestamped($"{flightSim.AircraftName} {(flightSim.ConnectionState.ToString().ToLower())}");
        }
    }
}
