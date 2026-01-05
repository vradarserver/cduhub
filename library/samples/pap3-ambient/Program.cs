// Copyright © 2025 onwards, Laurent Andre
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;
using System.Threading;
using WwDevicesDotNet;
using WwDevicesDotNet.WinWing.Pap3;

namespace Pap3AmbientTest
{
    /// <summary>
    /// Sample program to test PAP-3 ambient light sensor functionality.
    /// This demonstrates how to read and respond to ambient light sensor changes
    /// on the WinWing PAP-3 Primary Autopilot Panel.
    /// </summary>
    class Program
    {
        static void Main(string[] _)
        {
            Console.WriteLine("=== WinWing PAP-3 Ambient Light Sensor Test ===");
            Console.WriteLine();

            // Find and connect to PAP-3 device
            var deviceId = FrontpanelFactory.FindLocalDevices()
                .FirstOrDefault(d => d.Device == Device.WinWingPap3);

            if (deviceId == null)
            {
                Console.WriteLine("ERROR: No PAP-3 device found!");
                Console.WriteLine("Please ensure your PAP-3 is connected.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            using (var pap3 = FrontpanelFactory.ConnectLocal(deviceId) as Pap3Device)
            {
                if (pap3 == null)
                {
                    Console.WriteLine("ERROR: Failed to connect to PAP-3!");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Connected to: {pap3.DeviceId.Description}");
                Console.WriteLine();
                Console.WriteLine("The PAP-3 has ambient light sensors that detect surrounding light levels.");
                Console.WriteLine("Cover the sensors or change lighting to see the values change.");
                Console.WriteLine();
                Console.WriteLine("Press Q to quit");
                Console.WriteLine();

                // Set up event handlers for ambient light changes
                 pap3.AmbientLightChanged += (sender, args) =>
                 {
                     Console.WriteLine($"[EVENT] Ambient light percent changed: {pap3.AmbientLightPercent}%");
                 };

                // Turn on all LEDs and set displays to visualize brightness changes
                var leds = new Pap3Leds
                {
                    N1 = true,
                    Speed = true,
                    Vnav = true,
                    LvlChg = true,
                    HdgSel = true,
                    Lnav = true,
                    VorLoc = true,
                    App = true,
                    AltHold = true,
                    Vs = true,
                    CmdA = true,
                    CwsA = true,
                    CmdB = true,
                    CwsB = true,
                    AtArm = true,
                    FdL = true,
                    FdR = true
                };

                var state = new Pap3State
                {
                    Speed = 888,
                    PltCourse = 888,
                    CplCourse = 888,
                    Heading = 888,
                    Altitude = 88888,
                    VerticalSpeed = 8888
                };

                pap3.UpdateLeds(leds);
                pap3.UpdateDisplay(state);

                // Display current sensor values continuously
                Console.WriteLine("Real-time sensor readings:");
                Console.WriteLine("─────────────────────────────────────────────────────────");
                Console.WriteLine();

                 var lastPercent = -1;

                 bool running = true;
                 while (running)
                 {
                    // Check for quit key (non-blocking)
                    if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
                    {
                        running = false;
                        break;
                    }

                     // Display sensor values if they changed
                    if (pap3.AmbientLightPercent != lastPercent)
                     {
                         lastPercent = pap3.AmbientLightPercent;

                         // Move cursor to display position
                         Console.SetCursorPosition(0, Console.CursorTop);
                         Console.Write($"Ambient:      {lastPercent,3}%         ");
                         Console.WriteLine();

                         // Visual bar graph
                         Console.Write("Light Level:  ");
                         var barLength = (int)(lastPercent * 0.5); // 50 chars max
                         Console.Write(new string('█', barLength));
                         Console.Write(new string('░', 50 - barLength));
                         Console.WriteLine("  ");

                         // Move cursor back up
                         Console.SetCursorPosition(0, Console.CursorTop - 2);
                     }

                     Thread.Sleep(100);
                 }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Cleaning up...");

                // Turn off everything
                leds.N1 = false;
                leds.Speed = false;
                leds.Vnav = false;
                leds.LvlChg = false;
                leds.HdgSel = false;
                leds.Lnav = false;
                leds.VorLoc = false;
                leds.App = false;
                leds.AltHold = false;
                leds.Vs = false;
                leds.CmdA = false;
                leds.CwsA = false;
                leds.CmdB = false;
                leds.CwsB = false;
                leds.AtArm = false;
                leds.FdL = false;
                leds.FdR = false;

                state.Speed = null;
                state.PltCourse = null;
                state.CplCourse = null;
                state.Heading = null;
                state.Altitude = null;
                state.VerticalSpeed = null;

                pap3.UpdateLeds(leds);
                pap3.UpdateDisplay(state);
                pap3.SetBrightness(255, 255, 255);
            }

            Console.WriteLine("Done!");
        }
    }
}
