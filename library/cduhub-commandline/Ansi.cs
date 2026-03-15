// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;

namespace Cduhub.CommandLine
{
    public static class Ansi
    {
        public static readonly Chunk Blue = new(ConsoleColor.DarkBlue, ConsoleColor.Black);
        public static readonly Chunk BlueBold = new(ConsoleColor.Blue, ConsoleColor.Black);
        public static readonly Chunk Cyan = new(ConsoleColor.DarkCyan, ConsoleColor.Black);
        public static readonly Chunk CyanBold = new(ConsoleColor.Cyan, ConsoleColor.Black);
        public static readonly Chunk Red = new(ConsoleColor.DarkRed, ConsoleColor.Black);
        public static readonly Chunk RedBold = new(ConsoleColor.Red, ConsoleColor.Black);
        public static readonly Chunk Magenta = new(ConsoleColor.DarkMagenta, ConsoleColor.Black);
        public static readonly Chunk MagentaBold = new(ConsoleColor.Magenta, ConsoleColor.Black);
        public static readonly Chunk Yellow = new(ConsoleColor.DarkYellow, ConsoleColor.Black);
        public static readonly Chunk YellowBold = new(ConsoleColor.Yellow, ConsoleColor.Black);
        public static readonly Chunk White = new(ConsoleColor.Gray, ConsoleColor.Black);
        public static readonly Chunk WhiteBold = new(ConsoleColor.White, ConsoleColor.Black);
        public static readonly Chunk Regular = White;

        public class Chunk
        {
            public string? Text { get; }

            public ConsoleColor? Foreground { get; }

            public ConsoleColor? Background { get; }

            public Chunk(string? text)
            {
                Text = text;
            }

            public Chunk(ConsoleColor? foreground, ConsoleColor? background)
            {
                Foreground = foreground;
                Background = background;
            }

            public Chunk() : this(null, null)
            {
            }

            public Chunk(ConsoleColor foreground) : this(foreground, null)
            {
            }

            public static implicit operator Chunk(string text) => new(text);
        }

        public static void Write(params Chunk[] chunks)
        {
            if(!Console.IsOutputRedirected) {
                Emit(chunks, text => Console.Write(text), null);
            } else {
                EmitText(chunks, text => Console.Write(text), null);
            }
        }

        public static void WriteLine(params Chunk[] chunks)
        {
            if(!Console.IsOutputRedirected) {
                Emit(chunks, text => Console.Write(text), () => Console.WriteLine());
            } else {
                EmitText(chunks, text => Console.Write(text), () => Console.WriteLine());
            }
        }

        public static void Foreground(ConsoleColor colour)
        {
            if(!Console.IsOutputRedirected) {
                Console.ForegroundColor = colour;
            }
        }

        public static void RegularForeground() => Foreground(ConsoleColor.Gray);

        public static void Emit(Chunk[] chunks, Action<string> emitTextAction, Action? endOfSequenceAction)
        {
            var initialForeground = Console.ForegroundColor;
            var initialBackground = Console.BackgroundColor;
            var currentForeground = initialForeground;
            var currentBackground = initialBackground;

            void setForeground(ConsoleColor? colour)
            {
                if(colour != null && colour.Value != currentForeground) {
                    currentForeground = colour.Value;
                    Console.ForegroundColor = colour.Value;
                }
            }
            void setBackground(ConsoleColor? colour)
            {
                if(colour != null && colour.Value != currentBackground) {
                    currentBackground = colour.Value;
                    Console.BackgroundColor = colour.Value;
                }
            }

            try {
                foreach(var chunk in chunks) {
                    if(chunk.Text != null) {
                        emitTextAction(chunk.Text);
                    } else {
                        if(chunk.Foreground == null && chunk.Background == null) {
                            setForeground(initialForeground);
                            setBackground(initialBackground);
                        } else {
                            setForeground(chunk.Foreground);
                            setBackground(chunk.Background);
                        }
                    }
                }
            } finally {
                setForeground(initialForeground);
                setBackground(initialBackground);
            }

            endOfSequenceAction?.Invoke();
        }

        public static void EmitText(Chunk[] chunks, Action<string> emitTextAction, Action? endOfSequenceAction)
        {
            foreach(var textChunk in chunks.Select(chunk => chunk.Text).OfType<string>()) {
                emitTextAction(textChunk);
            }
            endOfSequenceAction?.Invoke();
        }
    }
}
