# MCDU-DOTNET

The NuGet package for the library can be found here:

https://www.nuget.org/packages/mcdu-dotnet/


## Supported Devices

### CDU Devices

* MCDU (described by `ICduMcdu`, which implements `ICdu`)
* PFP7 (described by `ICduPfp7`, which implements `ICdu`)
* PFP3N (described by `ICduPfp3N`, which implements `ICdu`)

### FGCP Devices

* FCU with or without one or two EFIS attached (described by `IFgcpFcu`,
  which implements `IFgcp`)


## Instantiating Devices

Discovery and instantiation of supported devices is done via the `DeviceFactory`
static.

> [!NOTE]
> In V1 this was called `CduFactory`. This still exists but is flagged as obsolete
> and will be removed in a future version of the library.



### Instantiating CDU Devices

`ConnectLocalCdu`: The non-generic version of this function takes various optional
filter parameters and returns an `ICdu` instance for the first CDU device that
matches the parameters. Unless told otherwise it will match against any CDU device
regardless of type (E.G. it will match against an MCDU or a PFP7, whatever it can
find).

If no CDU device can be found then it returns null.

`ConnectLocalCdu<T>`: This is similar to the non-generic version except it takes the
interface type for a specific type of CDU. If you are only interested in attaching
to an MCDU then you would pass `ICduMcdu`, for PFP7s you would pass `ICduPfp7` and
so on.

If the specific type of CDU device cannot be found then it returns null.



#### CDU Interfaces

* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/ICdu.cs
* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/ICduMcdu.cs
* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/ICduPfp7.cs
* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/ICduPfp3N.cs



### Instantiating FGCP Devices

`ConnectLocalFgcp`: The non-generic version of this function takes various optional
filter parameters and returns an `IFgcp` instance for the first FGCP device that
matches the parameters. Unless told otherwise it will match against any FGCP device
regardless of type.

If no FGCP device can be found then it returns null.

`ConnectLocalFgcp<T>`: This is similar to the non-generic version except it takes the
interface type for a specific type of FGCP, E.G. if you are only interested in
attaching to an FCU then you would pass `IFgcpFcu`.

If the specific type of FGCP device cannot be found then it returns null.



#### FGCP Interfaces

* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/IFgcp.cs
* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/IFgcpFcu.cs



### Discovering USB Devices

`FindLocalDevices` returns a collection of `UsbDevice` objects that describe all
supported devices attached to the local machine.

* https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/UsbDevice.cs



## Reading from the CDU

The `ICdu` interface exposes two events, `CduKeyDown` and `CduKeyUp`. These are passed
an event args that tells you which key was pressed or released. There are extension
methods on the `CduKey` enum to convert keys into different formats.

> [!NOTE]
> The events used to be called `KeyDown` and `KeyUp`, and the CduKey was originally
> called `Key`. All of these enums and events still exist but they have been marked
> as obsolete, and will be removed in a future version of the library.


## Writing to the CDU display

This is a two-step process. The `ICdu` exposes a `Screen` property which lets you
set the content of the display. Setting the content of a screen does not update the
CDU's display.

The `ICdu` exposes a function called `RefreshDisplay`. This function sends the
current content of `Screen` to the device.

By default `RefreshDisplay` will not refresh the display if nothing has changed since
the last update.



### Composing Output

The `Screen` class can be cumbersome to work with. There is a higher-level compositing
class called `Compositor` that is exposed on the `ICdu` via the `Output` property. It
offers a fluent interface for setting the content of a screen.



### Screen Buffers

Screens are not tied to an CDU, and they can be instantiated just like any other
object. There are a pair of functions, `CopyFrom` and `CopyTo`, that can be used to
copy the content of a screen buffer into the CDU's screen buffer.



### LEDs

Same process as per screen buffers - there is an `CduLamps` class that carries the state and
brightness of the LED lights. The class is copyable.

There is a `RefreshLamps` function on the CDU object to copy the current state of the
`Lamps` buffer to the device. If nothing has changed since the last refresh then, by
default, nothing is sent.

> [!NOTE]
> In previous versions of the library CduLamps was called `Leds`, RefreshLamps was
> called `RefreshLeds` and the lamps buffer property was also called `Leds`. These
> all still exist but have been marked as obsolete, and will be removed in a future
> version of the library.


### Display, LED and Keyboard backlight intensities

The `ICdu` interface has a property called `Backlights` with properties that let you
set the brightness levels for the display backlight, the keyboard backlight and the LED
intensities as a percentage from 0 to 100.

Note that if you set the display or LED brightness to 0% then you can't see anything.

There is a `RefreshBacklights` function on the CDU object to copy the current state of
the `Backlights` buffer to the device. If nothing has changed since the last refresh then,
by default, nothing is sent.

> [!NOTE]
> In earlier versions of the library there was no buffer for backlights at all, instead
> there were three backlight percentage properties on ICdu and any values assigned to
> them were immediately sent to the device. There was also a `RefreshBrightnesses` function
> to force a resend of the backlight intensities when required. All of these still exist
> but they have been marked as obsolete and will be removed in a future verison of the
> library.



### Colours

The display supports a palette of ten colours. The names and default values of the
colours follow WinWing's defaults, but you are free to reassign the colours to anything
you like via the `Palette` property. After changing the palette you need to call
`RefreshPalette` to send your changes to the device.



### Cleanup

The CDU device will retain its state after your program stops driving it - I.E. it will
continue to show whatever you last wrote to the screen.

There is a function called `Cleanup` that will clear the screen, turn off all of the LEDs
and set the brightness levels to 0 (overridable).


### Fonts

The CDU device supports 1BPP bitmap fonts at varying widths and heights. However
`mcdu-dotnet` only supports fonts of either 29 or 31 pixels high and between 17 and 23
pixels wide.

Fonts are described by an `McduFontFile` object:

https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/McduFontFile.cs

Examples of font files can be found in the `cduhub` library's resources folder:

https://github.com/vradarserver/cduhub/tree/main/library/cduhub/Resources



### Events

Besides the `CduKeyDown` and `CduKeyUp` events referenced elsewhere there is also the
`Disconnected` event, which is raised when the library detects that the device has been
disconnected.