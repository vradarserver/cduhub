// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using WwDevicesDotNet;

namespace Ambient
{
    class Program
    {
        static void Main(string[] _)
        {
            var deviceId = SelectDevice();
            using(var cdu = CduFactory.ConnectLocal(deviceId)) {
                Console.WriteLine($"Using {cdu.DeviceId}");
                cdu.Leds.TurnAllOn(true);
                cdu.RefreshLeds();

                ShowASplashOfColour(cdu);

                cdu.AutoBrightness.Enabled = true;
                cdu.ApplyAutoBrightness();

                cdu.LeftAmbientLightChanged += (_,_) => RefreshDisplay(cdu);
                cdu.RightAmbientLightChanged += (_,_) => RefreshDisplay(cdu);
                cdu.AmbientLightChanged += (_,_) => RefreshDisplay(cdu);
                RefreshDisplay(cdu);

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                    ;
                }

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

        static void RefreshDisplay(ICdu cdu)
        {
            cdu.Output
                .Line(1)
                .ClearRow()
                .Small()
                .Write(cdu.LeftAmbientLightNative.ToString("X4"))
                .RightToLeft()
                .Write(cdu.RightAmbientLightNative.ToString("X4"))
                .LeftToRight()
                .LabelTitleLine(2)
                .ClearRow()
                .Write("K/BRD")
                .Centered("DISP")
                .RightToLeft()
                .Write("LED")
                .LeftToRight()
                .LabelLine(2)
                .ClearRow()
                .Large()
                .Cyan()
                .Write($"{cdu.BacklightBrightnessPercent}%")
                .Centered($"{cdu.DisplayBrightnessPercent}%")
                .RightToLeft()
                .Write($"{cdu.LedBrightnessPercent}%")
                .LeftToRight()
                .White()
                .MiddleLine()
                .ClearRow()
                .Large()
                .Centred($"{cdu.AmbientLightPercent}%");
            cdu.RefreshDisplay();
        }

        private static void ShowASplashOfColour(ICdu cdu)
        {
            var line = 10;
            cdu.Output
                .Line(line++)
                .Centred($"<magenta>MAGENTA <white>WHITE")
                .Line(line++)
                .Centred($"<red>RED <brown>BROWN <khaki>KHAKI <grey>GREY")
                .Line(line++)
                .Centred($"<green>GREEN <yellow>YELLOW <cyan>CYAN <amber>AMBER");
            cdu.RefreshDisplay();
        }
    }
}
