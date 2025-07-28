<Query Kind="Statements">
  <Namespace>System.Windows</Namespace>
</Query>

// Fenix font is 9x15

var pixelBitmap = new string[] {
 //      C
 //  123456789
    "..XXXXX..",
    ".XX...XX.",
    "XX.....XX",
    "X.......X",
    "........X",
    "........X",
    ".......X.",
    "......X..",
    ".....X...",
    "....X....",
    "...X.....",
    "..X......",
    ".X.......",
    "X........",
    "XXXXXXXXX",
};
var xFactor = 2;
var yFactor = 2;
var leftOffset = 1;
var glyphWidth = 21;
var glyphHeight = 31;
var prefix = "        \"";
var suffix = "\"";

var output = new StringBuilder();

var extraLines = glyphHeight - (pixelBitmap.Length * yFactor);
if(extraLines < 0) {
    Console.WriteLine("TOO TALL");
    Console.WriteLine(glyphHeight);
    Console.WriteLine(pixelBitmap.Length * yFactor);
}
var blankLine = $"{prefix}{new String('.', glyphWidth)}{suffix},";
for(var i = 0;i < extraLines;++i) {
    output.AppendLine(blankLine);
}

for(var lineIdx = 0;lineIdx < pixelBitmap.Length;++lineIdx) {
    var line = pixelBitmap[lineIdx];
    var lineBuffer = new StringBuilder(prefix);
    for(var i = 0;i < leftOffset;++i) {
        lineBuffer.Append('.');
    }
    foreach(var ch in line) {
        for(var i = 0;i < xFactor;++i) {
            lineBuffer.Append(ch);
        }
    }
    while(lineBuffer.Length < glyphWidth + prefix.Length) {
        lineBuffer.Append('.');
    }
    lineBuffer.Append(suffix);
    for(var i = 0;i < yFactor;++i) {
        output.Append(lineBuffer.ToString());
        var lineSuffix = i + 1 < yFactor || lineIdx + 1 < pixelBitmap.Length
            ? ","
            : "";
        output.AppendLine(lineSuffix);
    }
}

Console.WriteLine(output.ToString());
Clipboard.SetText(output.ToString());