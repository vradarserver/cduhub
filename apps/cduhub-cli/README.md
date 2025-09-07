# CDU Hub Command-Line Interface

This runs the CDU Hub library from the command-line. It has no GUI and so
it should work under any operating system that Microsoft have released a
.NET Core 8 runtime for.



## Usage

`cduhub-cli.exe <command> <options>`

If no command is specified then `run` is assumed.



## Commands

### run

This runs the CDU Hub. By default it connects to the first CDU device that
it can find.

The program will keep running until you either press Q on the console or
use the QUIT option on the CDU device.

### list

Dumps lists of things.

#### devices

Dumps a list of devices that the HIDSharp library can detect. CDU Hub relies
upon HIDSharp for communication with the CDU, if HIDSharp can't see the CDU
then things are not going to work.

`--winwing`: Filters to only devices with the WinWing vendor ID (0x4098).

`--path`: Shows the device path.
