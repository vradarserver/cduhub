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

namespace Leds
{
    class Program
    {
        static void Main(string[] _)
        {
            var deviceId = SelectDevice();
            using(var cdu = CduFactory.ConnectLocal(deviceId)) {
                Console.WriteLine($"Using {cdu.DeviceId}");

                var supportedLeds = cdu.SupportedLeds
                    .OrderBy(led => led.Describe())
                    .ToArray();
                var leftLeds = new List<Led>();
                var rightLeds = new List<Led>();

                for(var idx = 0;idx < supportedLeds.Length;++idx) {
                    var led = supportedLeds[idx];
                    var list = idx < 6 ? leftLeds : rightLeds;
                    list.Add(led);
                }

                cdu.Output
                    .Small()
                    .Grey()
                    .RightLabel(5, "BRIGHT -5%<")
                    .RightLabel(6, "BRIGHT +5%<");

                for(var idx = 0;idx < leftLeds.Count;++idx) {
                    cdu.Output.LeftLabel(idx + 1, $">{leftLeds[idx].Describe()}");
                }
                for(var idx = 0;idx < rightLeds.Count;++idx) {
                    cdu.Output.RightLabel(idx + 1, $">{rightLeds[idx].Describe()}");
                }

                cdu.RefreshDisplay();

                cdu.KeyDown += (_, args) => {
                    var lsNumber = args.Key.ToLineSelectNumber();
                    if(lsNumber.Number != -1) {
                        var list = lsNumber.IsLeft ? leftLeds : rightLeds;
                        var idx = lsNumber.Number - 1;
                        if(idx < list.Count) {
                            var led = list[idx];
                            cdu.Leds.SetLed(
                                led,
                                !cdu.Leds.GetLed(led)
                            );
                        }

                        if(!lsNumber.IsLeft) {
                            switch(lsNumber.Number) {
                                case 5: cdu.LedBrightnessPercent = Math.Max(0, cdu.LedBrightnessPercent - 5); break;
                                case 6: cdu.LedBrightnessPercent = Math.Min(100, cdu.LedBrightnessPercent + 5); break;
                            }
                        }

                        cdu.RefreshLeds();
                    }
                };

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                cdu.Cleanup();
            }
        }

        static DeviceIdentifier SelectDevice()
        {
            var identifiers = CduFactory
                .FindLocalDevices()
                .OrderBy(r => r.UsbVendorId)
                .ThenBy(r => r.UsbProductId)
                .ToArray();
            var result = identifiers.FirstOrDefault();
            if(identifiers.Length > 1) {
                Console.WriteLine("Select device:");
                for(var idx = 0;idx < identifiers.Length;++idx) {
                    Console.WriteLine($"{idx + 1}: {identifiers[idx]}");
                }
                do {
                    result = null;
                    Console.Write("? ");
                    var number = Console.ReadLine();
                    if(int.TryParse(number, out var idx) && idx > 0 && idx <= identifiers.Length) {
                        result = identifiers[idx - 1];
                    }
                } while(result == null);
            }

            return result;
        }
    }
}
