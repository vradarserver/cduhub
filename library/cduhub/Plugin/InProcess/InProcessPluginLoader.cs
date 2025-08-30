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
using System.IO;
using System.Reflection;
using Cduhub.CommandLine;
using Newtonsoft.Json;

namespace Cduhub.Plugin.InProcess
{
    class InProcessPluginLoader
    {
        private readonly object _SyncLock = new object();

        private readonly List<string> _LoadedAssemblies = new List<string>();

        public string LoadPluginFromFolder(string pluginFolder)
        {
            lock(_SyncLock) {
                string errorMessage = null;

                var manifest = LoadManifestFromFolder(pluginFolder);
                if(manifest == null) {
                    errorMessage = "Bad manifest in folder";
                } else {
                    var dllFileName = Path.GetFullPath(
                        Path.Combine(pluginFolder, manifest.FileName)
                    );
                    if(!File.Exists(dllFileName)) {
                        errorMessage = $"No file called {dllFileName}";
                    } else if(dllFileName.Length <= pluginFolder.Length) {
                        errorMessage = "Plugin DLL not in plugin folder";
                    } else if(dllFileName[pluginFolder.Length] != Path.DirectorySeparatorChar
                            && dllFileName[pluginFolder.Length] != Path.AltDirectorySeparatorChar
                    ) {
                        errorMessage = "Plugin DLL walked out of plugin folder";
                    } else if(!_LoadedAssemblies.Contains(dllFileName)) {
                        if(!InformationalVersion.TryParse(manifest.MinimumHubVersion, out var minHubVersion)) {
                            errorMessage = "Cannot parse minimum hub version";
                        } else if(minHubVersion.CompareTo(CduhubVersions.LibraryVersion) > 0) {
                            errorMessage = "Plugin needs later version of CDU Hub";
                        } else {
                            var assembly = Assembly.LoadFrom(dllFileName);
                            _LoadedAssemblies.Add(dllFileName);
                            RegisterPluginsFrom(dllFileName, assembly);
                        }
                    }
                }

                return errorMessage;
            }
        }

        private InProcessManifest LoadManifestFromFolder(string pluginFolder)
        {
            InProcessManifest result = null;

            var fileName = Path.Combine(pluginFolder, PluginPaths.InProcessManifestFileName);
            if(File.Exists(fileName)) {
                var json = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<InProcessManifest>(json);
            }

            return result;
        }

        private void RegisterPluginsFrom(string dllFileName, Assembly assembly)
        {
            foreach(var candidateType in assembly.GetExportedTypes()) {
                if(typeof(IPluginDetail).IsAssignableFrom(candidateType)) {
                    var registration = (IPluginDetail)Activator.CreateInstance(candidateType);
                    ApplyRegistration(registration);
                }
            }
        }

        private void ApplyRegistration(IPluginDetail registration)
        {
            RegisteredPlugins.RegisterPlugin(registration.Id, plugin => {
                plugin.DisplayOrder =       registration.DisplayOrder;
                plugin.Label =              registration.Label;
                plugin.EntryPointPage =     registration.EntryPointPage;
                plugin.CreatePageCallback = registration.CreatePage;
            });
        }
    }
}
