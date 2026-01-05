# PAP-3 Ambient Light Sensor Sample

This sample demonstrates how to read and respond to ambient light sensor changes on the WinWing PAP-3 Primary Autopilot Panel.

## Features

- Real-time display of left and right ambient light sensor values (native hex values)
- Normalized ambient light percentage (0-100%)
- Visual bar graph representation of light levels
- Event-driven notifications when sensor values change

## How It Works

The PAP-3 device includes two ambient light sensors that continuously report light levels. The sensor values are:

- **Native values**: Raw sensor readings (0x0000 to 0x0FFF)
- **Percentage**: Normalized value from 0% (dark) to 100% (bright)

### Properties

The `Pap3Device` exposes the following ambient light properties:

```csharp
public int AmbientLightPercent { get; }         // Normalized percentage (0-100)
```

### Events

Three events are raised when sensor values change:

```csharp
event EventHandler AmbientLightChanged;         // Normalized percentage changed
```

## Usage

```csharp
using WwDevicesDotNet;
using WwDevicesDotNet.WinWing.Pap3;

// Connect to PAP-3 device
var deviceId = FrontpanelFactory.FindLocalDevices()
    .FirstOrDefault(d => d.Device == Device.WinWingPap3);

using(var pap3 = FrontpanelFactory.ConnectLocal(deviceId) as Pap3Device) {
    // Subscribe to ambient light change events

    pap3.AmbientLightChanged += (sender, args) => {
        Console.WriteLine($"Ambient light: {pap3.AmbientLightPercent}%");
    };

    // Access current values at any time
    Console.WriteLine($"Current light level: {pap3.AmbientLightPercent}%");
    
    // Your code here...
}
```

## Running the Sample

1. Ensure your PAP-3 device is connected
2. Run the sample:
   ```
   cd library/samples/pap3-ambient
   dotnet run
   ```
3. Cover the sensors or change room lighting to see values change
4. Press 'Q' to quit

## Use Cases

### Auto-Brightness Adjustment

You can use the ambient light sensor to automatically adjust brightness levels:

```csharp
pap3.AmbientLightChanged += (sender, args) => {
    var percent = pap3.AmbientLightPercent;
    
    // Map ambient light to brightness (example: inverse relationship for LEDs)
    byte brightness = (byte)(255 - (percent * 255 / 100));
    
    // Apply brightness (panel, LCD, LEDs)
    pap3.SetBrightness(brightness, brightness, brightness);
};
```

### Light Level Monitoring

Monitor when lighting conditions cross specific thresholds:

```csharp
pap3.AmbientLightChanged += (sender, args) => {
    if(pap3.AmbientLightPercent < 20) {
        Console.WriteLine("Low light detected - enabling night mode");
    } else if(pap3.AmbientLightPercent > 80) {
        Console.WriteLine("Bright light detected - enabling day mode");
    }
};
```

## See Also

- [PAP-3 LEDs Sample](../pap3-leds/README.md) - LED control demonstration
- [MCDU Ambient Sample](../ambient/Program.cs) - Similar implementation for MCDU devices
