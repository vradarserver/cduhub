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

namespace Cduhub
{
    public class Page
    {
        protected readonly Hub _Hub;
        protected bool _Prepared;

        public Screen Screen { get; }

        public Compositor Output { get; }

        public Leds Leds { get; }

        public virtual Key MenuKey { get; } = Key.McduMenu;

        public virtual bool DisableMenuKey { get; }

        public virtual Key ParentKey { get; } = Key.Blank2;

        public virtual bool DisableParentKey { get; }

        public Page(Hub hub)
        {
            _Hub = hub;
            Leds = new Leds();
            Screen = new Screen();
            Output = new Compositor(Screen);
        }

        public virtual void RefreshDisplay() => _Hub.RefreshDisplay(this);

        public virtual void RefreshLeds() => _Hub.RefreshLeds(this);

        public void PreparePage()
        {
            if(!_Prepared) {
                _Prepared = true;
                OnPreparePage();
            }
        }

        public virtual void OnPreparePage()
        {
        }

        public virtual void OnSelected(bool selected)
        {
        }

        public virtual void OnKeyDown(Key key)
        {
        }

        public virtual void OnKeyUp(Key key)
        {
        }
    }
}
