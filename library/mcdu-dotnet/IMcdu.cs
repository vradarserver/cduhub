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
    /// Represents an instance of an MCDU.
    /// </summary>
    public interface IMcdu : IDisposable
    {
        /// <summary>
        /// Whether this is the captain MCDU, first officer MCDU or observer MCDU.
        /// </summary>
        ProductId ProductId { get; }

        /// <summary>
        /// The MCDU's display.
        /// </summary>
        Screen Screen { get; }

        /// <summary>
        /// The MCDU's LED lights.
        /// </summary>
        Leds Leds { get; }

        /// <summary>
        /// Gets and sets the offset of the display from the left edge.
        /// </summary>
        int XOffset { get; set; }

        /// <summary>
        /// Gets and sets the offset of the display from the top edge.
        /// </summary>
        int YOffset { get; set; }

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
        /// Sends a font to the device.
        /// </summary>
        /// <param name="fontFileContent"></param>
        void UseFont(McduFontFile fontFileContent);

        /// <summary>
        /// Resets the display and turns everything off.
        /// </summary>
        void Cleanup();
    }
}
