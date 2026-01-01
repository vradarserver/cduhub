# Understanding the Palette System

## Key Concept: Color Names Are Palette Indices

**The most important thing to understand:**

When you write `Colour.White` or `Colour.Red`, you are **NOT** specifying an RGB color value.  
You are selecting a **palette index** (slot 0-10).

The actual RGB color displayed depends on what's stored in `cdu.Palette` at that index.

## How It Works

```
???????????????      ????????????????      ????????????????
? Your Code   ?      ? Palette      ?      ? Display      ?
?             ?      ?              ?      ?              ?
? Colour.     ? ???? ? Palette.     ? ???? ? Actual RGB   ?
? White       ?      ? White        ?      ? on screen    ?
? (index #2)  ?      ? RGB: FF FF FF?      ? (255,255,255)?
???????????????      ????????????????      ????????????????
```

## Example: Changing What "White" Means

```csharp
// Default behavior - "White" displays as white
cdu.Output
    .Colour(Colour.White)  // Index #2
    .Write("This is WHITE");
// Displays: White text (RGB: 255, 255, 255)

// Now customize the palette
cdu.Palette.White.Set(0xFF, 0x00, 0x00);  // Make index #2 = RED
cdu.RefreshPalette();

// Same code, different result!
cdu.Output
    .Colour(Colour.White)  // Still index #2
    .Write("This is WHITE");
// Displays: RED text (RGB: 255, 0, 0) even though we said "White"!
```

## Why This Design?

This palette system allows:

1. **Aircraft-specific colors** - Each aircraft can define its own palette
2. **Quick theme switching** - Change entire display appearance without modifying code
3. **Hardware efficiency** - WinWing devices work with palette indices, not RGB values
4. **Flexibility** - "White" can mean different things in different contexts

## Background Colors Work the Same Way

```csharp
// Background colors also use palette indices
cdu.Output
    .BGRed()  // Uses cdu.Palette.Red's RGB values
    .Colour(Colour.White)
    .Write("TEXT");

// Customize background color
cdu.Palette.Red.Set(0x80, 0x00, 0x00);  // Darker red
cdu.RefreshPalette();

// Now .BGRed() uses the darker red
cdu.Output
    .BGRed()  // Now uses (128, 0, 0) instead of (255, 0, 0)
    .Colour(Colour.White)
    .Write("TEXT");
```

## Complete Example

```csharp
using WwDevicesDotNet;

using(var cdu = CduFactory.ConnectLocal()) {
    // Step 1: Display with default palette
    cdu.Output
        .Line(0)
        .Colour(Colour.White)
        .BGRed()
        .Write("  ALERT  ")
        .BGBlack();
    cdu.RefreshDisplay();
    
    // At this point:
    // - Foreground uses default White (255, 255, 255)
    // - Background uses default Red (255, 0, 0)
    
    System.Threading.Thread.Sleep(2000);
    
    // Step 2: Customize the palette for an aircraft theme
    cdu.Palette.White.Set(0xFF, 0xFF, 0xE0);  // Slightly yellow
    cdu.Palette.Red.Set(0xC0, 0x00, 0x00);    // Darker red
    cdu.RefreshPalette();  // Apply to device
    
    // Step 3: Redraw - same code, different appearance!
    cdu.Output
        .Line(2)
        .Colour(Colour.White)  // Now yellowish
        .BGRed()               // Now darker red
        .Write("  ALERT  ")
        .BGBlack();
    cdu.RefreshDisplay();
}
```

## Built-In Palettes

The `cduhub` application provides several pre-configured palettes:

- **Fenix A320** - Matches Fenix Simulations A320
- **FlyByWire A32NX** - Matches FBW A320neo
- **ToLiss A32NX** - Matches ToLiss A321
- **X-Plane 12 A330** - Matches default X-Plane 12 A330

These palettes define custom RGB values for all 11 color indices to match each aircraft's specific color scheme.

## Accessing Palette in Code

```csharp
// Read current RGB values
var whiteRed = cdu.Palette.White.R;
var whiteGreen = cdu.Palette.White.G;
var whiteBlue = cdu.Palette.White.B;

// Modify a color
cdu.Palette.Amber.Set(0xFF, 0xC0, 0x00);  // Brighter amber

// Must call RefreshPalette() to send changes to device
cdu.RefreshPalette();

// The display automatically refreshes when you call RefreshPalette()
// so existing text picks up the new colors immediately
```

## Important API Calls

| Method | Purpose |
|--------|---------|
| `cdu.Palette.White.Set(r, g, b)` | Change RGB for a palette index |
| `cdu.RefreshPalette()` | Send palette changes to device |
| `cdu.Palette.White.CopyFrom(other)` | Copy RGB from another PaletteColour |
| `PaletteColour.Parse("FFFFFF")` | Parse hex string to RGB |

## Technical Details

### The 11 Palette Indices

| Index | Name | Default RGB | Default Hex |
|-------|------|-------------|-------------|
| 0 | Black | (0, 0, 0) | #000000 |
| 1 | Amber | (255, 165, 0) | #FFA500 |
| 2 | White | (255, 255, 255) | #FFFFFF |
| 3 | Cyan | (0, 255, 255) | #00FFFF |
| 4 | Green | (0, 255, 61) | #00FF3D |
| 5 | Magenta | (255, 99, 255) | #FF63FF |
| 6 | Red | (255, 0, 0) | #FF0000 |
| 7 | Yellow | (255, 255, 0) | #FFFF00 |
| 8 | Brown | (97, 92, 66) | #615C42 |
| 9 | Grey | (119, 119, 119) | #777777 |
| 10 | Khaki | (121, 115, 94) | #79735E |

### How It's Encoded

The library encodes **both** foreground and background palette indices in the USB packets:

```
USB Code = (foreground_index * 0x21) + (background_index * 0x03)
```

The device receives the indices, then looks up the actual RGB values from its internal palette memory.

## Summary

? **`Colour.White`** = Palette index #2, **not** RGB (255, 255, 255)  
? **`Colour.Red`** = Palette index #6, **not** RGB (255, 0, 0)  
? **Customize with** `cdu.Palette.White.Set(r, g, b)`  
? **Apply with** `cdu.RefreshPalette()`  
? **Background colors** use the same palette system

**The palette system gives you complete control over what each "color name" actually displays!**
