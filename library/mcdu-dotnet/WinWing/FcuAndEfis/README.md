# FCU and EFIS Panel USB Notes

These notes are for the Airbus FCU and EFIS panels.

## USB Vendor and Product IDs

The WinWing vendor ID is 0x4098.

The EFIS and FCU panels can be daisy-chained together, and the product ID
changes depending upon which devices are connected together.

The FCU unit can run standalone, but the EFIS units have no USB port and
must be attached to an FCU. All configurations must include an FCU.

The configuration doesn't affect which commands work. You can send commands
for a device that is not present.

| Configuration                | Product ID |
| ---                          | --- |
| FCU                          | 0xBB10 |
| FCU + Left EFIS              | 0xBC1D |
| FCU + Right EFIS             | 0xBC1E |
| FCU + Left EFIS + Right EFIS | 0xBA01 |

## Command Prefixes

Some of the commands apply to more than one panel. Those commands
distinguish between panels via the use of a command prefix. The
prefixes are:

| Marker | Hex ID | Device |
| ---    | ---    | --- |
| `{LE}` | `0DBF` | Left EFIS |
| `{FU}` | `10BB` | FCU |
| `{RE}` | `0EBF` | Right EFIS |

Where a command can apply to all three panels the notes will refer
to an `{ID}` marker. Subsitute in the appropriate two byte hex ID.


## Backlights and LEDs

These are set via an 02 packet of 14 bytes. The general form of the
packet is:

```
00 0102 03 04 05 06 0708 09 0a 0b 0c 0d
---------------------------------------
02 {ID} 00 00 03 49 {VV} 00 00 00 00 00
```

Replace `{ID}` with the command prefix for the panel.

Replace `{VV}` with the appropriate two byte variable value.


### Variable values for all devices

| Value   | Sets |
| ---     | --- |
| `00 VV` | Panel backlight (00 to FF) |
| `01 VV` | LCD backlight (00 to FF) |
| `11 VV` | Green LED backlight (00 to FF) |

### Variable values for both EFIS panels

| Value   | Sets |
| ---     | --- |
| `03 0V` | FD LED (0 = off, 1 = on) |
| `04 0V` | LS LED (0 = off, 1 = on) |
| `05 0V` | CSTR LED (0 = off, 1 = on) |
| `06 0V` | WPT LED (0 = off, 1 = on) |
| `07 0V` | VOR.D LED (0 = off, 1 = on) |
| `08 0V` | NDB LED (0 = off, 1 = on) |
| `09 0V` | ARPT LED (0 = off, 1 = on) |

### Variable values for FCU

| Value   | Sets |
| ---     | --- |
| `03 0V` | LOC LED (0 = off, 1 = on) |
| `05 0V` | AP1 LED (0 = off, 1 = on) |
| `07 0V` | AP2 LED (0 = off, 1 = on) |
| `09 0V` | A/THR LED (0 = off, 1 = on) |
| `0B 0V` | EXPED LED (0 = off, 1 = on) |
| `0D 0V` | APPR LED (0 = off, 1 = on) |
| `1E VV` | Yellow LED backlight (00 to FF) |



## LCD Segment Displays

All LCD segment displays are controlled via a 64 byte F0 report. The first
two bytes following the F0 are an incrementing sequence number. The notes
refer to this as `{SQ}`.


### EFIS

The command at byte 03 is 2B for both EFIS panels.

You cannot use the 2B command with the FCU. See later for instructions on the FCU.

The EFIS F0 packet has this shape:

```
00 0102 03 0405 06 07 08 09 0a 0b 0c 0d 0e 0f 10 11 12 13 14 15 16 17 18 19 1a 1b 1c 1d
---------------------------------------------------------------------------------------
f0 {SQ} 2b {ID} 00 00 02 01 00 00 11 11 11 00 00 09 00 00 00 00 00 00 00 XX XX XX XX XX (+34 zeros to pad to 64 bytes)
```

`{ID}` should be either `{LE}` or `{RE}` (see start of README).

The three bytes at 0c, 0d and 0e get filled with a varying value by SimAppPro, but seem
to work fine when each is just set to 11.

The bits of the 5 bytes starting at 0x19 control each of the LCD segments. A bit value of
1 turns the segment on, 0 turns it off.

The first 4 bytes correspond to the four digits of the air pressure setting. Each digit is
made up of exactly eight bits - 7 for the digit and 1 for the digit's trailing decimal point:

```
OFFSETS 19, 1A, 1B and 1C
BARO D1, D2, D3 and D4

  -10-
01    20
01    20
  -02-
04    40
04    40  80
  -08-    80

OFFSET 1D
01 = QFE
02 = QNH
04-80 = Unused
```

These are the values for the digits 0 through 9. Or the value with 0x80 to set a decimal
place after a digit.

These only work with the EFIS - the FCU is more complicated:

| Digit | Value |
| ---   | --- |
| 0     | 0x7D |
| 1     | 0x60 |
| 2     | 0x3E |
| 3     | 0x7A |
| 4     | 0x63 |
| 5     | 0x5B |
| 6     | 0x5F |
| 7     | 0x70 |
| 8     | 0x7F |
| 9     | 0x7B |



### FCU

You need to send two packets for the FCU. The first carries the payload, the second doesn't change
much but it won't work if it's not sent.

The FCU F0 packets have this shape (again, `{SQ}` is a two byte incrementing sequence number):

```
00 0102 03 0405 06 07 08 09 0a 0b 0c 0d 0e 0f 10 11 12 13 14 15 16 17 18 19 1a 1b 1c 1d 1e 1f 20 21 22 23 24 25 26 27 28 29
---------------------------------------------------------------------------------------------------------------------------
f0 {SQ} 31 {FU} 00 00 02 01 00 00 49 56 00 00 00 20 00 00 00 00 00 00 00 XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX (+23 zeros to pad to 64 bytes)
f0 {SQ} 11 {FU} 00 00 03 01 00 00 49 56 (+51 zeros to pad to 64 bytes)
```

The bits of the 16 bytes starting at 0x19 control each of the LCD segments. A bit value of
1 turns the segment on, 0 turns it off.

The SPD digits are the only ones where each digit corresponds to a byte in the payload. All of the
other digits are spread over the high nibble of one byte and the low nibble of the following byte.

Where the notes refer to two offsets (E.G. "1C/1D") it indicates that you take the high nibble bits
from 1C and the low nibble bits from 1D to form the corresponding digit.

Put another way - for this example:

```
OFFSET=1C/1D
HDG D1      

      -08-  
    80    04
    80    04
      -40-  
    20    02
10  20    02
10    -01-  
```

The digit 5 would need to have the 08, 80, 40, 02 and 01 bits set.

The high nibble bits are 80 + 40 = C0.

The low nibble bits are 08 + 02 + 01 = 0B.

To set the 1st digit of HDG to 5 you would set the top four bits at offset 1C to C0, 
and the bottom four bits at 1D to 0B.

When digits are split over two bytes WinWing will typically use the 1st bit of the top
nibble to toggle a segment corresponding to an entire word or symbol.


```
OFFSET=19        OFFSET=1A       OFFSET=1B
SPD D1           SPD D2          SPD D3
                                 
      -80-             -80-            -80-
    08    40         08    40        08    40
    08    40         08    40        08    40
      -04-             -04-            -04-
    02    20         02    20        02    20
01  02    20     01  02    20    01  02    20
01    -10-       01    -10-      01    -10-  

OFFSET=1C
01 = unused
02 = SPD DOT
04 = MACH
08 = SPD

OFFSET=1C/1D     OFFSET=1D/1E    OFFSET=1E/1F
HDG D1           HDG D2          HDG D3

      -08-             -08-            -08-
    80    04         80    04        80    04
    80    04         80    04        80    04
      -40-             -40-            -40-
    20    02         20    02        20    02
10  20    02     10  20    02    10  20    02
10    -01-       10    -01-      10    -01-

OFFSET=1F (high nibble)
10 = HDG DOT
20 = LAT
40 = TRK (heading select)
80 = HDG (heading select)

OFFSET=20 (low nibble)
01 = FPA
02 = TRK (HDG/TRK select)
04 = V/S
08 = HDG (HDG/TRK select)
10 = unused

OFFSET=20/21   OFFSET=21/22   OFFSET=22/23   OFFSET=23/24   OFFSET=24/25
ALT D1         ALT D2         ALT D3         ALT D4         ALT D5
               10 = ALT       10 = +---      10 = LVL/CH    10 = ---+

    -08-         -08-           -08-           -08-           -08-
  80    04     80    04       80    04       80    04       80    04
  80    04     80    04       80    04       80    04       80    04
    -40-         -40-           -40-           -40-           -40-
  20    02     20    02       20    02       20    02       20    02
  20    02     20    02       20    02       20    02       20    02
    -01-         -01-           -01-           -01-           -01-

OFFSET=25/26   OFFSET=26/27   OFFSET=27/28   OFFSET=28/29
V/S D1         V/S D2         V/S D3         V/S D4
10 = MINUS                    10 = PIPE      10 = ALT DOT

    -08-             -08-           -08-           -08-
  80    04         80    04       80    04       80    04
  80    04         80    04       80    04       80    04
    -40-             -40-           -40-           -40-
  20    02         20    02       20    02       20    02
  20    02     10  20    02       20    02       20    02
    -01-       10    -01-           -01-           -01-

OFFSET=29 (high nibble)
10 = unused
20 = unused
40 = V/S
80 = FPA
```


## Keys

The device constantly sends a stream of 01 reports. The reports are 25
bytes including the 01 report code.

The bits corresponding to each button are as follows (offset includes
the 01 report code and are zero based):

| Key               | Flag | Packet Byte Index |
| ---               | ---  | --- |
| FcuSpdMach        | 0x01 | 1 |
| FcuLoc            | 0x02 | 1 |
| FcuHdgTrkVsFpa    | 0x04 | 1 |
| FcuAp1            | 0x08 | 1 |
| FcuAp2            | 0x10 | 1 |
| FcuAThr           | 0x20 | 1 |
| FcuExped          | 0x40 | 1 |
| FcuMetricAlt      | 0x80 | 1 |
| FcuAppr           | 0x01 | 2 |
| FcuSpdDec         | 0x02 | 2 |
| FcuSpdInc         | 0x04 | 2 |
| FcuSpdPush        | 0x08 | 2 |
| FcuSpdPull        | 0x10 | 2 |
| FcuHdgDec         | 0x20 | 2 |
| FcuHdgInc         | 0x40 | 2 |
| FcuHdgPush        | 0x80 | 2 |
| FcuHdgPull        | 0x01 | 3 |
| FcuAltDec         | 0x02 | 3 |
| FcuAltInc         | 0x04 | 3 |
| FcuAltPush        | 0x08 | 3 |
| FcuAltPull        | 0x10 | 3 |
| FcuVsDec          | 0x20 | 3 |
| FcuVsInc          | 0x40 | 3 |
| FcuVsPush         | 0x80 | 3 |
| FcuVsPull         | 0x01 | 4 |
| FcuAlt100         | 0x02 | 4 |
| FcuAlt1000        | 0x04 | 4 |
| `<unused>`        | 0x08 | 4 |
| `<unused>`        | 0x10 | 4 |
| `<unused>`        | 0x20 | 4 |
| `<unused>`        | 0x40 | 4 |
| `<unused>`        | 0x80 | 4 |
| LeftFd            | 0x01 | 5 |
| LeftLs            | 0x02 | 5 |
| LeftCstr          | 0x04 | 5 |
| LeftWpt           | 0x08 | 5 |
| LeftVorD          | 0x10 | 5 |
| LeftNdb           | 0x20 | 5 |
| LeftArpt          | 0x40 | 5 |
| LeftBaroPush      | 0x80 | 5 |
| LeftBaroPull      | 0x01 | 6 |
| LeftBaroDec       | 0x02 | 6 |
| LeftBaroInc       | 0x04 | 6 |
| LeftInHg          | 0x08 | 6 |
| LeftHPa           | 0x10 | 6 |
| LeftModeLs        | 0x20 | 6 |
| LeftModeVor       | 0x40 | 6 |
| LeftModeNav       | 0x80 | 6 |
| LeftModeArc       | 0x01 | 7 |
| LeftModePlan      | 0x02 | 7 |
| LeftRange10       | 0x04 | 7 |
| LeftRange20       | 0x08 | 7 |
| LeftRange40       | 0x10 | 7 |
| LeftRange80       | 0x20 | 7 |
| LeftRange160      | 0x40 | 7 |
| LeftRange320      | 0x80 | 7 |
| LeftNeedle1Adf    | 0x01 | 8 |
| LeftNeedle1Off    | 0x02 | 8 |
| LeftNeedle1Vor    | 0x04 | 8 |
| LeftNeedle2Adf    | 0x08 | 8 |
| LeftNeedle2Off    | 0x10 | 8 |
| LeftNeedle2Vor    | 0x20 | 8 |
| `<unused>`        | 0x40 | 8 |
| `<unused>`        | 0x80 | 8 |
| RightFd           | 0x01 | 9 |
| RightLs           | 0x02 | 9 |
| RightCstr         | 0x04 | 9 |
| RightWpt          | 0x08 | 9 |
| RightVorD         | 0x10 | 9 |
| RightNdb          | 0x20 | 9 |
| RightArpt         | 0x40 | 9 |
| RightBaroPush     | 0x80 | 9 |
| RightBaroPull     | 0x01 | 10 |
| RightBaroDec      | 0x02 | 10 |
| RightBaroInc      | 0x04 | 10 |
| RightInHg         | 0x08 | 10 |
| RightHPa          | 0x10 | 10 |
| RightModeLs       | 0x20 | 10 |
| RightModeVor      | 0x40 | 10 |
| RightModeNav      | 0x80 | 10 |
| RightModeArc      | 0x01 | 11 |
| RightModePlan     | 0x02 | 11 |
| RightRange10      | 0x04 | 11 |
| RightRange20      | 0x08 | 11 |
| RightRange40      | 0x10 | 11 |
| RightRange80      | 0x20 | 11 |
| RightRange160     | 0x40 | 11 |
| RightRange320     | 0x80 | 11 |
| RightNeedle1Vor   | 0x01 | 12 |
| RightNeedle1Off   | 0x02 | 12 |
| RightNeedle1Adf   | 0x04 | 12 |
| RightNeedle2Vor   | 0x08 | 12 |
| RightNeedle2Off   | 0x10 | 12 |
| RightNeedle2Adf   | 0x20 | 12 |
