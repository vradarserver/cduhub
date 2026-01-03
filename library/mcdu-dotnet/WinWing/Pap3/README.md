# WinWing PAP-3 Primary Autopilot Panel Support

This folder contains the implementation for the WinWing PAP-3 Primary Autopilot Panel device.

## Implementation Status

### TODO Items:

1. **Hardware Protocol Discovery**
   - [X] Determine actual command prefix for PAP-3 (`_Pap3Prefix` = 0x0100, verified)
   - [X] Capture and analyze HID communication packets from the device
   - [X] Verify input report structure and length

2. **Control Mapping**
   - [X] Map all physical buttons to correct byte offsets and bit flags in `ControlMap.cs`

3. **Display Implementation**
   - [ ] Implement display command structure in `BuildDisplayCommands()`
   - [ ] Implement display encoding in `EncodeDisplays()`
   - [ ] Verify seven-segment display encoding (may differ from FCU/EFIS)
   - [ ] Test Speed/Mach mode switching
   - [ ] Test Course display
   - [ ] Test Heading display
   - [ ] Test Altitude display (including Flight Level mode)
   - [ ] Test Vertical Speed display

4. **LED Implementation**
   - [X] Verify LED command codes in `BuildLedCommands()`
   
5. **Brightness Control**
   - [X] Verify brightness command structure
   - [X] Test panel backlight control - Working (0x00)
   - [X] Test LCD backlight control - Working (0x01)
   - [X] Test LED brightness control - Working (0x02)

## Hardware Protocol - Brightness Commands

### Brightness Command Structure (Verified)

Brightness commands follow this format:
```
02 01 00 00 00 03 49 [TYPE] [VALUE] 00 00 00 00 00
```

### Brightness Control Mapping (All Verified Working)

| Type | Code | Status | Notes |
|------|------|--------|-------|
| Panel Backlight | 0x00 | Working | Panel background illumination |
| Digital Tube (LCD) | 0x01 | Working | Display backlight (independent control) |
| Marker Light (LED) | 0x02 | Working | LED brightness (independent control) |

All three brightness controls are now verified and working correctly!

### Example: Set Brightness Values

```
// Set panel brightness to 200:
02 01 00 00 00 03 49 00 C8 00 00 00 00 00

// Set digital tube brightness to 150:
02 01 00 00 00 03 49 01 96 00 00 00 00 00

// Set marker light (LED) brightness to 255:
02 01 00 00 00 03 49 02 FF 00 00 00 00 00
```

## Hardware Protocol - LED Commands

### LED Command Structure (Verified)

LED commands follow this format:
```
02 01 00 00 00 03 49 [LED_CODE] [VALUE] 00 00 00 00 00
```

- Byte 0: `0x02` (Command type)
- Bytes 1-2: `0x01 0x00` (Prefix - 0x0100)
- Bytes 3-4: `0x00 0x00`
- Bytes 5-6: `0x03 0x49` (LED command identifier)
- Byte 7: LED code (see table below)
- Byte 8: Value (`0x00` = off, `0x01` = on)
- Bytes 9-13: `0x00` (padding)

### LED Command Mapping

| Value | LED Name | Description |
|-------|----------|-------------|
| 0x03 | N1 | Autothrottle N1 mode |
| 0x04 | SPEED | Autothrottle Speed Hold mode |
| 0x05 | VNAV | Vertical Navigation mode |
| 0x06 | LVL_CHG | Level Change mode |
| 0x07 | HDG_SEL | Heading Select mode |
| 0x08 | LNAV | Lateral Navigation mode |
| 0x09 | VOR_LOC | VOR/LOC mode |
| 0x0A | APP | Approach mode |
| 0x0B | ALT_HLD | Altitude Hold mode |
| 0x0C | VS | Vertical Speed mode |
| 0x0D | A_CMD | Autopilot A Command |
| 0x0E | A_CWS | Autopilot A Control Wheel Steering |
| 0x0F | B_CMD | Autopilot B Command |
| 0x10 | B_CWS | Autopilot B Control Wheel Steering |
| 0x11 | AT_ARM | Autothrottle Armed |
| 0x12 | L_MA | Left Master Flight Director |
| 0x13 | R_MA | Right Master Flight Director |

### Example: Toggle N1 LED

```
// Turn N1 LED on:
02 01 00 00 00 03 49 03 01 00 00 00 00 00

// Turn N1 LED off:
02 01 00 00 00 03 49 03 00 00 00 00 00 00
```

## Files

- **Control.cs**: Enumeration of all PAP-3 controls (buttons, encoders)
- **ControlMap.cs**: Maps controls to HID input report positions (placeholder mappings)
- **Pap3State.cs**: Represents the display state for the panel
- **Pap3Leds.cs**: Represents the LED states for the panel
- **Pap3Device.cs**: Main device implementation implementing `IFrontpanel`

## Sample Programs

A complete LED test sample is available at:
- **[samples/pap3-leds](../../../samples/pap3-leds/)** - Interactive LED testing program

This sample demonstrates:
- Testing individual LEDs
- Testing LED groups (Autopilot, Autothrottle, Flight Director, Mode)
- Brightness control
- Event handling for button presses
- Complete LED control patterns

To run the sample:
```bash
cd library/samples/pap3-leds
dotnet run
```

## Usage Example

```csharp
using WwDevicesDotNet;
using WwDevicesDotNet.WinWing.Pap3;

// Connect to the PAP-3 device
var pap3 = FrontpanelFactory.ConnectLocal(
    device: Device.WinWingPap3,
    deviceType: DeviceType.Boeing737FrontPanel
);

if(pap3 != null) {
    Console.WriteLine($"Connected to: {pap3.DeviceId.Description}");

    // Update displays
    var state = new Pap3State {
        Speed = 250,
        SpeedIsMach = false,
        Course = 90,
        Heading = 270,
        Altitude = 10000,
        AltitudeIsFlightLevel = false,
        VerticalSpeed = 1000
    };
    pap3.UpdateDisplay(state);

    // Update LEDs
    var leds = new Pap3Leds {
        CmdA = true,        // Autopilot A Command engaged
        FdL = true,         // Left Flight Director on
        Lnav = true,        // LNAV mode active
        AltHold = true,     // Altitude Hold active
        N1 = true,          // N1 Autothrottle mode
        Speed = false,      // Speed Hold mode off
        AtArm = true        // Autothrottle armed
    };
    pap3.UpdateLeds(leds);

    // Set brightness (0-255)
    pap3.SetBrightness(
        panelBacklight: 200,
        lcdBacklight: 200,
        ledBacklight: 200
    );

    // Listen for control events
    pap3.ControlActivated += (sender, e) => {
        Console.WriteLine($"Control activated: {e.ControlId}");
        
        // Handle specific controls
        if(e.ControlId == "CmdA") {
            // Toggle CMD A LED
            leds.CmdA = !leds.CmdA;
            pap3.UpdateLeds(leds);
        }
    };

    pap3.ControlDeactivated += (sender, e) => {
        Console.WriteLine($"Control deactivated: {e.ControlId}");
    };

    // Keep running
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();

    // Cleanup
    pap3.Dispose();
}
```

## Testing Notes

When testing with actual hardware:

1. Connect the PAP-3 device and monitor HID communication using tools like:
   - Wireshark with USBPcap
   - HID API Monitor
   - USB Analyzer hardware

2. Capture packets for:
   - Button presses and releases
   - Rotary encoder movements (clockwise and counter-clockwise)
   - Display updates for all fields
   - LED state changes
   - Brightness adjustments

3. Update the code based on captured protocol data

## Related Documentation

- See the FcuAndEfis implementation for a working reference implementation
- The PAP-3 likely uses similar HID protocol patterns to the FCU/EFIS devices
- Consult WinWing's documentation if available

## Contributing

If you have a PAP-3 device and can help with protocol discovery:

1. Capture HID traffic while using the device
2. Document the packet structures
3. Update the TODO items above with your findings
4. Submit a pull request with the corrected implementation


# Investigation Notes
