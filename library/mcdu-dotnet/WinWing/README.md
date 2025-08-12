# WinWing Panels

These are the bits that all(*) WinWing panels have in common, more or less.

(*) "All" is stretching things a bit, I've only looked at two :)



## Command Prefix

Many commands begin with a two byte prefix. This prefix changes between panels.
I've been calling this the **Command Prefix** in the code. In these notes you
need to substitute references to `{CP}` with the appropriate two-byte code.

| Product | Command Prefix (hex) |
| ---     | --- |
| MCDU    | 32 bb |
| PFP-7   | 33 bb |



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
