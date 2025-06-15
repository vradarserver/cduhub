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
    class XPlaneMenu_Page : Page
    {
        private FlightSimMenu_Page _Parent;
        private XPlane_Page _XPlane_Page;

        public XPlaneMenu_Page(FlightSimMenu_Page parent, Hub hub) : base(hub)
        {
            _Parent = parent;
            Output
                .Green()
                .Centred("X-Plane 12 Menu")
                .White()
                .Newline()
                .Newline()
                .Centred("BLANK1 to swap MCDUs")
                .Newline()
                .Centred("BLANK2 for hub menu")
                .Amber()
                .LeftLabel(6, ">Cancel")
                .Cyan()
                .RightLabel(5, "Reconnect")
                .RightLabel(6, "X-Plane 12 MCDU<");
        }

        public override void OnKeyDown(Key key)
        {
            switch(key) {
                case Key.LineSelectLeft6:   _Hub.SelectPage(_Parent); break;
                case Key.LineSelectRight5:  Reconnect(); break;
                case Key.LineSelectRight6:  OpenXPlanePage(); break;
            }
        }

        private void Reconnect() => _XPlane_Page?.Reconnect();

        private void OpenXPlanePage()
        {
            if(_XPlane_Page == null) {
                _XPlane_Page = new XPlane_Page(_Hub);
            }
            _Hub.SelectPage(_XPlane_Page);
        }
    }
}
