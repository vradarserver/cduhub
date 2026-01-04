// Copyright © 2025 onwards, Andrew Whewell, Laurent Andre
// All rights reserved.
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
        static readonly Pap3Leds _leds = new Pap3Leds();
        static readonly Pap3State _displayState = new Pap3State();
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
                        case ConsoleKey.D2:
                            TestSpeedDisplay();
                            break;
                        case ConsoleKey.D3:
                            SearchIndicators();
                            break;
                        case ConsoleKey.D4:
                            VerifyIndicatorBits();
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
            
            // NOTE: Display refresh is disabled because EncodeDisplays() is not yet implemented.
            // Sending display packets with unencoded data clears the displays.
            // Uncomment this line once display encoding is working:
            // _pap3?.UpdateDisplay(_displayState);
        }

        static void ShowMenu()
        {
            Console.WriteLine("=== PAP-3 LED & Display Test Menu ===");
            Console.WriteLine();
            Console.WriteLine("Tests:");
            Console.WriteLine("  1 - Test all LEDs in sequence");
            Console.WriteLine("  2 - Test indicators in sequence (IAS/MACH, HDG/TRK, V/S/FPA)");
            Console.WriteLine("  3 - Search for indicators (Raw byte/bit testing)");
            Console.WriteLine("  4 - Verify specific indicator bits");
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
                _pap3?.SetBrightness(brightness, 0, 0);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Testing Digital Tube Backlight (LCD displays):");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Digital Tube: {brightness}");
                _pap3?.SetBrightness(0, brightness, 0);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Testing Marker Light (LEDs):");
            foreach(var brightness in brightnessLevels) {
                Console.WriteLine($"  Marker Light: {brightness}");
                _pap3?.SetBrightness(0, 0, brightness);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("  Restoring default brightness (all 255)");
            _pap3?.SetBrightness(255, 255, 255);
            
            TurnOffAllLeds();
            Console.WriteLine();
        }

        static void TestSpeedDisplay()
        {
            Console.WriteLine();
            Console.WriteLine("=== Indicator Sequence Test ===");
            Console.WriteLine();
            Console.WriteLine("This test will toggle all indicators in sequence.");
            Console.WriteLine("Watch the physical PAP-3 panel to verify each indicator.");
            Console.WriteLine();
            Console.WriteLine("Press any key to start, or Q to cancel...");
            
            var key = Console.ReadKey(intercept: true);
            if(key.Key == ConsoleKey.Q) {
                Console.WriteLine("Test cancelled.");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Starting indicator sequence test...");
            Console.WriteLine();

            // Test IAS indicator
            Console.WriteLine("Testing IAS indicator...");
            _displayState.Speed = 1;  // Dummy value to show indicator
            _displayState.SpeedIsMach = false;  // IAS mode
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(2000);

            // Test MACH indicator
            Console.WriteLine("Testing MACH indicator...");
            _displayState.Speed = 1;  // Dummy value to show indicator
            _displayState.SpeedIsMach = true;   // MACH mode
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(2000);

            // Clear speed indicators
            Console.WriteLine("Clearing speed indicators...");
            _displayState.Speed = null;  // Clear value to hide indicators
            _displayState.SpeedIsMach = false;
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(1000);

            // Test HDG indicator
            Console.WriteLine("Testing HDG indicator...");
            _displayState.Heading = 1;  // Dummy value to show indicator
            _displayState.HeadingIsTrack = false;  // HDG mode
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(2000);

            // Test TRK indicator
            Console.WriteLine("Testing TRK indicator...");
            _displayState.Heading = 1;  // Dummy value to show indicator
            _displayState.HeadingIsTrack = true;   // TRK mode
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(2000);

            // Clear heading indicators
            Console.WriteLine("Clearing heading indicators...");
            _displayState.Heading = null;  // Clear value to hide indicators
            _displayState.HeadingIsTrack = false;
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(1000);

            // Test V/S indicator
            Console.WriteLine("Testing V/S indicator...");
            _displayState.VerticalSpeed = 1;  // Dummy value to show indicator
            _displayState.VsIsFpa = false;  // V/S mode
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(2000);

            // Test FPA indicator
            Console.WriteLine("Testing FPA indicator...");
            _displayState.VerticalSpeed = 1;  // Dummy value to show indicator
            _displayState.VsIsFpa = true;   // FPA mode
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(2000);

            // Clear all indicators
            Console.WriteLine("Clearing all indicators...");
            _displayState.VerticalSpeed = null;  // Clear value to hide indicators
            _displayState.VsIsFpa = false;
            _pap3?.UpdateDisplay(_displayState);
            Thread.Sleep(1000);

            // Full sequence test: cycle through all indicators
            Console.WriteLine();
            Console.WriteLine("Full sequence: Cycling through all indicators...");
            Console.WriteLine();

            for(int cycle = 0; cycle < 3; cycle++) {
                Console.WriteLine($"Cycle {cycle + 1}/3:");
                
                Console.WriteLine("  → IAS");
                _displayState.Speed = 1;
                _displayState.SpeedIsMach = false;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(800);

                Console.WriteLine("  → MACH");
                _displayState.Speed = 1;
                _displayState.SpeedIsMach = true;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(800);

                Console.WriteLine("  → HDG");
                _displayState.Speed = null;
                _displayState.Heading = 1;
                _displayState.HeadingIsTrack = false;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(800);

                Console.WriteLine("  → TRK");
                _displayState.Heading = 1;
                _displayState.HeadingIsTrack = true;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(800);

                Console.WriteLine("  → V/S");
                _displayState.Heading = null;
                _displayState.VerticalSpeed = 1;
                _displayState.VsIsFpa = false;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(800);

                Console.WriteLine("  → FPA");
                _displayState.VerticalSpeed = 1;
                _displayState.VsIsFpa = true;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(800);

                Console.WriteLine("  → Clear");
                _displayState.VerticalSpeed = null;
                _displayState.VsIsFpa = false;
                _pap3?.UpdateDisplay(_displayState);
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Indicator sequence test complete!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        static void SearchIndicators()
        {
            Console.WriteLine();
            Console.WriteLine("=== Indicator Search Test (Raw Byte/Bit Testing) ===");
            Console.WriteLine();
            Console.WriteLine("This test sends raw bytes to discover indicator positions.");
            Console.WriteLine("We'll test each byte and bit in the display data area to find:");
            Console.WriteLine("  - IAS/MACH indicator");
            Console.WriteLine("  - HDG/TRK indicator");
            Console.WriteLine("  - V/S/FPA indicator");
            Console.WriteLine();
            Console.WriteLine("Strategy:");
            Console.WriteLine("  1. Test each byte position (0-29) with value 0xFF");
            Console.WriteLine("  2. Note which bytes light up indicators");
            Console.WriteLine("  3. For interesting bytes, test individual bits (0-7)");
            Console.WriteLine();
            Console.WriteLine("Press any key to start, or Q to cancel...");
            
            var key = Console.ReadKey(intercept: true);
            if(key.Key == ConsoleKey.Q) {
                Console.WriteLine("Test cancelled.");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Starting indicator search...");
            Console.WriteLine();

            // Phase 1: Byte-by-byte scan
            Console.WriteLine("=== Phase 1: Scanning bytes 0-29 for indicators ===");
            Console.WriteLine();
            
            var indicatorBytes = new List<int>();
            
            for(int bytePos = 0; bytePos < 30; bytePos++) {
                Console.WriteLine($"Testing byte {bytePos} (offset 0x{(0x1F + bytePos):X2})...");
                
                var rawState = new Pap3StateRaw();
                rawState.RawDisplayData[bytePos] = 0xFF;  // Test this byte
                
                _pap3?.UpdateDisplay(rawState);
                Thread.Sleep(300);
                
                Console.WriteLine($"  Byte {bytePos} = 0xFF");
                Console.WriteLine("  Do you see any indicator light up? (Y/N/S to skip)");
                
                key = Console.ReadKey(intercept: true);
                Console.WriteLine();
                
                if(key.Key == ConsoleKey.Y) {
                    Console.WriteLine($"  ✓ Byte {bytePos} affects an indicator!");
                    indicatorBytes.Add(bytePos);
                    
                    Console.WriteLine("  Which indicator? (1=IAS/MACH, 2=HDG/TRK, 3=V/S/FPA, 0=Other)");
                    key = Console.ReadKey(intercept: true);
                    Console.WriteLine();
                    
                    string indicatorName = key.Key switch {
                        ConsoleKey.D1 => "IAS/MACH",
                        ConsoleKey.D2 => "HDG/TRK",
                        ConsoleKey.D3 => "V/S/FPA",
                        _ => "Unknown"
                    };
                    Console.WriteLine($"  → Marked as {indicatorName} indicator");
                } else if(key.Key == ConsoleKey.S) {
                    Console.WriteLine("  Skipping remaining bytes...");
                    break;
                }
                
                Console.WriteLine();
            }

            // Phase 2: Bit-by-bit scan for identified bytes
            if(indicatorBytes.Count > 0) {
                Console.WriteLine();
                Console.WriteLine($"=== Phase 2: Testing individual bits for {indicatorBytes.Count} interesting byte(s) ===");
                Console.WriteLine();
                
                foreach(var bytePos in indicatorBytes) {
                    Console.WriteLine($"Testing bits in byte {bytePos} (offset 0x{(0x1F + bytePos):X2})");
                    Console.WriteLine();
                    
                    for(int bitPos = 0; bitPos < 8; bitPos++) {
                        byte bitMask = (byte)(1 << bitPos);
                        
                        Console.WriteLine($"  Bit {bitPos} (mask 0x{bitMask:X2})");
                        
                        var rawState = new Pap3StateRaw();
                        rawState.RawDisplayData[bytePos] = bitMask;
                        
                        _pap3?.UpdateDisplay(rawState);
                        Thread.Sleep(300);
                        
                        Console.WriteLine($"    Does an indicator light up? (Y/N/S to skip this byte)");
                        key = Console.ReadKey(intercept: true);
                        Console.WriteLine();
                        
                        if(key.Key == ConsoleKey.Y) {
                            Console.WriteLine($"    ✓ Byte {bytePos}, bit {bitPos} (0x{bitMask:X2}) lights an indicator!");
                            Console.WriteLine($"      In code: buffer[0x{(0x1F + bytePos):X2}] |= 0x{bitMask:X2};");
                        } else if(key.Key == ConsoleKey.S) {
                            break;
                        }
                    }
                    Console.WriteLine();
                }
            }

            // Summary
            Console.WriteLine();
            Console.WriteLine("=== Search Complete ===");
            Console.WriteLine();
            if(indicatorBytes.Count == 0) {
                Console.WriteLine("No indicator bytes found.");
                Console.WriteLine("The indicators may be at bytes you haven't reached yet,");
                Console.WriteLine("or they may use a different encoding method.");
            } else {
                Console.WriteLine($"Found {indicatorBytes.Count} byte(s) that affect indicators:");
                foreach(var bytePos in indicatorBytes) {
                    Console.WriteLine($"  - Byte {bytePos} (offset 0x{(0x1F + bytePos):X2})");
                }
                Console.WriteLine();
                Console.WriteLine("Update Pap3Device.BuildPap3DisplayCommands() with these findings!");
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(intercept: true);
            
            // Clear display
            ClearAllDisplays();
            _pap3?.UpdateDisplay(_displayState);
        }

        static void VerifyIndicatorBits()
        {
            Console.WriteLine();
            Console.WriteLine("=== Verify Indicator Bits ===");
            Console.WriteLine();
            Console.WriteLine("This test allows you to manually verify specific indicator bits.");
            Console.WriteLine("You can toggle bits and observe the corresponding indicators.");
            Console.WriteLine();
            Console.WriteLine("Press any key to start, or Q to cancel...");
            
            var key = Console.ReadKey(intercept: true);
            if(key.Key == ConsoleKey.Q) {
                Console.WriteLine("Test cancelled.");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Starting indicator bit verification...");
            Console.WriteLine();

            // Prepare a raw state for testing
            var rawState = new Pap3StateRaw();
            
            // Main loop for bit verification
            bool verifying = true;
            while(verifying) {
                Console.WriteLine("Current raw display data:");
                for(int i = 0; i < 30; i++) {
                    Console.Write($"{i:D2}: 0x{rawState.RawDisplayData[i]:X2}  ");
                }
                Console.WriteLine();
                
                Console.WriteLine("Enter byte/bit to toggle (e.g., '2 3' for byte 2, bit 3), or Q to quit:");
                
                string? input = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(input)) {
                    Console.WriteLine("Invalid input. Please specify a byte and bit.");
                    continue;
                }
                
                if(input.Trim().ToUpper() == "Q") {
                    verifying = false;
                    break;
                }
                
                // Parse byte and bit from input
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length != 2 || !int.TryParse(parts[0], out int byteNum) || !int.TryParse(parts[1], out int bitNum)) {
                    Console.WriteLine("Invalid input format. Please enter byte and bit numbers.");
                    continue;
                }
                
                if(byteNum < 0 || byteNum > 29 || bitNum < 0 || bitNum > 7) {
                    Console.WriteLine("Byte must be 0-29 and bit must be 0-7.");
                    continue;
                }
                
                // Toggle the specified bit in the raw display data
                byte mask = (byte)(1 << bitNum);
                rawState.RawDisplayData[byteNum] ^= mask;
                
                Console.WriteLine($"Toggled byte {byteNum}, bit {bitNum}. New value: 0x{rawState.RawDisplayData[byteNum]:X2}");
                
                // Update display with new raw state
                _pap3?.UpdateDisplay(rawState);
                
                Console.WriteLine("Observe the physical indicators and verify their response.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(intercept: true);
            }

            Console.WriteLine();
            Console.WriteLine("Indicator bit verification complete!");
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

        static void ClearAllDisplays()
        {
            _displayState.Speed = null;
            _displayState.Course = null;
            _displayState.Heading = null;
            _displayState.Altitude = null;
            _displayState.VerticalSpeed = null;
            _displayState.SpeedIsMach = false;
            _displayState.AltitudeIsFlightLevel = false;
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
                    _leds.AtArm = true;   // Switch ON → LED ON
                    Console.WriteLine($"        AT ARM LED: ON (switch ON)");
                    break;
                case "ATArmOff":
                    _leds.AtArm = false;  // Switch OFF → LED OFF
                    Console.WriteLine($"        AT ARM LED: OFF (switch OFF)");
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
