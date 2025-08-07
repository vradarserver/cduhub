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

namespace Cduhub
{
    public class FormHelper
    {
        private Action _DrawPageAction;

        public FormHelper(Action drawPageAction)
        {
            _DrawPageAction = drawPageAction;
        }

        public string YesNo(bool value) => value ? "YES" : "NO";

        public void ToggleBool(bool value, Action<bool> setValue)
        {
            setValue(!value);
            _DrawPageAction();
        }

        public bool IntegerValue(
            string text,
            Action<int> setValue,
            int minValue = int.MinValue,
            int maxValue = int.MaxValue
        )
        {
            var result = int.TryParse(text, out var value);
            result = result && value >= minValue && value <= maxValue;
            if(result) {
                setValue(value);
                _DrawPageAction();
            }
            return result;
        }

        public void CycleFontNames(string fontName, Action<string> setFontName, bool includeDefaultFontName)
        {
            var fontNames = Fonts.GetAllConfigNames(includeDefaultFontName);
            CycleStrings(fontNames, fontName, setFontName);
        }

        public void CyclePaletteNames(string paletteName, Action<string> setPaletteName, bool includeDefaultPaletteName)
        {
            var paletteNames = Palettes.GetAllConfigNames(includeDefaultPaletteName);
            CycleStrings(paletteNames, paletteName, setPaletteName);
        }

        public void CycleStrings(IEnumerable<string> allValues, string currentValue, Action<string> setNewValue, bool caseInsensitive = true)
        {
            var materialised = allValues?.ToArray() ?? Array.Empty<string>();
            var comparison = caseInsensitive
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            if(materialised.Length > 0) {
                var idx = -1;
                for(var i = 0;i < materialised.Length;++i) {
                    if(String.Equals(materialised[i], currentValue, comparison)) {
                        idx = i;
                        break;
                    }
                }

                if(idx == -1 || ++idx == materialised.Length) {
                    idx = 0;
                }

                setNewValue(materialised[idx]);
                _DrawPageAction();
            }
        }
    }
}
