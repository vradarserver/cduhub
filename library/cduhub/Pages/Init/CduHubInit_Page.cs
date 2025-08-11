// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Cduhub.Config;

namespace Cduhub.Pages.Init
{
    class CduHubInit_Page : CommonSettingsPage<CduhubSettings>
    {
        protected override string Title => "CDU HUB CONFIG";

        public CduHubInit_Page(Hub hub) : base(hub)
        {
            LeftOption("X-OFFSET",
                () => _Settings.DisplayOffset.XPixels.ToString(),
                () => _Form.IntegerFromScratchpad(Scratchpad,
                        v => _Settings.DisplayOffset.XPixels = v,
                        min: -150, max: 150
                      )
            );
            RightOption(
                "Y-OFFSET",
                () => _Settings.DisplayOffset.YPixels.ToString(),
                () => _Form.IntegerFromScratchpad(Scratchpad,
                        v => _Settings.DisplayOffset.YPixels = v,
                        min: -150, max: 150
                      )
            );
            LeftOption("FONT",
                () => _Settings.Font.FontName,
                () => _Form.CycleFontNames(
                        _Settings.Font.FontName,
                        v => _Settings.Font.FontName = v,
                        includeDefaultFontName: true
                    )
            );
            LeftOption("PALETTE",
                () => _Settings.PaletteName,
                () => _Form.CyclePaletteNames(
                        _Settings.PaletteName,
                        v => _Settings.PaletteName = v,
                        includeDefaultPaletteName: true
                    )
            );
        }

        protected override void ApplySettings() => _Hub.ReloadSettings();
    }
}
