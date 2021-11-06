namespace UmbracoDiscord.Core.Constants
{
    public static class DiscordApi
    {
        public const string BaseEndpoint = "https://discord.com/api/v9/";
        public const string AuthorizeEndpoint = BaseEndpoint + "oauth2/authorize";
        public const string TokenEndpoint = BaseEndpoint + "oauth2/token";
        public const string UserEndpoint = BaseEndpoint + "users/@me";
        public const string GuildEndpoint = BaseEndpoint + "users/@me/guilds";
        public const string GuildRolesEndpoint = BaseEndpoint + "guilds/{0}/roles";
        public const string GuildPermissionsEndpoint = BaseEndpoint + "guilds/{0}/members/{1}";
    }
}
