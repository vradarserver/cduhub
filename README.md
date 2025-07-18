# CDU HUB

[WinWing](https://uk.winwingsim.com/view/) sell a replica of the Airbus MCDU
panel. It plugs in over USB, it has a 24 x 14 alphanumeric colour display,
it's got ~70 backlight buttons, two ambient light sensors, half-a-dozen LEDs
and it looks really cool :)

It comes in very handy when flying Airbus aircraft in flight simulators, but I
want to use it outside of flight sims as well.

This application can display output on, and accept input from, the MCDU device.
The idea is that it can be left running all the time, it has its own built-in
set of pages and it will let other applications connect to it and display their
pages as well.

To avoid having to keep quitting and restarting whenever you want to use the
MCDU in a flight simulator the program lets you either stop driving the display
so that other programs can use the device, or (for supported A320 simulations)
it can display the output from the simulator and control the simulated MCDU.

The application should be able to run on any platform that .NET Core 8 supports.
This includes Windows, most major Linux distros and macOS.



## Credits

The MCDU image used as the basis for the Windows app logo was copied from
FlyByWire's A320 simulator:

https://github.com/flybywiresim/aircraft



## Installers

### Windows

Download and run the latest Windows installer from the release page here:

https://github.com/vradarserver/cduhub/releases


### Unix-like Operating Systems

*Coming soon*

If you're comfortable with compiling .NET Core applications on your machine,
and you have the .NET Core 8 SDK installed, then you can build and run the
CLI version of the application by cloning the repository and running this:

```bash
dotnet run --project apps/cduhub-cli/cduhub-cli.csproj
```

## Supported Flight Simulators

### MSFS2020 / 2024: Fenix A320

CDU Hub uses the Fenix browser EFB's MCDU page to read and control the MCDU
display for both the captain and first officer. You can flip between the two
MCDUs using the BLANK1 button (next to DATA) and you can jump back to the
CDU Hub menu using the BLANK2 button (next to AIRPORT).

Known issues:

* Font is the default WinWing font. I'll look into proper font support later.
* BRT and DIM don't do anything. This is because they're not supported in the
  browser MCDU. I'll have those operate independently of the simulator at
  a later date.


### MSFS2020 / 2024: SimBridge (aka FlyByWire A320neo)

This uses FlyByWire's SimBridge remote MCDU to read and control the mirrored
MCDU. When FlyByWire add support for separate pilot and first officer MCDUs
the program should be able to read and control them separately, and switch
between them using the BLANK1 button (next to DATA).

You can jump back to the CDU Hub menu using the BLANK2 button (next to AIRPORT).

Known issues:

* Font is the default WinWing font - see notes against Fenix support.
* BRT and DIM don't do anything - see notes against Fenix support.
* Centred text is not always perfectly aligned. The FBW A320 positions
  odd-length centred text at half-column positions, which I can't do on the
  WinWing. I don't see a way around this at the moment.
* There are disparities between the simulator MCDU and the remote MCDU. The
  program uses the remote MCDU, so if there are bits missing on the remote
  MCDU (E.G. the page arrows are occasionally missing) then c'est la vie. I
  imagine that FBW will fix those bits at some point.



### X-Plane: ToLiss

This uses ToLiss's custom datarefs to read and control the pilot and first-officer
MCDUs. You can flip between the two MCDUs using the BLANK1 button (next to DATA)
and you can jump back to the CDU Hub menu using the BLANK2 button (next to
AIRPORT).

Known issues:

* Pauses at startup. This is a side-effect of the mechanism used to interact with
  X-Plane.
* LEDs don't work. I've not actually seen them light up in the simulator yet so
  I'm not sure whether ToLiss simulate them.
* BRT and DIM only affect the simulated MCDU, not the real one. I'll look at this
  when I do the general BRT/DIM handling for the device.



### X-Plane 12: General Airbus

This uses X-Plane's standard datarefs for MCDUs to read and control the pilot and
first-officer MCDUs. You can flip between the two MCDUs using the BLANK1 button
(next to DATA) and you can jump back to the CDU Hub menu using the BLANK2 button
(next to AIRPORT). X-Plane 12 only.

Known issues:

* Only works on local machine. I think I read somewhere that X-Plane 12's
  web server doesn't accept connections from non-local addresses? Not sure.
  However you can configure any address for the sim, if Laminar adds support
  for non-local addresses in the future then it should work with no code
  changes here.
* Bit laggy. This is a by-product of fetching the MCDU datarefs by REST instead of
  WebSockets (which gets aborted after 100 seconds) or UDP (which requires 3200+
  datarefs for the generic MCDUs).
* SEC F-PLN and ATC-COMM don't appear to have commands for them. They don't do
  anything in the A330 so I'm assuming they're just not present.
* MCDU MENU doesn't do anything, I'm not sure I'm sending the right command there.
* BRT and DIM don't do anything. I think I can get them mirroring the simulation
  though, I'll do that later.
* LEDs don't work. I think I just need to find the commands for those.



## mcdu-dotnet

CDU Hub started off as a .NET Standard 2.0 library to read and write the WinWing
MCDU as a standalone display and keyboard. That library is `mcdu-dotnet` and it's
in the `library` folder. The README for it
[is here](library/mcdu-dotnet/README.md).

