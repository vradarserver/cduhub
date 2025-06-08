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
using System.Text;

namespace McduDotNet
{
    /// <summary>
    /// Implements the LEDs for the <see cref="Mcdu"/> implementation of <see cref="IMcdu"/>.
    /// </summary>
    class Leds : ILeds
    {
        private Mcdu _Mcdu;
        private bool _Initialising;

        private double _Brightness = 0.75;
        /// <inheritdoc/>
        public double Brightness
        {
            get => _Brightness;
            set => SetFieldAndSend(value, ref _Brightness, 0x02, () => (byte)(0xff * Brightness));
        }

        private bool _Fail;
        /// <inheritdoc/>
        public bool Fail
        {
            get => _Fail;
            set => SetFieldAndSend(value, ref _Fail, 0x08, () => (byte)(Fail ? 1 : 0));
        }

        private bool _Fm;
        /// <inheritdoc/>
        public bool Fm
        {
            get => _Fm;
            set => SetFieldAndSend(value, ref _Fm, 0x09, () => (byte)(Fm ? 1 : 0));
        }

        private bool _McduLed;
        /// <inheritdoc/>
        public bool Mcdu
        {
            get => _McduLed;
            set => SetFieldAndSend(value, ref _McduLed, 0x0A, () => (byte)(Mcdu ? 1 : 0));
        }

        private bool _Menu;
        /// <inheritdoc/>
        public bool Menu
        {
            get => _Menu;
            set => SetFieldAndSend(value, ref _Menu, 0x0B, () => (byte)(Menu ? 1 : 0));
        }

        private bool _Fm1;
        /// <inheritdoc/>
        public bool Fm1
        {
            get => _Fm1;
            set => SetFieldAndSend(value, ref _Fm1, 0x0C, () => (byte)(Fm1 ? 1 : 0));
        }

        private bool _Ind;
        /// <inheritdoc/>
        public bool Ind
        {
            get => _Ind;
            set => SetFieldAndSend(value, ref _Ind, 0x0D, () => (byte)(Ind ? 1 : 0));
        }

        private bool _Rdy;
        /// <inheritdoc/>
        public bool Rdy
        {
            get => _Rdy;
            set => SetFieldAndSend(value, ref _Rdy, 0x0E, () => (byte)(Rdy ? 1 : 0));
        }

        private bool _Line;
        /// <inheritdoc/>
        public bool Line
        {
            get => _Line;
            set => SetFieldAndSend(value, ref _Line, 0x0F, () => (byte)(Line ? 1 : 0));
        }

        private bool _Fm2;
        /// <inheritdoc/>
        public bool Fm2
        {
            get => _Fm2;
            set => SetFieldAndSend(value, ref _Fm2, 0x10, () => (byte)(Fm2 ? 1 : 0));
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="mcdu"></param>
        public Leds(Mcdu mcdu)
        {
            _Mcdu = mcdu;
        }

        /// <inheritdoc/>
        public void TurnAllOn(bool on)
        {
            Fail =  on;
            Fm =    on;
            Fm1 =   on;
            Fm2 =   on;
            Ind =   on;
            Line =  on;
            Mcdu =  on;
            Menu =  on;
            Rdy =   on;
        }

        internal void Initialise()
        {
            _Initialising = true;
            try {
                Brightness = Brightness;
                TurnAllOn(false);
            } finally {
                _Initialising = false;
            }
        }

        private void SetFieldAndSend<T>(T value, ref T backingField, byte indicatorCode, Func<byte> calculateByteValue)
        {
            if(_Initialising || !EqualityComparer<T>.Default.Equals(value, backingField)) {
                backingField = value;
                _Mcdu.SendLedOrBrightnessPacket(indicatorCode, calculateByteValue());
            }
        }
    }
}
