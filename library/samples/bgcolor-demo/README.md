# Background Color Usage Examples

This document demonstrates how to use background colors on WinWing MCDU displays.

## Quick Reference

The WinWing display supports **121 color combinations** (11 foreground × 11 background colors).

### Available Colors (Palette Indices)
- Black, Amber, White, Cyan, Green, Magenta, Red, Yellow, Brown, Grey, Khaki

> **Important:** `Colour.White`, `Colour.Red`, etc. are **palette indices**, not actual RGB colors!  
> The actual RGB values are defined in `cdu.Palette` and can be customized.  
> See [Understanding the Palette System](#understanding-the-palette-system) below.

## Understanding the Palette System

### Color Names vs. Actual Colors

When you use `Colour.White` or `Colour.Red`, you're **not** specifying an RGB color. You're selecting a **palette index**. The actual color displayed depends on the palette:

```csharp
// This sets the FOREGROUND to palette index "White"
// But what color is actually displayed? Check the palette!
cdu.Output
    .Colour(Colour.White)  // This is just an INDEX
    .Write("TEXT");
```

### Customizing Palette Colors

You can change what each palette index actually displays:

```csharp
// Change what "White" actually looks like
cdu.Palette.White.Set(0xFF, 0x00, 0x00);  // Now "White" is actually red!
cdu.RefreshPalette();  // Apply changes to device

// Now this text appears RED (even though we said "White")
cdu.Output
    .Colour(Colour.White)  // Index "White" ? but displays as RED
    .Write("This is RED!");
```

### Default Palette Values

The default RGB values for each palette index are:

| Palette Index | Default RGB | Hex Value |
|--------------|-------------|-----------|
| `Colour.Black` | (0, 0, 0) | `#000000` |
| `Colour.Amber` | (255, 165, 0) | `#FFA500` |
| `Colour.White` | (255, 255, 255) | `#FFFFFF` |
| `Colour.Cyan` | (0, 255, 255) | `#00FFFF` |
| `Colour.Green` | (0, 255, 61) | `#00FF3D` |
| `Colour.Magenta` | (255, 99, 255) | `#FF63FF` |
| `Colour.Red` | (255, 0, 0) | `#FF0000` |
| `Colour.Yellow` | (255, 255, 0) | `#FFFF00` |
| `Colour.Brown` | (97, 92, 66) | `#615C42` |
| `Colour.Grey` | (119, 119, 119) | `#777777` |
| `Colour.Khaki` | (121, 115, 94) | `#79735E` |

### Palette Customization Example

```csharp
// Customize palette for a specific aircraft
cdu.Palette.White.Set(0xFF, 0xFF, 0xE0);   // Slightly yellow white
cdu.Palette.Green.Set(0x00, 0xC0, 0x00);   // Darker green
cdu.Palette.Amber.Set(0xFF, 0xB3, 0x00);   // Brighter amber
cdu.RefreshPalette();

// Now when you use these colors, they use YOUR custom values
cdu.Output
    .BGAmber()              // Uses YOUR amber (0xFF, 0xB3, 0x00)
    .Colour(Colour.White)   // Uses YOUR white (0xFF, 0xFF, 0xE0)
    .Write("CUSTOM COLORS");
```

### Background Colors Work the Same Way

Background colors also use palette indices:

```csharp
// Background color "Red" uses whatever RGB value is in cdu.Palette.Red
cdu.Output
    .BGRed()  // Uses cdu.Palette.Red's RGB values
    .Colour(Colour.White)
    .Write("TEXT");

// You can customize the background palette index
cdu.Palette.Red.Set(0x80, 0x00, 0x00);  // Darker red
cdu.RefreshPalette();
// Now .BGRed() displays the darker red
```

## Usage Methods

### Method 1: Using Compositor API (Recommended)

```csharp
using(var cdu = CduFactory.ConnectLocal()) {
    // Set background color using fluent API
    cdu.Output
        .Line(0)
        .BGWhite()                    // Set white background
        .Colour(Colour.Black)         // Set black foreground
        .Write("  WARNING  ")
        .BGBlack();                   // Reset to black background
    
    cdu.RefreshDisplay();
}
```

### Method 2: Direct Cell Manipulation

```csharp
using(var cdu = CduFactory.ConnectLocal()) {
    // Set a single cell with background color
    var cell = cdu.Screen.Rows[0].Cells[0];
    cell.Character = 'A';
    cell.Colour = Colour.Black;           // Foreground (text color)
    cell.BackgroundColour = Colour.White; // Background
    cell.Small = false;
    
    cdu.RefreshDisplay();
}
```

### Method 3: Loop Through Row for Full Line

```csharp
// Create inverted text line (C-130 style) - LEGACY METHOD
// Better to use Compositor API (see Method 1)
string text = "  WARNING  ";
int line = 0;

for(int col = 0; col < text.Length && col < Metrics.Columns; col++) {
    var cell = cdu.Screen.Rows[line].Cells[col];
    cell.Character = text[col];
    cell.Colour = Colour.Black;
    cell.BackgroundColour = Colour.White;
    cell.Small = false;
}

cdu.RefreshDisplay();
```

## Compositor Background Color Methods

The `Compositor` class provides several ways to set background colors:

### Full Names
- `.BackgroundColour(Colour colour)` or `.BackgroundColor(Colour colour)`

### Short Names (Recommended)
- `.BGColour(Colour colour)` or `.BGColor(Colour colour)`

### Convenience Methods (Fastest)
- `.BGBlack()` - Black background
- `.BGAmber()` - Amber background
- `.BGWhite()` - White background
- `.BGCyan()` - Cyan background
- `.BGGreen()` - Green background
- `.BGMagenta()` - Magenta background
- `.BGRed()` - Red background
- `.BGYellow()` - Yellow background
- `.BGBrown()` - Brown background
- `.BGGrey()` / `.BGGray()` - Grey background
- `.BGKhaki()` - Khaki background

### Utility Method
- `.InvertColors()` - Swaps foreground and background colors

## Common Use Cases

### 1. Inverted Text (C-130 Style)

```csharp
// Using Compositor API (Recommended)
cdu.Output
    .Line(0)
    .BGWhite()
    .Colour(Colour.Black)
    .Write("  WARNING  ")
    .BGBlack();  // Reset to normal

// OR using Cell manipulation (Legacy)
for(int col = 0; col < Metrics.Columns; col++) {
    var cell = cdu.Screen.Rows[0].Cells[col];
    cell.Character = col < text.Length ? text[col] : ' ';
    cell.Colour = Colour.Black;
    cell.BackgroundColour = Colour.White;
}
```

### 2. Alert/Warning Headers

```csharp
// Using Compositor API (Recommended)
cdu.Output
    .Line(0)
    .BGRed()
    .Colour(Colour.White)
    .Centred("!! ALERT !!")
    .BGBlack();  // Reset to normal

// OR using Cell.Set() (Legacy)
string alertText = "!! ALERT !!";
int startCol = (Metrics.Columns - alertText.Length) / 2; // Center

for(int i = 0; i < alertText.Length; i++) {
    var cell = cdu.Screen.Rows[startCol + i];
    cell.Set(alertText[i], Colour.White, false, Colour.Red);
}
```

### 3. Multi-Color Line

```csharp
// Complex example with multiple color changes
cdu.Output
    .Line(5)
    .Colour(Colour.Yellow)
    .BGBlack()
    .Write("STATUS: ")
    .BGGreen()
    .Colour(Colour.Black)
    .Write(" OK ")
    .BGBlack()
    .Colour(Colour.White)
    .Write(" | TEMP: ")
    .BGAmber()
    .Colour(Colour.Black)
    .Write(" WARN ");

cdu.RefreshDisplay();
```

### 4. Progress Bar

```csharp
// Green bar showing progress (using Cell manipulation)
int progressPercent = 65;
int progressCells = (Metrics.Columns * progressPercent) / 100;

for(int col = 0; col < Metrics.Columns; col++) {
    var cell = cdu.Screen.Rows[5].Cells[col];
    cell.Character = '?';
    
    if(col < progressCells) {
        // Completed - green
        cell.Colour = Colour.Green;
        cell.BackgroundColour = Colour.Green;
    } else {
        // Remaining - dark
        cell.Colour = Colour.Grey;
        cell.BackgroundColour = Colour.Black;
    }
}
```

### 5. Highlighted Selection

```csharp
// Amber background for selected item
string[] menuItems = { "OPTION 1", "OPTION 2", "OPTION 3" };
int selectedIndex = 1;

for(int i = 0; i < menuItems.Length; i++) {
    var line = i * 2;
    var text = menuItems[i];
    bool isSelected = (i == selectedIndex);
    
    for(int col = 0; col < text.Length; col++) {
        var cell = cdu.Screen.Rows[line].Cells[col];
        cell.Character = text[col];
        cell.Colour = isSelected ? Colour.Black : Colour.White;
        cell.BackgroundColour = isSelected ? Colour.Amber : Colour.Black;
    }
}
```

## Color Combination Table (From Community Research)

Based on the HEX color table, here are some recommended combinations:

| Foreground | Background | HEX Code | Usage |
|------------|------------|----------|-------|
| Black | White | 0x0006 | Inverted text, headers |
| Black | Amber | 0x0003 | Warnings, selections |
| Black | Yellow | 0x0015 | Cautions |
| White | Red | 0x0054 | Critical alerts |
| White | Green | 0x004E | Status OK |
| Black | Cyan | 0x0009 | Information |
| White | Magenta | 0x0051 | Special modes |
| Black | Khaki | 0x001E | Dimmed emphasis |

## Important Notes

1. **Palette Indices**: `Colour.White`, `Colour.Red`, etc. are **palette indices**, not RGB colors
2. **Customizable Colors**: Change actual RGB values via `cdu.Palette.White.Set(r, g, b)` then `cdu.RefreshPalette()`
3. **Default Background**: If not specified, background defaults to `Colour.Black` palette index
4. **Performance**: Background colors don't affect performance - the hardware supports them natively
5. **Clearing**: `Cell.Clear()` resets background to black (palette index)
6. **Copying**: `Cell.CopyFrom()` includes background color palette index
7. **Display Buffer**: Background color palette indices are encoded in USB packets sent to device

### Palette System Details

The library provides several built-in palette presets in `cduhub`:
- **Fenix A320** palette
- **FlyByWire A32NX** palette  
- **ToLiss A32NX** palette
- **X-Plane 12 A330** palette

You can also create custom palettes via configuration files. Each palette defines the actual RGB values for all 11 color indices.

## Complete Example

See `library/samples/bgcolor-demo/Program.cs` for a complete working demonstration.

To run the demo:
```bash
cd library/samples/bgcolor-demo
dotnet run
```

## Technical Details

### Color Code Calculation

The WinWing hardware uses this formula:
```
code = (foreground_ordinal * 0x21) + (background_ordinal * 0x03)
```

Color ordinals (0-10): Black, Amber, White, Cyan, Green, Magenta, Red, Yellow, Brown, Grey, Khaki

### PackedValue Format

The `DisplayBufferFontAndColour` struct now uses a `ushort` (16 bits):
- Bits 0-3: Foreground color index (0-10)
- Bits 4-7: Background color index (0-10)  
- Bit 15: Small font flag
- Bits 8-14: Reserved/unused

This allows for efficient storage and fast USB packet encoding.
