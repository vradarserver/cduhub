# WinWing MCDU

> [!NOTE]
> This is what I know about the USB packets that the devices send and/or
expect to receive. There are very large gaps in my understanding, this
is not comprehensive.
>
> If you can fill any of the gaps then please do.

These are the bits that are unique about the MCDU. See
[WinWing Panels Readme](../README.md) for everything that it has in common
with other WinWing panels.

## Command Prefix

The command prefix is 0x32 0xbb.



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
| Dir              | 0x10 | 2 |
| Prog             | 0x20 | 2 |
| Perf             | 0x40 | 2 |
| Init             | 0x80 | 2 |
| Data             | 0x01 | 3 |
| Blank1           | 0x02 | 3 |
| Brt              | 0x04 | 3 |
| FPln             | 0x08 | 3 |
| RadNav           | 0x10 | 3 |
| FuelPred         | 0x20 | 3 |
| SecFPln          | 0x40 | 3 |
| AtcComm          | 0x80 | 3 |
| McduMenu         | 0x01 | 4 |
| Dim              | 0x02 | 4 |
| Airport          | 0x04 | 4 |
| Blank2           | 0x08 | 4 |
| LeftArrow        | 0x10 | 4 |
| UpArrow          | 0x20 | 4 |
| RightArrow       | 0x40 | 4 |
| DownArrow        | 0x80 | 4 |
| Digit1           | 0x01 | 5 |
| Digit2           | 0x02 | 5 |
| Digit3           | 0x04 | 5 |
| Digit4           | 0x08 | 5 |
| Digit5           | 0x10 | 5 |
| Digit6           | 0x20 | 5 |
| Digit7           | 0x40 | 5 |
| Digit8           | 0x80 | 5 |
| Digit9           | 0x01 | 6 |
| DecimalPoint     | 0x02 | 6 |
| Digit0           | 0x04 | 6 |
| Negative         | 0x08 | 6 |
| A                | 0x10 | 6 |
| B                | 0x20 | 6 |
| C                | 0x40 | 6 |
| D                | 0x80 | 6 |
| E                | 0x01 | 7 |
| F                | 0x02 | 7 |
| G                | 0x04 | 7 |
| H                | 0x08 | 7 |
| I                | 0x10 | 7 |
| J                | 0x20 | 7 |
| K                | 0x40 | 7 |
| L                | 0x80 | 7 |
| M                | 0x01 | 8 |
| N                | 0x02 | 8 |
| O                | 0x04 | 8 |
| P                | 0x08 | 8 |
| Q                | 0x10 | 8 |
| R                | 0x20 | 8 |
| S                | 0x40 | 8 |
| T                | 0x80 | 8 |
| U                | 0x01 | 9 |
| V                | 0x02 | 9 |
| W                | 0x04 | 9 |
| X                | 0x08 | 9 |
| Y                | 0x10 | 9 |
| Z                | 0x20 | 9 |
| Slash            | 0x40 | 9 |
| Space            | 0x80 | 9 |
| Ovfy             | 0x01 | 10 |
| Clr              | 0x02 | 10 |


# X and Y Offsets

On the MCDU if you set a glyph height of 31 and a Y offset of 0 then the 14 lines
of output line up pretty well with the line-select buttons.
