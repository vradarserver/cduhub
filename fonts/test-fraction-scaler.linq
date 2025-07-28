<Query Kind="Statements" />

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
//foreach(var line in pixelBitmap) {
//    Console.WriteLine(line);
//}
//Console.WriteLine();
var designX = pixelBitmap[0].Length;
var designY = pixelBitmap.Length;

var multiplyBy = 7;
var divideBy = 5;

var multiplyX = designX * multiplyBy;
var multiplyY = designY * multiplyBy;
var largeBitmap = new List<string>();
foreach(var pixelLine in pixelBitmap) {
    var buffer = new StringBuilder();
    foreach(var ch in pixelLine) {
        for(var i = 0;i < multiplyBy;++i) {
            buffer.Append(ch);
        }
    }
    for(var i = 0;i < multiplyBy;++i) {
        largeBitmap.Add(buffer.ToString());
    }
}

//foreach(var line in largeBitmap) {
//    Console.WriteLine(line);
//}
//Console.WriteLine();

var scaledBitmap = new List<string>();
for(var lineIdx = 0;lineIdx + divideBy <= multiplyY;lineIdx += divideBy) {
    var lineBuffer = new StringBuilder();
    var lineEnd = lineIdx + divideBy;
    for(var colIdx = 0;colIdx + divideBy <= multiplyX;colIdx += divideBy) {
        var colEnd = colIdx + divideBy;
        
        var countCells = 0;
        var countSet = 0;
        
        for(var pixLineIdx = lineIdx;pixLineIdx < lineEnd;++pixLineIdx) {
            var pixLine = largeBitmap[pixLineIdx];
            for(var pixColIdx = colIdx;pixColIdx < colEnd;++pixColIdx) {
                ++countCells;
                switch(pixLine[pixColIdx]) {
                    case '.':
                    case ' ':
                        break;
                    default:
                        ++countSet;
                        break;
                }
            }
        }

        lineBuffer.Append(countSet < countCells / 2
            ? '.'
            : 'X'
        );
    }
    scaledBitmap.Add(lineBuffer.ToString());
}

foreach(var line in scaledBitmap) {
    Console.WriteLine(line);
}