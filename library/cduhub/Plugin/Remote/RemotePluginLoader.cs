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
using Newtonsoft.Json;

namespace Cduhub.Plugin.Remote
{
    class RemotePluginLoader
    {
        public string LoadPluginFromFolder(string pluginFolder)
        {
            string errorMessage = null;

            var manifest = LoadManifestFromFolder(pluginFolder);
            if(manifest == null) {
                errorMessage = "Bad manifest in folder";
            } else if(manifest.Guid == Guid.Empty) {
                errorMessage = "Manifest has no ID";
            } else {
                ApplyRegistration(manifest);
            }

            return errorMessage;
        }

        private RemoteManifest LoadManifestFromFolder(string pluginFolder)
        {
            RemoteManifest result = null;

            var fileName = Path.Combine(pluginFolder, PluginPaths.RemoteManifestFilename);
            if(File.Exists(fileName)) {
                var json = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<RemoteManifest>(json);
            }

            return result;
        }

        private void ApplyRegistration(RemoteManifest manifest)
        {
            RegisteredPlugins.RegisterPlugin(manifest.Guid, plugin => {
                plugin.DisplayOrder =       manifest.DisplayOrder;
                plugin.Label =              manifest.Label;
                plugin.EntryPointPage =     manifest.EntryPoint;
                plugin.CreatePageCallback = hub => RemotePageFactory.CreateFor(manifest.Guid, hub);
            });
        }
    }
}
