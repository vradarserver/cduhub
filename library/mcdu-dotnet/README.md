# MCDU-DOTNET

> [!IMPORTANT]
> The library is still in the early stages of development. It is very likely
that interface, class and namespace names are all going to chop and change with
each release until things settle down.

The pre-release NuGet package for the library can be found here:

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


## Product Notes

[WinWing MCDU USB Packet Sniffing Notes](WINWING-MCDU-USB.md)
