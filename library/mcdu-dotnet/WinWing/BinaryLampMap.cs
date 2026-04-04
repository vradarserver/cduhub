// Copyright © 2026 onwards, Andrew Whewell
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

namespace McduDotNet.WinWing
{
    class BinaryLampMap
    {
        private BinaryLamp[] _BinaryLamps;
        public IReadOnlyList<BinaryLamp> BinaryLamps => _BinaryLamps;

        public BinaryLampMap(BinaryLamp[] binaryLamps)
        {
            _BinaryLamps = binaryLamps ?? throw new ArgumentNullException(nameof(binaryLamps));
        }

        public BinaryLampMap(BinaryLampMap map)
        {
            if(map == null) {
                throw new ArgumentNullException(nameof(map));
            }
            _BinaryLamps = new BinaryLamp[map._BinaryLamps.Length];
            Array.Copy(map._BinaryLamps, _BinaryLamps, _BinaryLamps.Length);
        }

        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is BinaryLampMap other && _BinaryLamps.Length == other._BinaryLamps.Length) {
                result = true;
                for(var idx = 0;idx < _BinaryLamps.Length;++idx) {
                    if(!(result = _BinaryLamps[idx] == other._BinaryLamps[idx])) {
                        break;
                    }
                }
            }

            return result;
        }

        public override int GetHashCode()
        {
            return _BinaryLamps.Length;
        }

        public void SetLamp(int index, bool on)
        {
            if(index < 0 || index >= _BinaryLamps.Length) {
                throw new IndexOutOfRangeException(nameof(index));
            }
            var lamp = _BinaryLamps[index];
            if(lamp.On != on) {
                _BinaryLamps[index] = new(lamp.ExternalId, lamp.LedId, on);
            }
        }

        public bool ContainsExternalId(int externalId)
        {
            var result = false;
            for(var idx = 0;idx < _BinaryLamps.Length;++idx) {
                if(_BinaryLamps[idx].ExternalId == externalId) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void CopyFrom(BinaryLampMap other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            if(_BinaryLamps.Length != other._BinaryLamps.Length) {
                throw new InvalidOperationException("Length mismatch");
            }
            for(var idx = 0;idx < _BinaryLamps.Length;++idx) {
                var lhs = _BinaryLamps[idx];
                var rhs = other._BinaryLamps[idx];
                if(lhs.ExternalId != rhs.ExternalId) {
                    throw new InvalidOperationException("External ID mismatch");
                }
                if(lhs.LedId != rhs.LedId) {
                    throw new InvalidOperationException("LED ID mismatch");
                }
                _BinaryLamps[idx] = rhs;
            }
        }

        public void CopyTo(BinaryLampMap? other) => other?.CopyFrom(this);
    }
}
