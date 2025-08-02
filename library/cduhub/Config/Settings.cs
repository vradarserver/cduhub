// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Cduhub.Config
{
    /// <summary>
    /// Base class for settings objects.
    /// </summary>
    public abstract class Settings
    {
        public int SettingsVersion { get; set; }

        /// <summary>
        /// Returns the name portion of the settings' filename. Do not include the
        /// extension or path, do not use invalid filename characters.
        /// </summary>
        /// <returns></returns>
        public abstract string GetName();

        /// <summary>
        /// Returns the non-zero version number of the settings. If the version on
        /// disk does not match this value then UpgradeSettings() is called.
        /// </summary>
        /// <returns></returns>
        public abstract int GetCurrentVersion();

        /// <summary>
        /// Called with the JSON for an earlier version of the settings file. Any
        /// values that could be parsed from the old JSON have already been parsed.
        /// The <see cref="SettingsVersion"/> property reflects the content of the
        /// JSON. On return <see cref="SettingsVersion"/> will be set to current.
        /// </summary>
        /// <param name="json"></param>
        public virtual void UpgradeSettings(string json)
        {
        }

        public static string NormaliseMcduUsableName(string name)
        {
            var result = (name ?? "").Trim();
            if(result.Length > 24) {
                result = result.Substring(0, 24);
            }
            return result;
        }
    }
}
