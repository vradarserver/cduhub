<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
    var fileName = "2025-07-14 font-upload-packets.txt";
    var folder = Path.GetDirectoryName(Util.CurrentQueryPath);
    var fullPath = Path.Combine(folder, fileName);
    
    var name = "WinWing Default Font";
    var fontFileName = Path.Combine(folder, "winwing-default-font.json");
    
    var fontFile = ParseUsbPacketsIntoFont(name, File.ReadAllLines(fullPath));
    var fontJson = JsonConvert.SerializeObject(fontFile, Newtonsoft.Json.Formatting.Indented);
    File.WriteAllText(fontFileName, fontJson);
}

class McduFontFile
{
    public string Name { get; set; }
    
    public int GlyphWidth { get; set; }
    
    public int GlyphHeight { get; set; }
    
    public McduGlyph[] BigGlyphs { get; set; } = Array.Empty<McduGlyph>();

    public McduGlyph[] SmallGlyphs { get; set; } = Array.Empty<McduGlyph>();
}

class McduGlyph
{
    public char Character { get; set; }
    
    public List<string> BitArray { get; set; } = new List<string>();
}

McduFontFile ParseUsbPacketsIntoFont(string fontName, IEnumerable<string> packetPayloads)
{
    McduFontFile result = new McduFontFile() {
        Name = fontName,
        GlyphWidth = 24,
        GlyphHeight = 30,
    };
    var glyphs = new List<McduGlyph>();
    
    var isFirst3C = true;
    var seenInterruption = true;
    var byteWidth = result.GlyphWidth / 8;
    McduGlyph glyph = null;
    var bitlineBuffer = new StringBuilder();
    
    void parse3C(byte[] pkt, int offset, int length)
    {
        for(;offset < length;++offset) {
            var b = pkt[offset];
            if(glyph == null) {
                glyph = new McduGlyph() {
                    Character = (char)b,
                };
            } else {
                for(var bit = 0x80;bit > 0;bit >>= 1) {
                    var isolated = (b & bit) != 0 ? 1 : 0;
                    bitlineBuffer.Append(isolated);
                }
                if(bitlineBuffer.Length >= result.GlyphWidth) {
                    glyph.BitArray.Add(bitlineBuffer.ToString());
                    bitlineBuffer.Clear();

                    if(glyph.BitArray.Count == result.GlyphHeight) {
                        glyphs.Add(glyph);
                        glyph = null;
                    }
                }
            }
        }
    }

    var packet = new byte[64];
    foreach(var packetText in packetPayloads) {
        var packetLen = 0;
        var trimmed = packetText.Trim();
        for(var idx = 0;idx + 1 < trimmed.Length;idx += 2) {
           packet[packetLen++] = (byte)(
               (ConvertNibble(trimmed[idx]) << 4)
               + ConvertNibble(trimmed[idx + 1])
           );
        }

        switch(packetLen) {
            case 64:
                if(packet[0] == 0xf0 && packet[1] == 0x00 && packet[3] == 0x3c) {
                    var offset = 4;
                    if(seenInterruption) {
                        offset += (isFirst3C ? 29 : 28);
                        seenInterruption = false;
                        isFirst3C = false;
                    }
                    parse3C(packet, offset, packetLen);
                } else {
                    if(!seenInterruption) {
                        bitlineBuffer.Append("#");
                    }
                    seenInterruption = true;
                }
                break;
            default:
                seenInterruption = true;
                break;
        }
    }
    
    result.BigGlyphs = glyphs.ToArray();
    
    return result;
}

int ConvertNibble(char ch)
{
    ch = char.ToLower(ch);
    return ch >= '0' && ch <= '9'
        ? ch - '0'
        : ch >= 'a' && ch <= 'f'
            ? (ch - 'a') + 10
            : throw new InvalidOperationException($"{ch} is not a hex digit");
}