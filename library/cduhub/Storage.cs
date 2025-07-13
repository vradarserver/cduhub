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
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Cduhub
{
    /// <summary>
    /// Manages the loading and saving of configuration objects.
    /// </summary>
    /// <remarks>
    /// Each configuration object is given a name. This is turned into a filename within the local
    /// configuration folder. Each page should endeavour to choose a name that is unique to the page.
    /// </remarks>
    public static class Storage
    {
        private static string _Folder;
        /// <summary>
        /// The folder where configuration files are saved.
        /// </summary>
        public static string Folder => _Folder;

        /// <summary>
        /// Static ctor.
        /// </summary>
        static Storage()
        {
            _Folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CduHub",
                "Config"
            );
        }

        /// <summary>
        /// Loads the settings stored against the name passed across and attempts to deserialise them into an object
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Load<T>(string name)
            where T: class, new()
        {
            T result = null;

            var fileName = BuildFileName(name);
            if(File.Exists(fileName)) {
                var json = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<T>(json);
            }

            return result ?? new T();
        }

        /// <summary>
        /// Saves the object against the name passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public static void Save<T>(string name, T obj)
        {
            var fileName = BuildFileName(name);

            if(obj != null) {
                var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                File.WriteAllText(fileName, json);
            } else {
                if(File.Exists(fileName)) {
                    File.Delete(fileName);
                }
            }
        }

        private static string BuildFileName(string name)
        {
            if(!Directory.Exists(_Folder)) {
                Directory.CreateDirectory(_Folder);
            }

            return Path.Combine(
                _Folder,
                $"{SanitiseName(name)}.json"
            );
        }

        private static string SanitiseName(string name)
        {
            var result = new StringBuilder();

            var invalidCharacters = Path.GetInvalidFileNameChars();

            foreach(var ch in name) {
                if(ch == ' ') {
                    result.Append('-');
                } else if(invalidCharacters.Contains(ch)) {
                    result.Append('_');
                } else {
                    result.Append(ch);
                }
            }

            return result.ToString();
        }
    }
}
