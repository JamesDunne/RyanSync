using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RyanSync
{
    [DataContract]
    public sealed class FileList
    {
        [DataMember(Name="baseUrl")]
        public string BaseUrl { get; set; }

        [DataMember(Name="files")]
        public string[] FileNames { get; set; }
    }
}
