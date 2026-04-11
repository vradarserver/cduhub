// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.CommandLine;
using McduDotNet;

namespace FgcpTest
{
    class Command_Connect : CommonCommand
    {
        private IFgcp? _Fgcp;
        private bool _PollMessageShown;

        public bool SuppressCleanup { get; set; }

        public bool Run()
        {
            var result = false;
            var tickCounter = 0;

            try {
                Console.WriteLine($"Press Q to quit");
                while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                    if(_Fgcp == null && (tickCounter % 10 == 0)) {
                        ConnectToFgcp();
                    }

                    ++tickCounter;
                    Thread.Sleep(100);
                }

                if(_Fgcp != null) {
                    _Fgcp.Disconnected -= Fgcp_Disconnected;
                    if(!SuppressCleanup) {
                        _Fgcp.Cleanup();
                    }
                }
            } finally {
                if(_Fgcp != null) {
                    _Fgcp.Dispose();
                }
            }

            return result;
        }

        private void ConnectToFgcp()
        {
            if(_Fgcp == null) {
                if(_PollMessageShown) {
                    Console.Write('.');
                } else {
                    Console.Write("Connecting to the first available FGCP");
                    _PollMessageShown = true;
                }
                _Fgcp = DeviceFactory.ConnectLocalFgcp();
                if(_Fgcp != null) {
                    Console.WriteLine();
                    Console.WriteLine($"Connected to {_Fgcp}");
                    _Fgcp.Disconnected += Fgcp_Disconnected;
                }
            }
        }

        private void Fgcp_Disconnected(object? sender, EventArgs e)
        {
            var fgcp = _Fgcp;
            if(fgcp != null) {
                Console.WriteLine($"{fgcp} disconnected");
                try {
                    fgcp.Dispose();
                } catch {
                    ;
                }
                _PollMessageShown = false;
                _Fgcp = null;
            }
        }
    }
}
