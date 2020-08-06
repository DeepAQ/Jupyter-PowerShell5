using Newtonsoft.Json;

namespace Jupyter_PowerShell5.Models
{
    public class Header
    {
        [JsonProperty("msg_id")] public string MessageId { get; set; }

        [JsonProperty("session")] public string Session { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("date")] public string Date { get; set; }

        [JsonProperty("msg_type")] public string MessageType { get; set; }

        [JsonProperty("version")] public string Version { get; set; }
    }
}