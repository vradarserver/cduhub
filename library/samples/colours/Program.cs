// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.CommandLine;
using Cduhub.CommandLine;
using WwDevicesDotNet;
using Newtonsoft.Json;

namespace Colours
{
    class Program
    {
        static Colour[] _Colours = [
            Colour.Gray,
            Colour.White,
            Colour.Amber,
            Colour.Red,
            Colour.Khaki,
            Colour.Green,
            Colour.Cyan,
            Colour.Magenta,
            Colour.Brown,
            Colour.Yellow,
        ];
        static int _StartColourOffset;
        static bool _FirstSetIsSmall;

        static int Main(string[] args)
        {
            var exitCode = 0;

            try {
                var worked = true;

                RootCommand rootCommand = new("Display colours on the CDU device") {
                    Options.FontFileInfoOption,
                    Options.UseFullWidthOption,
                };
                rootCommand.EnforceInHouseStandards();
                rootCommand.SetAction(
                    parseResult => {
                        ShowColours(
                            parseResult.GetValue(Options.FontFileInfoOption),
                            parseResult.GetValue(Options.UseFullWidthOption)
                        );
                    }
                );
                exitCode = rootCommand.Parse(args).Invoke();
                if(!worked) {
                    exitCode = 1;
                }
            } catch(Exception ex) {
                Console.WriteLine("Caught exception during processing:");
                Console.WriteLine(ex);
                exitCode = 2;
            }

            return exitCode;
        }

        static void ShowColours(
            FileInfo fontFileInfo,
            bool useFullWidth
        )
        {
            using(var cdu = CduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {cdu.DeviceId}");
                cdu.KeyDown += Cdu_KeyDown;

                var font = LoadFont(fontFileInfo);
                if(font != null) {
                    cdu.UseFont(font, useFullWidth);
                }

                DrawScreen(cdu);

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                cdu.Cleanup();
            }
        }

        static void DrawScreen(ICdu cdu)
        {
            cdu.Screen.Clear();

            ShowColourPairs(cdu.Output, _Colours, _StartColourOffset, smallFont: _FirstSetIsSmall);

            cdu.Output
                .Line(6)
                .Centered("<white>←↑→↓ <small>AND<large> DIR")
                .Line(-5);

            ShowColourPairs(cdu.Output, _Colours, _StartColourOffset, smallFont: !_FirstSetIsSmall);

            cdu.RefreshDisplay();
        }

        static void ShowColourPairs(Compositor output, Colour[] colours, int startOffset, bool smallFont)
        {
            var offset = startOffset;

            Colour nextColour()
            {
                var result = colours[offset];
                if(++offset == colours.Length) {
                    offset = 0;
                }
                return result;
            }

            for(var pairIdx = 0;pairIdx < colours.Length / 2;++pairIdx) {
                var left = nextColour();
                var right = nextColour();
                ShowColourPair(output, left, right, smallFont);
            }
        }

        static void ShowColourPair(Compositor output, Colour left, Colour right, bool smallFont)
        {
            output
                .Small(smallFont)
                .LeftToRight().Colour(left).Write(left.ToString().ToUpper())
                .RightToLeft().Color(right).Write(right.ToString().ToUpper())
                .LeftToRight().Newline();
        }

        static void Cdu_KeyDown(object sender, KeyEventArgs args)
        {
            var cdu = (ICdu)sender;

            void scrollBackwards(int offset)
            {
                _StartColourOffset -= offset;
                if(_StartColourOffset < 0) {
                    _StartColourOffset += _Colours.Length;
                }
            }
            void scrollForwards(int offset)
            {
                _StartColourOffset += offset;
                if(_StartColourOffset >= _Colours.Length) {
                    _StartColourOffset -= _Colours.Length;
                }
            }

            var redrawScreen = true;
            switch(args.Key) {
                case Key.DownArrow:     scrollBackwards(2); break;
                case Key.UpArrow:       scrollForwards(2); break;
                case Key.RightArrow:    scrollBackwards(1); break;
                case Key.LeftArrow:     scrollForwards(1); break;
                case Key.Dir:           _FirstSetIsSmall = !_FirstSetIsSmall; break;
                case Key.Init:
                    cdu.Palette.White.Set(0xff, 0xff, 0xff);
                    cdu.RefreshPalette();
                    break;
                case Key.SecFPln:
                    cdu.Palette.White.Set(0xff, 0x00, 0x00);
                    cdu.RefreshPalette();
                    break;
                case Key.AtcComm:
                    cdu.Palette.White.Set(0x00, 0xff, 0x00);
                    cdu.RefreshPalette();
                    break;
                case Key.McduMenu:
                    cdu.Palette.White.Set(0x00, 0x00, 0xff);
                    cdu.RefreshPalette();
                    break;
                default:
                    redrawScreen = false;
                    break;
            }

            if(redrawScreen) {
                DrawScreen(cdu);
            }
        }

        private static McduFontFile LoadFont(FileInfo fileInfo)
        {
            McduFontFile result = null;

            if(fileInfo != null) {
                if(!fileInfo.Exists) {
                    throw new ArgumentException($"{fileInfo} does not exist");
                } else {
                    try {
                        var json = File.ReadAllText(fileInfo.FullName);
                        result = JsonConvert.DeserializeObject<McduFontFile>(json);
                    } catch(Exception ex) {
                        throw new ArgumentException($"Could not parse font from {fileInfo}", ex);
                    }
                }
            }

            return result;
        }
    }
}
