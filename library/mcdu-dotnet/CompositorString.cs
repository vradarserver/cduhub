// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace wwDevicesDotNet
{
    /// <summary>
    /// Describes a string that has style changes embedded within it in faux-HTML markup.
    /// </summary>
    public class CompositorString
    {
        /// <summary>
        /// The plain text with all embedding removed.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// A collection of style changes.
        /// </summary>
        public CompositorStringStyleChange[] StyleChanges { get; } = Array.Empty<CompositorStringStyleChange>();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="text"></param>
        public CompositorString(string text)
        {
            (Text, StyleChanges) = Parse(text);
        }

        static Regex _EmbeddedStyleRegex = new Regex(
            @"\<(?<style>\<?(amber|brown|cyan|gray|green|grey|khaki|large|magenta|red|small|white|yellow))\>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public static (string text, CompositorStringStyleChange[] styleChanges) Parse(string text)
        {
            var textBuffer = new StringBuilder();
            var styleChanges = new List<CompositorStringStyleChange>();
            var matches = _EmbeddedStyleRegex.Matches(text ?? "");

            var textStart = 0;
            void extractText(int toIndex)
            {
                for(var idx = textStart;idx < toIndex;++idx) {
                    textBuffer.Append(text[idx]);
                }
                textStart = toIndex;
            }

            foreach(Match match in matches) {
                extractText(match.Index);
                textStart = match.Index + match.Length;

                var style = match.Groups["style"].Value;
                if(style[0] == '<') {
                    textBuffer.Append(style);
                    textBuffer.Append('>');
                } else {
                    CompositorStringStyle styleEnum;
                    switch(style.ToLower()) {
                        case "amber":   styleEnum = CompositorStringStyle.Amber; break;
                        case "brown":   styleEnum = CompositorStringStyle.Brown; break;
                        case "cyan":    styleEnum = CompositorStringStyle.Cyan; break;
                        case "gray":    styleEnum = CompositorStringStyle.Grey; break;
                        case "green":   styleEnum = CompositorStringStyle.Green; break;
                        case "grey":    styleEnum = CompositorStringStyle.Grey; break;
                        case "khaki":   styleEnum = CompositorStringStyle.Khaki; break;
                        case "large":   styleEnum = CompositorStringStyle.Large; break;
                        case "magenta": styleEnum = CompositorStringStyle.Magenta; break;
                        case "red":     styleEnum = CompositorStringStyle.Red; break;
                        case "small":   styleEnum = CompositorStringStyle.Small; break;
                        case "white":   styleEnum = CompositorStringStyle.White; break;
                        case "yellow":  styleEnum = CompositorStringStyle.Yellow; break;
                        default:        throw new NotImplementedException();
                    }
                    styleChanges.Add(new CompositorStringStyleChange(textBuffer.Length, styleEnum));
                }
            }

            extractText(text.Length);

            return (textBuffer.ToString(), styleChanges.ToArray());
        }

        /// <inheritdoc/>
        public override string ToString() => Text;
    }
}
