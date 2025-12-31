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
using WwDevicesDotNet;

namespace Cduhub.FlightSim
{
    public abstract class SimulatedMcdus : IFlightSimulatorMcdu
    {
        /// <inheritdoc/>
        public abstract string FlightSimulatorName { get; }

        /// <inheritdoc/>
        public abstract string AircraftName { get; }

        /// <inheritdoc/>
        public abstract DeviceType TargetDeviceType { get; }

        public Screen MasterScreen { get; }

        public Leds MasterLeds { get; }

        public abstract SimulatorMcduBuffer PilotBuffer { get; }

        public abstract SimulatorMcduBuffer FirstOfficerBuffer { get; }

        public virtual SimulatorMcduBuffer ObserverBuffer => FirstOfficerBuffer;

        public virtual bool IsObserverMcduPresent => false;

        public DeviceUser SelectedBufferDeviceUser { get; set; } = DeviceUser.Captain;

        public SimulatorMcduBuffer SelectedBuffer
        {
            get {
                switch(SelectedBufferDeviceUser) {
                    case DeviceUser.Captain:         return PilotBuffer;
                    case DeviceUser.FirstOfficer:    return FirstOfficerBuffer;
                    case DeviceUser.Observer:        return ObserverBuffer;
                }
                return null;
            }
        }

        /// <inheritdoc/>
        public ConnectionState ConnectionState { get; private set; }

        protected bool IsConnected => ConnectionState == ConnectionState.Connected;

        /// <inheritdoc/>
        public long CountMessagesFromSimulator { get; private set; }

        /// <inheritdoc/>
        public DateTime LastMessageTimeUtc { get; private set; }

        /// <inheritdoc/>
        public event EventHandler ConnectionStateChanged;

        protected virtual void OnConnectionStateChanged() => ConnectionStateChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler DisplayRefreshRequired;

        protected virtual void OnDisplayRefreshRequired() => DisplayRefreshRequired?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler LedsRefreshRequired;

        protected virtual void OnLedsRefreshRequired() => LedsRefreshRequired?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler MessageReceived;

        protected virtual void OnMessageReceived() => MessageReceived?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="deviceUser"></param>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        public SimulatedMcdus(DeviceUser deviceUser, Screen masterScreen, Leds masterLeds)
        {
            SelectedBufferDeviceUser = deviceUser == DeviceUser.NotApplicable
                ? DeviceUser.Captain
                : deviceUser;
            MasterScreen = masterScreen;
            MasterLeds = masterLeds;
        }

        /// <inheritdoc/>
        public void AdvanceSelectedBufferProductId()
        {
            switch(SelectedBufferDeviceUser) {
                case DeviceUser.Captain:
                    SelectedBufferDeviceUser = DeviceUser.FirstOfficer;
                    break;
                case DeviceUser.FirstOfficer:
                    SelectedBufferDeviceUser = IsObserverMcduPresent
                        ? DeviceUser.Observer
                        : DeviceUser.Captain;
                    break;
                case DeviceUser.Observer:
                    SelectedBufferDeviceUser = DeviceUser.Captain;
                    break;
            }
            RefreshSelectedScreen();
            RefreshSelectedLeds();
        }

        public void RefreshSelectedScreen()
        {
            MasterScreen.CopyFrom(SelectedBuffer.Screen);
            OnDisplayRefreshRequired();
        }

        public void RefreshSelectedLeds()
        {
            MasterLeds.CopyFrom(SelectedBuffer.Leds);
            OnLedsRefreshRequired();
        }

        /// <inheritdoc/>
        public abstract void SendKeyToSimulator(Key key, bool pressed);

        /// <inheritdoc/>
        public abstract void ReconnectToSimulator();

        /// <summary>
        /// Assigns <see cref="ConnectionState"/> and raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="connected"></param>
        protected void RecordConnectionState(ConnectionState state)
        {
            if(ConnectionState != state) {
                ConnectionState = state;
                OnConnectionStateChanged();
            }
        }

        /// <summary>
        /// Updates counters and statistics, and raises events, when a message is received from the simulator.
        /// </summary>
        protected void RecordMessageReceivedFromSimulator()
        {
            LastMessageTimeUtc = DateTime.UtcNow;
            ++CountMessagesFromSimulator;
            OnMessageReceived();
        }
    }
}
