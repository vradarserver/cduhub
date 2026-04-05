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

        protected IlluminationWriter? _LeftEfisIlluminationWriter;
        protected IlluminationWriter? _RightEfisIlluminationWriter;
        protected IlluminationWriter? _FcuIlluminationWriter;
        protected BinaryLampMap _LeftEfisBinaryLampMap = new(new BinaryLamp[] {
            new((int)FgcpLamp.Left_FD,      0x03),
            new((int)FgcpLamp.Left_LS,      0x04),
            new((int)FgcpLamp.Left_Cstr,    0x05),
            new((int)FgcpLamp.Left_Wpt,     0x06),
            new((int)FgcpLamp.Left_VorD,    0x07),
            new((int)FgcpLamp.Left_Ndb,     0x08),
            new((int)FgcpLamp.Left_Arpt,    0x09),
        });
        protected BinaryLampMap _RightEfisBinaryLampMap = new(new BinaryLamp[] {
            new((int)FgcpLamp.Right_FD,     0x03),
            new((int)FgcpLamp.Right_LS,     0x04),
            new((int)FgcpLamp.Right_Cstr,   0x05),
            new((int)FgcpLamp.Right_Wpt,    0x06),
            new((int)FgcpLamp.Right_VorD,   0x07),
            new((int)FgcpLamp.Right_Ndb,    0x08),
            new((int)FgcpLamp.Right_Arpt,   0x09),
        });
        protected BinaryLampMap _FcuBinaryLampMap = new(new BinaryLamp[] {
            new((int)FgcpLamp.Loc,          0x03),
            new((int)FgcpLamp.Ap1,          0x05),
            new((int)FgcpLamp.Ap2,          0x07),
            new((int)FgcpLamp.AThr,         0x09),
            new((int)FgcpLamp.Exped,        0x0b),
            new((int)FgcpLamp.Appr,         0x0d),
        });

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
    }
}
