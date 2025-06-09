# CDUHUB

The `cduhub` application is a cross-platform .NET Core application for Windows,
Linux and Mac OS that:

* Exposes a menu system for the MCDU.
* Lets you switch between pages in the MCDU.
* Lets other applications add their own pages to the MCDU, both locally and
across the LAN.
* Can display and control the MCDU natively for a number of A320 simulators.
* Can be told to stop driving the MCDU so that other software can drive it.



## mcdu-dotnet

`cduhub` started off as a .NET Standard 2.0 library to read and write the WinWing
MCDU as a standalone display and keyboard. That library is `mcdu-dotnet` and it's
in the `library` folder. The README for it
[is here](library/mcdu-dotnet/README.md).

