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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cduhub.FlightSim.XPlaneRestModels;
using McduDotNet;
using Newtonsoft.Json;

namespace Cduhub.FlightSim
{
    /// <summary>
    /// Another day, another attempt at getting sensible communication with XPlane. This would not be as sensible
    /// as WebSockets, but ClientWebSocket or X-Plane aborts after 100 seconds (no problems with SimBridge, so I
    /// suspect something about X-Plane's WebSocket server triggers a 100 second abort issue in .NET/Windows)...
    /// but it is a lot more sensible that subscribing to 3200+ datarefs over UDP.
    /// </summary>
    public abstract class XPlaneRestMcdus : SimulatedMcdus, IDisposable
    {
        private CancellationTokenSource _DownloadBaseDataCancellationTokenSource;
        private Task _DownloadBaseDataTask;
        protected Dictionary<string, DatarefInfoModel> _DatarefsByName = null;
        protected Dictionary<long, DatarefInfoModel> _DatarefsById = null;
        protected Dictionary<string, CommandInfoModel> _CommandsByName = null;
        protected Dictionary<long, CommandInfoModel> _CommandsById = null;
        protected System.Timers.Timer _RefreshDisplayTimer = new System.Timers.Timer(100) {
            AutoReset = false,
            Enabled = false,
        };

        public HttpClient HttpClient { get; }

        public override string FlightSimulatorName => FlightSimulatorNames.XPlane12;

        public override SimulatorMcduBuffer PilotBuffer { get; } = new SimulatorMcduBuffer();

        public override SimulatorMcduBuffer FirstOfficerBuffer { get; } = new SimulatorMcduBuffer();

        /// <summary>
        /// Gets or sets the address of the machine running X-Plane.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port that X-Plane's WebSocket server is listening to.
        /// </summary>
        public int Port { get; set; } = 8086;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="masterScreen"></param>
        /// <param name="masterLeds"></param>
        public XPlaneRestMcdus(HttpClient httpClient, Screen masterScreen, Leds masterLeds) : base(masterScreen, masterLeds)
        {
            HttpClient = httpClient;
            _RefreshDisplayTimer.Elapsed += Timer_Elapsed;
            _RefreshDisplayTimer.Start();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~XPlaneRestMcdus() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                try {
                    var timer = _RefreshDisplayTimer;
                    _RefreshDisplayTimer = null;
                    if(timer != null) {
                        timer.Stop();
                        timer.Dispose();
                    }
                } catch {
                }
            }
        }

        protected override void OnDisplayRefreshRequired()
        {
            base.OnDisplayRefreshRequired();
        }

        public override void ReconnectToSimulator()
        {
            InitialiseConnection();
        }

        private void InitialiseConnection()
        {
            StopDownloadingBaseData();
            _DownloadBaseDataCancellationTokenSource = new CancellationTokenSource();
            _DownloadBaseDataTask = Task.Run(() => DownloadBaseData(_DownloadBaseDataCancellationTokenSource.Token));
        }

        private void StopDownloadingBaseData()
        {
            var cts = _DownloadBaseDataCancellationTokenSource;
            var downloadTask = _DownloadBaseDataTask;

            RecordConnectionState(ConnectionState.Disconnecting);

            _DownloadBaseDataCancellationTokenSource = null;
            _DownloadBaseDataTask = null;
            _DatarefsById = null;
            _DatarefsByName = null;
            _CommandsById = null;
            _CommandsByName = null;

            if(downloadTask != null) {
                try {
                    cts.Cancel();
                } catch {}
                try {
                    Task.WaitAll(new Task[] { downloadTask }, 5000);
                } catch {}
                try {
                    cts.Dispose();
                } catch {}
            }

            RecordConnectionState(ConnectionState.Disconnected);
        }

        private async Task DownloadBaseData(CancellationToken cancellationToken)
        {
            var httpClient = HttpClient;
            var downloaded = false;
            while(!cancellationToken.IsCancellationRequested && !downloaded) {
                try {
                    RecordConnectionState(ConnectionState.Connecting);
                    await FetchKnownDatarefs(httpClient, cancellationToken);
                    await FetchKnownCommands(httpClient, cancellationToken);
                    downloaded = true;
                    RecordConnectionState(ConnectionState.Connected);
                } catch(HttpRequestException) {
                    RecordConnectionState(ConnectionState.Disconnected);
                    _DatarefsById = null;
                    _DatarefsByName = null;
                    _CommandsById = null;
                    _CommandsByName = null;
                    await Task.Delay(5000);
                }
            }
        }

        protected virtual async Task FetchKnownDatarefs(HttpClient client, CancellationToken cancellationToken)
        {
            var datarefsByName = new Dictionary<string, DatarefInfoModel>();
            var datarefsById = new Dictionary<long, DatarefInfoModel>();

            var datarefs = await GetJson<KnownDatarefsModel>(client, "datarefs", cancellationToken);
            foreach(var dataref in datarefs.Data) {
                if(!datarefsByName.ContainsKey(dataref.Name)) {
                    datarefsByName.Add(dataref.Name, dataref);
                }
                if(!datarefsById.ContainsKey(dataref.Id)) {
                    datarefsById.Add(dataref.Id, dataref);
                }
            }
            _DatarefsByName = datarefsByName;
            _DatarefsById = datarefsById;
        }

        protected virtual async Task FetchKnownCommands(HttpClient client, CancellationToken cancellationToken)
        {
            var commandsByName = new Dictionary<string, CommandInfoModel>();
            var commandsById = new Dictionary<long, CommandInfoModel>();

            var commands = await GetJson<KnownCommandsModel>(client, "Commands", cancellationToken);
            foreach(var command in commands.Data) {
                if(!commandsByName.ContainsKey(command.Name)) {
                    commandsByName.Add(command.Name, command);
                }
                if(!commandsById.ContainsKey(command.Id)) {
                    commandsById.Add(command.Id, command);
                }
            }
            _CommandsByName = commandsByName;
            _CommandsById = commandsById;
        }

        protected Uri BuildUri(string path) => new Uri($"http://{Host}:{Port}/api/v2/{path}");

        protected async Task<string> GetString(HttpClient client, string path, CancellationToken cancellationToken)
        {
            var uri = BuildUri(path);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request, cancellationToken);
            var result = response.IsSuccessStatusCode && !cancellationToken.IsCancellationRequested
                ? await response.Content.ReadAsStringAsync()
                : null;

            return result;
        }

        protected async Task<T> GetJson<T>(HttpClient client, string path, CancellationToken cancellationToken)
        {
            var json = await GetString(client, path, cancellationToken);
            return !String.IsNullOrEmpty(json) && !cancellationToken.IsCancellationRequested
                ? JsonConvert.DeserializeObject<T>(json)
                : default;
        }

        protected async Task<string> GetDataRefValue(string dataRefName, CancellationToken cancellationToken)
        {
            string result = null;

            var datarefs = _DatarefsByName;
            if(datarefs?.TryGetValue(dataRefName, out var info) ?? false) {
                var data = await GetJson<DatarefValueModel>(HttpClient, $"datarefs/{info.Id}/value", cancellationToken);
                result = data?.Data;
            }

            return result;
        }

        protected string GetDataRef(string dataRefName)
        {
            var result = Task.Run(() => GetDataRefValue(dataRefName, CancellationToken.None)).Result;
            RecordMessageReceivedFromSimulator();
            return result;
        }

        protected async Task PostJson<T>(HttpClient client, string path, T content, CancellationToken cancellationToken)
        {
            var uri = BuildUri(path);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonConvert.SerializeObject(content);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        protected async Task PostActivateCommand(string commandName, float duration, CancellationToken cancellationToken)
        {
            var commands = _CommandsByName;
            if(commands?.TryGetValue(commandName, out var info) ?? false) {
                var command = new ActivateCommandModel() {
                    Duration = duration,
                };
                await PostJson(HttpClient, $"command/{info.Id}/activate", command, cancellationToken);
            }
        }

        protected async Task ActivateCommandOrReconnect(string commandName, float duration = 0F)
        {
            try {
                if(IsConnected) {
                    await PostActivateCommand(commandName, duration, CancellationToken.None);
                }
            } catch {
                InitialiseConnection();
            }
        }

        protected abstract void DownloadMcduContent();

        protected virtual void Timer_Elapsed(object sender, EventArgs args)
        {
            try {
                if(IsConnected) {
                    DownloadMcduContent();
                }
            } catch {
                InitialiseConnection();
            }
            _RefreshDisplayTimer?.Start();
        }
    }
}
