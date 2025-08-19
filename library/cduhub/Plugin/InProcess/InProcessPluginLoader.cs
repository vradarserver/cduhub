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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cduhub.CommandLine;
using Newtonsoft.Json;

namespace Cduhub.Plugin.InProcess
{
    public static class InProcessPluginLoader
    {
        private static readonly object _SyncLock = new object();

        private static readonly List<string> _LoadedAssemblies = new List<string>();

        public static string Folder { get; }

        public static Dictionary<string, string> LoadErrors { get; } = new Dictionary<string, string>();

        static InProcessPluginLoader()
        {
            Folder = Path.Combine(WorkingFolder.Folder, "Plugins");
            if(!Directory.Exists(Folder)) {
                Directory.CreateDirectory(Folder);
            }
        }

        public static void LoadPlugins()
        {
            lock(_SyncLock) {
                foreach(var pluginFolder in Directory.GetDirectories(Folder)) {
                    try {
                        var errorMessage = LoadPluginFromFolder(pluginFolder);
                        if(!String.IsNullOrEmpty(errorMessage)) {
                            LoadErrors[pluginFolder] = errorMessage;
                        }
                    } catch(Exception ex) {
                        if(LoadErrors.TryGetValue(pluginFolder, out var error)) {
                            LoadErrors[pluginFolder] = $"{error}. {ex.Message}";
                        } else {
                            LoadErrors[pluginFolder] = ex.Message;
                        }
                    }
                }
            }
        }

        private static string LoadPluginFromFolder(string pluginFolder)
        {
            string errorMessage = null;

            var manifest = LoadManifestFromFolder(pluginFolder);
            if(manifest == null) {
                errorMessage = "No manifest in folder";
            } else {
                var dllFileName = Path.GetFullPath(
                    Path.Combine(pluginFolder, manifest.FileName)
                );
                if(!File.Exists(dllFileName)) {
                    errorMessage = $"No file called {dllFileName}";
                } else if(!_LoadedAssemblies.Contains(dllFileName)) {
                    if(!InformationalVersion.TryParse(manifest.MinimumHubVersion, out var minHubVersion)) {
                        errorMessage = "Cannot parse minimum hub version";
                    } else if(minHubVersion.CompareTo(CduhubVersions.LibraryVersion) > 0) {
                        errorMessage = "Plugin needs later version of CDU Hub";
                    } else {
                        var assembly = Assembly.LoadFrom(dllFileName);
                        _LoadedAssemblies.Add(dllFileName);
                        RegisterPluginsFrom(assembly);
                    }
                }
            }

            return errorMessage;
        }

        private static Manifest LoadManifestFromFolder(string pluginFolder)
        {
            Manifest result = null;

            var fileName = Path.Combine(pluginFolder, "Manifest.json");
            if(File.Exists(fileName)) {
                var json = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<Manifest>(json);
            }

            return result;
        }

        private static void RegisterPluginsFrom(Assembly assembly)
        {
            foreach(var candidateType in assembly.GetExportedTypes()) {
                if(typeof(IPluginDetail).IsAssignableFrom(candidateType)) {
                    var registration = (IPluginDetail)Activator.CreateInstance(candidateType);
                    ApplyRegistration(registration);
                }
            }
        }

        private static void ApplyRegistration(IPluginDetail registration)
        {
            RegisteredPlugins.RegisterPlugin(registration.Id, plugin => {
                plugin.DisplayOrder = registration.DisplayOrder;
                plugin.Label =        registration.Label;
                plugin.ShowOnPage =   registration.ShowOnPage;
            });
        }
    }
}
