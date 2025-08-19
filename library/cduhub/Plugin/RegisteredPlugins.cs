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
using System.Linq;
using System.Text;

namespace Cduhub.Plugin
{
    public static class RegisteredPlugins
    {
        private readonly static object _SyncLock = new object();
        private readonly static Dictionary<Guid, RegisteredPlugin> _PluginIdMap = new Dictionary<Guid, RegisteredPlugin>();

        public static int CountLoaded
        {
            get {
                lock(_SyncLock) {
                    return _PluginIdMap.Count;
                }
            }
        }

        /// <summary>
        /// Raised whenever a plugin is registered or deregistered.
        /// </summary>
        public static event EventHandler RegisteredPluginsChanged;

        private static void OnRegisteredPluginsChanged() => RegisteredPluginsChanged?.Invoke(null, EventArgs.Empty);

        public static RegisteredPlugin RegisterPlugin(
            Guid pluginId,
            Action<RegisteredPlugin> configureCallback,
            bool configureExistingPlugin = false
        )
        {
            if(pluginId == Guid.Empty) {
                throw new ArgumentOutOfRangeException(nameof(pluginId));
            }

            lock(_SyncLock) {
                if(_PluginIdMap.TryGetValue(pluginId, out var result)) {
                    if(configureExistingPlugin) {
                        configureCallback?.Invoke(result);
                    }
                } else {
                    result = new RegisteredPlugin(pluginId);
                    configureCallback?.Invoke(result);
                    _PluginIdMap.Add(pluginId, result);
                }
                OnRegisteredPluginsChanged();
                return result;
            }
        }

        public static bool EntryPointHasPlugins(EntryPointPage showOnPage)
        {
            lock(_SyncLock) {
                return _PluginIdMap
                    .Values
                    .Any(candidate => candidate.EntryPointPage == showOnPage);
            }
        }

        public static IReadOnlyList<RegisteredPlugin> AllForEntryPoint(EntryPointPage showOnPage)
        {
            lock(_SyncLock) {
                return _PluginIdMap
                    .Values
                    .Where(candidate => candidate.EntryPointPage == showOnPage)
                    .OrderBy(plugin => plugin.DisplayOrder)
                    .ThenBy(plugin => plugin.Label)
                    .ToArray();
            }
        }
    }
}
