using Newtonsoft.Json;

namespace Jupyter_PowerShell5.Models
{
    public abstract class StatusBase
    {
        public const string Ok = "ok";
        public const string Error = "error";

        [JsonProperty("status")] public string Status { get; set; } = Ok;
    }
}