# WinWing Panels

> [!NOTE]
> This is what I know about the USB packets that the devices send and/or
expect to receive. There are very large gaps in my understanding, this
is not comprehensive.
>
> If you can fill any of the gaps then please do.

These are the bits that all(*) WinWing panels have in common, more or less.

(*) "All" is stretching things a bit, I've only looked at two (so far).



## Command Prefix

Many commands begin with a two byte prefix. This prefix changes between panels.
I've been calling this the **Command Prefix** in the code. In these notes you
need to substitute references to `{CP}` with the appropriate two-byte code.

| Product | Command Prefix (hex) |
| ---     | --- |
| MCDU    | 32 bb |
| PFP-7   | 33 bb |

It's only a sample of two so far, but it is interesting that the CPs look similar
to the USB products IDs, and also that the interval between the MCDU and PFP-7 CPs
is the same as the interval between their product IDs.


## Illumination

Setting the keyboard backlight, display backlight, LED brightness and
turning LEDs on or off all involves the same 14 byte packet to send
a {CP} ... 03 49 command.

The packet bytes are:

```
02 {CP} 00 00 03 49 <VARIABLE WORD> 00 00 00 00 00
```

The two bytes of the variable word vary depending on what you want to
control.

Brightness bytes range from 00 (off) to FF (full-on).

On / off values are either 0 (off) or 1 (on).

Note that setting the LED brightness to 0 will prevent any LED from displaying.


| Byte 1 | Byte 2                        | Device |
| ---    | ---                           | --- |
| 0x00   | Keyboard backlight brightness | ALL |
| 0x01   | Display backlight brightness  | ALL |
| 0x02   | LED brightness                | ALL |
| 0x03   | DSPY LED on / off             | PFP-7 |
| 0x04   | FAIL LED on / off             | PFP-7 |
| 0x05   | MSG LED on / off              | PFP-7 |
| 0x06   | OFST LED on / off             | PFP-7 |
| 0x07   | EXEC LED on / off             | PFP-7 |
| 0x08   | Fail LED on / off             | MCDU |
| 0x09   | FM LED on / off               | MCDU |
| 0x0a   | Mcdu LED on / off             | MCDU |
| 0x0b   | Menu LED on / off             | MCDU |
| 0x0c   | FM1 LED on / off              | MCDU |
| 0x0d   | IND LED on / off              | MCDU |
| 0x0e   | RDY LED on / off              | MCDU |
| 0x0f   | Blank / Line LED on / off     | MCDU |
| 0x10   | FM2 LED on / off              | MCDU |



## Display Output

These notes apply to all panels, there are no command prefixes, no device-specific
differences.

The screen is a 24 x 14 alphanumeric display. Individual cells on the
display are not directly addressable, you need to send the entire screen to
the device.

The screen is filled by sending multiple 64 byte packets. Each packet always
starts with 0xF2, and then a sequence is generated for each cell and appended
to the packet. When the packet is filled it is sent, even if it contains a partial
sequence for a cell - the remaining bytes of the sequence are sent at the start
of the next packet.

E.G:

```
F2 <cell sequence><cell sequence>...<partial cell sequence>
F2 <remainder of cell sequence><cell sequence> etc.
```

The last F2 packet is padded with zeros to 64 bytes before sending.



### Cell Sequence

The length of a cell sequence depends on the size of the codepoint for the
character occupying the cell.

| Length | Meaning |
| ---    | --- |
| 2      | Colour and font value, big-endian |
| 1-4    | UTF-8 byte sequence for the character, big-endian |

To calculate the colour and font value you start by looking up the foreground
colour ordinal:

| Ordinal | WinWing Default Colour |
| ---     | --- |
| 0       | Black |
| 1       | Amber |
| 2       | White |
| 3       | Cyan |
| 4       | Green |
| 5       | Magenta |
| 6       | Red |
| 7       | Yellow |
| 8       | Brown |
| 9       | Grey |
| 10      | Khaki |

Multiply the ordinal by 0x21 (33 decimal).

If the character is to be rendered in the large font then add 0.

If the character is to be rendered in the small font then add 0x16B (363).

If this is the first cell of the screen then add 1.

If this is the last cell of the screen then add 2.
