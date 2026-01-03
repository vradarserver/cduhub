# PAP-3 LED Test Sample

This sample demonstrates how to control the LEDs on a WinWing PAP-3 Primary Autopilot Panel device using a timer-based refresh approach.

## Overview

The PAP-3 has 17 LEDs that can be controlled individually:

**Autothrottle LEDs:**
- N1
- SPEED (Speed Hold)
- AT ARM (Autothrottle Armed)

**Autopilot Mode LEDs:**
- LNAV (Lateral Navigation)
- VNAV (Vertical Navigation)
- LVL CHG (Level Change)
- HDG SEL (Heading Select)
- VOR LOC (VOR/Localizer)
- APP (Approach)
- ALT HOLD (Altitude Hold)
- V/S (Vertical Speed)

**Autopilot Command LEDs:**
- CMD A (Autopilot A Command)
- CWS A (Autopilot A Control Wheel Steering)
- CMD B (Autopilot B Command)
- CWS B (Autopilot B Control Wheel Steering)

**Flight Director LEDs:**
- FD L (Left Master Flight Director)
- FD R (Right Master Flight Director)

## Architecture

This sample uses a **timer-based refresh approach**:

- A timer fires every 250ms
- On each timer tick, the current LED state is sent to the device
- This ensures the hardware stays synchronized with the software state
- No change tracking is needed - we simply send the current state

### Benefits

- **Simple**: No need to track what changed  
- **Robust**: Recovers automatically from communication errors  
- **Reliable**: Hardware always reflects the current software state  
- **Efficient**: 250ms is fast enough for any real-world application  

### Input Responsiveness

The device input loop has been optimized for fast response:
- HID read timeout reduced to 100ms (from 1000ms)
- Removed polling sleep delays
- Events are raised immediately when buttons are pressed/released
- Input events should appear in console within ~100ms of button press

## Features

This sample provides:

1. **Sequential LED Test** - Tests each LED one at a time
2. **Interactive LED Control** - Press physical buttons to toggle their corresponding LEDs:
   - All 17 LEDs can be toggled by pressing their associated buttons
   - LED state is maintained and synced every 250ms
   - Perfect for testing button-to-LED mapping
3. **Brightness Test** - Tests three independent brightness controls:
   - **Panel Backlight** (0x00) - Controls panel backlight ? Working
   - **Digital Tube Backlight** (0x01) - Controls LCD/Display backlight ? Working
   - **Marker Light** (0x02) - Controls LED brightness ? Working
4. **Real-time Input Monitoring** - Shows button press/release events as they happen

### Interactive LED Control

The sample implements **stateful LED management** with button-to-LED mapping:

```
Press N1 button      ? Toggle N1 LED
Press SPEED button   ? Toggle SPEED LED
Press FD L button    ? Toggle FD L LED
Press LNAV button    ? Toggle LNAV LED
... and so on for all 17 LEDs
```

Each button press toggles its corresponding LED state, which is then synchronized to the hardware every 250ms by the refresh timer. The console shows the current LED state after each toggle.

**Supported Buttons:**
- **Autothrottle:** N1, SPEED, AT ARM
- **Navigation:** LNAV, VNAV, LVL CHG, HDG SEL, VOR LOC
- **Approach:** APP, ALT HOLD, V/S
- **Autopilot:** CMD A, CMD B, CWS A, CWS B
- **Flight Director:** FD L, FD R

### Brightness Control (All Verified)

Based on hardware testing, the PAP-3 brightness control has **three fully independent controls**:

| Parameter | Code | Status | Notes |
|-----------|------|--------|-------|
| panelBacklight | 0x00 | Yes (Working) | Panel background illumination |
| lcdBacklight | 0x01 | Yes (Working) | Controls digital tube displays |
| ledBacklight | 0x02 | Yes (Working) | Controls LED (marker light) brightness |

All three brightness controls are now verified and working correctly with the PAP-3 hardware!

## Requirements

- WinWing PAP-3 Primary Autopilot Panel connected via USB
- .NET 8.0 or later

## Running the Sample

```bash
cd library/samples/pap3-leds
dotnet run
```

## Usage

Once the program starts, you'll see a menu:

```
=== PAP-3 LED Test Menu ===

Tests:
  1 - Test all LEDs in sequence

Controls:
  0 - Turn off all LEDs
  B - Test brightness levels (Backlight, Digital Tube, Marker Light)
  H - Show this menu
  Q - Quit
```

Press the corresponding key to run a test. You can also press physical buttons on the PAP-3 device to see input events logged to the console.

## Code Examples

### Turn on specific LEDs

```csharp
var leds = new Pap3Leds {
    CmdA = true,        // Autopilot A engaged
    FdL = true,         // Left Flight Director on
    Lnav = true,        // LNAV mode active
    AltHold = true,     // Altitude Hold active
    N1 = true,          // N1 Autothrottle mode
    AtArm = true        // Autothrottle armed
};
// Timer will sync these changes within 250ms
```

### Set independent brightness levels

```csharp
// Set brightness values independently (0-255)
pap3.SetBrightness(
    panelBacklight: 200,        // Panel/Backlight
    lcdBacklight: 180,          // Digital Tube Backlight
    ledBacklight: 255           // Marker Light (LED brightness)
);
```

### Listen for button events

```csharp
// Maintain LED state
var leds = new Pap3Leds();

// Set up timer for regular refresh (250ms)
var refreshTimer = new System.Timers.Timer(250);
refreshTimer.Elapsed += (sender, e) => pap3.UpdateLeds(leds);
refreshTimer.Start();

// Toggle LEDs when buttons are pressed
pap3.ControlActivated += (sender, e) => {
    switch(e.ControlId) {
        case "N1":
            leds.N1 = !leds.N1;
            Console.WriteLine($"N1 LED: {(leds.N1 ? "ON" : "OFF")}");
            break;
        case "FdL":
            leds.FdL = !leds.FdL;
            Console.WriteLine($"FD L LED: {(leds.FdL ? "ON" : "OFF")}");
            break;
        case "Lnav":
            leds.Lnav = !leds.Lnav;
            Console.WriteLine($"LNAV LED: {(leds.Lnav ? "ON" : "OFF")}");
            break;
        // ... handle other buttons
    }
};

// The timer will automatically sync the LED state to hardware
```

## LED Command Protocol

The PAP-3 uses the following command structure for LEDs:

```
02 01 00 00 00 03 49 [LED_CODE] [VALUE] 00 00 00 00 00
```

Where:
- `LED_CODE` is the LED identifier (0x03 to 0x13)
- `VALUE` is 0x00 (off) or 0x01 (on)

See the main README in `library/mcdu-dotnet/WinWing/Pap3/README.md` for complete LED command mapping.

## Troubleshooting

**Device not found:**
- Ensure the PAP-3 is connected via USB
- Check Windows Device Manager to verify the device is recognized
- Try unplugging and reconnecting the device

**LEDs not responding:**
- Verify the device is properly initialized
- Check that no other application is using the device
- Try restarting the sample program

**Some LEDs don't work:**
- Verify the LED mapping matches your hardware version
- Check for hardware issues with specific LEDs

## Related Documentation

- [PAP-3 Implementation README](../../mcdu-dotnet/WinWing/Pap3/README.md)
- [IFrontpanel Interface](../../mcdu-dotnet/IFrontpanel.cs)
- [FrontpanelFactory](../../mcdu-dotnet/FrontpanelFactory.cs)
