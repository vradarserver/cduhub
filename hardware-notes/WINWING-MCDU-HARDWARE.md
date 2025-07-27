# Hardware

I had a little peek inside but didn't dig too deeply. The main board is
silk-screened `WINWING WW-CDU-V1.00 2024\04` and it has a couple of ribbon
cables attached to the display, looks like one carries power and the other
data. It felt like the main board has a press-fit connector into another
board underneath, with three plastic lugs on the back case maybe holding it
in position... but as my unit is still working and I didn't fancy wrestling
with the ribbon cables I didn't try popping it out.

What was apparent though is that the device uses an ST Microelectronics
STM32F429IGT6 microcontroller. ST have some blurb about it here along with
the datasheet:

https://www.st.com/en/microcontrollers-microprocessors/stm32f429ig.html

The microcontroller seems to be able to drive the display and manage the
USB communications, it looks really interesting. I'm assuming that WinWing
are probably using some off-the-shelf library with this. It seems to be quite
a popular part, Google has loads of results for it. I can see why.

The other big chip visible on the main board was a Winbond W9825G6KH, which
is 256 Mbit of DRAM.
