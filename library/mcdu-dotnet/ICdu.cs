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

namespace WwDevicesDotNet
{
    /// <summary>
    /// The common interface for all CDU devices that the library can display text on, and
    /// take keyboard input from.
    /// </summary>
    public interface ICdu : IDisposable
    {
        /// <summary>
        /// The USB and library identifiers for the device.
        /// </summary>
        DeviceIdentifier DeviceId { get; }

        /// <summary>
        /// The CDU screen buffer. Changes to the screen buffer are not sent to the device
        /// until <see cref="RefreshDisplay"/> is called.
        /// </summary>
        Screen Screen { get; }

        /// <summary>
        /// The CDU LED light buffer. Changes to the LED lights are not sent to the device
        /// until <see cref="RefreshLeds"/> is called.
        /// </summary>
        Leds Leds { get; }

        /// <summary>
        /// The CDU LED palette buffer. Changes to the palette buffer are not sent to the
        /// device until <see cref="RefreshPalette"/> is called.
        /// </summary>
        Palette Palette { get; }

        /// <summary>
        /// Returns a read-only collection of LEDs that the device supports. Reads and
        /// writes of unsupported LEDs are silently ignored.
        /// </summary>
        IReadOnlyList<Led> SupportedLeds { get; }

        /// <summary>
        /// Returns a read-only collection of keys that the device supports.
        /// </summary>
        IReadOnlyList<Key> SupportedKeys { get; }

        /// <summary>
        /// Gets and sets the display backlight as a percentage between 0 and 100. Changes to
        /// this value are immediately sent to the device, but see <see cref="RefreshBrightnesses"/>.
        /// </summary>
        int DisplayBrightnessPercent { get; set; }

        /// <summary>
        /// Gets and sets the keyboard backlight as a percentage between 0 and 100. Changes to
        /// this value are immediately sent to the device, but see <see cref="RefreshBrightnesses"/>.
        /// </summary>
        int BacklightBrightnessPercent { get; set; }

        /// <summary>
        /// Gets and sets the LED intensity as a percentage between 0 and 100. Changes to
        /// this value are immediately sent to the device, but see <see cref="RefreshBrightnesses"/>.
        /// </summary>
        int LedBrightnessPercent { get; set; }

        /// <summary>
        /// Gets and sets the offset of the left edge of the screen text.
        /// </summary>
        int XOffset { get; set; }

        /// <summary>
        /// Gets and sets the offset of the top edge of the screen text.
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
        /// Gets the auto-brightness settings.
        /// </summary>
        AutoBrightnessSettings AutoBrightness { get; }

        /// <summary>
        /// A fluent interface for drawing into the <see cref="Screen"/> buffer.
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
        /// Raised on a background thread when the display is changing.
        /// </summary>
        /// <remarks>
        /// This is passed a clone of an internal display buffer. It is called on a
        /// background thread so that the event handler cannot inadvertently slow down the
        /// updating of the display, or cause the program to deadlock on the USB device
        /// lock.
        /// </remarks>
        event EventHandler<DisplayChangingEventArgs> DisplayChanging;

        /// <summary>
        /// Raised on a background thread when the display palette is changing.
        /// </summary>
        /// <remarks>
        /// This is passed a clone of an internal display palette buffer. It is called on
        /// a background thread so that the event handler cannot inadvertently slow down
        /// the updating of the display, or cause the program to deadlock on the USB
        /// device lock.
        /// </remarks>
        event EventHandler<PaletteChangingEventArgs> PaletteChanging;

        /// <summary>
        /// Raised on a background thread when the font is changing.
        /// </summary>
        /// <remarks>
        /// This is passed a clone of an internal display font buffer. It is called on a
        /// background thread so that the event handler cannot inadvertently slow down the
        /// updating of the display, or cause the program to deadlock on the USB device
        /// lock.
        /// </remarks>
        event EventHandler<FontChangingEventArgs> FontChanging;

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
        /// Raised when the USB device has been disconnected.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Copies the content of <see cref="Screen"/> to the display.
        /// </summary>
        /// <param name="skipDuplicateCheck">
        /// The display is normally not refreshed if the library has not detected a change
        /// to the <see cref="Screen"/> buffer content since the last call. Setting this
        /// parameter to true skips that test.
        /// </param>
        void RefreshDisplay(bool skipDuplicateCheck = false);

        /// <summary>
        /// Copies the content of <see cref="Leds"/> to the unit.
        /// </summary>
        /// <param name="skipDuplicateCheck">
        /// The LEDs are normally not refreshed if the library has not detected a change
        /// to the <see cref="Leds"/> buffer content since the last call. Setting this
        /// parameter to true skips that test.
        /// </param>
        void RefreshLeds(bool skipDuplicateCheck = false);

        /// <summary>
        /// Copies the content of <see cref="Palette"/> to the device.
        /// </summary>
        /// <param name="skipDuplicateCheck">
        /// The palette is normally not refreshed if the library has not detected a change
        /// to the <see cref="Palette"/> buffer content since the last call. Setting this
        /// parameter to true skips that test.
        /// </param>
        /// <param name="forceDisplayRefresh">
        /// Refreshing the palette has no effect on text already on screen. Setting this
        /// flag to true calls <see cref="RefreshDisplay"/> with the duplicate check
        /// skipped after the palette has been changed.
        /// </param>
        void RefreshPalette(bool skipDuplicateCheck = false, bool forceDisplayRefresh = true);

        /// <summary>
        /// Resets the device's backlight and LED intensities to the <see
        /// cref="DisplayBrightnessPercent"/>, <see cref="BacklightBrightnessPercent"/>
        /// and <see cref="LedBrightnessPercent"/> values. Note that, unlike most other
        /// properties, changes to the backlight values are immediately sent to the device,
        /// you do not need to routintely call this function.
        /// </summary>
        void RefreshBrightnesses();

        /// <summary>
        /// Apply the auto-brightness settings. Changes to the auto-brightness settings
        /// are not automatically applied as they are made, you need to call this once the
        /// settings are completely configured.
        /// </summary>
        void ApplyAutoBrightness();

        /// <summary>
        /// Sends a font to the device.
        /// </summary>
        /// <param name="fontFileContent"></param>
        /// <param name="useFullWidth">
        /// True to use the full width of the CDU, false for tighter kerning that looks
        /// more accurate but leaves a gap between either side of the display.
        /// </param>
        /// <param name="skipDuplicateCheck">
        /// True to force the font to be uploaded even if it has previously been uploaded.
        /// For backwards compatability reasons this defaults to true, unlike most other
        /// "skip duplicate" checks.
        /// </param>
        void UseFont(McduFontFile fontFileContent, bool useFullWidth, bool skipDuplicateCheck = true);

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

        /// <summary>
        /// True if the device supports the key passed across.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsKeySupported(Key key);

        /// <summary>
        /// True if the device supports the LED passed across.
        /// </summary>
        /// <param name="led"></param>
        /// <returns></returns>
        bool IsLedSupported(Led led);
    }
}
