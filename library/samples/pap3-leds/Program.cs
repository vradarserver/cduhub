// Copyright © 2025 onwards, Laurent Andre
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#nullable enable

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
        static readonly Pap3Leds _Leds = new Pap3Leds();
        static readonly Pap3State _DisplayState = new Pap3State();
        static IFrontpanel? _Pap3;
        static System.Timers.Timer? _RefreshTimer;
        const int _RefreshIntervalMs = 250;

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

            using(_Pap3 = FrontpanelFactory.ConnectLocal(deviceId)) {
                if(_Pap3 == null) {
                    Console.WriteLine("ERROR: Failed to connect to PAP-3!");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Connected to: {_Pap3.DeviceId.Description}");
                Console.WriteLine($"Refresh interval: {_RefreshIntervalMs}ms");
                Console.WriteLine();

                // Set up event handlers
                _Pap3.ControlActivated += OnControlActivated;
                _Pap3.ControlDeactivated += OnControlDeactivated;
                _Pap3.Disconnected += OnDisconnected;

                // Start refresh timer
                _RefreshTimer = new System.Timers.Timer(_RefreshIntervalMs);
                _RefreshTimer.Elapsed += OnRefreshTimer;
                _RefreshTimer.AutoReset = true;
                _RefreshTimer.Start();

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
                        case ConsoleKey.D:
                            TestAllDisplays();
                            break;
                        case ConsoleKey.S:
                            TestSpeedDisplay();
                            break;
                        case ConsoleKey.C:
                            TestCourseDisplays();
                            break;
                        case ConsoleKey.G:
                            TestHeadingDisplay();
                            break;
                        case ConsoleKey.A:
                            TestAltitudeDisplay();
                            break;
                        case ConsoleKey.V:
                            TestVerticalSpeedDisplay();
                            break;
                        case ConsoleKey.I:
                            TestIndicatorsSequence();
                            break;
                        case ConsoleKey.X:
                            ClearAllDisplays();
                            _Pap3?.UpdateDisplay(_DisplayState);
                            Console.WriteLine("All displays cleared.");
                            break;
                        case ConsoleKey.M:
                            ShowMenu();
                            break;
                        default:
                            Console.WriteLine($"Unknown key: {key.Key}");
                            break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Cleaning up...");
                _RefreshTimer?.Stop();
                _RefreshTimer?.Dispose();
                TurnOffAllLeds();
                if(_Pap3 != null)
                {
                    _Pap3.UpdateLeds(_Leds); // Final update
                }
            }

            Console.WriteLine("Done!");
        }

        static void OnRefreshTimer(object? sender, ElapsedEventArgs e)
        {
            // Regularly refresh the LED state
            // This ensures the hardware stays in sync with the model
            _Pap3?.UpdateLeds(_Leds);
        }

        static void ShowMenu()
        {
            Console.WriteLine("=== PAP-3 LED & Display Test Menu ===");
            Console.WriteLine();
            Console.WriteLine("LED Tests:");
            Console.WriteLine("  1 - Test all LEDs in sequence");
            Console.WriteLine("  0 - Turn off all LEDs");
            Console.WriteLine("  B - Test brightness levels");
            Console.WriteLine();
            Console.WriteLine("Display Tests:");
            Console.WriteLine("  D - Test all displays (count 0-999 on all displays)");
            Console.WriteLine("  S - Test Speed display (IAS/MACH with indicators)");
            Console.WriteLine("  C - Test Course displays (PLT & CPL)");
            Console.WriteLine("  G - Test Heading display (HDG/TRK)");
            Console.WriteLine("  A - Test Altitude display");
            Console.WriteLine("  V - Test Vertical Speed display (V/S/FPA)");
            Console.WriteLine("  I - Test all indicators sequence");
            Console.WriteLine("  X - Clear all displays");
            Console.WriteLine();
            Console.WriteLine("Other:");
            Console.WriteLine("  M - Show this menu");
            Console.WriteLine("  Q - Quit");
            Console.WriteLine();
            Console.WriteLine("Interactive Mode:");
            Console.WriteLine("  Press any physical button on the PAP-3 to toggle its LED!");
            Console.WriteLine();
        }

        static void TestAllLedsSequence()
        {
            Console.WriteLine();
            Console.WriteLine("Testing all LEDs in sequence...");
            TurnOffAllLeds();

            var ledTests = new Dictionary<string, Action<bool>>
            {
                { "N1", val => _Leds.N1 = val },
                { "SPEED", val => _Leds.Speed = val },
                { "VNAV", val => _Leds.Vnav = val },
                { "LVL CHG", val => _Leds.LvlChg = val },
                { "HDG SEL", val => _Leds.HdgSel = val },
                { "LNAV", val => _Leds.Lnav = val },
                { "VOR LOC", val => _Leds.VorLoc = val },
                { "APP", val => _Leds.App = val },
                { "ALT HOLD", val => _Leds.AltHold = val },
                { "V/S", val => _Leds.Vs = val },
                { "CMD A", val => _Leds.CmdA = val },
                { "CWS A", val => _Leds.CwsA = val },
                { "CMD B", val => _Leds.CmdB = val },
                { "CWS B", val => _Leds.CwsB = val },
                { "AT ARM", val => _Leds.AtArm = val },
                { "FD L", val => _Leds.FdL = val },
                { "FD R", val => _Leds.FdR = val }
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
            _Leds.N1 = true;
            _Leds.Speed = true;
            _Leds.Vnav = true;
            _Leds.LvlChg = true;
            _Leds.HdgSel = true;
            _Leds.Lnav = true;
            _Leds.VorLoc = true;
            _Leds.App = true;
            _Leds.AltHold = true;
            _Leds.Vs = true;
            _Leds.CmdA = true;
            _Leds.CwsA = true;
            _Leds.CmdB = true;
            _Leds.CwsB = true;
            _Leds.AtArm = true;
            _Leds.FdL = true;
            _Leds.FdR = true;
            Thread.Sleep(500);

            // Test brightness levels for each component
            byte[] brightnessLevels = { 50, 100, 150, 200, 255, 200, 150, 100, 50 };
            
            Console.WriteLine();
            Console.WriteLine("Testing Panel Backlight:");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Panel: {brightness}");
                _Pap3?.SetBrightness(brightness, 0, 0);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Testing Digital Tube Backlight (LCD displays):");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Digital Tube: {brightness}");
                _Pap3?.SetBrightness(0, brightness, 0);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Testing Marker Light (LEDs):");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Marker Light: {brightness}");
                _Pap3?.SetBrightness(0, 0, brightness);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("  Restoring default brightness (all 255)");
            _Pap3?.SetBrightness(255, 255, 255);
            
            TurnOffAllLeds();
            Console.WriteLine();
        }

        static void TestSpeedDisplay()
        {
            Console.WriteLine();
            Console.WriteLine("=== Speed Display Test (PLT Course) ===");
            Console.WriteLine();
            Console.WriteLine("Testing IAS mode (0-999 knots)...");
            
            _DisplayState.SpeedIsMach = false;
            
            // Count up from 0 to 999
            for(int speed = 0; speed <= 999; speed += 10) {
                _DisplayState.Speed = speed;
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rSpeed: {speed:000} IAS");
                Thread.Sleep(50);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Testing MACH mode (0.00 - 0.99)...");
            
            _DisplayState.SpeedIsMach = true;
            
            // MACH values: displayed as 000-099 representing 0.00 to 0.99
            for(int mach = 0; mach <= 99; mach++) {
                _DisplayState.Speed = mach;
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rSpeed: 0.{mach:00} MACH");
                Thread.Sleep(50);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Test complete! Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            ClearAllDisplays();
            _Pap3?.UpdateDisplay(_DisplayState);
        }

        static void TestCourseDisplays()
        {
            Console.WriteLine();
            Console.WriteLine("=== Course Displays Test (PLT & CPL) ===");
            Console.WriteLine();
            Console.WriteLine("Testing synchronized course displays (0-359 degrees)...");
            
            // Test both course displays simultaneously
            for(int course = 0; course <= 359; course += 5) {
                _DisplayState.PltCourse = course;
                _DisplayState.CplCourse = (course + 180) % 360; // CPL Course (offset)
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rPLT: {course:000}°  CPL: {_DisplayState.CplCourse:000}°");
                Thread.Sleep(50);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Test complete! Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            ClearAllDisplays();
            _Pap3?.UpdateDisplay(_DisplayState);
        }

        static void TestHeadingDisplay()
        {
            Console.WriteLine();
            Console.WriteLine("=== Heading Display Test ===");
            Console.WriteLine();
            Console.WriteLine("Testing HDG mode (0-359 degrees)...");
            
            _DisplayState.HeadingIsTrack = false;
            
            // Count from 0 to 359
            for(int heading = 0; heading <= 359; heading += 5) {
                _DisplayState.Heading = heading;
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rHeading: {heading:000}° HDG");
                Thread.Sleep(50);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Testing TRK mode (0-359 degrees)...");
            
            _DisplayState.HeadingIsTrack = true;
            
            for(int heading = 0; heading <= 359; heading += 5) {
                _DisplayState.Heading = heading;
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rHeading: {heading:000}° TRK");
                Thread.Sleep(50);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Test complete! Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            ClearAllDisplays();
            _Pap3?.UpdateDisplay(_DisplayState);
        }

        static void TestAltitudeDisplay()
        {
            Console.WriteLine();
            Console.WriteLine("=== Altitude Display Test ===");
            Console.WriteLine();
            Console.WriteLine("Testing altitude (0 - 50,000 feet)...");
            
            // Count from 0 to 50,000 in increments
            for(int altitude = 0; altitude <= 50000; altitude += 500) {
                _DisplayState.Altitude = altitude;
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rAltitude: {altitude:00000} ft");
                Thread.Sleep(30);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Testing flight level mode...");
            
            _DisplayState.AltitudeIsFlightLevel = true;
            
            // Count from FL000 to FL500
            for(int fl = 0; fl <= 500; fl += 10) {
                _DisplayState.Altitude = fl * 100; // FL is in hundreds of feet
                _Pap3?.UpdateDisplay(_DisplayState);
                Console.Write($"\rFlight Level: FL{fl:000}");
                Thread.Sleep(30);
            }
            
            _DisplayState.AltitudeIsFlightLevel = false;
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Test complete! Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            ClearAllDisplays();
            _Pap3?.UpdateDisplay(_DisplayState);
        }

        static void TestVerticalSpeedDisplay()
        {
            Console.WriteLine();
            Console.WriteLine("=== Vertical Speed Display Test ===");
            Console.WriteLine();
            Console.WriteLine("Testing V/S mode (-9999 to +9999 fpm)...");
            
            _DisplayState.VsIsFpa = false;
            
            // Count from -9999 to +9999
            for(int vs = -9999; vs <= 9999; vs += 200) {
                _DisplayState.VerticalSpeed = vs;
                _Pap3?.UpdateDisplay(_DisplayState);
                string sign = vs >= 0 ? "+" : "";
                Console.Write($"\rVertical Speed: {sign}{vs:0000} fpm V/S");
                Thread.Sleep(20);
            }
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Testing FPA mode (-9.9 to +9.9 degrees)...");
            
            _DisplayState.VsIsFpa = true;
            
            // FPA values: typically -9.9 to +9.9 degrees, displayed as -099 to +099
            for(int fpa = -99; fpa <= 99; fpa += 5) {
                _DisplayState.VerticalSpeed = fpa * 10; // Scale for display
                _Pap3?.UpdateDisplay(_DisplayState);
                string sign = fpa >= 0 ? "+" : "";
                double fpaValue = fpa / 10.0;
                Console.Write($"\rFlight Path Angle: {sign}{fpaValue:0.0}° FPA");
                Thread.Sleep(30);
            }
            
            _DisplayState.VsIsFpa = false;
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Test complete! Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            ClearAllDisplays();
            _Pap3?.UpdateDisplay(_DisplayState);
        }

        static void TestIndicatorsSequence()
        {
            Console.WriteLine();
            Console.WriteLine("=== All Indicators Sequence Test ===");
            Console.WriteLine();
            Console.WriteLine("This test cycles through all indicators to verify they work correctly.");
            Console.WriteLine("Watch the physical PAP-3 panel to confirm each indicator lights up.");
            Console.WriteLine();
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(intercept: true);
            Console.WriteLine();

            // Test Speed indicators
            Console.WriteLine("Testing IAS indicator...");
            _DisplayState.Speed = 250;
            _DisplayState.SpeedIsMach = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);

            Console.WriteLine("Testing MACH indicator...");
            _DisplayState.Speed = 82;
            _DisplayState.SpeedIsMach = true;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);

            _DisplayState.Speed = null;
            _DisplayState.SpeedIsMach = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(500);

            // Test Heading indicators
            Console.WriteLine("Testing HDG indicator...");
            _DisplayState.Heading = 270;
            _DisplayState.HeadingIsTrack = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);

            Console.WriteLine("Testing TRK indicator...");
            _DisplayState.Heading = 270;
            _DisplayState.HeadingIsTrack = true;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);

            _DisplayState.Heading = null;
            _DisplayState.HeadingIsTrack = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(500);

            // Test V/S indicators
            Console.WriteLine("Testing V/S indicator...");
            _DisplayState.VerticalSpeed = 2000;
            _DisplayState.VsIsFpa = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);
            _DisplayState.VerticalSpeed = 2000;
            _DisplayState.VsIsFpa = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);

            Console.WriteLine("Testing FPA indicator...");
            _DisplayState.VerticalSpeed = 30;
            _DisplayState.VsIsFpa = true;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(2000);

            _DisplayState.VerticalSpeed = null;
            _DisplayState.VsIsFpa = false;
            _Pap3?.UpdateDisplay(_DisplayState);
            Thread.Sleep(500);

            // Rapid cycling test
            Console.WriteLine();
            Console.WriteLine("Rapid cycling through all indicators (3 cycles)...");
            for(int cycle = 0; cycle < 3; cycle++) {
                Console.WriteLine($"Cycle {cycle + 1}/3");
                
                _DisplayState.Speed = 250;
                _DisplayState.SpeedIsMach = false;
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(400);

                _DisplayState.Speed = 82;
                _DisplayState.SpeedIsMach = true;
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(400);

                _DisplayState.Speed = null;
                _DisplayState.Heading = 270;
                _DisplayState.HeadingIsTrack = false;
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(400);

                _DisplayState.HeadingIsTrack = true;
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(400);

                _DisplayState.Heading = null;
                _DisplayState.VerticalSpeed = 2000;
                _DisplayState.VsIsFpa = false;
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(400);

                _DisplayState.VsIsFpa = true;
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(400);

                ClearAllDisplays();
                _Pap3?.UpdateDisplay(_DisplayState);
                Thread.Sleep(200);
            }

            Console.WriteLine();
            Console.WriteLine("Indicator sequence test complete!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        static void TestAllDisplays()
        {
            Console.WriteLine();
            Console.WriteLine("=== All Displays Count Test ===");
            Console.WriteLine();
            Console.WriteLine("Counting 0-999 on all displays simultaneously...");
            Console.WriteLine();

            for(int count = 0; count <= 999; count += 5) {
                _DisplayState.Speed = count;           // PLT Course (IAS)
                _DisplayState.PltCourse = count;          // CPL Course
                _DisplayState.CplCourse = count;          // CPL Course
                _DisplayState.Heading = count % 360;   // Heading (wrap at 360)
                _DisplayState.Altitude = count * 10;   // Altitude (scaled)
                _DisplayState.VerticalSpeed = (count - 500) * 2; // V/S (centered at 500)
                
                _Pap3?.UpdateDisplay(_DisplayState);
                
                Console.Write($"\rCount: {count:000}  |  PLT: {count:000}  CPL: {count:000}  HDG: {count % 360:000}  ALT: {count * 10:00000}  V/S: {(count - 500) * 2:+0000;-0000}");
                Thread.Sleep(30);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Test complete! Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            ClearAllDisplays();
            _Pap3?.UpdateDisplay(_DisplayState);
        }

        static void TurnOffAllLeds()
        {
            _Leds.N1 = false;
            _Leds.Speed = false;
            _Leds.Vnav = false;
            _Leds.LvlChg = false;
            _Leds.HdgSel = false;
            _Leds.Lnav = false;
            _Leds.VorLoc = false;
            _Leds.App = false;
            _Leds.AltHold = false;
            _Leds.Vs = false;
            _Leds.CmdA = false;
            _Leds.CwsA = false;
            _Leds.CmdB = false;
            _Leds.CwsB = false;
            _Leds.AtArm = false;
            _Leds.FdL = false;
            _Leds.FdR = false;
        }

        static void ClearAllDisplays()
        {
            _DisplayState.Speed = null;
            _DisplayState.PltCourse = null;
            _DisplayState.CplCourse = null;
            _DisplayState.Heading = null;
            _DisplayState.Altitude = null;
            _DisplayState.VerticalSpeed = null;
            _DisplayState.SpeedIsMach = false;
            _DisplayState.HeadingIsTrack = false;
            _DisplayState.VsIsFpa = false;
            _DisplayState.AltitudeIsFlightLevel = false;
        }

        static void OnControlActivated(object? sender, FrontpanelEventArgs e)
        {
            Console.WriteLine($"[EVENT] Control activated: {e.ControlId}");
            
            // Toggle LED state based on which control was pressed
            switch(e.ControlId) {
                // Autothrottle buttons
                case "N1":
                    _Leds.N1 = !_Leds.N1;
                    Console.WriteLine($"        N1 LED: {(_Leds.N1 ? "ON" : "OFF")}");
                    break;
                case "Speed":
                    _Leds.Speed = !_Leds.Speed;
                    Console.WriteLine($"        SPEED LED: {(_Leds.Speed ? "ON" : "OFF")}");
                    break;
                case "ATArmOn":
                    _Leds.AtArm = true;   // Switch ON → LED ON
                    Console.WriteLine($"        AT ARM LED: ON (switch ON)");
                    break;
                case "ATArmOff":
                    _Leds.AtArm = false;  // Switch OFF → LED OFF
                    Console.WriteLine($"        AT ARM LED: OFF (switch OFF)");
                    break;

                // Flight Director buttons (Pilot and Copilot)
                // Note: These use switch position logic (not toggle)
                case "PltFdOn":
                    _Leds.FdL = true;   // Switch ON → LED ON
                    Console.WriteLine($"        FD L (Pilot) LED: ON (switch ON)");
                    break;
                case "PltFdOff":
                    _Leds.FdL = false;  // Switch OFF → LED OFF
                    Console.WriteLine($"        FD L (Pilot) LED: OFF (switch OFF)");
                    break;
                case "CplFdOn":
                    _Leds.FdR = true;   // Switch ON → LED ON
                    Console.WriteLine($"        FD R (Copilot) LED: ON (switch ON)");
                    break;
                case "CplFdOff":
                    _Leds.FdR = false;  // Switch OFF → LED OFF
                    Console.WriteLine($"        FD R (Copilot) LED: OFF (switch OFF)");
                    break;

                // Autopilot mode buttons
                case "Lnav":
                    _Leds.Lnav = !_Leds.Lnav;
                    Console.WriteLine($"        LNAV LED: {(_Leds.Lnav ? "ON" : "OFF")}");
                    break;
                case "Vnav":
                    _Leds.Vnav = !_Leds.Vnav;
                    Console.WriteLine($"        VNAV LED: {(_Leds.Vnav ? "ON" : "OFF")}");
                    break;
                case "LvlChg":
                    _Leds.LvlChg = !_Leds.LvlChg;
                    Console.WriteLine($"        LVL CHG LED: {(_Leds.LvlChg ? "ON" : "OFF")}");
                    break;
                case "HdgSel":
                    _Leds.HdgSel = !_Leds.HdgSel;
                    Console.WriteLine($"        HDG SEL LED: {(_Leds.HdgSel ? "ON" : "OFF")}");
                    break;
                case "VorLoc":
                    _Leds.VorLoc = !_Leds.VorLoc;
                    Console.WriteLine($"        VOR LOC LED: {(_Leds.VorLoc ? "ON" : "OFF")}");
                    break;
                case "App":
                    _Leds.App = !_Leds.App;
                    Console.WriteLine($"        APP LED: {(_Leds.App ? "ON" : "OFF")}");
                    break;
                case "AltHold":
                    _Leds.AltHold = !_Leds.AltHold;
                    Console.WriteLine($"        ALT HOLD LED: {(_Leds.AltHold ? "ON" : "OFF")}");
                    break;
                case "Vs":
                    _Leds.Vs = !_Leds.Vs;
                    Console.WriteLine($"        V/S LED: {(_Leds.Vs ? "ON" : "OFF")}");
                    break;

                // Autopilot command buttons
                case "CmdA":
                    _Leds.CmdA = !_Leds.CmdA;
                    Console.WriteLine($"        CMD A LED: {(_Leds.CmdA ? "ON" : "OFF")}");
                    break;
                case "CmdB":
                    _Leds.CmdB = !_Leds.CmdB;
                    Console.WriteLine($"        CMD B LED: {(_Leds.CmdB ? "ON" : "OFF")}");
                    break;
                case "CwsA":
                    _Leds.CwsA = !_Leds.CwsA;
                    Console.WriteLine($"        CWS A LED: {(_Leds.CwsA ? "ON" : "OFF")}");
                    break;
                case "CwsB":
                    _Leds.CwsB = !_Leds.CwsB;
                    Console.WriteLine($"        CWS B LED: {(_Leds.CwsB ? "ON" : "OFF")}");
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
