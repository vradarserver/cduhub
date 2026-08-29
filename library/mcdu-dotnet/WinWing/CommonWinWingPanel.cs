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
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace McduDotNet.WinWing
{
    /// <summary>
    /// Code that all WinWing panels have in common.
    /// </summary>
    abstract class CommonWinWingPanel : IDisposable, ICdu
    {
        protected const byte _KeyboardBacklightId = 0x00;
        protected const byte _DisplayBacklightId = 0x01;
        protected const byte _LedBrightnessId = 0x02;

        protected abstract byte CommandPrefix { get; }
        // One of the differences between panels seems to be that the first byte of the
        // XXBB commands differs between each device. That sequence is 4 characters in
        // string commands, so a two character name that holds the correct XXBB sequence
        // for the device will be 4 characters long when surrounded by the {} inline
        // string substitution characters, and not make the packets look squiggly when
        // written as hex strings. Hence why the name is particularly terse.
        protected string CP { get; }

        protected abstract BinaryLampMap BinaryLampMap { get; }

        protected abstract Func<CduKey, (int Flag, int Offset)> KeyToFlagOffsetCallback { get; }

        protected readonly Screen _EmptyScreen = new();
        protected HidDevice _HidDevice;
        protected string _HidDevicePath;

        protected HidStream? _HidStream;
        protected UsbWriter? _UsbWriter;
        protected ScreenWriter? _ScreenWriter;
        protected IlluminationWriter? _IlluminationWriter;
        private FontWriter? _FontWriter;
        private PaletteWriter? _PaletteWriter;
        private CduKeyboardReader? _KeyboardReader;
        private CancellationTokenSource? _InputLoopCancellationTokenSource;
        private Task? _InputLoopTask;

        public UsbDevice UsbDevice { get; }

        /// <inheritdoc/>
        public Screen Screen { get; }

        /// <inheritdoc/>
        public CduLamps Lamps { get; }

        private CduLamp[]? _SupportedLamps;
        /// <inheritdoc/>
        public IReadOnlyList<CduLamp> SupportedLamps
        {
            get {
                var result = _SupportedLamps;
                if(result == null) {
                    result = BinaryLampMap
                        .BinaryLamps
                        .Select(r => (CduLamp)r.ExternalId)
                        .ToArray();
                    _SupportedLamps = result;
                }
                return result;
            }
        }

        /// <inheritdoc/>
        public CduBacklights Backlights { get; }

        /// <inheritdoc/>
        public IReadOnlyList<CduKey> SupportedCduKeys { get; }

        /// <inheritdoc/>
        public Palette Palette { get; }

        /// <inheritdoc/>
        public int XOffset { get; set; }

        /// <inheritdoc/>
        public int YOffset { get; set; }

        /// <inheritdoc/>
        public Compositor Output { get; }

        /// <inheritdoc/>
        public AutoBrightnessSettings AutoBrightness { get; } = new();

        /// <inheritdoc/>
        public bool HasAmbientLightSensor => true;

        /// <inheritdoc/>
        public int LeftAmbientLightNative { get; private set; }

        /// <inheritdoc/>
        public int RightAmbientLightNative { get; private set; }

        /// <inheritdoc/>
        public int AmbientLightPercent { get; private set; }

        /// <inheritdoc/>
        public event EventHandler? LeftAmbientLightChanged;

        protected virtual void OnLeftAmbientLightChanged() => LeftAmbientLightChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler? RightAmbientLightChanged;

        protected virtual void OnRightAmbientLightChanged() => RightAmbientLightChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event EventHandler? AmbientLightChanged;

        protected virtual void OnAmbientLightChanged() => AmbientLightChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler<CduKeyEventArgs>? CduKeyDown;

        /// <summary>
        /// Raises <see cref="CduKeyDown"/>. Doesn't bother creating args unless something
        /// is listening.
        /// </summary>
        /// <param name="createArgs"></param>
        protected virtual void OnCduKeyDown(Func<CduKeyEventArgs> createArgs)
        {
            if(CduKeyDown != null) {
                CduKeyDown?.Invoke(this, createArgs());
            }
        }

        /// <inheritdoc/>
        public event EventHandler<CduKeyEventArgs>? CduKeyUp;

        /// <summary>
        /// Raises <see cref="CduKeyUp"/>. Doesn't bother creating args unless something
        /// is listening.
        /// </summary>
        /// <param name="createArgs"></param>
        protected virtual void OnCduKeyUp(Func<CduKeyEventArgs> createArgs)
        {
            if(CduKeyUp != null) {
                CduKeyUp?.Invoke(this, createArgs());
            }
        }

        /// <inheritdoc/>
        public event EventHandler<DisplayChangingEventArgs>? DisplayChanging;

        /// <summary>
        /// Raises <see cref="DisplayChanging"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDisplayChanging(DisplayChangingEventArgs args) => DisplayChanging?.Invoke(this, args);

        /// <inheritdoc/>
        public event EventHandler<PaletteChangingEventArgs>? PaletteChanging;

        /// <summary>
        /// Raises <see cref="PaletteChanging"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPaletteChanging(PaletteChangingEventArgs args) => PaletteChanging?.Invoke(this, args);

        /// <inheritdoc/>
        public event EventHandler<FontChangingEventArgs>? FontChanging;

        /// <summary>
        /// Raises <see cref="FontChanging"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFontChanging(FontChangingEventArgs args) => FontChanging?.Invoke(this, args);

        public event EventHandler? Disconnected;

        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        public CommonWinWingPanel(HidDevice hidDevice, UsbDevice usbDevice)
        {
            CP = $"{CommandPrefix:x2}bb";
            _HidDevice = hidDevice;
            _HidDevicePath = hidDevice.DevicePath;
            UsbDevice = usbDevice;
            Lamps = new();
            Screen = new();
            Output = new(Screen);
            Backlights = new();
            Palette = new();
            HidSharpDeviceWatcher.RegisterDisconnectedCallback(
                _HidDevicePath,
                OnDisconnected
            );

            SupportedCduKeys = Enum.GetValues(typeof(CduKey))
                .OfType<CduKey>()
                .Where(key => IsKeySupported(key))
                .ToArray();

#pragma warning disable CS0618 // Type or member is obsolete
            _DeviceId = new(usbDevice);
            _Leds = new(Lamps);
            SupportedKeys = Enum.GetValues(typeof(Key))
                .OfType<Key>()
                .Where(key => IsKeySupported(key))
                .ToArray();
#pragma warning restore CS0618 // Type or member is obsolete
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
                HidSharpDeviceWatcher.DeregisterDisconnectedCallbacks(_HidDevicePath);

                _InputLoopCancellationTokenSource?.Cancel();
                _InputLoopTask?.Wait(5000);
                _InputLoopTask = null;
                _KeyboardReader = null;

                _UsbWriter = null;
                _ScreenWriter = null;
                _IlluminationWriter = null;
                _FontWriter = null;
                _PaletteWriter = null;

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

            _KeyboardReader = new CduKeyboardReader(
                _HidStream,
                KeyToFlagOffsetCallback,
                ProcessKeyboardEvent,
                ProcessAmbientLightChange
            );

            _ScreenWriter = new ScreenWriter(_UsbWriter) {
                UpdatingDeviceCallback = args => OnDisplayChanging(args),
            };
            _IlluminationWriter = new IlluminationWriter(
                _UsbWriter,
                (ushort)((CommandPrefix << 8) | 0xbb)
            );
            _FontWriter = new FontWriter(_UsbWriter) {
                UpdatingDeviceCallback = args => OnFontChanging(args),
            };
            _PaletteWriter = new PaletteWriter(_UsbWriter, CP) {
                UpdatingDeviceCallback = args => OnPaletteChanging(args),
            };

            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => _KeyboardReader.RunInputLoop(
                _InputLoopCancellationTokenSource.Token
            ));

            PanelSpecificInitialisation();

            InitialiseBasicFontsAndColours();
            RefreshLamps();
            RefreshBacklights();
        }

        protected virtual void PanelSpecificInitialisation()
        {
        }

        protected void InitialiseBasicFontsAndColours()
        {
            _UsbWriter?.LockForOutput(() => {
                var packets = new string[] {
                    $"f0000138{CP}00001e0100005f6331000000000000{CP}0000180100005f6331000008000000340018000e001800{CP}0000190100005f633100000e00000000",
                    $"f00002380000000100050000000200000000000000{CP}0000190100005f633100000e0000000100060000000300000000000000{CP}00001901000000000000",
                    $"f00003385f633100000e0000000200000000ff0400000000000000{CP}0000190100005f633100000e000000020000a5ffff0500000000000000{CP}00000000",
                    $"f00004380000190100005f633100000e0000000200ffffffff0600000000000000{CP}0000190100005f633100000e0000000200ffff00ff0700000000000000",
                    $"f000053800000000{CP}0000190100005f633100000e00000002003dff00ff0800000000000000{CP}0000190100005f633100000e0000000200ff6300000000",
                    $"f0000638ffff0900000000000000{CP}0000190100005f633100000e00000002000000ffff0a00000000000000{CP}0000190100005f633100000e0000000000",
                    $"f00007380000020000ffffff0b00000000000000{CP}0000190100005f633100000e0000000200425c61ff0c00000000000000{CP}0000190100005f00000000",
                    $"f0000838633100000e0000000200777777ff0d00000000000000{CP}0000190100005f633100000e00000002005e7379ff0e00000000000000{CP}0000000000",
                    $"f000093800190100005f633100000e0000000300000000ff0f00000000000000{CP}0000190100005f633100000e000000030000a5ffff100000000000000000",
                    $"f0000a38000000{CP}0000190100005f633100000e0000000300ffffffff1100000000000000{CP}0000190100005f633100000e0000000300ffff0000000000",
                    $"f0000b38ff1200000000000000{CP}0000190100005f633100000e00000003003dff00ff1300000000000000{CP}0000190100005f633100000e000000000000",
                    $"f0000c38000300ff63ffff1400000000000000{CP}0000190100005f633100000e00000003000000ffff1500000000000000{CP}0000190100005f6300000000",
                    $"f0000d383100000e000000030000ffffff1600000000000000{CP}0000190100005f633100000e0000000300425c61ff1700000000000000{CP}000000000000",
                    $"f0000e38190100005f633100000e0000000300777777ff1800000000000000{CP}0000190100005f633100000e00000003005e7379ff19000000000000000000",
                    $"f0000f380000{CP}0000190100005f633100000e0000000400000000001a00000000000000{CP}0000190100005f633100000e00000004000100000000000000",
                    $"f00010381b00000000000000{CP}0000190100005f633100000e0000000400020000001c00000000000000{CP}00001a0100005f633100000100000000000000",
                    $"f000111202{CP}00001c0100005f6331000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                };
                foreach(var packet in packets) {
                    _UsbWriter.SendStringPacket(packet);
                }
            });
        }

        protected virtual void ProcessKeyboardEvent(CduKey key, bool pressed)
        {
            if(pressed) {
                OnCduKeyDown(() => new CduKeyEventArgs(key, pressed));
                #pragma warning disable CS0618 // Type or member is obsolete
                OnKeyDown(() => new KeyEventArgs((Key)key, pressed));
                #pragma warning restore CS0618 // Type or member is obsolete
            } else {
                OnCduKeyUp(() => new CduKeyEventArgs(key, pressed));
                #pragma warning disable CS0618 // Type or member is obsolete
                OnKeyUp(() => new KeyEventArgs((Key)key, pressed));
                #pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        protected virtual void ProcessAmbientLightChange(UInt16 leftSensor, UInt16 rightSensor)
        {
            var left = LeftAmbientLightNative;
            var right = RightAmbientLightNative;
            var avg = AmbientLightPercent;

            LeftAmbientLightNative = leftSensor;
            RightAmbientLightNative = rightSensor;
            var mul = ((double)LeftAmbientLightNative + (double)RightAmbientLightNative) / 2.0;
            mul /= 0xfff;
            AmbientLightPercent = (int)(100.0 * mul);

            ApplyAutoBrightness();

            if(left != LeftAmbientLightNative) {
                OnLeftAmbientLightChanged();
            }
            if(right != RightAmbientLightNative) {
                OnRightAmbientLightChanged();
            }
            if(avg != AmbientLightPercent) {
                OnAmbientLightChanged();
            }
        }

        /// <inheritdoc/>
        public void RefreshDisplay(bool skipDuplicateCheck = false)
        {
            _ScreenWriter?.SendScreenToDisplay(
                Screen,
                skipDuplicateCheck,
                suppressUpdatingDeviceCallback: DisplayChanging == null
            );
        }

        /// <inheritdoc/>
        public void ApplyAutoBrightness()
        {
            if(AutoBrightness.Enabled) {
                Backlights.KeyboardPercent = AutoBrightness
                    .KeyboardBacklight
                    .BrightnessForAmbientPercent(AmbientLightPercent);
                Backlights.DisplayPercent = AutoBrightness
                    .DisplayBacklight
                    .BrightnessForAmbientPercent(AmbientLightPercent);
                Backlights.LedPercent = AutoBrightness
                    .LedIntensity
                    .IntensityForAmbientPercent(AmbientLightPercent);

                RefreshBacklights();
            }
        }

        /// <inheritdoc/>
        public void RefreshLamps(bool skipDuplicateCheck = false)
        {
            for(var idx = 0;idx < BinaryLampMap.BinaryLamps.Count;++idx) {
                var lamp = BinaryLampMap.BinaryLamps[idx];
                var on = lamp.On;
                switch((CduLamp)lamp.ExternalId) {
                    case CduLamp.Dspy:  on = Lamps.Dspy; break;
                    case CduLamp.Exec:  on = Lamps.Exec; break;
                    case CduLamp.Fail:  on = Lamps.Fail; break;
                    case CduLamp.Fm:    on = Lamps.Fm; break;
                    case CduLamp.Fm1:   on = Lamps.Fm1; break;
                    case CduLamp.Fm2:   on = Lamps.Fm2; break;
                    case CduLamp.Ind:   on = Lamps.Ind; break;
                    case CduLamp.Line:  on = Lamps.Line; break;
                    case CduLamp.Mcdu:  on = Lamps.Mcdu; break;
                    case CduLamp.Menu:  on = Lamps.Menu; break;
                    case CduLamp.Msg:   on = Lamps.Msg; break;
                    case CduLamp.Ofst:  on = Lamps.Ofst; break;
                    case CduLamp.Rdy:   on = Lamps.Rdy; break;
                }
                BinaryLampMap.SetLamp(idx, on);
            }
            _IlluminationWriter?.SetLamps(BinaryLampMap, skipDuplicateCheck);
        }

        /// <inheritdoc/>
        public void UseFont(McduFontFile fontFileContent, bool useFullWidth, bool skipDuplicateCheck = false)
        {
            _UsbWriter?.LockForOutput(() => {
                var fontUploaded = _FontWriter != null && _ScreenWriter != null && _FontWriter.SendFont(
                    fontFileContent,
                    CP,
                    useFullWidth,
                    () => {
                        _ScreenWriter.SendScreenToDisplay(
                            _EmptyScreen,
                            skipDuplicateCheck: false,
                            suppressUpdatingDeviceCallback: DisplayChanging == null
                        );
                    },
                    Backlights.DisplayPercent,
                    XOffset,
                    YOffset,
                    skipDuplicateCheck,
                    suppressUpdatingDeviceCallback: FontChanging == null
                );
                if(fontUploaded) {
                    // As of time of writing the packet map includes a pile of {CP}...1901 commands to
                    // set the colours to WinWing's defaults. If I remove this then the font goes weird.
                    // So for now I'm just resending the colour palette to override the colours that the
                    // font set up. This will need refining at some point once I understand the meaning
                    // of the {CP}s being sent at the end of the font setup.
                    // TODO: Try to remove colour setup from font upload.
                    //
                    // One advantage of resending the palette is that we also refresh the display, which
                    // we need to do anyway. If SendPalette() is removed in the future then you will have
                    // to replace it with RefreshDisplay.
                    _PaletteWriter?.ReestablishPaletteAndRefreshDisplay(_ScreenWriter!, Screen);
                }
            });
        }

        /// <inheritdoc/>
        public void RefreshPalette(
            bool skipDuplicateCheck = false,
            bool forceDisplayRefresh = true
        )
        {
            if(_ScreenWriter != null) {
                _PaletteWriter?.SendPalette(
                    Palette.ToWinWingOrdinalColours(),
                    _ScreenWriter,
                    Screen,
                    skipDuplicateCheck,
                    forceDisplayRefresh,
                    suppressUpdatingDeviceCallback: PaletteChanging == null
                );
            }
        }

        /// <inheritdoc/>
        public void RefreshBacklights(bool skipDuplicateCheck = false)
        {
            if(_IlluminationWriter != null) {
                _IlluminationWriter.SetIntensity(_DisplayBacklightId,   Backlights.DisplayPercent,  skipDuplicateCheck: skipDuplicateCheck);
                _IlluminationWriter.SetIntensity(_KeyboardBacklightId,  Backlights.KeyboardPercent, skipDuplicateCheck: skipDuplicateCheck);
                _IlluminationWriter.SetIntensity(_LedBrightnessId,      Backlights.LedPercent,      skipDuplicateCheck: skipDuplicateCheck);
            }
        }

        /// <inheritdoc/>
        public void ResetBrightnesses(
            int ledPercent = 70,
            int displayPercent = 90,
            int keyboardPercent = 70
        )
        {
            Backlights.DisplayPercent = displayPercent;
            Backlights.LedPercent = ledPercent;
            Backlights.KeyboardPercent = keyboardPercent;
            RefreshBacklights();
        }

        /// <inheritdoc/>
        public void Cleanup(
            int backlightBrightnessPercent = 0,
            int displayBrightnessPercent = 0,
            int lampBrightnessPercent = 0
        )
        {
            Screen.Clear();
            Lamps.TurnAllOn(false);
            _IlluminationWriter?.SetIntensity(_KeyboardBacklightId, lampBrightnessPercent, skipDuplicateCheck: true);
            _IlluminationWriter?.SetIntensity(_DisplayBacklightId, displayBrightnessPercent, skipDuplicateCheck: true);
            _IlluminationWriter?.SetIntensity(_LedBrightnessId, backlightBrightnessPercent, skipDuplicateCheck: true);
            RefreshDisplay();
            RefreshLamps();
        }

        /// <inheritdoc/>
        public bool IsKeySupported(CduKey key) => KeyToFlagOffsetCallback(key).Flag != 0;

        /// <summary>
        /// True if the device supports the LED lamp passed across.
        /// </summary>
        /// <param name="lamp"></param>
        /// <returns></returns>
        public bool IsLampSupported(CduLamp lamp) => BinaryLampMap.ContainsExternalId((int)lamp);

        #region Obsolete members retained for short term backwards compatability

#pragma warning disable CS0618 // Type or member is obsolete

        private DeviceIdentifier _DeviceId;
        [Obsolete("Use UsbDevice")]
        public DeviceIdentifier DeviceId => _DeviceId;

        private Leds _Leds;
        [Obsolete("Use Lamps")]
        public Leds Leds => _Leds;

        private IReadOnlyList<Led>? _SupportedLeds;
        [Obsolete("Use SupportedLamps")]
        public IReadOnlyList<Led> SupportedLeds
        {
            get {
                var result = _SupportedLeds;
                if(result == null) {
                    result = SupportedLamps
                        .Where(lamp => Enum.IsDefined(typeof(Led), (Led)lamp))
                        .OfType<Led>()
                        .ToArray();
                    _SupportedLeds = result;
                }
                return result;
            }
        }

        [Obsolete("Use RefreshLamps")]
        public void RefreshLeds(bool skipDuplicateCheck = false)
        {
            RefreshLamps(skipDuplicateCheck);
        }

        [Obsolete("Use IsLampSupported")]
        public bool IsLedSupported(Led led) => IsLampSupported((CduLamp)led);

        [Obsolete("Use SupportedCduKeys instead")]
        public IReadOnlyList<Key> SupportedKeys { get; }

        [Obsolete("Use CduKeyDown instead")]
        public event EventHandler<KeyEventArgs>? KeyDown;

        [Obsolete("Use OnCduKeyDown instead")]
        protected virtual void OnKeyDown(Func<KeyEventArgs> createArgs)
        {
            if(KeyDown != null) {
                KeyDown?.Invoke(this, createArgs());
            }
        }

        [Obsolete("Use CduKeyUp instead")]
        public event EventHandler<KeyEventArgs>? KeyUp;

        [Obsolete("Use OnCduKeyUp instead")]
        protected virtual void OnKeyUp(Func<KeyEventArgs> createArgs)
        {
            if(KeyUp != null) {
                KeyUp?.Invoke(this, createArgs());
            }
        }

        [Obsolete("Use the CduKey version instead")]
        public bool IsKeySupported(Key key) => KeyToFlagOffsetCallback((CduKey)key).Flag != 0;

        [Obsolete("Use Backlights.DisplayPercent instead")]
        public int DisplayBrightnessPercent
        {
            get => Backlights.DisplayPercent;
            set {
                Backlights.DisplayPercent = value;
                RefreshBacklights();
            }
        }

        [Obsolete("Use Backlights.KeyboardPercent instead")]
        public int BacklightBrightnessPercent
        {
            get => Backlights.KeyboardPercent;
            set {
                Backlights.KeyboardPercent = value;
                RefreshBacklights();
            }
        }

        [Obsolete("Use Backlights.LedPercent instead")]
        public int LedBrightnessPercent
        {
            get => Backlights.LedPercent;
            set {
                Backlights.LedPercent = value;
                RefreshBacklights();
            }
        }

        [Obsolete("Use RefreshBacklights instead")]
        public void RefreshBrightnesses() => RefreshBacklights();

#pragma warning restore CS0618 // Type or member is obsolete

        #endregion
    }
}
