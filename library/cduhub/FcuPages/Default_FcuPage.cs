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
using System.Diagnostics;
using System.Globalization;
using System.Text;
using McduDotNet;

namespace Cduhub.FcuPages
{
    class Default_FcuPage : FcuPage
    {
        public enum Show
        {
            Nothing,
            HoursMinutes,
            Seconds,
            DayMonth,
        }

        private class Countdown
        {
            public int UnitsRemaining;
            public int SecondsPerUnit;
            public int SecondsRemaining;
            public bool SuppressTick;

            public void SecondHasPassed()
            {
                if(SuppressTick) {
                    SuppressTick = false;
                } else if(SecondsRemaining > 0) {
                    --SecondsRemaining;
                    UnitsRemaining = SecondsRemaining / SecondsPerUnit;
                    UnitsRemaining += SecondsRemaining > SecondsPerUnit
                        ? (SecondsRemaining % SecondsPerUnit != 0) ? 1 : 0
                        : 0;
                }
            }
        }

        private int _Tick;
        private Show _ShowInLeftBaro = Show.HoursMinutes;
        private Show _ShowInRightBaro = Show.DayMonth;
        private Show _ShowInAltitude = Show.HoursMinutes;
        private Show _ShowInVerticalSpeed = Show.Seconds;
        private System.Timers.Timer? _RefreshTimer;
        private long _LastTimeShown;
        private Countdown? _MinuteCountdown;
        private Countdown? _SecondCountdown;
        private long? _StopwatchSeconds;

        public Default_FcuPage(FcuHub fcuHub) : base(fcuHub)
        {
        }

        public override void OnSelected(bool selected)
        {
            base.OnSelected(selected);
            if(!selected) {
                StopRefreshTimer();
            } else {
                PopulateDisplays();
                StartRefreshTimer();
            }
        }

        public override void OnFcuKeyDown(FcuKey fcuKey)
        {
            base.OnFcuKeyDown(fcuKey);
            switch(fcuKey) {
                case FcuKey.LeftFd:     Advance(ref _ShowInLeftBaro); break;
                case FcuKey.RightFd:    Advance(ref _ShowInRightBaro); break;
                case FcuKey.FcuExped:   Advance(ref _ShowInAltitude); break;
                case FcuKey.FcuAppr:    Advance(ref _ShowInVerticalSpeed); break;
                case FcuKey.FcuSpdInc:  SetCountdownTarget(ref _MinuteCountdown, seconds: 60, increment: true); break;
                case FcuKey.FcuSpdDec:  SetCountdownTarget(ref _MinuteCountdown, seconds: 60, increment: false); break;
                case FcuKey.FcuSpdPush: _MinuteCountdown = null; break;
                case FcuKey.FcuHdgInc:  SetCountdownTarget(ref _SecondCountdown, seconds: 1, increment: true); break;
                case FcuKey.FcuHdgDec:  SetCountdownTarget(ref _SecondCountdown, seconds: 1, increment: false); break;
                case FcuKey.FcuHdgPush: _SecondCountdown = null; break;
                case FcuKey.FcuSpdMach: ToggleStopwatch(); break;
            }
            PopulateDisplays();
        }

        private void Advance(ref Show show)
        {
            switch(show) {
                case Show.Nothing:      show = Show.HoursMinutes; break;
                case Show.HoursMinutes: show = Show.Seconds; break;
                case Show.Seconds:      show = Show.DayMonth; break;
                case Show.DayMonth:     show = Show.Nothing; break;
                default:                throw new NotImplementedException();
            }
        }

        private void SetCountdownTarget(ref Countdown? countdown, int seconds, bool increment)
        {
            if(increment) {
                if(countdown != null) {
                    countdown.SuppressTick = true;
                    countdown.UnitsRemaining = Math.Min(999, countdown.UnitsRemaining + 1);
                    countdown.SecondsRemaining = countdown.UnitsRemaining * seconds;
                } else {
                    countdown = new Countdown() {
                        SuppressTick = true,
                        SecondsPerUnit = seconds,
                        SecondsRemaining = 1 * seconds,
                        UnitsRemaining = 1
                    };
                }
            } else {
                if(countdown != null) {
                    if(countdown.UnitsRemaining == 0) {
                        countdown = null;
                    } else {
                        countdown.SuppressTick = true;
                        --countdown.UnitsRemaining;
                        countdown.SecondsRemaining = countdown.UnitsRemaining * seconds;
                    }
                }
            }
        }

        private void ToggleStopwatch()
        {
            _StopwatchSeconds = _StopwatchSeconds != null
                ? null
                : 0;
        }

        private void StartRefreshTimer()
        {
            _RefreshTimer?.Dispose();
            _RefreshTimer = new() {
                AutoReset = false,
                Interval = 100,
            };
            _RefreshTimer.Elapsed += RefreshTimer_Elapsed;
            _RefreshTimer.Start();
        }

        private void StopRefreshTimer()
        {
            _RefreshTimer?.Dispose();
            _RefreshTimer = null;
        }

        private void PopulateDisplays()
        {
            var nowOffset = DateTimeOffset.Now;
            var now = nowOffset.LocalDateTime;

            PopulateDisplay(Displays.Altitude.AltitudeDigits, _ShowInAltitude, now);
            PopulateDisplay(Displays.Altitude.VerticalSpeedDigits, _ShowInVerticalSpeed, now, prefix: " ");
            PopulateDisplay(Displays.LeftBarometer.BaroDigits, _ShowInLeftBaro, now);
            PopulateDisplay(Displays.RightBarometer.BaroDigits, _ShowInRightBaro, now);

            if(_StopwatchSeconds == null) {
                PopulateCountdownDisplay(Displays.Speed.SpeedDigits, _MinuteCountdown);
                PopulateCountdownDisplay(Displays.Heading.HeadingDigits, _SecondCountdown);
            } else {
                PopulateStopwatch(
                    Displays.Speed.SpeedDigits,
                    Displays.Heading.HeadingDigits
                );
            }

            Lamps.Exped = _ShowInAltitude != Show.Nothing;
            Lamps.Appr =  _ShowInVerticalSpeed != Show.Nothing;
            Lamps.LeftEfis.FD = _ShowInLeftBaro != Show.Nothing;
            Lamps.RightEfis.FD = _ShowInRightBaro != Show.Nothing;

            RefreshDisplays();
            RefreshLamps();
        }

        private void PopulateDisplay(
            S7DigitCollection digits,
            Show show,
            DateTime now,
            string prefix = ""
        )
        {
            digits.ClearDisplay();
            var buffer = new StringBuilder(prefix);
            var digitsLength = digits.Count - buffer.Length;

            switch(show) {
                case Show.Nothing:
                    break;
                case Show.HoursMinutes:
                    if(digitsLength == 4) {
                        buffer.Append(now.ToString("HH.mm"));
                    } else {
                        buffer.Append(now.ToString("HH-mm"));
                    }
                    break;
                case Show.Seconds:
                    buffer.Append(S7TextAlign.Right.PadText(
                        now.ToString("ss"),
                        digitsLength,
                        ignoreDecimals: true)
                    );
                    break;
                case Show.DayMonth:
                    var shortDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    var dayThenMonth = shortDateFormat.IndexOf('d') < shortDateFormat.IndexOf('M');
                    if(digitsLength == 4) {
                        buffer.Append(now.ToString(dayThenMonth ? "ddMM" : "MMdd"));
                    } else {
                        buffer.Append(now.ToString(dayThenMonth ? "dd MM" : "MM dd"));
                    }
                    break;
            }

            digits.SetFrom(buffer.ToString());
        }

        private void PopulateCountdownDisplay(S7DigitCollection digits, Countdown? countdown)
        {
            if(countdown == null) {
                digits.ClearDisplay();
            } else {
                var remainingUnits = countdown.UnitsRemaining;
                if(remainingUnits == 0) {
                    remainingUnits = countdown.SecondsRemaining;
                }

                if(remainingUnits > 0) {
                    digits.SetFrom(
                        remainingUnits.ToString("0", CultureInfo.InvariantCulture),
                        S7TextAlign.Right
                    );
                } else {
                    S7 fillSegments = 0;
                    switch(_Tick % 3) {
                        case 0:     fillSegments = S7CharacterSet.GenericDigits['0']; break;
                        case 1:     fillSegments = S7CharacterSet.Punctuation['-']; break;
                        case 2:     fillSegments = (S7)0xFFFF; break;
                    }
                    digits.FillDisplay(fillSegments);
                }
            }
        }

        private void PopulateStopwatch(S7DigitCollection minuteDigits, S7DigitCollection secondDigits)
        {
            minuteDigits.ClearDisplay();
            secondDigits.ClearDisplay();

            var seconds = _StopwatchSeconds;
            if(seconds != null) {
                var minutes = seconds / 60;
                seconds -= minutes * 60;
                if(minutes > 999) {
                    minutes = 999;
                    seconds = 999;
                }

                minuteDigits.SetFrom(minutes.Value.ToString("0"), S7TextAlign.Right);
                secondDigits.SetFrom(seconds.Value.ToString("00"), S7TextAlign.Right);
            }
        }

        private long ResolveTime(DateTime time)
        {
            return time.Ticks / 10000000;
        }

        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var resolvedTime = ResolveTime(DateTime.Now);
            if(resolvedTime != _LastTimeShown) {
                ++_Tick;
                _LastTimeShown = resolvedTime;
                _MinuteCountdown?.SecondHasPassed();
                _SecondCountdown?.SecondHasPassed();
                if(_StopwatchSeconds != null) {
                    ++_StopwatchSeconds;
                }
                PopulateDisplays();
            }
            _RefreshTimer?.Start();
        }
    }
}
