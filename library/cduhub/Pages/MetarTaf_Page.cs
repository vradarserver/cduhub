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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Cduhub.Config;
using WwDevicesDotNet;

namespace Cduhub.Pages
{
    class MetarTaf_Page : Page
    {
        private const string _Url = "https://aviationweather.gov/api/data/metar?ids={0}&format=raw&taf=true";

        private MetarSettings _Settings;
        private FormHelper _Form;
        private Timer _Timer = null;
        private DateTime _LastDownloadUtc;
        private DateTime _BackoffThreshold;
        private string _Metar;
        private string[] _Taf;
        private bool _OutputIsReport = false;
        private Colour _OutputColour = Colour.White;
        private IReadOnlyList<string> _OutputLines = Array.Empty<string>();
        private int _CurrentPageIndex = 0;

        public MetarTaf_Page(Hub hub) : base(hub)
        {
            Scratchpad = new Scratchpad();
            _Form = new FormHelper(DrawOptions);
        }

        public override void OnSelected(bool selected)
        {
            if(selected) {
                _Settings = ConfigStorage.Load<MetarSettings>();
                DrawOptions();
                DownloadReport();

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
                .LabelLine(6).ClearRow()
                .LeftLabel(6, "<red><small>>BACK")
                .RightLabel(6, $"<cyan>{MetarSettings.DescribeReports(_Settings.Download)}");
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

        public override void OnCommonKeyDown(CommonKey key)
        {
            switch(key) {
                case CommonKey.LineSelectLeft1:
                    CopyScratchpadToStationCode();
                    break;
                case CommonKey.LineSelectRight1:
                    CopyScratchpadToRefreshMinutes();
                    break;
                case CommonKey.LineSelectLeft6:
                    _Hub.ReturnToParent();
                    break;
                case CommonKey.LineSelectRight6:
                    _Form.CycleEnum(
                        _Settings.Download,
                        v => {
                            _Settings.Download = v;
                            DrawOptions();
                            if(_OutputIsReport) {
                                ShowReports();
                            }
                        },
                        formatValue: MetarSettings.DescribeReports
                    );
                    break;
                case CommonKey.LeftArrowOrPrevPage:
                    --_CurrentPageIndex;
                    ShowPage();
                    break;
                case CommonKey.RightArrowOrNextPage:
                    ++_CurrentPageIndex;
                    ShowPage();
                    break;
                default:
                    base.OnCommonKeyDown(key);
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
                DownloadReport();
            }
        }

        private void CopyScratchpadToRefreshMinutes()
        {
            if(!_Form.IntegerValue(Scratchpad.Text, v => _Settings.RefreshMinutes = v, min: 0, max: 60 * 24)) {
                Scratchpad.ShowFormatError();
            } else {
                Scratchpad.Clear();
            }
        }

        private void DownloadReport()
        {
            _LastDownloadUtc = DateTime.UtcNow;

            var stationCode = _Settings.StationCode;
            var reports = _Settings.Download;
            if(!String.IsNullOrEmpty(stationCode)) {
                Task.Run(async () => await DownloadMetarAsync(stationCode));
            }
        }

        private async Task DownloadMetarAsync(string stationCode)
        {
            try {
                var url = String.Format(_Url, stationCode);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var client = CommonHttpClient.HttpClient;

                request.Headers.UserAgent.ParseAdd($"CDUHub-{Assembly.GetExecutingAssembly().GetName().Version}");

                var response = await client.SendAsync(request);
                if(!response.IsSuccessStatusCode) {
                    const int backoffMinutes = 5;
                    _BackoffThreshold = DateTime.UtcNow.AddMinutes(backoffMinutes);
                    ShowBadResponse(url, response.StatusCode, backoffMinutes);
                } else {
                    var outcome = await response.Content.ReadAsStringAsync();
                    var lines = outcome
                        ?.Split('\n')
                        .Select(line => line.Trim())
                        .ToArray()
                        ?? Array.Empty<string>();
                    _Metar = lines.Length > 0 ? lines[0] : "";
                    _Taf = lines.Skip(1).ToArray();
                    if(_Taf.Length == 1 && String.IsNullOrEmpty(_Taf[0])) {
                        _Taf = Array.Empty<string>();
                    }
                    if(!String.IsNullOrEmpty(_Metar) && _Taf.Length == 0) {
                        _Taf = new string[] { "NONE AVAILABLE" };
                    }
                    ShowReports();
                }
            } catch(Exception ex) {
                ShowException(ex);
            }
        }

        private void ShowReports()
        {
            _OutputIsReport = true;

            var output = new StringBuilder();
            switch(_Settings.Download) {
                case MetarSettings.Reports.Metar:
                    output.Append(_Metar ?? "");
                    break;
                case MetarSettings.Reports.Taf:
                    output.Append(String.Join("\n", _Taf ?? Array.Empty<string>()));
                    break;
                case MetarSettings.Reports.MetarAndTaf:
                    output.Append(_Metar ?? "");
                    if(_Taf?.Length > 0) {
                        if(output.Length > 0) {
                            output.Append("\n >> ");
                            if(!_Taf[0].StartsWith("TAF ")) {
                                output.Append("TAF ");
                            }
                        }
                        output.Append(String.Join("\n", _Taf ?? Array.Empty<string>()));
                    }
                    break;
            }
            if(output.Length == 0) {
                output.Append("No reports downloaded");
            }
            _OutputLines = output
                .ToString()
                .WrapAtWhitespace(Metrics.Columns);
            _OutputColour = Colour.Green;
            _CurrentPageIndex = 0;
            ShowPage();
        }

        private void ShowBadResponse(string url, HttpStatusCode statusCode, int backoffMinutes)
        {
            _OutputIsReport = false;
            _OutputLines = $"URL {url} returned status {(int)statusCode}:{statusCode} - backing off for {backoffMinutes} minutes"
                .WrapAtWhitespace(Metrics.Columns);
            _OutputColour = Colour.Amber;
            _CurrentPageIndex = 0;
            ShowPage();
        }

        private void ShowException(Exception ex)
        {
            _OutputIsReport = false;
            _OutputLines = $"Exception caught during METAR download: {ex.Message}"
                .WrapAtWhitespace(Metrics.Columns);
            _OutputColour = Colour.Red;
            _CurrentPageIndex = 0;
            ShowPage();
        }

        private void ShowPage()
        {
            const int linesPerPage = 9;
            var lines = _OutputLines ?? Array.Empty<string>();
            var maxPages = lines.Count / linesPerPage;
            _CurrentPageIndex = Math.Max(0, Math.Min(_CurrentPageIndex, maxPages));
            var offset = _CurrentPageIndex * linesPerPage;

            Output
                .Small().White()
                .Line(0)
                .Write($"<grey>{_LastDownloadUtc.ToLocalTime():HH:mm}")
                .RightToLeft()
                .Write($"{_CurrentPageIndex + 1}/{maxPages + 1}")
                .LeftToRight()
                .Line(3)
                .Colour(_OutputColour)
                .Lines(lines, offset: offset, maxLines: linesPerPage, clearLines: true);
            RefreshDisplay();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.UtcNow;
            if(_BackoffThreshold != default) {
                if(now >= _BackoffThreshold) {
                    _BackoffThreshold = default;
                    DownloadReport();
                }
            } else if(_LastDownloadUtc != default) {
                var refreshMinutes = _Settings?.RefreshMinutes ?? 0;
                if(refreshMinutes > 0) {
                    var threshold = _LastDownloadUtc.AddMinutes(refreshMinutes);
                    if(now >= threshold) {
                        DownloadReport();
                    }
                }
            }
            OverlayTime();

            _Timer?.Start();
        }
    }
}
