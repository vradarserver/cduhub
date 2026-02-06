// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.Linq;

namespace Cduhub.CommandLine
{
    public static class RootCommandExtensions
    {
        public static RootCommand EnforceInHouseStandards(this RootCommand rootCommand)
        {
            rootCommand.TreatUnmatchedTokensAsErrors = true;
            RemoveDefaultVersionOption(rootCommand);
            AddCustomHelp(rootCommand);

            return rootCommand;
        }

        public static IReadOnlyList<T> FindOptionsOfType<T>(this RootCommand rootCommand)
            where T: Option
        {
            return rootCommand
                .Options
                .OfType<T>()
                .ToArray();
        }

        public static RootCommand RemoveDefaultVersionOption(this RootCommand rootCommand)
        {
            var defaultVersionOptions = rootCommand.FindOptionsOfType<VersionOption>();
            foreach(var garbage in defaultVersionOptions) {
                rootCommand.Options.Remove(garbage);
            }

            return rootCommand;
        }

        public static RootCommand AddCustomHelp(this RootCommand rootCommand)
        {
            var defaultHelpOptions = rootCommand.FindOptionsOfType<HelpOption>();
            foreach(var help in defaultHelpOptions) {
                help.Action = new BuildInfoHelpAction(help.Action as HelpAction);
            }
            return rootCommand;
        }
    }
}
