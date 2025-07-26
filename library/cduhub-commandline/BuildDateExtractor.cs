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
using System.Reflection;

namespace Cduhub.CommandLine
{
    /// <summary>
    /// Methods for extracting the build date from the PE header of an assembly.
    /// </summary>
    /// <remarks>
    /// Note that .NET compilers always emit Windows PE headers, even if the application
    /// is to be executed on other operating systems. However, this isn't going to work
    /// for applications that are pre-jitted into native assemblies, so there is an argument
    /// for moving this to the Windows-only library.
    /// </remarks>
    public static class BuildDateExtractor
    {
        private const int _PEHeaderOffset = 60;
        private const int _PELinkerTimestampOffset = 8;

        /// <summary>
        /// Reads the linker timestamp from a PE header at UTC.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        /// <remarks>
        /// Based on code from here: http://stackoverflow.com/questions/1600962/displaying-the-build-date
        /// </remarks>
        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var result = DateTime.MinValue;

            var fileName = assembly.Location;
            if(!String.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
                var buffer = new byte[2048];
                using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if(_PEHeaderOffset + 4 < bytesRead) {
                        var linkerHeaderOffset = BitConverter.ToInt32(buffer, _PEHeaderOffset);
                        var timestampOffset = linkerHeaderOffset + _PELinkerTimestampOffset;
                        if(timestampOffset + 4 < bytesRead) {
                            var timestamp = BitConverter.ToInt32(buffer, timestampOffset);
                            result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            result = result.AddSeconds(timestamp);
                        }
                    }
                }
            }

            return result;
        }
    }
}
