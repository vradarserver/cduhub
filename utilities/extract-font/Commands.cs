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

namespace ExtractFont
{
    static class Commands
    {
        public static Command CreateOptionsFileCommand = new("create-options", "Create or update an options file") {
            Options.CreateOptionsFileOption,
            Options.FontFileTypeOption,
            Options.OverwriteOptionsFileOption
        };

        public static Command DumpFontFileCommand = new("dump-font", "Read and dump font information from a font resource") {
            Options.FontFileTypeOption,
            Options.OptionsFileOption,
            Options.MandatoryOutputFileNameOption,
        };

        public static Command ExtractFromFontFile = new("from-file", "Extracts fonts from a font file") {
            Options.FontFileTypeOption,
            Options.OptionsFileOption,
            Options.MandatoryOutputFileNameOption,
        };

        public static Command ExtractFromPacketDumpCommand = new("from-packets", "Extract font (and optionally map) from a USB packet dump") {
            Options.CommandPrefixOption,
            Options.PacketsFileOption,
            Options.FontFileOption,
            Options.NameOption,
            Options.MapFileOption,
        };

        static Commands()
        {
            CreateOptionsFileCommand.SetAction(parse => {
                Program.Worked = Command_CreateOptionsFile.Run(
                    parse.GetValue(Options.FontFileTypeOption),
                    parse.GetValue(Options.CreateOptionsFileOption),
                    parse.GetValue(Options.OverwriteOptionsFileOption)
                );
            });

            DumpFontFileCommand.SetAction(parse => {
                Program.Worked = Command_DumpFontFile.Run(
                    parse.GetValue(Options.FontFileTypeOption),
                    parse.GetValue(Options.OptionsFileOption),
                    parse.GetValue(Options.MandatoryOutputFileNameOption)
                );
            });

            ExtractFromFontFile.SetAction(parse => {
                Program.Worked = Command_ExtractFromFontFile.Run(
                    parse.GetValue(Options.FontFileTypeOption),
                    parse.GetValue(Options.OptionsFileOption),
                    parse.GetValue(Options.MandatoryOutputFileNameOption)
                );
            });

            ExtractFromPacketDumpCommand.SetAction(parse => {
                Program.Worked = Command_ExtractFromPacketDump.Run(
                    parse.GetValue(Options.CommandPrefixOption),
                    parse.GetValue(Options.NameOption),
                    parse.GetRequiredValue(Options.PacketsFileOption),
                    parse.GetRequiredValue(Options.FontFileOption),
                    parse.GetValue(Options.MapFileOption)
                );
            });
        }
    }
}
