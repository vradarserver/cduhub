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
using System.Threading;
using HidSharp;

namespace WwDevicesDotNet
{
    /// <summary>
    /// The abstract base for a class that takes a HidStream that's owned by its parent
    /// and reads reports from it.
    /// </summary>
    /// <remarks><para>
    /// HidStream turned out to be great for sending stuff, not so great for reading. It
    /// could well be me messing something up, but for all the world it looks like it is
    /// trying to buffer the input coming from the devices. The WinWing MCDU is sending
    /// packets like they're going out of style, certainly faster than I can process them,
    /// and consequently that buffer gets out of hand. The end result is a very laggy input
    /// that gets worse over time.
    /// </para><para>
    /// What I would like to do is just repeatedly poll for the next report from the device
    /// and ignore everything else. I can poll fast enough to keep the input responsive.
    /// But to do that I would need to disable HidSharp's buffering, and I could not see a
    /// nice way to manage that.
    /// </para><para>
    /// So... I'm doing it the nasty way and just emptying the buffer manually before I poll
    /// for the next report. It isn't ideal, but it works. Long term I'm going to look at
    /// alternatives to HidSharp.
    /// </para><para>
    /// This base abstracts away the fiddling with buffers.
    /// </para></remarks>
    abstract class UsbPollingReader
    {
        // The USB reader does not own this object. It is owned by the parent. It is
        // disposable but the parent is responsible for disposing of it.
        protected readonly HidStream _HidStream;

        private readonly byte[] _ClearBuffer;

        /// <summary>
        /// The size of the report packet expected from the device.
        /// </summary>
        protected abstract int PacketSize { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="hidStream"></param>
        public UsbPollingReader(HidStream hidStream)
        {
            _HidStream = hidStream;
            _ClearBuffer = new byte[PacketSize];
        }

        public void RunInputLoop(CancellationToken cancellationToken)
        {
            var readBuffer = new byte[PacketSize];

            _HidStream.ReadTimeout = 1000;
            while(!cancellationToken.IsCancellationRequested) {
                try {
                    var keepReading = true;
                    if(_HidStream.CanRead && keepReading) {
                        ClearHidStreamBuffer();
                        try {
                            var bytesRead = _HidStream.Read(readBuffer, 0, readBuffer.Length);
                            if (bytesRead > 0) {
                                if(readBuffer[0] == 1 && bytesRead >= PacketSize) {
                                    ReportReceived(readBuffer, bytesRead);
                                }
                            }
                        } catch(TimeoutException) {
                            ;
                        } catch(ObjectDisposedException) {
                            keepReading = false;
                        }
                    }
                } catch(IOException) {
                    // These will happen when the device is disconnected. Under Windows we can look for the Win32
                    // exception and tell for sure, but that won't fly on other platforms. For now I'm going to
                    // assume that any IO exception during the input loop is indicative of the device being
                    // disconnected.
                    //
                    // There is a strong argument for raising the disconnected event here. However, we're also
                    // listening to HidSharp's device change event and raising from there, so we risk a reentrant
                    // raise if we do. Also if the event handler disposes of this MCDU on a disconnect then the
                    // dispose will block waiting for us to finish, but we would be blocking waiting on the event
                    // handler... the wait would timeout eventually but it wouldn't be very nice.
                }

                // Give up our timeslice
                Thread.Sleep(1);
            }
        }

        protected abstract void ReportReceived(byte[] readBuffer, int bytesRead);

        /// <summary>
        /// Tries to clear out HidStream's buffer by reading it with a very short timeout
        /// and discarding packets until eventually I get a timeout exception from it. This
        /// is less than ideal.
        /// </summary>
        private void ClearHidStreamBuffer()
        {
            // TODO: Does HidSharp really not support polling without buffering? I think I
            // missed something.

            var timeout = _HidStream.ReadTimeout;
            try {
                _HidStream.ReadTimeout = 1;
                while(_HidStream.Read(_ClearBuffer, 0, _ClearBuffer.Length) > 0) {
                    ;
                }
            } catch(TimeoutException) {
                ;
            } catch(ObjectDisposedException) {
                ;
            } finally {
                _HidStream.ReadTimeout = timeout;
            }
        }
    }
}
