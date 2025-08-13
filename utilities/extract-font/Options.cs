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
using System.CommandLine.Parsing;
using System.Text;
using Cduhub.CommandLine.Parsers;
using Cduhub.CommandLine.Validators;

namespace ExtractFont
{
    static class Options
    {
        public static readonly Option<FileInfo> PacketsFileOption = new("--packets", "-p") {
            Description = "The file that contains the USB packet dump to extract fonts from",
            Required = true,
            CustomParser = result => GetFileInfo(result, mustExist: true),
        };

        public static readonly Option<FileInfo> FontFileOption = new("--font", "-f") {
            Description = "The name of the font file to write (uses font name if not specified)",
            Required = false,
            CustomParser = result => GetFileInfo(result, mustExist: false),
        };

        public static readonly Option<FileInfo> MapFileOption = new("--map") {
            Description = "The name of the optional packet map file to write",
            Required = false,
            CustomParser = result => GetFileInfo(result, mustExist: false),
        };

        public static readonly Option<string> NameOption = new("--name") {
            Description = "The name of the font",
            Required = false,
            DefaultValueFactory = result => "Font",
        };

        public static readonly Option<FileInfo> CreateOptionsFileOption = new("--options", "-o") {
            Description = "The options file to create",
            Required = true,
        };

        public static readonly Option<bool> OverwriteOptionsFileOption = new("--overwrite") {
            Description = "Overwrite the options file instead of modifying it",
            DefaultValueFactory = _ => false,
        };

        public static readonly Option<FontFileType> FontFileTypeOption = new("--type", "-t") {
            Description = "The type of font file",
            DefaultValueFactory = _ => FontFileType.BiosFont,
        };

        public static readonly Option<FileInfo> OptionsFileOption = new("--options", "-o") {
            Description = "The options file to load configuration from",
            Required = true,
            Validators = { FileExistsValidator.Validate }
        };

        public static readonly Option<FileInfo> MandatoryOutputFileNameOption = new("--file", "-f") {
            Description = "The file to write to",
            Required = true,
        };

        public static readonly Option<byte[]> CommandPrefixOption = new("--command-prefix", "-cp") {
            Description = "The command prefix to look for in hex (PFP-3N = 31BB, MCDU = 32BB, PFP-7 = 33BB)",
            Required = true,
            CustomParser = result => HexByteParser.ParseByteArray(result),
        };

        public static string SanitiseFileName(string fileName, char replaceInvalidWith = '-')
        {
            var result = new StringBuilder(fileName);
            var invalid = Path.GetInvalidFileNameChars();
            for(var idx = 0;idx < result.Length;++idx) {
                if(invalid.Contains(result[idx])) {
                    result[idx] = replaceInvalidWith;
                }
            }

            return result.ToString();
        }

        public static FileInfo GetFileInfo(ArgumentResult result, bool mustExist)
        {
            FileInfo fileInfo = null;
            if(result.Tokens.Count == 1) {
                var fileName = result.Tokens[0].Value;
                if(mustExist && !File.Exists(fileName)) {
                    result.AddError($"{fileName} does not exist");
                } else {
                    fileInfo = new(fileName);
                }
            }
            return fileInfo;
        }
    }
}
