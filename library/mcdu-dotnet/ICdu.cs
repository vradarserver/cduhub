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

namespace McduDotNet
{
    /// <summary>
    /// The common interface for all CDU devices that the library can display text on and
    /// take keyboard input from.
    /// </summary>
    public interface ICdu : IDisposable
    {
        /// <summary>
        /// The USB and library identifiers for the device.
        /// </summary>
        DeviceIdentifier DeviceId { get; }

        /// <summary>
        /// The MCDU's display.
        /// </summary>
        Screen Screen { get; }

        /// <summary>
        /// The MCDU's LED lights.
        /// </summary>
        Leds Leds { get; }

        /// <summary>
        /// The MCDU's colour palette.
        /// </summary>
        Palette Palette { get; }

        /// <summary>
        /// Gets and sets the display brightness as a percentage between 0 and 100.
        /// </summary>
        int DisplayBrightnessPercent { get; set; }

        /// <summary>
        /// Gets and sets the backlight brightness as a percentage between 0 and 100.
        /// </summary>
        int BacklightBrightnessPercent { get; set; }

        /// <summary>
        /// Gets and sets the LED brightness as a percentage between 0 and 100.
        /// </summary>
        int LedBrightnessPercent { get; set; }

        /// <summary>
        /// Gets and sets the offset of the display from the left edge.
        /// </summary>
        int XOffset { get; set; }

        /// <summary>
        /// Gets and sets the offset of the display from the top edge.
        /// </summary>
        int YOffset { get; set; }

        /// <summary>
        /// True if the device has ambient light sensors.
        /// </summary>
        bool HasAmbientLightSensor { get; }

        /// <summary>
        /// Gets the native value from the device's left ambient light sensor. The meaning
        /// of the value is device dependent.
        /// </summary>
        int LeftAmbientLightNative { get; }

        /// <summary>
        /// Gets the native value from the device's right ambient light sensor. The meaning
        /// of the value is device dependent.
        /// </summary>
        int RightAmbientLightNative { get; }

        /// <summary>
        /// Gets a normalised ambient light value calculated from <see
        /// cref="LeftAmbientLightNative"/> and <see cref="RightAmbientLightNative"/>,
        /// where 0 is completely dark and 100 is completely illuminated.
        /// </summary>
        int AmbientLightPercent { get; }

        /// <summary>
        /// A fluent interface for drawing into the <see cref="Screen"/>.
        /// </summary>
        Compositor Output { get; }

        /// <summary>
        /// Raised when a key is pressed.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyDown;

        /// <summary>
        /// Raised when a key is released.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyUp;

        /// <summary>
        /// Raised when <see cref="LeftAmbientLightNative"/> changes.
        /// </summary>
        event EventHandler LeftAmbientLightChanged;

        /// <summary>
        /// Raised when <see cref="RightAmbientLightNative"/> changes.
        /// </summary>
        event EventHandler RightAmbientLightChanged;

        /// <summary>
        /// Raised when <see cref="AmbientLightPercent"/> changes.
        /// </summary>
        event EventHandler AmbientLightChanged;

        /// <summary>
        /// Raised when the MCDU has been disconnected.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Copies the content of <see cref="Screen"/> to the display.
        /// </summary>
        /// <param name="skipDuplicateCheck">
        /// The display is normally not refreshed if the library thinks that nothing has changed since the last
        /// refresh. Setting this parameter to true skips that test.
        /// </param>
        void RefreshDisplay(bool skipDuplicateCheck = false);

        /// <summary>
        /// Copies the content of <see cref="Leds"/> to the unit.
        /// </summary>
        /// <param name="skipDuplicateCheck">
        /// The LEDs are normally not refreshed if the library thinks that nothing has changed since the last
        /// refresh. Setting this parameter to true skips that test.
        /// </param>
        void RefreshLeds(bool skipDuplicateCheck = false);

        /// <summary>
        /// Copies the content of <see cref="Palette"/> to the device.
        /// </summary>
        /// <param name="skipDuplicateCheck">
        /// The palette is not normally refreshed if the library thinks that nothing has
        /// changed since the last refresh. Setting this parameter to true skips that
        /// test.
        /// </param>
        /// <param name="forceDisplayRefresh">
        /// Refreshing the palette has no effect on text already on screen. Setting this
        /// flag to true calls <see cref="RefreshDisplay"/> with the duplicate check
        /// skipped.
        /// </param>
        void RefreshPalette(bool skipDuplicateCheck = false, bool forceDisplayRefresh = true);

        /// <summary>
        /// Sets the backlight, display and LED brightnesses even if the code believes that
        /// they have not changed.
        /// </summary>
        void RefreshBrightnesses();

        /// <summary>
        /// Sends a font to the device.
        /// </summary>
        /// <param name="fontFileContent"></param>
        /// <param name="useFullWidth">
        /// True to use the full width of the CDU, false for tighter kerning that looks more
        /// accurate but leaves a gap between either side of the display.
        /// </param>
        void UseFont(McduFontFile fontFileContent, bool useFullWidth);

        /// <summary>
        /// Resets the display and turns everything off.
        /// </summary>
        /// <param name="backlightBrightnessPercent">Defaults to 0.</param>
        /// <param name="displayBrightnessPercent">Defaults to 0.</param>
        /// <param name="ledBrightnessPercent">Defaults to 0.</param>
        void Cleanup(
            int backlightBrightnessPercent = 0,
            int displayBrightnessPercent = 0,
            int ledBrightnessPercent = 0
        );
    }
}
