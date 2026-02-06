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
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;

namespace Cduhub.CommandLine
{
    class BuildInfoHelpAction : SynchronousCommandLineAction
    {
        private readonly InformationalVersion _Version;
        private readonly HelpAction? _PreviousHelpAction;

        public BuildInfoHelpAction(HelpAction? previousHelpAction)
        {
            _Version = InformationalVersion.FromAssembly(
                Assembly.GetEntryAssembly()
            );
            _PreviousHelpAction = previousHelpAction;
        }

        public override int Invoke(ParseResult parseResult)
        {
            EmitBuildAndCopyrightNotice();
            return _PreviousHelpAction?.Invoke(parseResult) ?? 0;
        }

        public void EmitBuildAndCopyrightNotice()
        {
            var assembly = Assembly.GetEntryAssembly();
            var applicationName = Path.GetFileNameWithoutExtension(assembly.Location);
            var buildTime = BuildDateExtractor.GetLinkerTimestampUtc(assembly);
            var copyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var copyright = copyrightAttribute?.Copyright;

            Console.WriteLine(
                $"{applicationName} " +
                $"{_Version} " +
                $"built {buildTime:yyyy-MMM-dd HH:mm:ss} UTC " +
                $"from commit {_Version.CommitHash.Substring(0, 10)}"
            );
            if(!String.IsNullOrEmpty(copyright)) {
                Console.WriteLine(copyright);
            }
            Console.WriteLine();
        }
    }
}
