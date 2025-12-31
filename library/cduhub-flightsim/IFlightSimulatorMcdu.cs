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
using wwDevicesDotNet;

namespace Cduhub.FlightSim
{
    public interface IFlightSimulatorMcdu
    {
        /// <summary>
        /// The name of the flight simulator.
        /// </summary>
        string FlightSimulatorName { get; }

        /// <summary>
        /// The name of the add-on product or aircraft.
        /// </summary>
        string AircraftName { get; }

        /// <summary>
        /// The general type of CDU device that the flight simulator targets. This will be
        /// <see cref="DeviceType.NotSpecified"/> if the simulator is not for a specific
        /// aircraft.
        /// </summary>
        DeviceType TargetDeviceType { get; }

        /// <summary>
        /// The content of the simulated pilot MCDU.
        /// </summary>
        SimulatorMcduBuffer PilotBuffer { get; }

        /// <summary>
        /// The content of the simulated first-officer MCDU.
        /// </summary>
        SimulatorMcduBuffer FirstOfficerBuffer { get; }

        /// <summary>
        /// The content of the simulated observer's MCDU.
        /// </summary>
        SimulatorMcduBuffer ObserverBuffer { get; }

        /// <summary>
        /// True if the aircraft has, and the simulator simulates, an observer's MCDU. If this is false then
        /// <see cref="ObserverBuffer"/> will be the first officer's MCDU.
        /// </summary>
        bool IsObserverMcduPresent { get; }

        /// <summary>
        /// The user of the selected simulated MCDU.
        /// </summary>
        DeviceUser SelectedBufferDeviceUser { get; set; }

        /// <summary>
        /// The content of the selected simulated MCDU.
        /// </summary>
        SimulatorMcduBuffer SelectedBuffer { get; }

        /// <summary>
        /// True if we have an active connection to the simulated MCDU.
        /// </summary>
        ConnectionState ConnectionState { get; }

        /// <summary>
        /// The number of messages received from the simulator.
        /// </summary>
        long CountMessagesFromSimulator { get; }

        /// <summary>
        /// The time at UTC that the last message was received.
        /// </summary>
        DateTime LastMessageTimeUtc { get; }

        /// <summary>
        /// Raised when <see cref="ConnectionState"/> changes.
        /// </summary>
        event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raised when the USB MCDU needs to have its display updated.
        /// </summary>
        event EventHandler DisplayRefreshRequired;

        /// <summary>
        /// Raised when the USB MCDU needs to have its LEDs updated.
        /// </summary>
        event EventHandler LedsRefreshRequired;

        /// <summary>
        /// Raised when <see cref="CountMessagesFromSimulator"/> and <see cref="LastMessageTimeUtc"/> are
        /// updated.
        /// </summary>
        event EventHandler MessageReceived;

        /// <summary>
        /// Sends a key to the simulated MCDU.
        /// </summary>
        /// <param name="mcduKey"></param>
        /// <param name="pressed"></param>
        void SendKeyToSimulator(Key mcduKey, bool pressed);

        /// <summary>
        /// Select the next simulated MCDU.
        /// </summary>
        void AdvanceSelectedBufferProductId();

        /// <summary>
        /// Reconnects to the simulated MCDU.
        /// </summary>
        void ReconnectToSimulator();
    }
}
