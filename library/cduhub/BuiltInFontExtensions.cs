﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace Cduhub
{
    public static class BuiltInFontExtensions
    {
        // These are stored in configuration files, never change them.
        public const string DefaultFontReference =  "DEFAULT";
        public const string DefaultFontConfigName = B612;
        public const string A320 =                  "A320";
        public const string A320NEO =               "A320NEO";
        public const string B612 =                  "B612";
        public const string EGA =                   "EGA";

        public static string ConfigName(this BuiltInFont builtInFont)
        {
            switch(builtInFont) {
                case BuiltInFont.A320:      return A320;
                case BuiltInFont.A320Neo:   return A320NEO;
                case BuiltInFont.B612:      return B612;
                case BuiltInFont.EGA:       return EGA;
                default:                    throw new NotImplementedException();
            }
        }

        public static BuiltInFont? ToBuiltInFont(string configName)
        {
            BuiltInFont? result = null;

            configName = (configName ?? "").Trim();
            if(configName == "" || String.Equals(configName, DefaultFontReference, StringComparison.InvariantCultureIgnoreCase)) {
                configName = DefaultFontConfigName;
            }

            foreach(BuiltInFont value in Enum.GetValues(typeof(BuiltInFont))) {
                var valueName = ConfigName(value);
                if(String.Equals(configName, valueName, StringComparison.InvariantCultureIgnoreCase)) {
                    result = value;
                    break;
                }
            }

            return result;
        }
    }
}
