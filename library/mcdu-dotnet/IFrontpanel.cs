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

namespace WwDevicesDotNet
{
    /// <summary>
    /// The common interface for all frontpanel devices (FCU, EFIS, etc.) that the library
    /// can interact with.
    /// </summary>
    public interface IFrontpanel : IDisposable
    {
        /// <summary>
        /// The USB and library identifiers for the device.
        /// </summary>
        DeviceIdentifier DeviceId { get; }

        /// <summary>
        /// Gets a value indicating whether the device is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Raised when a button or control is pressed/activated.
        /// </summary>
        event EventHandler<FrontpanelEventArgs> ControlActivated;

        /// <summary>
        /// Raised when a button or control is released/deactivated.
        /// </summary>
        event EventHandler<FrontpanelEventArgs> ControlDeactivated;

        /// <summary>
        /// Raised when the USB device has been disconnected.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Updates the display(s) on the frontpanel.
        /// </summary>
        /// <param name="state">The state to display.</param>
        void UpdateDisplay(IFrontpanelState state);

        /// <summary>
        /// Updates the LEDs on the frontpanel.
        /// </summary>
        /// <param name="leds">The LED states to apply.</param>
        void UpdateLeds(IFrontpanelLeds leds);

        /// <summary>
        /// Sets the brightness levels for the frontpanel.
        /// </summary>
        /// <param name="panelBacklight">Panel backlight brightness (0-255).</param>
        /// <param name="lcdBacklight">LCD backlight brightness (0-255).</param>
        /// <param name="ledBacklight">LED backlight brightness (0-255).</param>
        void SetBrightness(byte panelBacklight, byte lcdBacklight, byte ledBacklight);
    }

    /// <summary>
    /// Event arguments for frontpanel control events.
    /// </summary>
    public class FrontpanelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the control identifier.
        /// </summary>
        public string ControlId { get; }

        /// <summary>
        /// Gets the raw data associated with this event.
        /// </summary>
        public byte[] RawData { get; }

        public FrontpanelEventArgs(string controlId, byte[] rawData)
        {
            ControlId = controlId;
            RawData = rawData;
        }
    }

    /// <summary>
    /// Event arguments for frontpanel rotary encoder events.
    /// </summary>
    public class FrontpanelRotaryEventArgs : FrontpanelEventArgs
    {
        /// <summary>
        /// Gets the direction of rotation. Positive for clockwise, negative for counter-clockwise.
        /// </summary>
        public int Direction { get; }

        public FrontpanelRotaryEventArgs(string controlId, int direction, byte[] rawData)
            : base(controlId, rawData)
        {
            Direction = direction;
        }
    }

    /// <summary>
    /// Interface for frontpanel display state.
    /// </summary>
    public interface IFrontpanelState
    {
    }

    /// <summary>
    /// Interface for frontpanel LED states.
    /// </summary>
    public interface IFrontpanelLeds
    {
    }
}
