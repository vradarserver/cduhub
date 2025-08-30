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
using System.Net;

namespace Cduhub
{
    public static class Validate
    {
        public static bool IsValidForAirportIcao(string icao)
        {
            return icao?.Length == 4
                && !icao.Any(ch => ch < 'A' || ch > 'Z');
        }

        public static bool IsValidForWebSocketUri(
            string host = "localhost",
            int port = 1234
        )
        {
            bool result;
            try {
                result = Uri.TryCreate($"ws://{host}:{port}/endpoint", UriKind.Absolute, out _);
            } catch {
                result = false;
            }

            return result;
        }

        public static bool IsValidForHttpEndPoint(
            string host = "127.0.0.1",
            int port = 1234
        )
        {
            bool result;
            try {
                result = Uri.TryCreate($"http://{host}:{port}/endpoint", UriKind.Absolute, out _);
            } catch {
                result = false;
            }

            return result;
        }

        public static bool IsValidForUdpEndPoint(
            string host = "127.0.0.1",
            int port = 1234
        )
        {
            bool result;
            try {
                result = IPAddress.TryParse(host, out var address);
                result = result && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort;
                if(result) {
                    _ = new IPEndPoint(address, port);
                }
            } catch {
                result = false;
            }

            return result;
        }

        public static bool IsValidDirectoryName(string directoryName)
        {
            var result = !String.IsNullOrWhiteSpace(directoryName);
            if(result) {
                var invalidCharacters = Path.GetInvalidPathChars();
                foreach(var ch in directoryName) {
                    result = ch != Path.AltDirectorySeparatorChar
                          && ch != Path.DirectorySeparatorChar
                          && !invalidCharacters.Contains(ch);
                    if(!result) {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
