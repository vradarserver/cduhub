# WinWing PAP-3 Primary Autopilot Panel Support

This folder contains the implementation for the WinWing PAP-3 Primary Autopilot Panel device.

## Implementation Status

### TODO Items:

1. **Hardware Protocol Discovery**
   - [X] Determine actual command prefix for PAP-3 (`_Pap3DisplayPrefix` = 0x0FBF, verified)
   - [X] Capture and analyze HID communication packets from the device
   - [X] Verify input report structure and length

2. **Control Mapping**
   - [X] Map all physical buttons to correct byte offsets and bit flags in `ControlMap.cs`

3. **Display Implementation**
   - [X] Implement display command structure in `BuildDisplayCommands()`
   - [ ] **NOT IMPLEMENTED**: Display encoding in `EncodeDisplays()`
   - [ ] The display encoding logic is placeholder only and does not work
   - [ ] Display functionality requires hardware reverse engineering to determine correct byte mappings
   - [ ] **NEEDS HARDWARE TESTING**: Verify seven-segment display byte positions
   - [ ] **NEEDS HARDWARE TESTING**: Test Speed/Mach mode switching
   - [ ] **NEEDS HARDWARE TESTING**: Test Course display (PLT and CPL)
   - [ ] **NEEDS HARDWARE TESTING**: Test Heading display
   - [ ] **NEEDS HARDWARE TESTING**: Test Altitude display (including Flight Level mode)
   - [ ] **NEEDS HARDWARE TESTING**: Test Vertical Speed display with sign indicators

4. **LED Implementation**
   - [X] Verify LED command codes in `BuildLedCommands()`
   
5. **Brightness Control**
   - [X] Verify brightness command structure
   - [X] Test panel backlight control - Working (0x00)
   - [X] Test LCD backlight control - Working (0x01)
   - [X] Test LED brightness control - Working (0x02)

## Display Implementation Notes

**DISPLAY FUNCTIONALITY IS NOT IMPLEMENTED**

The `EncodeDisplays()` method contains placeholder code only. The actual display encoding for PAP-3 has not been reverse engineered.

**Why Display Support is Missing:**
- PAP-3 display encoding differs significantly from FCU/EFIS
- The nibble-swapped encoding pattern from FCU/EFIS does not directly apply
- Hardware reverse engineering is required to determine:
  1. Exact byte positions for each display segment
  2. Segment bit mappings within each byte  
  3. How indicators (MACH, TRK, FPA, etc.) are encoded
  4. How the 30-byte display data block is structured

**Current Status:**
- Display command packets are sent with correct structure (verified from packet dumps)
- LED control is fully functional
- Brightness control is fully functional
- Display data encoding is placeholder and **will not work correctly**

**For Developers:**
If you want to implement display support:
1. Capture HID packets from SimAppPro while it updates displays
2. Analyze the 30-byte display data block (offset 0x1F-0x3C)
3. Map which bytes/bits control which segments
4. Implement proper encoding in `EncodeDisplays()`

## Sample Programs

A complete LED and display test sample is available at:
- **[samples/pap3-leds](../../../samples/pap3-leds/)** - Interactive LED and display testing program

This sample demonstrates:
- Testing individual LEDs
- Testing all LEDs in sequence
- **Speed display testing** - Display various speeds in knots and Mach mode
- **Indicator search testing** - Find which bits control IAS/MACH, HDG/TRK, V/S/FPA indicators
- Brightness control (Panel, LCD, LED)
- Event handling for button presses
- Complete LED control patterns

To run the sample:
```bash
cd library/samples/pap3-leds
dotnet run
```

### New Display Tests

**Option 2: Test Speed Display**
- Tests speed display with values from 0 to 999 knots
- Tests MACH mode with values 0.50 to 0.90
- Helps verify if speed encoding is working correctly

**Option 3: Search for Indicators**
- Interactive test to find indicator bit positions
- Tests IAS/MACH indicator toggle
- Tests HDG/TRK indicator toggle  
- Tests V/S/FPA indicator toggle
- User confirms visually if indicators change on physical panel
- Results help identify correct byte/bit positions in code

**Option 4: Verify Indicator Bits**
- Manually toggle specific byte/bit combinations
- Interactive verification of discovered indicator positions
- Helps confirm exact mappings before implementation

## Indicator Mapping (VERIFIED)

The following indicator positions have been discovered through hardware testing and are now implemented:

### IAS/MACH Indicators

| Indicator | Byte Offset | Bit Mask | Buffer Index | Notes |
|-----------|-------------|----------|--------------|-------|
| **MACH** | 0x2E (46) | 0x80 | payload[0x2E] | Primary position |
| **MACH** | 0x32 (50) | 0x80 | payload[0x32] | Nibble-swap repeat |
| **IAS** | 0x36 (54) | 0x80 | payload[0x36] | Speed indicator |

**Implementation:**
```csharp
if (state.SpeedIsMach) {
    payload[0x2E] |= 0x80;  // MACH indicator
    payload[0x32] |= 0x80;  // MACH indicator (repeat)
} else {
    payload[0x36] |= 0x80;  // IAS indicator
}
```

### HDG/TRK Indicators

| Indicator | Byte Offset | Bit Mask | Buffer Index | Notes |
|-----------|-------------|----------|--------------|-------|
| **TRK** | 0x2A (42) | 0x08 | payload[0x2A] | Track mode |
| **TRK** | 0x2E (46) | 0x08 | payload[0x2E] | Nibble-swap repeat |
| **HDG** | 0x32 (50) | 0x08 | payload[0x32] | Heading mode |
| **HDG** | 0x36 (54) | 0x08 | payload[0x36] | Nibble-swap repeat |

**Implementation:**
```csharp
if (state.HeadingIsTrack) {
    payload[0x2A] |= 0x08;  // TRK indicator
    payload[0x2E] |= 0x08;  // TRK indicator (repeat)
} else {
    payload[0x32] |= 0x08;  // HDG indicator
    payload[0x36] |= 0x08;  // HDG indicator (repeat)
}
```

### V/S/FPA Indicators

| Indicator | Byte Offset | Bit Mask | Buffer Index | Notes |
|-----------|-------------|----------|--------------|-------|
| **FPA** | 0x30 (48) | 0x80 | payload[0x30] | Flight Path Angle |
| **FPA** | 0x34 (52) | 0x80 | payload[0x34] | Nibble-swap repeat |
| **V/S** | 0x38 (56) | 0x80 | payload[0x38] | Vertical Speed |

**Implementation:**
```csharp
if (state.VsIsFpa) {
    payload[0x30] |= 0x80;  // FPA indicator
    payload[0x34] |= 0x80;  // FPA indicator (repeat)
} else {
    payload[0x38] |= 0x80;  // V/S indicator
}
```

### Understanding the Nibble-Swap Pattern

The PAP-3 uses a nibble-swapped encoding similar to FCU/EFIS where:
- **Primary indicator bit** is set at one byte position
- **Repeat indicator bit** is set at a second byte position (4 bytes later)
- This pattern ensures the indicator is visible across the nibble-swapped digit encoding

**Pattern observed:**
- TRK: Bytes 0x2A + 0x2E (Δ = 4 bytes)
- HDG: Bytes 0x32 + 0x36 (Δ = 4 bytes)
- MACH: Bytes 0x2E + 0x32 (Δ = 4 bytes)
- FPA: Bytes 0x30 + 0x34 (Δ = 4 bytes)

The 4-byte spacing suggests the indicators are interleaved with digit encoding in the display data block.


Data LCD starts a 0x1D to 0x38

