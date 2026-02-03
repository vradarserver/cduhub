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
using System.IO;
using System.Text;
using HidSharp;

namespace McduDotNet
{
    /// <summary>
    /// Manages the sending of packets to a USB device.
    /// </summary>
    class UsbWriter
    {
        private readonly object _OutputLock = new();

        // The USB writer does not own this object. It is owned by the parent. It is
        // disposable but the parent is responsible for disposing of it.
        private readonly HidStream _HidStream;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidStream"></param>
        public UsbWriter(HidStream hidStream)
        {
            _HidStream = hidStream;
        }

        /// <summary>
        /// Ensures that only one thread writes to the device at once. This is a great
        /// way to deadlock the program! Be careful that the action does not block on
        /// other locks.
        /// </summary>
        /// <param name="action"></param>
        public void LockForOutput(Action action)
        {
            lock(_OutputLock) {
                action();
            }
        }

        /// <summary>
        /// Sends a packet to the device.
        /// </summary>
        /// <param name="bytes"></param>
        public void SendPacket(byte[] bytes)
        {
            var stream = _HidStream;
            try {
                stream?.Write(bytes);
            } catch(IOException) {
                // This can happen when the device is disconnected mid-write
                ;
            }
        }

        /// <summary>
        /// Sends a hex string packet to the device.
        /// </summary>
        /// <param name="packet"></param>
        public void SendStringPacket(string packet)
        {
            var bytes = packet.ToByteArray();
            SendPacket(bytes);
        }

        /// <summary>
        /// As per <see cref="SendStringPacket"/> but this pads the <paramref name="packet"/>
        /// out with zeros until it reaches <paramref name="packetSize"/>.
        /// </summary>
        /// <param name="packetSize"></param>
        /// <param name="packet"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SendStringPacket(int packetSize, string packet)
        {
            if(packet.Length % 2 != 0) {
                throw new InvalidOperationException($"{packet} is not an even length");
            }
            if(packet.Length == packetSize * 2) {
                SendStringPacket(packet);
            } else {
                var buffer = new StringBuilder(packet);
                buffer.Append(packet);
                while(buffer.Length < packet.Length) {
                    buffer.Append("00");
                }
                SendStringPacket(buffer.ToString());
            }
        }
    }
}
