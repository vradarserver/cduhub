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
using System.Text;
using Cduhub.Config;
using McduDotNet;
using Newtonsoft.Json;

namespace Cduhub
{
    /// <summary>
    /// Manages the fonts available to a page.
    /// </summary>
    public static class Fonts
    {
        private static CustomFontSettings _Settings;
        private static readonly McduFontFile _B612Regular;
        private static readonly McduFontFile _FenixRegular;
        private static readonly McduFontFile _FlyByWireRegular;
        private static readonly object _SyncLock = new object();
        private static Dictionary<string, McduFontFile> _CustomFontMap = new Dictionary<string, McduFontFile>(StringComparer.InvariantCultureIgnoreCase);

        public static Exception LoadSettingsException { get; }

        public static McduFontFile DefaultFont => _B612Regular;

        public static McduFontFile B612Regular => _B612Regular;

        public static McduFontFile FenixRegular => _FenixRegular;

        public static McduFontFile FlyByWireRegular => _FlyByWireRegular;

        static Fonts()
        {
            LoadSettingsException = null;
            try {
                _Settings = ConfigStorage.Load<CustomFontSettings>();
            } catch(Exception ex) {
                LoadSettingsException = ex;
                _Settings = new CustomFontSettings();
            }

            _B612Regular = LoadFont(CduHubResources.b612_font_21x31_json);
            _FenixRegular = LoadFont(CduHubResources.fenix_font21x31_json);
            _FlyByWireRegular = LoadFont(CduHubResources.flybywire_font_21x31_json);
        }

        public static McduFontFile LoadFont(byte[] jsonBytes, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var json = encoding.GetString(jsonBytes);
            return JsonConvert.DeserializeObject<McduFontFile>(json);
        }

        public static McduFontFile LoadBuiltInFont(BuiltInFont builtInFont)
        {
            switch(builtInFont) {
                case BuiltInFont.B612Regular:       return _B612Regular;
                case BuiltInFont.FenixRegular:      return _FenixRegular;
                case BuiltInFont.FlyByWireRegular:  return _FlyByWireRegular;
                default:                            throw new NotImplementedException();
            }
        }

        public static McduFontFile LoadFontByConfigName(string configName)
        {
            McduFontFile result = null;

            configName = (configName ?? "").Trim();

            if(configName != "") {
                var builtInFont = BuiltInFontExtensions.ToBuiltInFont(configName);
                if(builtInFont != null) {
                    result = LoadBuiltInFont(builtInFont.Value);
                } else {
                    result = LoadCustomFont(configName);
                }
            }

            return result ?? _B612Regular;
        }

        public static IReadOnlyList<string> GetAllConfigNames(bool includeDefault)
        {
            var result = new List<string>();
            if(includeDefault) {
                result.Add(BuiltInFontExtensions.DefaultFontConfigName);
            }
            result.AddRange(Enum
                .GetValues(typeof(BuiltInFont))
                .OfType<BuiltInFont>()
                .Select(fontId => BuiltInFontExtensions.ConfigName(fontId))
                .OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase)
            );
            var builtInNames = result.ToArray();
            result.AddRange(_Settings
                .CustomFonts
                .Where(setting => setting.Enable)
                .Select(setting => setting.NormalisedSettingsName())
                .Where(name => !builtInNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase)
            );

            return result;
        }

        private static McduFontFile LoadCustomFont(string configName)
        {
            lock(_SyncLock) {
                if(!_CustomFontMap.TryGetValue(configName, out var result)) {
                    var customSettings = _Settings
                        .CustomFonts
                        .Where(setting => setting.Enable)
                        .FirstOrDefault(setting => String.Equals(
                            configName,
                            setting.NormalisedSettingsName(),
                            StringComparison.InvariantCultureIgnoreCase
                        ));
                    try {
                        if(customSettings != null && File.Exists(customSettings.FontFileName)) {
                            var json = File.ReadAllText(customSettings.FontFileName);
                            result = JsonConvert.DeserializeObject<McduFontFile>(json);
                        }
                    } catch(Exception) {
                        // TODO: Add logging
                    }
                    _CustomFontMap.Add(configName, result);
                }

                return result;
            }
        }
    }
}
