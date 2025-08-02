// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;

namespace Cduhub.Config
{
    /// <summary>
    /// Holds the common settings for all CDUHub applications.
    /// </summary>
    public class CduhubSettings : Settings
    {
        public override string GetName() => "cduhub-settings";

        public override int GetCurrentVersion() => 2;

        public class BrightnessSettings
        {
            public int ButtonSteps { get; set; } = 10;

            public int StartAtStep { get; set; } = 10;

            public bool PersistBetweenSessions { get; set; } = true;
        }

        public BrightnessSettings Brightness { get; set; } = new BrightnessSettings();

        public class BacklightSettings
        {
            public int BacklightPercent { get; set; } = 80;

            public int TurnOffWhenBrightnessExceedsPercent { get; set; } = 100;
        }

        public BacklightSettings Backlight { get; set; } = new BacklightSettings();

        public class OffsetSettings
        {
            public int XPixels { get; set; }

            public int YPixels { get; set; }
        }

        public OffsetSettings DisplayOffset { get; set; } = new OffsetSettings();

        public FontReference Font { get; set; } = new FontReference();

        public string PaletteName { get; set; } = BuiltInPaletteExtensions.DefaultPaletteReference;

        public class CleanupSettings
        {
            public int DisplayBrightnessPercentOnExit { get; set; } = 0;

            public int BacklightBrightnessPercentOnExit { get; set; } = 0;
        }

        public CleanupSettings Cleanup { get; set; } = new CleanupSettings();
    }
}
