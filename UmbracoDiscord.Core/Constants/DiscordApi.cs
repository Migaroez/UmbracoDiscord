using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoDiscord.Core.Constants
{
    public static class DiscordApi
    {
        public const string BaseEndpoint = "https://discord.com/api/v8/";
        public const string TokenEndpoint = BaseEndpoint + "oauth2/token";
        public const string UserEndpoint = BaseEndpoint + "users/@me";
        public const string GuildEndpoint = BaseEndpoint + "users/@me/guilds";
        public const string GuildRolesEndpoint = BaseEndpoint + "guilds/{0}/roles";
        public const string GuildPermissionsEndpoint = BaseEndpoint + "guilds/{0}/members/{1}";
    }
}
