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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;
using Cduhub.CommandLine;
using Newtonsoft.Json;

namespace Cduhub
{
    public class GithubUpdateChecker : IDisposable
    {
        [DataContract]
        class GithubApiReleaseInfoSubset
        {
            [DataMember(Name = "html_url")]
            public string HtmlUrl { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "tag_name")]
            public string TagName { get; set; }
        }

        private Timer _Timer = new Timer();
        private HttpClient _HttpClient;

        public const string LatestReleaseUrl = "https://api.github.com/repos/VRadarServer/cduhub/releases/latest";

        public static readonly GithubUpdateChecker DefaultInstance;

        private UpdateInfo _UpdateInfo;
        public UpdateInfo UpdateInfo
        {
            get => _UpdateInfo;
            set {
                if(value?.RemoteVersion.ToString() != _UpdateInfo?.RemoteVersion.ToString()) {
                    _UpdateInfo = value;
                    OnUpdateInfoChanged();
                }
            }
        }

        public event EventHandler UpdateInfoChanged;

        protected virtual void OnUpdateInfoChanged()
        {
            UpdateInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        public GithubUpdateChecker()
        {
            _Timer = new Timer() {
                AutoReset = false,
                Enabled = false,
            };
            _Timer.Elapsed += Timer_Elapsed;
        }

        static GithubUpdateChecker()
        {
            DefaultInstance = new GithubUpdateChecker();
            Task.Run(() => DefaultInstance.StartCheckingAsync(CommonHttpClient.HttpClient));
        }

        ~GithubUpdateChecker() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                var timer = _Timer;
                _Timer = null;

                if(timer != null) {
                    try {
                        timer.Dispose();
                    } catch {
                        ;
                    }
                }
            }
        }

        public async Task StartCheckingAsync(HttpClient httpClient)
        {
            _HttpClient = httpClient;
            await CheckForUpdate();
        }

        private async Task CheckForUpdate()
        {
            var successfulDownload = false;

            var client = _HttpClient;
            try {
                var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseUrl);
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github+json")
                );
                request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                request.Headers.UserAgent.ParseAdd($"CDUHub-{Assembly.GetExecutingAssembly().GetName().Version}");

                var response = await client.SendAsync(request);
                if(response.IsSuccessStatusCode) {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var latestRelease = JsonConvert.DeserializeObject<GithubApiReleaseInfoSubset>(jsonText);
                    successfulDownload = true;

                    ProcessDownload(latestRelease);
                }
            } catch {
                // TODO: Add logging
                ;
            }

            var timer = _Timer;
            if(timer != null) {
                try {
                    timer.Interval = successfulDownload
                        ? 1000 * 60 * 60 * 24
                        : 1000 * 60 * 5;
                    timer.Start();
                } catch(ObjectDisposedException) {
                    ;
                }
            }
        }

        private void ProcessDownload(GithubApiReleaseInfoSubset githubRelease)
        {
            if(githubRelease != null) {
                if(!InformationalVersion.TryParse(githubRelease.TagName, out var version)) {
                    InformationalVersion.TryParse(githubRelease.Name, out version);
                }
                if(version != null) {
                    var updateInfo = new UpdateInfo(version, githubRelease.HtmlUrl);
                    UpdateInfo = updateInfo;
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() => CheckForUpdate());
        }
    }
}
