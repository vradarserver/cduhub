// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text.RegularExpressions;

namespace Cduhub.FlightSim.XPlaneUdpModels
{
    public class XPlaneDataRefValue
    {
        private static Regex _ArrayDataRefRegex = new Regex(@"^(?<dataRef>.*)\[(?<idx>\d+)\]$", RegexOptions.Compiled);

        public string DataRef { get; }

        public float Value { get; }

        public XPlaneDataRefValue(string dataRef, float value)
        {
            DataRef = dataRef;
            Value = value;
        }

        public override string ToString() => $"{DataRef}={Value}";

        /// <summary>
        /// Splits <see cref="DataRef"/> s that represent an array name into the name and the array index.
        /// </summary>
        /// <returns>
        /// A tuple whose first item is the dataref name without the index element and whose second item is
        /// the index into the array. If <see cref="DataRef"/> is not an array name then <see cref="DataRef"/>
        /// is returned unchanged in the first item and -1 is returned in the second.
        /// </returns>
        public (string DataRef, int Index) ParseArrayDataRef()
        {
            var match = _ArrayDataRefRegex.Match(DataRef);
            if(!match.Success || !int.TryParse(match.Groups["idx"].Value, out var idx)) {
                idx = -1;
            }
            return !match.Success
                ? (DataRef, idx)
                : (match.Groups["dataRef"].Value, idx);
        }
    }
}
