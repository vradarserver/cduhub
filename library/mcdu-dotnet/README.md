# MCDU-DOTNET

The NuGet package for the library can be found here:

https://www.nuget.org/packages/mcdu-dotnet/


## PRELIMINARY DOCUMENTATION

To instantiate an MCDU object call the `McduFactory` static:

```
using(var mcdu = McduFactory.ConnectLocal())
```

You can pass an override to specify which MCDU product to instantiate. There is
also a function that enumerates the MCDU USB devices that are currently active.

The `ConnectLocal` function returns an instance of an `IMcdu` interface.



## Reading from the MCDU

The `IMcdu` interface exposes two events, `KeyDown` and `KeyUp`. These are passed
an event args that tells you which key was pressed or released. There are extension
methods on the `Key` enum to convert keys into different formats.



## Writing to the MCDU

This is a two-step process. The `IMcdu` exposes a `Screen` property which lets you
set the content of the display. Setting the content of a screen does not update the
MCDU's display.

The `IMcdu` exposes a function called `RefreshDisplay`. This function sends the
current content of `Screen` to the device.

By default `RefreshDisplay` will not refresh the display if nothing has changed since
the last update.



### Composing Output

The `Screen` class can be cumbersome to work with. There is a higher-level compositing
class called `Compositor` that is exposed on the `IMcdu` via the `Output` property. It
offers a fluent interface for setting the content of a screen.



### Screen Buffers

Screens are not tied to an MCDU, and they can be instantiated just like any other
object. There are a pair of functions, `CopyFrom` and `CopyTo`, that can be used to
copy the content of a screen buffer into the MCDU's screen buffer.



### LEDs

Same process as per screen buffers - there is an `Led` class that carries the state and
brightness of the LED lights. The class is copyable.

There is a `RefreshLeds` function on the MCDU object to copy the current state of the
`Led` buffer to the device. If nothing has changed since the last refresh then, by
default, nothing is sent.



### Display, LED and Backlight Brightness

There are properties for each of these on the `IMcdu` interface that let you specify the
brightness levels as a percentage from 0 to 100. Note that if you set the display or LED
brightness to 0% then you can't see anything.

The brightness properties will not send commands to the device if it detects no change
from what was last sent. If you want to force the library to send brightness levels then
you can call `RefreshBrightnesses`.



### Cleanup

The MCDU device will retain its state after your program stops driving it - I.E. it will
continue to show whatever you last wrote to the screen.

There is a function called `Cleanup` that will clear the screen, turn off all of the LEDs
and set the brightness levels to 0 (overridable).


### Fonts

The MCDU device supports 1BPP bitmap fonts at varying widths and heights. However
`mcdu-dotnet` only supports fonts of either 29 or 31 pixels high and between 17 and 23
pixels wide.

Fonts are described by an `McduFontFile` object:

https://github.com/vradarserver/cduhub/blob/main/library/mcdu-dotnet/McduFontFile.cs

Examples of font files can be found in the `cduhub` library's resources folder:

https://github.com/vradarserver/cduhub/tree/main/library/cduhub/Resources



### Events

Besides the `KeyDown` and `KeyUp` events referenced elsewhere there is also the
`Disconnected` event, which is raised when the library detects that the device has been
disconnected.