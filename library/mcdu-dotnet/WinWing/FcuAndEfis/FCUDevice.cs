using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;

namespace wwDevicesDotNet.WinWing.FcuAndEfis
{
    /// <summary>
    /// Represents a WinWing FCU (Flight Control Unit) device.
    /// Handles communication with the physical FCU hardware via HID protocol.
    /// </summary>
    public class FCUDevice : IDisposable
    {
        const int _VendorId = 0x4098;  // WinWing vendor ID
        const int _ProductId = 0xBB10; // FCU product ID (verify this value)

        HidDevice _HidDevice;
        HidStream _HidStream;
        bool _Disposed;
        CancellationTokenSource _InputLoopCancellationTokenSource;
        Task _InputLoopTask;

        /// <summary>
        /// Gets a value indicating whether the device is connected.
        /// </summary>
        public bool IsConnected => _HidStream != null;

        /// <summary>
        /// Event raised when the device state changes.
        /// </summary>
        public event EventHandler<FcuStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Event raised when the device is disconnected.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="FCUDevice"/> class.
        /// </summary>
        public FCUDevice()
        {
        }

        /// <summary>
        /// Discovers and connects to the first available FCU device.
        /// </summary>
        /// <returns>True if a device was found and connected; otherwise, false.</returns>
        public bool Connect()
        {
            var devices = DeviceList.Local
                .GetHidDevices(_VendorId, _ProductId)
                .ToList();
            
            if (devices.Count == 0)
                return false;

            _HidDevice = devices[0];
            
            if (!_HidDevice.TryOpen(out _HidStream))
                return false;

            // Subscribe to device list changes for disconnect detection
            DeviceList.Local.Changed += HidSharpDeviceList_Changed;

            // Start reading input reports on a background task
            _InputLoopCancellationTokenSource = new CancellationTokenSource();
            _InputLoopTask = Task.Run(() => RunInputLoop(_InputLoopCancellationTokenSource.Token));

            return true;
        }

        /// <summary>
        /// Disconnects from the device.
        /// </summary>
        public void Disconnect()
        {
            DeviceList.Local.Changed -= HidSharpDeviceList_Changed;

            // Stop the input loop
            _InputLoopCancellationTokenSource?.Cancel();
            _InputLoopTask?.Wait(5000);
            _InputLoopTask = null;

            // Close and dispose the stream
            var hidStream = _HidStream;
            _HidStream = null;
            try
            {
                hidStream?.Dispose();
            }
            catch
            {
                ;
            }

            _HidDevice = null;
        }

        /// <summary>
        /// Sends a command to the FCU device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public void SendCommand(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Device is not connected.");

            _HidStream.Write(data);
        }

        /// <summary>
        /// Updates the FCU display and LEDs.
        /// </summary>
        /// <param name="state">The state to display on the FCU.</param>
        public void UpdateDisplay(FcuDisplayState state)
        {
            if (!IsConnected)
                return;

            var data = BuildDisplayCommand(state);
            SendCommand(data);
        }

        void RunInputLoop(CancellationToken cancellationToken)
        {
            var readBuffer = new byte[64]; // Adjust size based on FCU report size
            _HidStream.ReadTimeout = 1000;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_HidStream != null && _HidStream.CanRead)
                    {
                        var bytesRead = _HidStream.Read(readBuffer, 0, readBuffer.Length);
                        if (bytesRead > 0)
                        {
                            ProcessReport(readBuffer, bytesRead);
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // Expected when no data available
                }
                catch (ObjectDisposedException)
                {
                    // Stream was disposed
                    break;
                }
                catch (System.IO.IOException)
                {
                    // Device disconnected
                    break;
                }

                // Yield to prevent busy-waiting
                Thread.Sleep(1);
            }
        }

        void ProcessReport(byte[] data, int length)
        {
            // Parse the incoming HID report and raise StateChanged event
            var eventArgs = new FcuStateChangedEventArgs(data, length);
            StateChanged?.Invoke(this, eventArgs);
        }

        byte[] BuildDisplayCommand(FcuDisplayState state)
        {
            // TODO: Implement protocol for updating FCU displays and LEDs
            // This will depend on the specific WinWing FCU protocol
            throw new NotImplementedException();
        }

        void HidSharpDeviceList_Changed(object sender, DeviceListChangedEventArgs e)
        {
            if (_HidDevice != null)
            {
                var devicePresent = DeviceList.Local
                    .GetHidDevices()
                    .Any(device => device.DevicePath == _HidDevice.DevicePath);
                
                if (!devicePresent)
                {
                    OnDisconnected();
                }
            }
        }

        void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_Disposed)
                return;

            Disconnect();
            _Disposed = true;
        }
    }

    /// <summary>
    /// Event arguments for FCU state changes.
    /// </summary>
    public class FcuStateChangedEventArgs : EventArgs
    {
        public byte[] RawData { get; }
        public int Length { get; }

        public FcuStateChangedEventArgs(byte[] data, int length)
        {
            RawData = new byte[length];
            Array.Copy(data, RawData, length);
            Length = length;
        }
    }

    /// <summary>
    /// Represents the display state of the FCU.
    /// </summary>
    public class FcuDisplayState
    {
        public int? Speed { get; set; }
        public int? Heading { get; set; }
        public int? Altitude { get; set; }
        public int? VerticalSpeed { get; set; }
    
        // LED states
        public bool SpeedManaged { get; set; }
        public bool HeadingManaged { get; set; }
        public bool AltitudeManaged { get; set; }
    }
}
