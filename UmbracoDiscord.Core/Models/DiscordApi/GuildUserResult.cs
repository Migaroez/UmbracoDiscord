using Newtonsoft.Json;

namespace UmbracoDiscord.Core.Models.DiscordApi
{
    public class GuildUserResult
    {
        [JsonProperty("roles")]
        public decimal[] Roles { get; set; }
    }
}
