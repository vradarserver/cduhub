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
using Cduhub.Config;
using Cduhub.Plugin.InProcess;
using Cduhub.Plugin.Remote;

namespace Cduhub.Plugin
{
    public static class PluginLoader
    {
        private static object _SyncLock = new object();
        private static readonly InProcessPluginLoader _InProcessPluginLoader = new InProcessPluginLoader();
        private static readonly RemotePluginLoader _RemotePluginLoader = new RemotePluginLoader();

        private static HashSet<string> _PluginFolders = new HashSet<string>();
        public static IReadOnlyList<string> PluginFolders
        {
            get {
                lock(_SyncLock) {
                    return _PluginFolders.ToArray();
                }
            }
        }

        private static readonly Dictionary<string, string[]> _LoadErrors = new Dictionary<string, string[]>();
        public static IReadOnlyList<(string, string[])> LoadErrors
        {
            get {
                lock(_SyncLock) {
                    return _LoadErrors
                        .Select(kvp => (kvp.Key, kvp.Value))
                        .ToArray();
                }
            }
        }

        static PluginLoader()
        {
            var folder = PluginPaths.PluginsFolderFullPath;
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
        }

        public static void LoadPlugins()
        {
            var settings = ConfigStorage.Load<CduhubSettings>();

            var errors = new List<string>();
            foreach(var pluginFullPath in Directory.GetDirectories(PluginPaths.PluginsFolderFullPath)) {
                var pluginFolder = Path.GetFileName(pluginFullPath);

                errors.Clear();
                var loadCounts = 0;

                try {
                    void addError(string err)
                    {
                        if(!String.IsNullOrEmpty(err)) {
                            errors.Add(err);
                        }
                    }
                    if(HasInProcessManifest(pluginFullPath)) {
                        ++loadCounts;
                        if(settings.Plugin.InProcessEnabled) {
                            addError(
                                _InProcessPluginLoader.LoadPluginFromFolder(pluginFullPath)
                            );
                        }
                    }
                    if(HasRemoteManifest(pluginFullPath)) {
                        ++loadCounts;
                        if(settings.Plugin.OutOfProcessEnabled) {
                            addError(
                                _RemotePluginLoader.LoadPluginFromFolder(pluginFullPath)
                            );;
                        }
                    }
                    if(loadCounts == 0) {
                        addError("No manifest in folder");
                    }
                } catch(Exception ex) {
                    errors.Add(ex.Message);
                }

                if(errors.Count == 0) {
                    _LoadErrors.Remove(pluginFolder);
                } else {
                    _LoadErrors[pluginFolder] = errors.ToArray();
                }
                if(errors.Count != 0 || loadCounts == 0) {
                    _PluginFolders.Remove(pluginFullPath);
                } else {
                    _PluginFolders.Add(pluginFullPath);
                }
            }
        }

        private static bool HasInProcessManifest(string pluginFolder)
        {
            var path = Path.Combine(pluginFolder, PluginPaths.InProcessManifestFileName);
            return File.Exists(path);
        }

        private static bool HasRemoteManifest(string pluginFolder)
        {
            var path = Path.Combine(pluginFolder, PluginPaths.RemoteManifestFilename);
            return File.Exists(path);
        }
    }
}
