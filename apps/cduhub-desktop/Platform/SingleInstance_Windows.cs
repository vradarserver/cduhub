// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Cduhub.DesktopGui.Platform
{
    /// <summary>
    /// Implementation of <see cref="ISingleInstance"/> for Windows. Uses the same global
    /// mutex that cduhub-windows uses.
    /// </summary>
    /// <remarks>
    /// If both cduhub-windows and cduhub-desktop were to run concurrently then they would
    /// both try to control the device, chaos would ensue. Users need to choose one or
    /// t'other, not both.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    sealed class SingleInstance_Windows : ISingleInstance
    {
        /// <summary>
        /// The mutex name. This is the same as cduhub-windows, don't change one without
        /// changing the other.
        /// </summary>
        const string _Name = @"Global\CduHub-SGEZ8Z2CM8UA";

        /// <inheritdoc/>
        public Mutex? Acquire()
        {
            var security = new MutexSecurity();
            security.AddAccessRule(new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, domainSid: null),
                MutexRights.FullControl,
                AccessControlType.Allow
            ));

            var mutex = MutexAcl.Create(
                initiallyOwned: false,
                _Name,
                out var _,
                security
            );

            return SingleInstanceMutex.TryAcquire(mutex);
        }
    }
}
