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
using System.Collections.Generic;
using System.Text;
using McduDotNet;

namespace Cduhub.FcuPages
{
    class Default_FcuPage : FcuPage
    {
        public enum ShowFour
        {
            Nothing,
            HoursMinutes,
            Seconds,
            DayMonth,
        }

        private ShowFour _ShowInLeftBaro = ShowFour.HoursMinutes;
        private ShowFour _ShowInRightBaro = ShowFour.DayMonth;
        private ShowFour _ShowInAltitude = ShowFour.HoursMinutes;
        private ShowFour _ShowInVerticalSpeed = ShowFour.Seconds;
        private System.Timers.Timer? _RefreshTimer;
        private long _LastTimeShown;

        public Default_FcuPage(FcuHub fcuHub) : base(fcuHub)
        {
        }

        public override void OnSelected(bool selected)
        {
            base.OnSelected(selected);
            if(selected) {
                PopulateDisplays();
                StartTimer();
            } else {
                StopTimer();
            }
        }

        public override void OnFcuKeyDown(FcuKey fcuKey)
        {
            base.OnFcuKeyDown(fcuKey);
            switch(fcuKey) {
                case FcuKey.LeftFd:
                    _ShowInLeftBaro = Advance(_ShowInLeftBaro); break;
                case FcuKey.RightFd:
                    _ShowInRightBaro = Advance(_ShowInRightBaro); break;
                case FcuKey.FcuExped:
                    _ShowInAltitude = Advance(_ShowInAltitude); break;
                case FcuKey.FcuAppr:
                    _ShowInVerticalSpeed = Advance(_ShowInVerticalSpeed); break;
            }
            PopulateDisplays();
        }

        private ShowFour Advance(ShowFour show)
        {
            switch(show) {
                case ShowFour.Nothing:      return ShowFour.HoursMinutes;
                case ShowFour.HoursMinutes: return ShowFour.Seconds;
                case ShowFour.Seconds:      return ShowFour.DayMonth;
                case ShowFour.DayMonth:     return ShowFour.Nothing;
                default:                    throw new NotImplementedException();
            }
        }

        private void StartTimer()
        {
            _RefreshTimer?.Dispose();
            _RefreshTimer = new() {
                AutoReset = false,
                Interval = 100,
            };
            _RefreshTimer.Elapsed += RefreshTimer_Elapsed;
            _RefreshTimer.Start();
        }

        private void StopTimer()
        {
            _RefreshTimer?.Dispose();
            _RefreshTimer = null;
        }

        private void PopulateDisplays()
        {
            var now = DateTime.Now;
            PopulateFourDigitDisplay(Displays.Altitude.AltitudeDigits, _ShowInAltitude, now);
            PopulateFourDigitDisplay(Displays.Altitude.VerticalSpeedDigits, _ShowInVerticalSpeed, now, prefix: " ");
            PopulateFourDigitDisplay(Displays.LeftBarometer.BaroDigits, _ShowInLeftBaro, now);
            PopulateFourDigitDisplay(Displays.RightBarometer.BaroDigits, _ShowInRightBaro, now);

            Lamps.Exped = _ShowInAltitude != ShowFour.Nothing;
            Lamps.Appr =  _ShowInVerticalSpeed != ShowFour.Nothing;
            Lamps.LeftEfis.FD = _ShowInLeftBaro != ShowFour.Nothing;
            Lamps.RightEfis.FD = _ShowInRightBaro != ShowFour.Nothing;

            RefreshDisplays();
            RefreshLamps();
        }

        private void PopulateFourDigitDisplay(
            S7DigitCollection digits,
            ShowFour show,
            DateTime now,
            string prefix = ""
        )
        {
            digits.ClearDisplay();
            var buffer = new StringBuilder(prefix);
            var digitsLength = digits.Count - buffer.Length;

            switch(show) {
                case ShowFour.Nothing:
                    break;
                case ShowFour.HoursMinutes:
                    if(digitsLength == 4) {
                        buffer.Append(now.ToString("HH.mm"));
                    } else {
                        buffer.Append(now.ToString("HH-mm"));
                    }
                    break;
                case ShowFour.Seconds:
                    buffer.Append(now.ToString(" ss"));
                    break;
                case ShowFour.DayMonth:
                    if(digitsLength == 4) {
                        buffer.Append(now.ToString("ddMM"));
                    } else {
                        buffer.Append(now.ToString("dd MM"));
                    }
                    break;
            }

            digits.SetFrom(buffer.ToString());
        }

        private long ResolveTime(DateTime time)
        {
            return time.Ticks / 10000000;
        }

        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var resolvedTime = ResolveTime(DateTime.Now);
            if(resolvedTime != _LastTimeShown) {
                _LastTimeShown = resolvedTime;
                PopulateDisplays();
            }
            _RefreshTimer?.Start();
        }
    }
}
