// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Cduhub.DesktopGui
{
    public partial class AboutWindow : Window
    {
        private const string _LicenseText =
@"BSD 3-Clause License

Copyright (c) 2025 onwards, Andrew Whewell

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS IS""
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.";

        private const string _CreditText =
@"The MCDU image used as the basis for the icon came from the EFB of FlyByWire's
A32nx project here:
https://github.com/flybywiresim/aircraft/tree/master/fbw-a32nx/src

HidSharp was used to manage the USB side of things:
https://github.com/IntergatedCircuits/HidSharp

GraphQL-Client was used to manage the WebSocket conversation with the Fenix EFB:
https://github.com/graphql-dotnet/graphql-client";

        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            _TextBox_ThisVersion.Text = CduhubVersions.LibraryVersion.ToString();

            var updateInfo = CduhubVersions.UpdateInfo;
            _TextBox_LatestVersion.Text = updateInfo?.RemoteVersion.ToString() ?? "";
            _TextBox_ReleaseUrl.Text = updateInfo?.ReleaseUrl ?? "";
            _Link_OpenReleaseUrl.IsEnabled = !String.IsNullOrEmpty(updateInfo?.ReleaseUrl);

            _Text_License.Text = _LicenseText;
            _Text_Credit.Text = _CreditText;
        }

        private void Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenReleaseUrl_Click(object? sender, RoutedEventArgs e)
        {
            Shell.Open(CduhubVersions.UpdateInfo?.ReleaseUrl);
        }
    }
}
