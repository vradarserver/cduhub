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
    class Root_Page : Page
    {
        private Clock_Page _ClockPage;
        private FlightSimMenu_Page _FlightSimsPage;

        public Root_Page(Hub hub) : base(hub)
        {
            _ClockPage = new Clock_Page(hub);
            _FlightSimsPage = new FlightSimMenu_Page(hub);
        }

        public override void OnPrepareScreen()
        {
            Output
                .Clear()
                .Centred("<green>CDU <small>HUB")
                .LeftLabel(1, ">CLOCK")
                .RightLabel(1, "FLIGHT SIMS<")
                .RightLabel(6, "<red>QUIT<");

            Leds.Mcdu = Leds.Menu = true;
        }

        public override void OnKeyDown(Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:   _Hub.SelectPage(_ClockPage); break;
                case Key.LineSelectRight1:  _Hub.SelectPage(_FlightSimsPage); break;
                case Key.LineSelectRight6:  _Hub.Shutdown(); break;
            }
        }
    }
}
