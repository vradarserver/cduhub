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
using System.Linq;
using HidSharp;

namespace McduDotNet
{
    /// <summary>
    /// A static class that hooks HidSharp.DeviceList.Local.Changed and calls a callback
    /// when specific devices disconnect. If no devices register an interest in disconnect
    /// events then the class unhooks HidSharp.
    /// </summary>
    static class HidSharpDeviceWatcher
    {
        /// <summary>
        /// Ties a callback action to a USB device path.
        /// </summary>
        class Callback
        {
            public string DevicePath { get; }

            public Action CallbackAction { get; }

            public Callback(string devicePath, Action callback)
            {
                DevicePath = devicePath;
                CallbackAction = callback;
            }
        }

        private static readonly object _SyncLock = new();
        private static bool _HookedHidSharp;
        private static readonly List<Callback> _DisconnectedCallbacks = new();

        /// <summary>
        /// Registers a callback that runs when the device identified by the USB device
        /// path is disconnected. More than one callback can be registered against the
        /// same path. Callbacks are automatically deregistered if the device becomes
        /// disconnected.
        /// </summary>
        /// <param name="devicePath"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void RegisterDisconnectedCallback(string devicePath, Action callback)
        {
            if(devicePath == null) {
                throw new ArgumentNullException(nameof(devicePath));
            }
            if(callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }

            var callbackRecord = new Callback(devicePath, callback);
            lock(_SyncLock) {
                _DisconnectedCallbacks.Add(callbackRecord);

                if(!_HookedHidSharp) {
                    _HookedHidSharp = true;
                    HidSharp.DeviceList.Local.Changed += HidSharpDeviceList_Changed;
                }
            }
        }

        /// <summary>
        /// Removes all callbacks that would otherwise run when the device identified by
        /// the USB device path is disconnected. You are allowed to pass paths that have
        /// never been, or are no longer, registered.
        /// </summary>
        /// <param name="devicePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void DeregisterDisconnectedCallbacks(string devicePath)
        {
            if(devicePath == null) {
                throw new ArgumentNullException(nameof(devicePath));
            }

            lock(_SyncLock) {
                var removeCallbacks = _DisconnectedCallbacks
                    .Where(r => r.DevicePath == devicePath)
                    .ToArray();
                foreach(var removeCallback in removeCallbacks) {
                    _DisconnectedCallbacks.Remove(removeCallback);
                }

                if(_DisconnectedCallbacks.Count == 0 && _HookedHidSharp) {
                    _HookedHidSharp = false;
                    HidSharp.DeviceList.Local.Changed -= HidSharpDeviceList_Changed;
                }
            }
        }

        private static void HidSharpDeviceList_Changed(object sender, DeviceListChangedEventArgs e)
        {
            HashSet<string> registeredDevicePaths = new();
            lock(_SyncLock) {
                foreach(var callback in _DisconnectedCallbacks) {
                    registeredDevicePaths.Add(callback.DevicePath);
                }
            }

            var devices = HidSharp
                .DeviceList
                .Local
                .GetHidDevices();
            foreach(var device in devices) {
                registeredDevicePaths.Remove(device.DevicePath);
            }

            if(registeredDevicePaths.Count > 0) {
                Callback[] callbacks;
                lock(_SyncLock) {
                    callbacks = _DisconnectedCallbacks
                        .Where(candidate => registeredDevicePaths.Contains(candidate.DevicePath))
                        .ToArray();
                    foreach(var callback in callbacks) {
                        DeregisterDisconnectedCallbacks(callback.DevicePath);
                    }
                }

                foreach(var callback in callbacks) {
                    callback.CallbackAction();
                }
            }
        }
    }
}
