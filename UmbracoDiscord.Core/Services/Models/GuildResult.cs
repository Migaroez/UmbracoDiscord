using Newtonsoft.Json;

namespace UmbracoDiscord.Core.Services.Models
{
    public class GuildResult
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("owner")]
        public bool Owner { get; set; }

        [JsonProperty("permissions")]
        public string Permissions { get; set; }

        [JsonProperty("features")]
        public string[] Features { get; set; }
    }
}
