// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Reflection;
using Microsoft.Win32;

namespace Cduhub.WindowsGui
{
    /// <summary>
    /// Manages auto-startup for the Windows CDU Hub runner.
    /// </summary>
    public static class AutoStartup
    {
        /// <summary>
        /// Application name in registry. Don't change this.
        /// </summary>
        public const string AutoStartValueName = "cduhub-windows";

        /// <summary>
        /// Path to the application.
        /// </summary>
        public static readonly string ApplicationPath = Assembly.GetEntryAssembly().Location;

        /// <summary>
        /// True if auto-startup is enabled. This does not test to see whether the application path
        /// is correct.
        /// </summary>
        public static bool IsEnabled
        {
            get {
                var result = false;

                try {
                    using(var key = OpenAutoStartKey(forWriting: false)) {
                        result = key?.GetValue(AutoStartValueName) != null;
                    }
                } catch {
                    // TODO: Add logging
                    result = false;
                }

                return result;
            }
        }

        /// <summary>
        /// Configures or removes configuration with Windows to automatically start the application
        /// when the current user logs in.
        /// </summary>
        /// <param name="enable"></param>
        public static void Enable(bool enable)
        {
            using(var key = OpenAutoStartKey(forWriting: true)) {
                if(enable) {
                    key.SetValue(AutoStartValueName, ApplicationPath);
                } else {
                    key.DeleteValue(AutoStartValueName);
                }
            }
        }

        private static RegistryKey OpenAutoStartKey(bool forWriting)
        {
            return Registry
                .CurrentUser
                .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", forWriting);
        }
    }
}
