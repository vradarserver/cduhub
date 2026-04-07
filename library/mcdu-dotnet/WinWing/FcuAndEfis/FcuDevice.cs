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
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace McduDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// The default implementation of <see cref="IFgcpFcu"/> and <see cref="IFgcp"/> for the
    /// WinWing FCU and optional two EFIS devices.
    /// </summary>
    class FcuDevice : IFgcpFcu
    {
        private const byte _PanelIntensityId =      0x00;
        private const byte _DisplayIntensityId =    0x01;
        private const byte _GreenLedIntensityId =   0x11;
        private const byte _ExpedIntensityId =      0x1E;

        protected HidDevice _HidDevice;
        protected HidStream? _HidStream;
        protected UsbWriter? _UsbWriter;
        protected FcuDisplayWriter? _DisplayWriter;
        protected FcuKeyboardReader? _KeyboardReader;

        protected IlluminationWriter? _LeftEfisIlluminationWriter;
        protected IlluminationWriter? _RightEfisIlluminationWriter;
        protected IlluminationWriter? _FcuIlluminationWriter;
        protected BinaryLampMap _LeftEfisBinaryLampMap = LampMaps.CreateLeftEfisMap();
        protected BinaryLampMap _RightEfisBinaryLampMap = LampMaps.CreateRightEfisMap();
        protected BinaryLampMap _FcuBinaryLampMap = LampMaps.CreateFcuMap();

        private CancellationTokenSource? _InputLoopCancellationTokenSource;
        private Task? _InputLoopTask;

        /// <inheritdoc/>
        public UsbDevice UsbDevice { get; }

        /// <inheritdoc/>
        public bool IsLeftEfisPresent => (UsbDevice.EquipmentType & EquipmentType.LeftEfis) != 0;

        /// <inheritdoc/>
        public bool IsRightEfisPresent => (UsbDevice.EquipmentType & EquipmentType.RightEfis) != 0;

        /// <inheritdoc/>
        public FcuBacklights Backlights { get; } = new();

        /// <inheritdoc/>
        public FcuDisplays Displays { get; } = new();

        /// <inheritdoc/>
        public FcuLamps Lamps { get; } = new();

        /// <inheritdoc/>
        public event EventHandler<FcuKeyEventArgs>? FcuKeyDown;

        /// <summary>
        /// Raises <see cref="FcuKeyDown"/>. Only creates the args if something is
        /// listening.
        /// </summary>
        /// <param name="createArgs"></param>
        protected virtual void OnFcuKeyDown(Func<FcuKeyEventArgs> createArgs)
        {
            if(FcuKeyDown != null) {
                var args = createArgs();
                FcuKeyDown?.Invoke(this, args);
            }
        }

        /// <inheritdoc/>
        public event EventHandler<FcuKeyEventArgs>? FcuKeyUp;

        /// <summary>
        /// Raises <see cref="FcuKeyUp"/>. Only creates the args if something is
        /// listening.
        /// </summary>
        /// <param name="createArgs"></param>
        protected virtual void OnFcuKeyUp(Func<FcuKeyEventArgs> createArgs)
        {
            if(FcuKeyUp != null) {
                var args = createArgs();
                FcuKeyUp?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidDevice"></param>
        /// <param name="usbDevice"></param>
        public FcuDevice(HidDevice hidDevice, UsbDevice usbDevice)
        {
            _HidDevice = hidDevice;
            UsbDevice = usbDevice;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _InputLoopCancellationTokenSource?.Cancel();
                _InputLoopTask?.Wait(5000);
                _InputLoopTask = null;
                _KeyboardReader = null;

                _UsbWriter = null;
                _DisplayWriter = null;
                _LeftEfisIlluminationWriter = null;
                _RightEfisIlluminationWriter = null;
                _FcuIlluminationWriter = null;

                var hidStream = _HidStream;
                _HidStream = null;
                try {
                    hidStream?.Dispose();
                } catch {
                    ;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() => UsbDevice.ToString();

        /// <summary>
        /// Initialises the USB device to a known state.
        /// </summary>
        public void Initialise()
        {
            var maxOutputReportLength = _HidDevice.GetMaxOutputReportLength();
            if(maxOutputReportLength < 64) {
                throw new McduException(
                    $"HID device {_HidDevice} reported an invalid max output report length of {maxOutputReportLength}"
                );
            }
            if(!_HidDevice.TryOpen(out _HidStream)) {
                throw new McduException($"Could not open a stream to {_HidDevice}");
            }
            _UsbWriter = new UsbWriter(_HidStream);

            _DisplayWriter = new FcuDisplayWriter(
                _UsbWriter,
                IsLeftEfisPresent,
                IsRightEfisPresent
            ) {
               // UpdatingDeviceCallback = args => OnDisplayChanging(args),
            };

            _KeyboardReader = new FcuKeyboardReader(
                _HidStream,
                KeyboardMap.InputReport01FlagAndOffset,
                ProcessKeyboardEvent
            );
            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => _KeyboardReader.RunInputLoop(
                _InputLoopCancellationTokenSource.Token
            ));

            _LeftEfisIlluminationWriter = new IlluminationWriter(
                _UsbWriter,
                0x0DBF
            );
            _RightEfisIlluminationWriter = new IlluminationWriter(
                _UsbWriter,
                0x0EBF
            );
            _FcuIlluminationWriter = new IlluminationWriter(
                _UsbWriter,
                0x10BB
            );

            Backlights.PanelPercent = 50;
            Backlights.DisplayPercent = 50;
            Backlights.LedPercent = 50;
            Backlights.ExpedPercent = 50;

            RefreshBacklights();
            RefreshDisplays();
            RefreshLamps();
        }

        /// <inheritdoc/>
        public void Cleanup()
        {
            Displays.ClearDisplays();
            Lamps.TurnAllOn(on: false);
            Backlights.PanelPercent = 0;
            Backlights.ExpedPercent = 0;
            Backlights.DisplayPercent = 50;
            Backlights.LedPercent = 50;

            RefreshBacklights();
            RefreshDisplays();
            RefreshLamps();
        }

        public void RefreshBacklights(bool skipDuplicateCheck = false)
        {
            SendIntensity(Backlights.LeftEfis, _LeftEfisIlluminationWriter, skipDuplicateCheck);
            SendIntensity(Backlights.Fcu, skipDuplicateCheck);
            SendIntensity(Backlights.RightEfis, _RightEfisIlluminationWriter, skipDuplicateCheck);
        }

        private void SendIntensity(FcuBacklightFcuSet backlights, bool skipDuplicateCheck)
        {
            if(_FcuIlluminationWriter != null) {
                SendIntensity(backlights, _FcuIlluminationWriter, skipDuplicateCheck);
                _FcuIlluminationWriter.SetIntensity(_ExpedIntensityId, backlights.ExpedPercent, skipDuplicateCheck);
            }
        }

        private void SendIntensity(
            FcuBacklightSet backlights,
            IlluminationWriter? writer,
            bool skipDuplicateCheck
        )
        {
            if(writer != null) {
                writer.SetIntensity(_PanelIntensityId, backlights.PanelPercent, skipDuplicateCheck);
                writer.SetIntensity(_DisplayIntensityId, backlights.DisplayPercent, skipDuplicateCheck);
                writer.SetIntensity(_GreenLedIntensityId, backlights.GreenLedPercent, skipDuplicateCheck);
            }
        }

        /// <inheritdoc/>
        public void RefreshDisplays(bool skipDuplicateCheck = false)
        {
            _DisplayWriter?.SendSegmentedDisplays(
                Displays,
                skipDuplicateCheck
            );
        }

        /// <inheritdoc/>
        public void RefreshLamps(bool skipDuplicateCheck = false)
        {
            CopyLampsToBinaryLampMap(_LeftEfisBinaryLampMap);
            CopyLampsToBinaryLampMap(_RightEfisBinaryLampMap);
            CopyLampsToBinaryLampMap(_FcuBinaryLampMap);

            _LeftEfisIlluminationWriter?.SetLamps(_LeftEfisBinaryLampMap, skipDuplicateCheck);
            _FcuIlluminationWriter?.SetLamps(_FcuBinaryLampMap, skipDuplicateCheck);
            _RightEfisIlluminationWriter?.SetLamps(_RightEfisBinaryLampMap, skipDuplicateCheck);
        }

        private void CopyLampsToBinaryLampMap(BinaryLampMap binaryLampMap)
        {
            for(var idx = 0;idx < binaryLampMap.BinaryLamps.Count;++idx) {
                var lamp = binaryLampMap.BinaryLamps[idx];
                var on = Lamps.GetLamp((FgcpLamp)lamp.ExternalId);
                if(on != null) {
                    binaryLampMap.SetLamp(idx, on.Value);
                }
            }
        }

        protected virtual void ProcessKeyboardEvent(FcuKey key, bool pressed)
        {
            if(pressed) {
                OnFcuKeyDown(() => new FcuKeyEventArgs(key, pressed));
            } else {
                OnFcuKeyUp(() => new FcuKeyEventArgs(key, pressed));
            }
        }
    }
}
