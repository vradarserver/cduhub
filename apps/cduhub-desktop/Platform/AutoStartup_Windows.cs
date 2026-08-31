// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Cduhub.DesktopGui.Platform
{
    /// <summary>
    /// Implementation of <see cref="IAutoStartup"/> for Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    sealed class AutoStartup_Windows : IAutoStartup
    {
        // HKCU\Software\Microsoft\Windows\CurrentVersion\Run
        private const string _RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        // Inherit the WinForms value - don't change this, it'll leave the old setting orphaned
        private const string _ValueName = "cduhub-windows";

        private static string? ApplicationPath => Environment.ProcessPath;

        /// <inheritdoc/>
        public bool IsSupported => !String.IsNullOrEmpty(ApplicationPath);

        /// <inheritdoc/>
        public bool IsEnabled
        {
            get {
                var result = false;
                try {
                    using(var key = Registry.CurrentUser.OpenSubKey(_RunKeyPath, writable: false)) {
                        result = String.Equals(
                            key?.GetValue(_ValueName) as string,
                            ApplicationPath,
                            StringComparison.OrdinalIgnoreCase
                        );
                    }
                } catch {
                    result = false;
                }
                return result;
            }
        }

        /// <inheritdoc/>
        public void Enable(bool enable)
        {
            if(IsSupported) {
                using(var key = Registry.CurrentUser.OpenSubKey(_RunKeyPath, writable: true)) {
                    if(key != null) {
                        if(enable) {
                            key.SetValue(_ValueName, ApplicationPath!);
                        } else if(key.GetValue(_ValueName) != null) {
                            key.DeleteValue(_ValueName);
                        }
                    }
                }
            }
        }
    }
}
