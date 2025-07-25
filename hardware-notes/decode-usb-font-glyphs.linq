<Query Kind="Statements" />

// Find packets in the output that start f0 00 ?? 3c
// The first 3c entry has a preamble - this is the preamble for the WWT font:
//     32 bb 00 00 07 01 00 00 be 90 06 00 00 0c 02 00 00 05 00 00 00 00 00 00 00 00 02 00 00
// and then you get the character code over 4 bytes:
//     20 00 00 00 = space (little endian)
// and then the next 90 bytes are the glyph bitmap arranged as 29 lines of 3 bytes. The glyph
// is actually 23 bits wide, the last bit of each line is ignored. The next glyph immediately
// follows the last. Only look at the f0 00 ?? 3c lines.
//
// The font glyphs are interspersed with other row types, three packets with these headers:
//    f0 01 ?? 00  (the ?? continues the established sequence of ??)
//    f0 01 XX 00  (the XX is not in the established sequence, but appears to be in a sequence relative to other 2nd line f0 01 sets)
//    f0 01 YY 00  (as per XX)
//
// The first 3c following those has a preamble that is similar to but not the same as the first preamble - these are the first few
// preambles collected together, you can see the pattern:
// 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d      0e 0f 10 11 12 13 14 15 16 17 18 19 1a      1b 1c 1d 1e 1f 20
// -- -- -- -- -- -- -- -- -- -- -- -- -- --      -- -- -- -- -- -- -- -- -- -- -- -- --      -- -- -- -- -- --
// f0 00 ce 3c 32 bb 00 00 07 01 00 00 be 90      06 00 00 0c 02 00 00 05 00 00 00 00 00      00 00 00 02 00 00
// f0 00 d9 3c 32 bb 00 00 07 01 00 00 d4 90 +16  06 00 00 0c 02 00 00 05 00 00 00 00 02 +02  00 00 00 02 00 00
// f0 00 e4 3c 32 bb 00 00 07 01 00 00 ea 90 +16  06 00 00 0c 02 00 00 05 00 00 00 00 04 +02  00 00 00 02 00 00
// f0 00 ef 3c 32 bb 00 00 07 01 00 00 02 91 +18  06 00 00 0c 02 00 00 05 00 00 00 00 06 +02  00 00 00 02 00 00
// f0 00 fa 3c 32 bb 00 00 07 01 00 00 19 91 +17  06 00 00 0c 02 00 00 05 00 00 00 00 08 +02  00 00 00 02 00 00
// f0 00 05 3c 32 bb 00 00 07 01 00 00 2f 91 +16  06 00 00 0c 02 00 00 05 00 00 00 00 0a +02  00 00 00 02 00 00
// f0 00 10 3c 32 bb 00 00 07 01 00 00 47 91 +18  06 00 00 0c 02 00 00 05 00 00 00 00 0c +02  00 00 00 02 00 00
// f0 00 1b 3c 32 bb 00 00 07 01 00 00 5d 91 +16  06 00 00 0c 02 00 00 05 00 00 00 00 0e +02  00 00 00 02 00 00
// f0 00 26 3c 32 bb 00 00 07 01 00 00 75 91 +18  06 00 00 0c 02 00 00 05 00 00 00 00 10 +02  00 00 00 02 00 00
//       SEQ                           LL HH                                          ^^
//
// It is clear that all the preambles are the same length - but if I start taking bits straight after the end of the preamble then
// (a) I am one byte short of getting me to the start of the next character and (b) the first two or three bytes after the preamble are
// wrong.
//
// OK - turns out when the 3C stream is interrupted the first packet in the interruption is an f0 00 xx 12 packet. The first byte
// after the 12 is the missing byte:
//
// 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 10 11
// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
// f0 00 d7 12 cf 32 bb 00 00 05 01 00 00 be 90 06 00 01 (zero padding to 0x40 bytes)
// f0 00 e2 12 00 32 bb 00 00 05 01 00 00 d4 90 06 00 01
//             BIT
//
// The portion after the missing byte is very similar to the start of the following preamble, E.G.:
// f0 00 d7 12 cf 32 bb 00 00 05 01 00 00 be 90 06 00 01
// f0 00 ce 3c    32 bb 00 00 07 01 00 00 be 90 06 00 00
//
// That pattern repeats for all of the large font glyphs. Then things get freaky before the small font begins. It looks
// like I definitely need to figure these preamble blocks out. Also it's wrong to describe them as a preamble, because
// on the small font the "preamble" preceeding the small space glyph starts at the end of a packet that contains an
// earlier preamble, and finishes at the start of the next packet.
//
// In the case of the small font the interruption in the 3c stream takes the same form as before - lots of zeros - and
// then the next preamble starts at the beginning of the packet.
// 

var text = "00000000000000000000f00000f00000f00000f00000f00000f00000f00000f00000f00000f00000f00000f00000f00000f00000f00000f0"
         //+ "\n"
         + "0000000000000000000000f00000f00000f00000f000000000000000000000"
         //+ "f001de70039c78031c78071c780e1c701c1ef01c0fe0380100000000000000"
         //+ "\n"
         //+ "000000"
         ;

var showBitsPerLine = 24;
var actualBitsPerLine = 23;
var lineNumber = 0;
var bitNumber = 0;

void showLineNumber()
{
    Console.Write((++lineNumber).ToString("x4"));
    Console.Write(' ');
}

Console.Write("     ");
for(var idx = 0;idx < showBitsPerLine;++idx) {
    var ch = ((idx + 1) % 10).ToString();
    if(ch == "0") {
        ch = ".";
    }
    Console.Write(ch);
}
Console.WriteLine();

foreach(var ch in text) {
    if(ch == '\n') {
        Console.WriteLine();
        if(bitNumber > 0) {
            Console.Write("     ");
            for(var idx = 0;idx < bitNumber;++idx) {
                Console.Write(' ');
            }
        }
    } else {
        var nibble = ch >= '0' && ch <= '9'
            ? ch - '0'
            : ((char.ToLower(ch)) - 'a') + 10;
        for(var bit = 8;bit > 0;bit /= 2) {
            if(bitNumber == 0) {
                showLineNumber();            
            }
            if(bitNumber >= actualBitsPerLine) {
                Console.Write('.');
            } else {
                var isolated = (nibble & bit) != 0 ? 1 : 0;
                Console.Write(isolated);
            }
            if(++bitNumber == showBitsPerLine) {
                Console.WriteLine();
                bitNumber = 0;
            }
        }
    }
}
Console.WriteLine();