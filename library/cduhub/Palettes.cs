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
using Cduhub.Config;
using wwDevicesDotNet;
using Newtonsoft.Json;

namespace Cduhub
{
    /// <summary>
    /// Manages the palettes available to a page.
    /// </summary>
    public static class Palettes
    {
        private static CustomPaletteSettings _Settings;
        private static readonly Palette _FenixA320Palette;
        private static readonly Palette _FlyByWireA32NXPalette;
        private static readonly Palette _TolissA32NXPalette;
        private static readonly Palette _XPlane12A330Palette;
        private static readonly object _SyncLock = new object();
        private static Dictionary<string, Palette> _CustomPaletteMap = new Dictionary<string, Palette>(StringComparer.InvariantCultureIgnoreCase);

        public static Exception LoadSettingsException { get; }

        public static Palette DefaultPalette => FenixA320Palette;

        public static Palette FenixA320Palette => _FenixA320Palette;

        public static Palette FlyByWireA32NXPalette => _FlyByWireA32NXPalette;

        public static Palette TolissA32NXPalette => _TolissA32NXPalette;

        public static Palette XPlane12A330Palette => _XPlane12A330Palette;

        static Palettes()
        {
            LoadSettingsException = null;
            try {
                _Settings = ConfigStorage.Load<CustomPaletteSettings>();
            } catch(Exception ex) {
                LoadSettingsException = ex;
                _Settings = new CustomPaletteSettings();
            }

            _FenixA320Palette = ParsePalette(CduHubResources.fenix_a320_palette_json);
            _FlyByWireA32NXPalette = ParsePalette(CduHubResources.fbw_a32nx_palette_json);
            _TolissA32NXPalette = ParsePalette(CduHubResources.toliss_a32nx_palette_json);
            _XPlane12A330Palette = ParsePalette(CduHubResources.xplane_12_a330_palette_json);
        }

        public static Palette ParsePalette(byte[] jsonBytes, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var json = encoding.GetString(jsonBytes);
            var customPalette = JsonConvert.DeserializeObject<CustomPalette>(json);
            return customPalette?.ToPalette();
        }

        public static Palette LoadBuiltInPalette(BuiltInPalette builtInPalette)
        {
            switch(builtInPalette) {
                case BuiltInPalette.FenixA320:      return FenixA320Palette;
                case BuiltInPalette.FlyByWireA32NX: return FlyByWireA32NXPalette;
                case BuiltInPalette.TolissA32NX:    return TolissA32NXPalette;
                case BuiltInPalette.XPlane12A330:   return XPlane12A330Palette;
                default:                            throw new NotImplementedException();
            }
        }

        public static Palette LoadByConfigName(string configName)
        {
            Palette result = null;

            configName = (configName ?? "").Trim();

            if(configName != "") {
                var builtInPalette = BuiltInPaletteExtensions.ToBuiltInPalette(configName);
                if(builtInPalette != null) {
                    result = LoadBuiltInPalette(builtInPalette.Value);
                } else {
                    result = LoadCustomPalette(configName);
                }
            }

            return result ?? DefaultPalette;
        }

        public static IReadOnlyList<string> GetAllConfigNames(bool includeDefault)
        {
            var result = new List<string>();
            if(includeDefault) {
                result.Add(BuiltInPaletteExtensions.DefaultPaletteConfigName);
            }
            result.AddRange(Enum
                .GetValues(typeof(BuiltInPalette))
                .OfType<BuiltInPalette>()
                .Select(paletteId => BuiltInPaletteExtensions.ConfigName(paletteId))
                .OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase)
            );
            var builtInNames = result.ToArray();
            result.AddRange(_Settings
                .Palettes
                .Where(setting => setting.Enable)
                .Select(setting => setting.NormalisedSettingsName())
                .Where(name => !builtInNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase)
            );

            return result;
        }

        private static Palette LoadCustomPalette(string configName)
        {
            lock(_SyncLock) {
                if(!_CustomPaletteMap.TryGetValue(configName, out var result)) {
                    var customPalette = _Settings
                        .Palettes
                        .Where(setting => setting.Enable)
                        .FirstOrDefault(setting => String.Equals(
                            configName,
                            setting.NormalisedSettingsName(),
                            StringComparison.InvariantCultureIgnoreCase
                        ));
                    try {
                        if(customPalette != null) {
                            result = customPalette.ToPalette();
                        }
                    } catch(Exception) {
                        // TODO: Add logging
                    }
                    _CustomPaletteMap.Add(configName, result);
                }

                return result;
            }
        }
    }
}
