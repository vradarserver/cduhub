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
            var usbDevice = SelectDevice();
            using(var cdu = DeviceFactory.ConnectLocalCdu(usbDevice)) {
                if(cdu == null) {
                    Console.WriteLine("No device connected");
                } else {
                    Console.WriteLine($"Using {cdu.UsbDevice}");

                    var supportedLamps = cdu.SupportedLamps
                        .OrderBy(led => led.Describe())
                        .ToArray();
                    var leftLamps = new List<CduLamp>();
                    var rightLamps = new List<CduLamp>();

                    for(var idx = 0;idx < supportedLamps.Length;++idx) {
                        var lamp = supportedLamps[idx];
                        var list = idx < 6 ? leftLamps : rightLamps;
                        list.Add(lamp);
                    }

                    cdu.Output
                        .Small()
                        .Grey()
                        .RightLabel(5, "BRIGHT -5%<")
                        .RightLabel(6, "BRIGHT +5%<");

                    for(var idx = 0;idx < leftLamps.Count;++idx) {
                        cdu.Output.LeftLabel(idx + 1, $">{leftLamps[idx].Describe()}");
                    }
                    for(var idx = 0;idx < rightLamps.Count;++idx) {
                        cdu.Output.RightLabel(idx + 1, $">{rightLamps[idx].Describe()}");
                    }

                    cdu.RefreshDisplay();

                    cdu.KeyDown += (_, args) => {
                        var lsNumber = args.Key.ToLineSelectNumber();
                        if(lsNumber.Number != -1) {
                            var list = lsNumber.IsLeft ? leftLamps : rightLamps;
                            var idx = lsNumber.Number - 1;
                            if(idx < list.Count) {
                                var lamp = list[idx];
                                cdu.Lamps.SetLamp(
                                    lamp,
                                    !cdu.Lamps.GetLamp(lamp)
                                );
                            }

                            if(!lsNumber.IsLeft) {
                                switch(lsNumber.Number) {
                                    case 5: cdu.LampBrightnessPercent = Math.Max(0, cdu.LampBrightnessPercent - 5); break;
                                    case 6: cdu.LampBrightnessPercent = Math.Min(100, cdu.LampBrightnessPercent + 5); break;
                                }
                            }

                            cdu.RefreshLamps();
                        }
                    };

                    Console.WriteLine($"Press Q to quit");
                    while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                    cdu.Cleanup();
                }
            }
        }

        static UsbDevice? SelectDevice()
        {
            var usbDevices = DeviceFactory
                .FindLocalDevices()
                .OrderBy(r => r.Id.VendorId)
                .ThenBy(r => r.Id.ProductId)
                .ToArray();
            var result = usbDevices.FirstOrDefault();
            if(usbDevices.Length > 1) {
                Console.WriteLine("Select device:");
                for(var idx = 0;idx < usbDevices.Length;++idx) {
                    Console.WriteLine($"{idx + 1}: {usbDevices[idx]}");
                }
                do {
                    result = null;
                    Console.Write("? ");
                    var number = Console.ReadLine();
                    if(int.TryParse(number, out var idx) && idx > 0 && idx <= usbDevices.Length) {
                        result = usbDevices[idx - 1];
                    }
                } while(result == null);
            }

            return result;
        }
    }
}
