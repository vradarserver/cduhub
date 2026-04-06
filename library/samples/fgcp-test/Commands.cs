// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.CommandLine;
using Cduhub.CommandLine;

namespace FgcpTest
{
    static class Commands
    {
        static Commands()
        {
            Root.EnforceInHouseStandards();

            Connect.SetAction(parse => {
                var command = new Command_Connect() {
                    SuppressCleanup = parse.GetValue(Options.SuppressCleanup),
                };
                Program.Worked = command.Run();
            });

            FcuClock.SetAction(parse => {
                var command = new Command_FcuClock() {
                    SuppressCleanup = parse.GetValue(Options.SuppressCleanup),
                };
                Program.Worked = command.Run();
            });

            FcuDisplays.SetAction(parse => {
                var command = new Command_FcuDisplays() {
                    SuppressCleanup = parse.GetValue(Options.SuppressCleanup),
                };
                Program.Worked = command.Run();
            });

            FcuLamps.SetAction(parse => {
                var command = new Command_FcuLamps() {
                    SuppressCleanup = parse.GetValue(Options.SuppressCleanup),
                };
                Program.Worked = command.Run();
            });

            ShowDevices.SetAction(parse => {
                var command = new Command_ShowDevices();
                Program.Worked = command.Run();
            });
        }

        public static Command Connect = new("connect", "Test connection to a local FGCP device") {
            Options.SuppressCleanup,
        };

        public static Command FcuClock = new("fcu-clock", "Show a clock on the FCU") {
            Options.SuppressCleanup,
        };

        public static Command FcuDisplays = new("fcu-displays", "Test the FCU segment displays") {
            Options.SuppressCleanup,
        };

        public static Command FcuLamps = new("fcu-lamps", "Test the FCU LED lamps") {
            Options.SuppressCleanup,
        };

        public static Command ShowDevices = new("show-devices", "Show USB devices") {
        };

        public static RootCommand Root = new("Tests interactions with an FGCP (I.E. an FCU or MCP) device.") {
            Commands.ShowDevices,
            Commands.Connect,
            Commands.FcuDisplays,
            Commands.FcuLamps,
            Commands.FcuClock,
        };
    }
}
