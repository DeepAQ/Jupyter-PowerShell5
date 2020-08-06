using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jupyter_PowerShell5.Models
{
    public class Message
    {
        [JsonIgnore] public List<byte[]> Identities { get; set; } = new List<byte[]>();
        
        [JsonIgnore] public string Signature { get; set; }
        
        [JsonProperty("header")] public Header Header { get; set; }

        [JsonProperty("parent_header")] public Header ParentHeader { get; set; }

        [JsonProperty("metadata")] public JObject MetaData { get; set; }

        [JsonProperty("content")] public JObject Content { get; set; }

        [JsonProperty("buffers")] public List<byte[]> Buffers { get; set; } = new List<byte[]>();
    }
}