// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using McduDotNet;

namespace Cduhub.Pages
{
    class FenixMenu_Page : Page
    {
        public FenixMenu_Page(Hub hub) : base(hub)
        {
        }

        public override void OnPreparePage()
        {
            Output
                .Centred("<green>FENIX A32X")
                .Newline(2)
                .Centred("BLANK1 <small>TO SWAP <large>MCDUs")
                .Newline()
                .Centred("BLANK2 <small>FOR HUB <large>MENU")
                .RightLabel(3, "<cyan>MCDU<")
                .LeftLabel(5, "<amber><small>>INIT")
                .LeftLabel(6, "<red><small>>BACK");
        }

        public override void OnKeyDown(Key key)
        {
            switch(key) {
                case Key.LineSelectLeft5:   _Hub.CreateAndSelectPage<FenixInit_Page>(); break;
                case Key.LineSelectLeft6:   _Hub.ReturnToParent(); break;
                case Key.LineSelectRight3:  _Hub.CreateAndSelectPage<Fenix_Page>(); break;
            }
        }
    }
}
