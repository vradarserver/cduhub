﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cduhub.FlightSim.XPlaneRestModels
{
    [DataContract]
    public class KnownDatarefsModel
    {
        [DataMember]
        public List<DatarefInfoModel> Data { get; set; } = new List<DatarefInfoModel>();
    }

    [DataContract]
    public class DatarefInfoModel
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember(Name = "is_writable")]
        public bool IsWritable { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember(Name = "value_type")]
        public string ValueType { get; set; }

        public override string ToString() => Name;
    }

    [DataContract]
    public class DatarefValueModel
    {
        [DataMember(Name = "data")]
        public string Data { get; set; }
    }

    [DataContract]
    public class KnownCommandsModel
    {
        [DataMember]
        public List<CommandInfoModel> Data { get; set; } = new List<CommandInfoModel>();
    }

    [DataContract]
    public class CommandInfoModel
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class ActivateCommandModel
    {
        [DataMember(Name = "duration")]
        public float Duration { get; set; }
    }
}
