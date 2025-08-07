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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;
using Cduhub.Config;
using McduDotNet;
using Newtonsoft.Json;

namespace Cduhub.Pages
{
    class Metar_Page : Page
    {
        private const string _MetarUrl = "https://aviationweather.gov/api/data/metar?ids={0}&format=json";

        private MetarSettings _Settings;
        private FormHelper _Form;
        private Timer _Timer = null;
        private DateTime _LastDownloadUtc;
        private DateTime _BackoffThreshold;

        public Metar_Page(Hub hub) : base(hub)
        {
            Scratchpad = new Scratchpad();
            _Form = new FormHelper(DrawOptions);
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                _Settings = ConfigStorage.Load<MetarSettings>();
                DrawOptions();
                DownloadMetar();

                _Timer = new Timer() {
                    Enabled = true,
                    AutoReset = false,
                    Interval = 1000,
                };
                _Timer.Elapsed += Timer_Elapsed;
            } else {
                ConfigStorage.Save(_Settings);
                var timer = _Timer;
                _Timer = null;
                try {
                    timer?.Dispose();
                } catch {
                    ;
                }
            }
        }

        private void DrawOptions()
        {
            OverlayTime(suppressRefresh: true);
            Output
                .Large().White()
                .LabelTitleLine(1)
                .ClearRow(2)
                .LeftLabelTitle(1, "<small> ICAO")
                .LeftLabel(1, $"<cyan>{SanitiseInput(_Settings.StationCode)}")
                .RightLabelTitle(1, "<small>REFRESH ")
                .RightLabel(1, $"<cyan>{(_Settings.RefreshMinutes == 0 ? "NEVER" : $"{_Settings.RefreshMinutes} <small>MINS")}")
                .LeftLabel(6, "<red><small>>BACK");
            CopyScratchpadIntoDisplay();
            RefreshDisplay();
        }

        private void OverlayTime(bool suppressRefresh = false)
        {
            var line = Screen.Line;
            var col = Screen.Column;
            Output
                .Line(0, resetColumn: false)
                .Small().White()
                .Centered($"{DateTime.Now:HH:mm}")
                .Line(line, resetColumn: false)
                .Column(col);
            if(!suppressRefresh) {
                RefreshDisplay();
            }
        }

        public override void OnKeyDown(Key key)
        {
            switch(key) {
                case Key.LineSelectLeft1:
                    CopyScratchpadToStationCode();
                    break;
                case Key.LineSelectRight1:
                    CopyScratchpadToRefreshMinutes();
                    break;
                case Key.LineSelectLeft6:
                    _Hub.ReturnToParent();
                    break;
                default:
                    base.OnKeyDown(key);
                    break;
            }
        }

        private void CopyScratchpadToStationCode()
        {
            var text = Scratchpad.Text.Trim().ToUpper();
            if(!Validate.IsValidForAirportIcao(text)) {
                Scratchpad.ShowFormatError();
            } else {
                _Settings.StationCode = text;
                Scratchpad.Clear();
                DrawOptions();
                DownloadMetar();
            }
        }

        private void CopyScratchpadToRefreshMinutes()
        {
            if(!_Form.IntegerValue(Scratchpad.Text, v => _Settings.RefreshMinutes = v, minValue: 0, maxValue: 60 * 24)) {
                Scratchpad.ShowFormatError();
            } else {
                Scratchpad.Clear();
            }
        }

        private void DownloadMetar()
        {
            _LastDownloadUtc = DateTime.UtcNow;

            var stationCode = _Settings.StationCode;
            if(!String.IsNullOrEmpty(stationCode)) {
                Task.Run(async () => await DownloadMetarAsync(stationCode));
            }
        }

        [DataContract]
        class MetarSubset
        {
            [DataMember(Name = "rawOb")]
            public string RawObservation { get; set; }
        }

        private async Task DownloadMetarAsync(string stationCode)
        {
            try {
                var url = String.Format(_MetarUrl, stationCode);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var client = _Hub.HttpClient;

                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
                request.Headers.UserAgent.ParseAdd($"CDUHub-{Assembly.GetExecutingAssembly().GetName().Version}");

                var response = await client.SendAsync(request);
                if(!response.IsSuccessStatusCode) {
                    const int backoffMinutes = 5;
                    _BackoffThreshold = DateTime.UtcNow.AddMinutes(backoffMinutes);
                    ShowBadResponse(url, response.StatusCode, backoffMinutes);
                } else {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var metars = JsonConvert.DeserializeObject<MetarSubset[]>(jsonText);
                    ShowMetar(metars);
                }
            } catch(Exception ex) {
                ShowException(ex);
            }
        }

        private void ShowMetar(MetarSubset[] metars)
        {
            var metar = metars.FirstOrDefault();
            ShowOutput(
                Colour.Green,
                metar?.RawObservation ?? "No METAR downloaded"
            );
        }

        private void ShowBadResponse(string url, HttpStatusCode statusCode, int backoffMinutes)
        {
            ShowOutput(
                Colour.Amber,
                $"URL {url} returned status {(int)statusCode}:{statusCode} - backing off for {backoffMinutes} minutes"
            );
        }

        private void ShowException(Exception ex)
        {
            ShowOutput(
                Colour.Red,
                $"Exception caught during METAR download: {ex.Message}"
            );
        }

        private void ShowOutput(Colour colour, string text)
        {
            Output
                .Small()
                .Line(0)
                .RightToLeft()
                .Write($"<grey>{DateTime.Now:HH:mm}")
                .LeftToRight()
                .Line(3)
                .Green()
                .WrapText(text, 9, clearLines: true);
            RefreshDisplay();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.UtcNow;
            if(_BackoffThreshold != default) {
                if(now >= _BackoffThreshold) {
                    _BackoffThreshold = default;
                    DownloadMetar();
                }
            } else if(_LastDownloadUtc != default) {
                var refreshMinutes = _Settings?.RefreshMinutes ?? 0;
                if(refreshMinutes > 0) {
                    var threshold = _LastDownloadUtc.AddMinutes(refreshMinutes);
                    if(now >= threshold) {
                        DownloadMetar();
                    }
                }
            }
            OverlayTime();

            _Timer?.Start();
        }
    }
}
