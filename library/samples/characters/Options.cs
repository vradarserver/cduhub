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
using McduDotNet;
using Newtonsoft.Json;

namespace Characters
{
    static class Options
    {
        public static Option<McduFontFile> FontOption = new("--font", "-f") {
            Description = "Load font file",
            CustomParser = (arg) => {
                McduFontFile result = null;
                var fileName = arg.Tokens.Count == 1 ? arg.Tokens[0].Value : null;
                if(fileName == null) {
                    throw new ArgumentException("Missing font filename");
                } else if(!File.Exists(fileName)) {
                    throw new ArgumentException($"{fileName} does not exist");
                } else {
                    try {
                        var json = File.ReadAllText(fileName);
                        result = JsonConvert.DeserializeObject<McduFontFile>(json);
                    } catch(Exception ex) {
                        throw new ArgumentException($"Could not parse font from {fileName}", ex);
                    }
                }
                return result;
            },
        };

        public static Option<bool> UseFullWidthOption = new("--fullWidth", "-fw") {
            Description = "Use full display width",
        };

        public static Option<int> XOffsetOption = new("--xOffset", "-x") {
            Description = "Set X offset",
        };

        public static Option<int> YOffsetOption = new("--yOffset", "-y") {
            Description = "Set Y offset",
        };
    }
}
