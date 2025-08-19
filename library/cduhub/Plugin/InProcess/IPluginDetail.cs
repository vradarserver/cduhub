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

namespace Cduhub.Plugin.InProcess
{
    /// <summary>
    /// The interface that describes a plugin page exposed by a plugin DLL.
    /// </summary>
    public interface IPluginDetail
    {
        /// <summary>
        /// The page ID. This must be unique across all plugins.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The label to use for the page in the plugin menu.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The relative order of the plugin in the plugin menu. Lower values are
        /// displayed before higher. Default is zero. Two plugins with the same display
        /// order are sorted by label.
        /// </summary>
        int DisplayOrder { get; }

        /// <summary>
        /// The page to display the plugin on.
        /// </summary>
        EntryPointPage EntryPointPage { get; }

        /// <summary>
        /// Creates a new page instance for the hub passed across.
        /// </summary>
        /// <param name="hub"></param>
        /// <returns></returns>
        Page CreatePage(Hub hub);
    }
}
