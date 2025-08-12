# WinWing PFP-7

> [!NOTE]
> This is what I know about the USB packets that the devices send and/or
expect to receive. There are very large gaps in my understanding, this
is not comprehensive.
>
> If you can fill any of the gaps then please do.

These are the bits that are unique about the PFP-7. See
[WinWing Panels Readme](../README.md) for everything that it has in common
with other WinWing panels.

## Command Prefix

The command prefix is 0x33 0xbb.



## Key Bitflags

Offsets are zero-based in decimal from the start of the packet.

| Key              | Flag | Packet Byte Index |
| ---              | ---  | --- |
| LineSelectLeft1  | 0x01 | 1 |
| LineSelectLeft2  | 0x02 | 1 |
| LineSelectLeft3  | 0x04 | 1 |
| LineSelectLeft4  | 0x08 | 1 |
| LineSelectLeft5  | 0x10 | 1 |
| LineSelectLeft6  | 0x20 | 1 |
| LineSelectRight1 | 0x40 | 1 |
| LineSelectRight2 | 0x80 | 1 |
| LineSelectRight3 | 0x01 | 2 |
| LineSelectRight4 | 0x02 | 2 |
| LineSelectRight5 | 0x04 | 2 |
| LineSelectRight6 | 0x08 | 2 |
| InitRef          | 0x10 | 2 |
| Rte              | 0x20 | 2 |
| DepArr           | 0x40 | 2 |
| Altn             | 0x80 | 2 |
| VNav             | 0x01 | 3 |
| Dim              | 0x02 | 3 |
| Brt              | 0x04 | 3 |
| Fix              | 0x08 | 3 |
| Legs             | 0x10 | 3 |
| Hold             | 0x20 | 3 |
| FmcComm          | 0x40 | 3 |
| Prog             | 0x80 | 3 |
| Exec             | 0x01 | 4 |
| Menu             | 0x02 | 4 |
| NavRad           | 0x04 | 4 |
| PrevPage         | 0x08 | 4 |
| NextPage         | 0x10 | 4 |
| Digit1           | 0x20 | 4 |
| Digit2           | 0x40 | 4 |
| Digit3           | 0x80 | 4 |
| Digit4           | 0x01 | 5 |
| Digit5           | 0x02 | 5 |
| Digit6           | 0x04 | 5 |
| Digit7           | 0x08 | 5 |
| Digit8           | 0x10 | 5 |
| Digit9           | 0x20 | 5 |
| DecimalPoint     | 0x40 | 5 |
| Digit0           | 0x80 | 5 |
| PositiveNegative | 0x01 | 6 |
| A                | 0x02 | 6 |
| B                | 0x04 | 6 |
| C                | 0x08 | 6 |
| D                | 0x10 | 6 |
| E                | 0x20 | 6 |
| F                | 0x40 | 6 |
| G                | 0x80 | 6 |
| H                | 0x01 | 7 |
| I                | 0x02 | 7 |
| J                | 0x04 | 7 |
| K                | 0x08 | 7 |
| L                | 0x10 | 7 |
| M                | 0x20 | 7 |
| N                | 0x40 | 7 |
| O                | 0x80 | 7 |
| P                | 0x01 | 8 |
| Q                | 0x02 | 8 |
| R                | 0x04 | 8 |
| S                | 0x08 | 8 |
| T                | 0x10 | 8 |
| U                | 0x20 | 8 |
| V                | 0x40 | 8 |
| W                | 0x80 | 8 |
| X                | 0x01 | 9 |
| Y                | 0x02 | 9 |
| Z                | 0x04 | 9 |
| Space            | 0x08 | 9 |
| Del              | 0x10 | 9 |
| Slash            | 0x20 | 9 |
| Clr              | 0x40 | 9 |
