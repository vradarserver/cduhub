<Query Kind="Statements" />

// Find packets in the output that start f0 00 ?? 3c
// The first 3c entry has a preamble - this is the preamble for the WWT font:
//     32 bb 00 00 07 01 00 00 be 90 06 00 00 0c 02 00 00 05 00 00 00 00 00 00 00 00 02 00 00
// and then you get the character code:
//     20 = space
// and then the next 90 bytes are the glyph bitmap arranged as 30 lines of 3 bytes. The next
// glyph immediately follows the last. Only look at the f0 00 ?? 3c lines.
//
// The font glyphs are interspersed with other row types, three packets with these headers:
//    f0 01 ?? 00  (the ?? continues the established sequence of ??)
//    f0 01 XX 00  (the XX is not in the established sequence, but appears to be in a sequence relative to other 2nd line f0 01 sets)
//    f0 01 YY 00  (as per XX)
//
// The first 3c following those has a preamble that is similar to but not the same as the first preamble - this
// is the first preamble that appears midway through 0x25's (%) glyph. It is 2 bytes longer (31 / 0x1f) than first one for space (29 / 0x1d)
//     32 bb 00 00 07 01 00 00 d4 90 06 00 00 0c 02 00 00 05 00 00 00 00 02 00 00 00 02 00 00 f0 01 (and previous is:)
//     32 bb 00 00 07 01 00 00 be 90 06 00 00 0c 02 00 00 05 00 00 00 00 00 00 00 00 02 00 00
//
// Next interruption is within 2b (+)'s glyph. This was interrupted by 4 lines and then the next 3c had this preamble:
//-----00-01-02-03-04-05-06-07-08-09-0a-0b-0c-0d-0e-0f-10-11-12-13-14-15-16-17-18-19-1a-1b-1c-1d-1e-----
//     32 bb 00 00 07 01 00 00 ea 90 06 00 00 0c 02 00 00 05 00 00 00 00 04 00 00 00 02 00 00 00 00 (and previous two are:)
//     32 bb 00 00 07 01 00 00 d4 90 06 00 00 0c 02 00 00 05 00 00 00 00 02 00 00 00 02 00 00 f0 01
//     32 bb 00 00 07 01 00 00 be 90 06 00 00 0c 02 00 00 05 00 00 00 00 00 00 00 00 02 00 00
//
// Whatever the preambles are they don't seem to be influenced by the content of the glyphs - I can change the bit patterns
// and the modified glyphs upload just fine.

var text = "000000000000000000000000000000000000000000"
        // assuming 31 preamble
         + "0000000000000038000038000038000038000038000fffe00fffe00fff"
         + "e0003800003800003800003800003800003800000000000000000000000000000000000000"
         ;

var bitsPerLine = 24;
var lineNumber = 0;
var bitNumber = 0;

void showLineNumber()
{
    Console.Write((++lineNumber).ToString("x4"));
    Console.Write(' ');
}

Console.Write("     ");
for(var idx = 0;idx < bitsPerLine;++idx) {
    var ch = ((idx + 1) % 10).ToString();
    if(ch == "0") {
        ch = ".";
    }
    Console.Write(ch);
}
Console.WriteLine();

foreach(var ch in text) {
    var nibble = ch >= '0' && ch <= '9'
        ? ch - '0'
        : ((char.ToLower(ch)) - 'a') + 10;
    for(var bit = 8;bit > 0;bit /= 2) {
        if(bitNumber == 0) {
            showLineNumber();            
        }
        var isolated = (nibble & bit) != 0 ? 1 : 0;
        Console.Write(isolated);
        if(++bitNumber == bitsPerLine) {
            Console.WriteLine();
            bitNumber = 0;
        }
    }
}
Console.WriteLine();