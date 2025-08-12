// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using McduDotNet;

namespace Clock
{
    class Program
    {
        static void Main(string[] _)
        {
            var deviceId = SelectDevice();
            using(var cdu = CduFactory.ConnectLocal(deviceId)) {
                Console.WriteLine($"Using {cdu.DeviceId}");

                Console.WriteLine($"Press Q to quit");
                while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                    var now = DateTime.Now;

                    cdu.Output
                        .Clear()
                        .MiddleLine().CentreFor("00:00:00")
                        .White().Write(now.Hour, "00").Yellow().Write(':')
                        .White().Write(now.Minute, "00").Yellow().Write(':')
                        .White().Write(now.Second, "00");
                    cdu.RefreshDisplay();

                    Thread.Sleep(100);
                }

                cdu.Cleanup();
            }
        }

        static DeviceIdentifier SelectDevice()
        {
            var identifiers = CduFactory.FindLocalDevices();
            var result = identifiers.FirstOrDefault();
            if(identifiers.Count > 1) {
                Console.WriteLine("Select device:");
                for(var idx = 0;idx < identifiers.Count;++idx) {
                    Console.WriteLine($"{idx + 1}: {identifiers[idx]}");
                }
                do {
                    result = null;
                    Console.Write("? ");
                    var number = Console.ReadLine();
                    if(int.TryParse(number, out var idx) && idx > 0 && idx <= identifiers.Count) {
                        result = identifiers[idx - 1];
                    }
                } while(result == null);
            }

            return result;
        }
    }
}
