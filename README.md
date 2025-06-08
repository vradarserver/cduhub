# MCDU-DOTNET

The WinWing MCDU is a flight sim peripheral that looks and behaves like an
Airbus MCDU panel:

https://uk.winwingsim.com/view/goods-details.html?id=945

It has a 24 x 14 alphanumeric display, two ambient light sensors and 74 backlit
buttons. It looks really cool.

The only problem is that, as far as I can see, there are no open source
libraries for using it as a display and/or an input device. The closest I have
found is MobiFlight (https://www.mobiflight.com/en/index.html), but there are a
couple of problems with it:

1. MobiFlight is open source but its interaction with the MCDU is via a DLL that
ships with the source called `MobiFlightWwFcu.dll`. I can't find the source for
that DLL.

2. You can link to the DLL and interact with it as per usual, but the
way you send output to the device is by starting a WebSocket server, pass the
WebSocket server to the MCDU object and then send messages to the WebSocket
server. It is fine for its intended purpose, but a bit clunky for a program that
just wants to drive the display.

The intention here is to have a small .NET Standard 2.0 library that uses
HidSharp (https://www.zer7.com/software/hidsharp) to interact with the device,
and for the library to be cross-platform.

**(Turns out HidSharp buffers input reports internally, which makes for a laggy
input experience when they're coming in every 1/100th of a second - I'm working
around it for now but might need to switch to something that supports unbuffered
input)**



## USB Packet Sniffing Notes

These are observations made using WireShark and USBPcap while the device is being
driven by either MobiFlight or SimAppPro.

The device has the vendor ID 0x4098 and one of three product IDs depending on
how it's been configured:

| Position      | Product ID |
| ---           | --- |
| Captain       | 0xBB36 |
| First Officer | 0xBB3E |
| Observer      | 0xBB3A |

The device has three endpoints. The 0 endpoint responds to configuration
requests, the 1 endpoint behaves as a joystick and the 2 endpoint is the
display.

Neither MobiFlight nor SimAppPro seem to be sending feature requests to
endpoint 0.



### Initialising the Display

Both applications begin by sending a set of initialisation packets. If these
packets are omitted then the device will not display output.



#### MobiFlight Initialisation

MobiFlight's is shorter than SimAppPro's. It starts with 17x 64 byte payloads
for report code F0:

```
01 HID Data: f0 0001 3832bb00001e0100005f633100000000000032bb0000180100005f6331000008000000340018000e00180032bb0000190100005f633100000e00000000
02 HID Data: f0 0002 38000000010005000000020000000000000032bb0000190100005f633100000e000000010006000000030000000000000032bb00001901000000000000
03 HID Data: f0 0003 385f633100000e0000000200000000ff040000000000000032bb0000190100005f633100000e000000020000a5ffff050000000000000032bb00000000
04 HID Data: f0 0004 380000190100005f633100000e0000000200ffffffff060000000000000032bb0000190100005f633100000e0000000200ffff00ff0700000000000000
05 HID Data: f0 0005 380000000032bb0000190100005f633100000e00000002003dff00ff080000000000000032bb0000190100005f633100000e0000000200ff6300000000
06 HID Data: f0 0006 38ffff090000000000000032bb0000190100005f633100000e00000002000000ffff0a0000000000000032bb0000190100005f633100000e0000000000
07 HID Data: f0 0007 380000020000ffffff0b0000000000000032bb0000190100005f633100000e0000000200425c61ff0c0000000000000032bb0000190100005f00000000
08 HID Data: f0 0008 38633100000e0000000200777777ff0d0000000000000032bb0000190100005f633100000e00000002005e7379ff0e0000000000000032bb0000000000
09 HID Data: f0 0009 3800190100005f633100000e0000000300000000ff0f0000000000000032bb0000190100005f633100000e000000030000a5ffff100000000000000000
0a HID Data: f0 000a 3800000032bb0000190100005f633100000e0000000300ffffffff110000000000000032bb0000190100005f633100000e0000000300ffff0000000000
0b HID Data: f0 000b 38ff120000000000000032bb0000190100005f633100000e00000003003dff00ff130000000000000032bb0000190100005f633100000e000000000000
0c HID Data: f0 000c 38000300ff63ffff140000000000000032bb0000190100005f633100000e00000003000000ffff150000000000000032bb0000190100005f6300000000
0d HID Data: f0 000d 383100000e000000030000ffffff160000000000000032bb0000190100005f633100000e0000000300425c61ff170000000000000032bb000000000000
0e HID Data: f0 000e 38190100005f633100000e0000000300777777ff180000000000000032bb0000190100005f633100000e00000003005e7379ff19000000000000000000
0f HID Data: f0 000f 38000032bb0000190100005f633100000e0000000400000000001a0000000000000032bb0000190100005f633100000e00000004000100000000000000
10 HID Data: f0 0010 381b0000000000000032bb0000190100005f633100000e0000000400020000001c0000000000000032bb00001a0100005f633100000100000000000000
11 HID Data: f0 0011 120232bb00001c0100005f6331000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
```

and then follows that up with two 14 byte payloads with report code
02:

```
01 HID Data: 0232bb00000349 00cc 0000000000
02 HID Data: 0232bb00000349 01ff 0000000000
```

After this the device is ready for display.


#### SimAppPro Initialisation

This was the sequence I observed - however, if I send this to the device then it
clears the startup logo (which the MobiFlight sequence does not do) but I can't
drive the screen.

```
01 HID Data: 0201000000010100000000000000
02 HID Data: 0201000000010200000000000000
03 HID Data: 0201000000010300000000000000
04 HID Data: 020100000004059c000000000000
05 HID Data: 02010000000405a0000000000000
06 HID Data: 02010000000405a4000000000000
07 HID Data: 0201000000040504000000000000
08 HID Data: 0201000000011800000000000000
09 HID Data: f0020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
```

then 3 seconds later:

```
01 HID Data: 0201000000010100000000000000
02 HID Data: 0201000000010200000000000000
03 HID Data: 0201000000010300000000000000
04 HID Data: 020100000004059c000000000000
05 HID Data: 02010000000405a0000000000000
06 HID Data: 02010000000405a4000000000000
07 HID Data: 0201000000040504000000000000
08 HID Data: 0201000000011800000000000000
09 HID Data: f0020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0a HID Data: 0201000000010000000000000000
0b HID Data: 0201000000010100000000000000
0c HID Data: 0201000000010200000000000000
0d HID Data: 0201000000010300000000000000
0e HID Data: 020100000004059c000000000000
0f HID Data: 02010000000405a0000000000000
10 HID Data: 02010000000405a4000000000000
11 HID Data: 0201000000040504000000000000
12 HID Data: 0201000000011800000000000000
13 HID Data: f0020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
```

then 0.5 seconds later:

```
01 HID Data: f000011532bb000012010000860900000004000000ff06070d000000000000000000000000000000000000000000000000000000000000000000000000000000
02 HID Data: f000023832bb000013010000860900000004000000ff06070d32bb000010010000860900000008000000000000008002e00132bb000003010000860900000000
03 HID Data: f00003180000000000000032bb000005010000860900000100000000000000000000000000000000000000000000000000000000000000000000000000000000
04 HID Data: f0010400000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
05 HID Data: f0010100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
06 HID Data: f0010100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
07 HID Data: 0232bb000003490f000000000000
08 HID Data: 0232bb0000034908000000000000
09 HID Data: 0232bb0000034909000000000000
0a HID Data: 0232bb000003490c000000000000
0b HID Data: 0232bb0000034910000000000000
0c HID Data: 0232bb000003490d000000000000
0d HID Data: 0232bb000003490a000000000000
0e HID Data: 0232bb000003490e000000000000
0f HID Data: 0232bb0000034902ff0000000000
```

then 2 seconds later and every 3 seconds after that:

```
01 HID Data: 0201000000010000000000000000
02 HID Data: 0201000000010000000000000000
03 HID Data: 0201000000010000000000000000
```

### Heartbeat

Both MobiFlight and SimAppPro send a 14 byte 02 packet roughly every three
seconds, which I'm assuming is a heartbeat.

I've yet to observe any ill effect from not sending it. The display remains
active without it, it continues to show whatever was last sent.

The difference between the two is MobiFlight sends it twice in a row whereas
SimAppPro sends it three times:

```
0201000000010000000000000000
```

### The Character Display

Once the display has been successfully initialised both applications send
multiple F2 packets to populate the display. Each character is represented
by a two byte sequence that determines colour and font size, followed by one
to three bytes of UTF8.

#### Colour and Font Code

This is a two-byte sequence which indicates the colour of the character,
whether it is rendered in the small or large font and whether it is the
first character being rendered to the screen, the last character being
rendered or neither.

The first character on screen adds 1 to the first byte.

The last character on screen adds 2 to the first byte.

The full set of observed codes are:

| Value | Colour  | Font  |
| ---   | ---     | ---   |
| 2100  | Amber   | Large |
| 8C01  | Amber   | Small |
| 0801  | Brown   | Large |
| 7302  | Brown   | Small |
| 6300  | Cyan    | Large |
| CE01  | Cyan    | Small |
| 8400  | Green   | Large |
| EF01  | Green   | Small |
| 2901  | Grey    | Large |
| 9402  | Grey    | Small |
| 4A01  | Khaki   | Large |
| B502  | Khaki   | Small |
| A500  | Magenta | Large |
| 1002  | Magenta | Small |
| C600  | Red     | Large |
| 3102  | Red     | Small |
| 4200  | White   | Large |
| AD01  | White   | Small |
| E700  | Yellow  | Large |
| 5202  | Yellow  | Small |

So for example, if you want a large yellow A (0x41 in UTF8) then the
sequence for the character cell would be:

+ First character on screen: `E8 00 41`
+ Last character on screen: `E9 00 41`
+ Any other position on screen: `E7 00 41`

The characters are sent sequentially from top-left to bottom right. If a packet
is shorter than 64 bytes then the remainder is padded out with zeros. Multi-byte
UTF8 is sent big-endian.

The first/last character markers are used even if the full set of characters does
not fill the screen. If you don't fill the screen then whatever was
previously on display stays on display.



### Input Reports

Input reports with a 25 byte payload including the report code 01 are sent every
1/100th of a second. E.G.:

```
HID Data: 0101000000000000000000000000000000ca02c602ca02c602
```

So far I've figured out this for the payload, offsets & lengths in decimal:

| Offset | Length | Meaning |
| ---    | ---    | --- |
| 0      | 1      | 01 = report type |
| 1      | 10     | Key bitflags |
| 11     | 5      | ?? |
| 16     | 2      | Little endian left ambient sensor value  |
| 18     | 2      | Little endian right ambient sensor value |
| 20     | 4      | Previous 4 bytes repeated? |

I'm guessing the LED states and backlight level are in there somewhere.

#### Key Bitflags

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



### Backlight and LEDs

Part of the initialisation sent for MobiFlight was this:

```
01 HID Data: 0232bb00000349 00cc 0000000000
02 HID Data: 0232bb00000349 01ff 0000000000
```

Turns out that this report controls the backlight and LEDs. The first of the two
isolated bytes indicates which value to set (it is a discrete value, not a flag)
and the second indicates the value. For LEDs 0 = off, 1 = on. For brightness
values 0 = off, FF = full.

All values are in hex.

| First Byte | Value | Controls |
| ---        | ---   | --- |
| 00         | 0-FF  | Button backlight |
| 01         | 0-FF  | Display backlight |
| 02         | 0-FF  | LED backlight |
| 03-07      | ??    | ?? |
| 08         | 0/1   | FAIL LED |
| 09         | 0/1   | FM LED |
| 0A         | 0/1   | MCDU LED |
| 0B         | 0/1   | MENU LED |
| 0C         | 0/1   | FM1 LED |
| 0D         | 0/1   | IND LED |
| 0E         | 0/1   | RDY LED |
| 0F         | 0/1   | LINE LED |
| 10         | 0/1   | FM2 LED |
| 11-FF      | ??    | ?? |
