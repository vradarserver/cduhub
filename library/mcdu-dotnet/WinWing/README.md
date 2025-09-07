# WinWing Panels

> [!NOTE]
> This is what I know about the USB packets that the devices send and/or
expect to receive. There are very large gaps in my understanding, this
is not comprehensive.
>
> If you can fill any of the gaps then please do.

These are the bits that all(*) WinWing panels have in common, more or less.

(*) "All" is stretching things a bit, I've only looked at two (so far).


## FCU and EFIS Notes

Notes regarding the FCU and EFIS panels [can be found here](FcuAndEfis/README.md).


## Command Prefix

Many commands begin with a two byte prefix. This prefix changes between panels.
I've been calling this the **Command Prefix** in the code. In these notes you
need to substitute references to `{CP}` with the appropriate two-byte code.

| Product | Command Prefix (hex) |
| ---     | --- |
| PFP-3N  | 31 bb (unconfirmed) |
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



## Fonts

What I did here was dump SimAppPro sending a font to the device and then pore
over the dump to figure out where the glyphs were being sent and how they were
arranged. I then wrote a utility to turn that dump into a structured packet map
(currently JSON, and very large) that identifies the positions of all of the
glpyhs, and then I have some code take any set of glyphs you like and write them
into the packet map. The packets are then replayed to the device. It works, but
it's not elegant. It would be better if the code could take the font glyphs and
build the required `{CP}` commands from them.

The "replay a modified set of packets" approach is not intended to be the end of
the matter, it's just a way of getting the fonts working so that I can concentrate
on other things.

I have some notes elsewhere on the `{CP}` commands that carry font glyphs to the
device, I'll write them up later. Those notes can be seen in code form in the
`extract-font` utility's `WinwingMcduUsbExtractor` class, which is what's responsible
for building the packet maps that mcdu-dotnet uses.


### Glyphs

The font glyphs sent to the device are 1 bit-per-pixel bitmaps. The height and width
of the bitmaps is variable. Widths that are not divisible by 8 have a final byte on
each row that is padded to keep the rows aligned to a byte.

There's no sub-pixel rendering, no kerning, no line height - if you want spacing
between the glyphs then you need to build that spacing into the glyph itself.



### X and Y offset

Part of the set of `{CP}` commands sent to describe the font includes the setting up
of the X and Y offsets for the display. These supply the top-left corner of the
display, the point at which the device will start drawing characters when F2 packets
get sent to it.


## Colour Palettes

Same story as per fonts. The `{CP}` commands that set the foreground and background
colours are easy to identify but they do not work in isolation, a pile of other
`{CP}` commands need to be sent before and after them, and if they are not sent then
things don't work.

The `{CP}` ... 19 01 command is responsible for setting foreground and background
colours. There are different flavours of 19 01 command but the "02" and "03"
are the ones of interest. Like previous commands if they run past the end of a
packet then the sequence continues at the start of the next packet. Each colour
must be sent in ordinal order.

```
{CP} 00 00 19 01 00 00 04 17 01 00 00 0e 00 00 00 FB 00 BB GG RR AA SS 00 00 00 00 00 00 00
```

where:

| Byte Code | Meaning |
| ---       | --- |
| FB        | 02 for foreground colours, 03 for background colours |
| BB        | Blue |
| GG        | Green |
| RR        | Red |
| AA        | Alpha (seems broken, SimAppPro always sends FF) |
| SS        | Incrementing sequence number across all 19 01 commands |

There are other 19 01 commands sent before and after the sequence of 02 and 03
commands but I don't know what they do.

Changing the palette has no effect on the existing display, you need to redraw it before
you will see the new colours.
