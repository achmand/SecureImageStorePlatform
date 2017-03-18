using Newtonsoft.Json;

namespace WebApplication.Models
{
    public sealed class RolesJson
    {
        [JsonProperty("RoleName")]
        public string RoleName { get; set; }
    }
}