// Copyright © 2025 onwards, Andrew Whewell, Laurent Andre
// All rights reserved//
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using WwDevicesDotNet;
using WwDevicesDotNet.WinWing.Pap3;
using System.Timers;

namespace Pap3LedsTest
{
    /// <summary>
    /// Sample program to test PAP-3 LEDs.
    /// This demonstrates how to control all LEDs on the WinWing PAP-3 Primary Autopilot Panel.
    /// Uses a timer-based refresh approach to regularly update the device.
    /// </summary>
    class Program
    {
        static Pap3Leds _leds = new Pap3Leds();
        static IFrontpanel? _pap3;
        static System.Timers.Timer? _refreshTimer;
        const int RefreshIntervalMs = 250;

        static void Main(string[] _)
        {
            Console.WriteLine("=== WinWing PAP-3 LED Test ===");
            Console.WriteLine();

            // Find and connect to PAP-3 device
            var deviceId = FrontpanelFactory.FindLocalDevices()
                .FirstOrDefault(d => d.Device == Device.WinWingPap3);

            if(deviceId == null) {
                Console.WriteLine("ERROR: No PAP-3 device found!");
                Console.WriteLine("Please ensure your PAP-3 is connected.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            using(_pap3 = FrontpanelFactory.ConnectLocal(deviceId)) {
                if(_pap3 == null) {
                    Console.WriteLine("ERROR: Failed to connect to PAP-3!");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Connected to: {_pap3.DeviceId.Description}");
                Console.WriteLine($"Refresh interval: {RefreshIntervalMs}ms");
                Console.WriteLine();

                // Set up event handlers
                _pap3.ControlActivated += OnControlActivated;
                _pap3.ControlDeactivated += OnControlDeactivated;
                _pap3.Disconnected += OnDisconnected;

                // Start refresh timer
                _refreshTimer = new System.Timers.Timer(RefreshIntervalMs);
                _refreshTimer.Elapsed += OnRefreshTimer;
                _refreshTimer.AutoReset = true;
                _refreshTimer.Start();

                // Display menu
                ShowMenu();

                // Main loop
                bool running = true;
                while(running) {
                    var key = Console.ReadKey(intercept: true);
                    
                    switch(key.Key) {
                        case ConsoleKey.Q:
                            running = false;
                            break;
                        case ConsoleKey.D1:
                            TestAllLedsSequence();
                            break;
                        case ConsoleKey.D0:
                            TurnOffAllLeds();
                            break;
                        case ConsoleKey.B:
                            TestBrightness();
                            break;
                        case ConsoleKey.H:
                            ShowMenu();
                            break;
                        default:
                            Console.WriteLine($"Unknown key: {key.Key}");
                            break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Cleaning up...");
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                TurnOffAllLeds();
                _pap3.UpdateLeds(_leds); // Final update
            }

            Console.WriteLine("Done!");
        }

        static void OnRefreshTimer(object? sender, ElapsedEventArgs e)
        {
            // Regularly refresh the LED state
            // This ensures the hardware stays in sync with the model
            _pap3?.UpdateLeds(_leds);
        }

        static void ShowMenu()
        {
            Console.WriteLine("=== PAP-3 LED Test Menu ===");
            Console.WriteLine();
            Console.WriteLine("Tests:");
            Console.WriteLine("  1 - Test all LEDs in sequence");
            Console.WriteLine();
            Console.WriteLine("Controls:");
            Console.WriteLine("  0 - Turn off all LEDs");
            Console.WriteLine("  B - Test brightness levels (Backlight, Digital Tube, Marker Light)");
            Console.WriteLine("  H - Show this menu");
            Console.WriteLine("  Q - Quit");
            Console.WriteLine();
            Console.WriteLine("Interactive Mode:");
            Console.WriteLine("  Press any physical button on the PAP-3 to toggle its LED!");
            Console.WriteLine("  Supported: N1, SPEED, LNAV, VNAV, LVL CHG, HDG SEL, VOR LOC,");
            Console.WriteLine("            APP, ALT HOLD, V/S, CMD A, CMD B, CWS A, CWS B,");
            Console.WriteLine("            AT ARM, FD Pilot (L), FD Copilot (R)");
            Console.WriteLine();
        }

        static void TestAllLedsSequence()
        {
            Console.WriteLine();
            Console.WriteLine("Testing all LEDs in sequence...");
            TurnOffAllLeds();

            var ledTests = new Dictionary<string, Action<bool>>
            {
                { "N1", val => _leds.N1 = val },
                { "SPEED", val => _leds.Speed = val },
                { "VNAV", val => _leds.Vnav = val },
                { "LVL CHG", val => _leds.LvlChg = val },
                { "HDG SEL", val => _leds.HdgSel = val },
                { "LNAV", val => _leds.Lnav = val },
                { "VOR LOC", val => _leds.VorLoc = val },
                { "APP", val => _leds.App = val },
                { "ALT HOLD", val => _leds.AltHold = val },
                { "V/S", val => _leds.Vs = val },
                { "CMD A", val => _leds.CmdA = val },
                { "CWS A", val => _leds.CwsA = val },
                { "CMD B", val => _leds.CmdB = val },
                { "CWS B", val => _leds.CwsB = val },
                { "AT ARM", val => _leds.AtArm = val },
                { "FD L", val => _leds.FdL = val },
                { "FD R", val => _leds.FdR = val }
            };

            foreach(var test in ledTests) {
                Console.Write($"  {test.Key}...");
                test.Value(true);
                Thread.Sleep(300);
                test.Value(false);
                Console.WriteLine(" OK");
            }

            Console.WriteLine("All LEDs tested!");
            Console.WriteLine();
        }

        static void TestBrightness()
        {
            Console.WriteLine();
            Console.WriteLine("Testing brightness levels...");
            Console.WriteLine();
            Console.WriteLine("  Turning on all LEDs...");
            
            // Turn on all LEDs
            _leds.N1 = true;
            _leds.Speed = true;
            _leds.Vnav = true;
            _leds.LvlChg = true;
            _leds.HdgSel = true;
            _leds.Lnav = true;
            _leds.VorLoc = true;
            _leds.App = true;
            _leds.AltHold = true;
            _leds.Vs = true;
            _leds.CmdA = true;
            _leds.CwsA = true;
            _leds.CmdB = true;
            _leds.CwsB = true;
            _leds.AtArm = true;
            _leds.FdL = true;
            _leds.FdR = true;
            Thread.Sleep(500);

            // Test brightness levels for each component
            byte[] brightnessLevels = { 50, 100, 150, 200, 255, 200, 150, 100, 50 };
            
            Console.WriteLine();
            Console.WriteLine("Testing Panel Backlight:");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Panel: {brightness}");
                _pap3?.SetBrightness(brightness, 00, 000);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Testing Digital Tube Backlight (LCD displays):");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Digital Tube: {brightness}");
                _pap3?.SetBrightness(00, brightness, 00);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Testing Marker Light (LEDs):");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Marker Light: {brightness}");
                _pap3?.SetBrightness(000, 00, brightness);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("  Restoring default brightness (all 255)");
            _pap3?.SetBrightness(255, 255, 255);
            
            TurnOffAllLeds();
            Console.WriteLine();
        }

        static void TurnOffAllLeds()
        {
            _leds.N1 = false;
            _leds.Speed = false;
            _leds.Vnav = false;
            _leds.LvlChg = false;
            _leds.HdgSel = false;
            _leds.Lnav = false;
            _leds.VorLoc = false;
            _leds.App = false;
            _leds.AltHold = false;
            _leds.Vs = false;
            _leds.CmdA = false;
            _leds.CwsA = false;
            _leds.CmdB = false;
            _leds.CwsB = false;
            _leds.AtArm = false;
            _leds.FdL = false;
            _leds.FdR = false;
        }

        static void OnControlActivated(object? sender, FrontpanelEventArgs e)
        {
            Console.WriteLine($"[EVENT] Control activated: {e.ControlId}");
            
            // Toggle LED state based on which control was pressed
            switch(e.ControlId) {
                // Autothrottle buttons
                case "N1":
                    _leds.N1 = !_leds.N1;
                    Console.WriteLine($"        N1 LED: {(_leds.N1 ? "ON" : "OFF")}");
                    break;
                case "Speed":
                    _leds.Speed = !_leds.Speed;
                    Console.WriteLine($"        SPEED LED: {(_leds.Speed ? "ON" : "OFF")}");
                    break;
                case "ATArmOn":
                case "ATArmOff":
                    _leds.AtArm = !_leds.AtArm;
                    Console.WriteLine($"        AT ARM LED: {(_leds.AtArm ? "ON" : "OFF")}");
                    break;

                // Flight Director buttons (Pilot and Copilot)
                // Note: These use switch position logic (not toggle)
                case "PltFdOn":
                    _leds.FdL = true;   // Switch ON → LED ON
                    Console.WriteLine($"        FD L (Pilot) LED: ON (switch ON)");
                    break;
                case "PltFdOff":
                    _leds.FdL = false;  // Switch OFF → LED OFF
                    Console.WriteLine($"        FD L (Pilot) LED: OFF (switch OFF)");
                    break;
                case "CplFdOn":
                    _leds.FdR = true;   // Switch ON → LED ON
                    Console.WriteLine($"        FD R (Copilot) LED: ON (switch ON)");
                    break;
                case "CplFdOff":
                    _leds.FdR = false;  // Switch OFF → LED OFF
                    Console.WriteLine($"        FD R (Copilot) LED: OFF (switch OFF)");
                    break;

                // Autopilot mode buttons
                case "Lnav":
                    _leds.Lnav = !_leds.Lnav;
                    Console.WriteLine($"        LNAV LED: {(_leds.Lnav ? "ON" : "OFF")}");
                    break;
                case "Vnav":
                    _leds.Vnav = !_leds.Vnav;
                    Console.WriteLine($"        VNAV LED: {(_leds.Vnav ? "ON" : "OFF")}");
                    break;
                case "LvlChg":
                    _leds.LvlChg = !_leds.LvlChg;
                    Console.WriteLine($"        LVL CHG LED: {(_leds.LvlChg ? "ON" : "OFF")}");
                    break;
                case "HdgSel":
                    _leds.HdgSel = !_leds.HdgSel;
                    Console.WriteLine($"        HDG SEL LED: {(_leds.HdgSel ? "ON" : "OFF")}");
                    break;
                case "VorLoc":
                    _leds.VorLoc = !_leds.VorLoc;
                    Console.WriteLine($"        VOR LOC LED: {(_leds.VorLoc ? "ON" : "OFF")}");
                    break;
                case "App":
                    _leds.App = !_leds.App;
                    Console.WriteLine($"        APP LED: {(_leds.App ? "ON" : "OFF")}");
                    break;
                case "AltHold":
                    _leds.AltHold = !_leds.AltHold;
                    Console.WriteLine($"        ALT HOLD LED: {(_leds.AltHold ? "ON" : "OFF")}");
                    break;
                case "Vs":
                    _leds.Vs = !_leds.Vs;
                    Console.WriteLine($"        V/S LED: {(_leds.Vs ? "ON" : "OFF")}");
                    break;

                // Autopilot command buttons
                case "CmdA":
                    _leds.CmdA = !_leds.CmdA;
                    Console.WriteLine($"        CMD A LED: {(_leds.CmdA ? "ON" : "OFF")}");
                    break;
                case "CmdB":
                    _leds.CmdB = !_leds.CmdB;
                    Console.WriteLine($"        CMD B LED: {(_leds.CmdB ? "ON" : "OFF")}");
                    break;
                case "CwsA":
                    _leds.CwsA = !_leds.CwsA;
                    Console.WriteLine($"        CWS A LED: {(_leds.CwsA ? "ON" : "OFF")}");
                    break;
                case "CwsB":
                    _leds.CwsB = !_leds.CwsB;
                    Console.WriteLine($"        CWS B LED: {(_leds.CwsB ? "ON" : "OFF")}");
                    break;

                default:
                    // Button doesn't have an associated LED
                    Console.WriteLine($"        (No LED for {e.ControlId})");
                    break;
            }
        }

        static void OnControlDeactivated(object? sender, FrontpanelEventArgs e)
        {
            Console.WriteLine($"[EVENT] Control deactivated: {e.ControlId}");
        }

        static void OnDisconnected(object? sender, EventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("WARNING: PAP-3 device disconnected!");
            Console.WriteLine();
        }
    }
}
