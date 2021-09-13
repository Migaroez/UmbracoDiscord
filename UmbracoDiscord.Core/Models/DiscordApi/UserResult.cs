using Newtonsoft.Json;

namespace UmbracoDiscord.Core.Models.DiscordApi
{
    public class UserResult
    {
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("flags")]
        public int Flags { get; set; }

        [JsonProperty("id")]
        public decimal Id { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("mfa_enabled")]
        public bool Mfa_Enabled { get; set; }

        [JsonProperty("public_flags")]
        public string PublicFlags { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }
}
