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
using System.IO;
using System.Reflection;
using Cduhub.Plugin.InProcess;

namespace Cduhub
{
    public static class HubBootstrap
    {
        public static bool IsBooted { get; private set; }

        public static void Boot()
        {
            if(!IsBooted) {
                IsBooted = true;

                // We just need to tickle the update checker to kick it off
                if(GithubUpdateChecker.DefaultInstance == null) {
                    System.Diagnostics.Debug.Write("This will never show");
                }

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                Plugin.InProcess.InProcessPluginLoader.LoadPlugins();
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(
            object sender,
            ResolveEventArgs args
        )
        {
            Assembly result = null;

            var assemblyName = new AssemblyName(args.Name);
            var searchSpec = $"{assemblyName.Name}.*";
            var pluginFolders = InProcessPluginLoader.PluginFolders;
            foreach(var folder in pluginFolders) {
                foreach(var candidateFileName in Directory.GetFiles(folder, searchSpec)) {
                    var actualExt = Path.GetExtension(candidateFileName);
                    if(String.Equals(actualExt, ".dll", StringComparison.OrdinalIgnoreCase)) {
                        var fullPath = Path.GetFullPath(candidateFileName);
                        if(File.Exists(fullPath)) {
                            result = Assembly.LoadFrom(fullPath);
                        }
                        break;
                    }
                }
                if(result != null) {
                    break;
                }
            }

            return result;
        }
    }
}
