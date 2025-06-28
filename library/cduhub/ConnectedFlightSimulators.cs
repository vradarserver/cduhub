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
using System.Linq;
using Cduhub.FlightSim;

namespace Cduhub
{
    /// <summary>
    /// A collection of flight simulators that the hub is maintaining a connection to.
    /// </summary>
    public static class ConnectedFlightSimulators
    {
        private static readonly object _WriteLock = new object();

        private static volatile IFlightSimulatorMcdu[] _FlightSimulatorMcdus = Array.Empty<IFlightSimulatorMcdu>();

        /// <summary>
        /// Raised when one of the connected flight simulators raises an event.
        /// </summary>
        public static event EventHandler FlightSimulatorStateChanged;

        /// <summary>
        /// Raises <see cref="FlightSimulatorStateChanged"/>.
        /// </summary>
        private static void OnFlightSimulatorStateChanged() => FlightSimulatorStateChanged?.Invoke(null, EventArgs.Empty);

        /// <summary>
        /// Adds a new flight simulator MCDU to the collection.
        /// </summary>
        /// <param name="mcdu"></param>
        /// <returns>True if the MCDU was added.</returns>
        public static bool AddFlightSimulatorMcdu(IFlightSimulatorMcdu mcdu)
        {
            var result = mcdu != null;
            if(result) {
                lock(_WriteLock) {
                    if(result = !_FlightSimulatorMcdus.Contains(mcdu)) {
                        var newList = new IFlightSimulatorMcdu[_FlightSimulatorMcdus.Length + 1];
                        Array.Copy(_FlightSimulatorMcdus, newList, _FlightSimulatorMcdus.Length);
                        newList[newList.Length - 1] = mcdu;
                        _FlightSimulatorMcdus = newList;
                    }
                }
                if(result) {
                    mcdu.MessageReceived += FlightSimulatorMcdu_MessageReceived;
                    OnFlightSimulatorStateChanged();
                }
            }

            return result;
        }

        /// <summary>
        /// Removes a flight simulator MCDU from the collection.
        /// </summary>
        /// <param name="mcdu"></param>
        /// <returns>True if the MCDU was removed from the collection.</returns>
        public static bool RemoveFlightSimulatorMcdu(IFlightSimulatorMcdu mcdu)
        {
            var result = mcdu != null;
            if(result) {
                lock(_WriteLock) {
                    if(result = _FlightSimulatorMcdus.Contains(mcdu)) {
                        var newList = new IFlightSimulatorMcdu[_FlightSimulatorMcdus.Length - 1];
                        var offset = 0;
                        for(var idx = 0;idx < _FlightSimulatorMcdus.Length;++idx) {
                            var extant = _FlightSimulatorMcdus[idx];
                            if(extant == mcdu) {
                                offset = -1;
                            } else {
                                newList[idx + offset] = extant;
                            }
                        }
                        _FlightSimulatorMcdus = newList;
                    }
                }
                if(result) {
                    // This means we could potentially raise the state change event for an MCDU that is no longer
                    // in the list. I don't think I care about this very much - for one thing, we don't (as of
                    // time of writing) say *which* simulator changed state, for another the removal is probably
                    // coming during shutdown of the simulator connection and the odds of getting any messages may
                    // be quite low.
                    mcdu.MessageReceived -= FlightSimulatorMcdu_MessageReceived;
                    OnFlightSimulatorStateChanged();
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all connected <see cref="IFlightSimulatorMcdu"/>s.
        /// </summary>
        /// <returns></returns>
        public static IFlightSimulatorMcdu[] GetFlightSimulatorMcdus()
        {
            var result = _FlightSimulatorMcdus;
            return result;
        }

        private static void FlightSimulatorMcdu_MessageReceived(object sender, EventArgs args)
        {
            OnFlightSimulatorStateChanged();
        }
    }
}
