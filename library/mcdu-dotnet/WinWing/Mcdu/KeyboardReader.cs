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
using HidSharp;

namespace McduDotNet.WinWing.Mcdu
{
    /// <summary>
    /// Reads the WinWing MCDU's keyboard and raises events on the parent device when keys
    /// are pressed or released, or when the ambient light sensors change value.
    /// </summary>
    class KeyboardReader : UsbPollingReader
    {
        private readonly Action<Key, bool> _KeyPressAction;
        private readonly Action<UInt16, UInt16> _AmbientLightChangedAction;
        private readonly InputReport _InputReport_Previous = new InputReport();
        private readonly InputReport _InputReport_Current = new InputReport();
        private (UInt64, UInt64, UInt64) _PreviousInputReportDigest = (0,0,0);

        /// <inheritdoc/>
        protected override int PacketSize => InputReport.PacketLength;

        public KeyboardReader(
            HidStream hidStream,
            Action<Key, bool> keyPressAction,
            Action<UInt16, UInt16> ambientLightChangedAction
        ) : base(hidStream)
        {
            _KeyPressAction = keyPressAction;
            _AmbientLightChangedAction = ambientLightChangedAction;
        }

        protected override void ReportReceived(byte[] readBuffer, int bytesRead)
        {
            _InputReport_Current.CopyFrom(readBuffer, 0, bytesRead);
            var digest = _InputReport_Current.ToDigest();
            if(   digest.Item1 != _PreviousInputReportDigest.Item1
               || digest.Item2 != _PreviousInputReportDigest.Item2
               || digest.Item3 != _PreviousInputReportDigest.Item3
            ) {
                try {
                    foreach(Key key in Enum.GetValues(typeof(Key))) {
                        var pressed = _InputReport_Current.IsKeyPressed(key);
                        var wasPressed = _InputReport_Previous.IsKeyPressed(key);
                        if(pressed != wasPressed) {
                            _KeyPressAction(key, pressed);
                        }
                    }
                } catch {
                    // Swallow exceptions for now - ultimately we want the events raised on a different thread
                }

                var previousAmbient = _InputReport_Previous.AmbientLightSensorValues();
                var currentAmbient = _InputReport_Current.AmbientLightSensorValues();
                if(previousAmbient != currentAmbient) {
                    _AmbientLightChangedAction(
                        currentAmbient.LeftSensor,
                        currentAmbient.RightSensor
                    );
                }

                _InputReport_Previous.CopyFrom(_InputReport_Current);
                _PreviousInputReportDigest = digest;
            }
        }
    }
}
