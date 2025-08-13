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
using System.CommandLine.Parsing;

namespace Cduhub.CommandLine.Parsers
{
    public class HexByteParser
    {
        public static byte ParseByte(ArgumentResult argResult, byte defaultValue = 0)
        {
            var result = defaultValue;
            if(argResult.Tokens.Count == 1) {
                var originalToken = argResult.Tokens[0].Value.ToString();
                var token = originalToken;
                if(token.StartsWith("0x") || token.StartsWith("0X")) {
                    token = token.Substring(2);
                }
                var parsed = TryParseByte(token, out var parsedValue);
                if(parsed) {
                    result = (byte)parsedValue;
                } else {
                    argResult.AddError($"{originalToken} is not a hex byte");
                }
            }

            return result;
        }

        public static byte[] ParseByteArray(ArgumentResult argResult)
        {
            var result = new List<byte>();
            if(argResult.Tokens.Count == 1) {
                var originalToken = argResult.Tokens[0].Value.ToString();
                var token = originalToken;
                if(token.StartsWith("0x") || token.StartsWith("0X")) {
                    token = token.Substring(2);
                }
                var parsed = false;
                if(token.Length > 0) {
                    if(token.Length % 2 != 0) {
                        argResult.AddError($"A byte array hex string must be an even length of digits");
                    } else {
                        parsed = true;
                        for(var chunkIdx = 0;parsed && chunkIdx < token.Length;chunkIdx += 2) {
                            var chunk = token.Substring(chunkIdx, 2);
                            parsed = TryParseByte(chunk, out var byteValue);
                            if(!parsed) {
                                break;
                            }
                            result.Add(byteValue);
                        }
                    }
                }
                if(!parsed) {
                    argResult.AddError($"{originalToken} is not a hex byte array string");
                }
            }

            return result.ToArray();
        }

        private static bool TryParseByte(string chunk, out byte value)
        {
            var parsed = false;
            value = 0;
            if(chunk.Length > 0 && chunk.Length < 3) {
                for(var idx = 0;idx < chunk.Length;++idx) {
                    var ch = chunk[idx];
                    if(!TryConvertNibble(ch, out var nibble)) {
                        parsed = false;
                        break;
                    }
                    if(idx == 1) {
                        value = (byte)(value << 4);
                    }
                    value = (byte)(value | nibble);
                    parsed = true;
                }
            }

            return parsed;
        }

        private static bool TryConvertNibble(char ch, out byte nibble)
        {
            var result = true;
            ch = char.ToLower(ch);
            if(ch >= '0' && ch <= '9') {
                nibble = (byte)(ch - '0');
            } else if(ch >= 'a' && ch <= 'f') {
                nibble = (byte)((ch - 'a') + 10);
            } else if(ch >= 'A' && ch <= 'F') {
                nibble = (byte)((ch - 'A') + 10);
            } else {
                nibble = 0;
                result = false;
            }

            return result;
        }

    }
}
