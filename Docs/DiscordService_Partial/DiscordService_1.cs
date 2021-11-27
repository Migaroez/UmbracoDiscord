using Flurl.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using UmbracoDiscord.Core.Models.DiscordApi;

namespace UmbracoDiscord.Core.Partial
{
    public class DiscordService
    {
        private readonly IConfiguration _configuration;

        public DiscordService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<GuildResult>> GetAvailableGuilds()
        {
            return await Constants.DiscordApi.GuildEndpoint.WithHeader("Authorization", "Bot " + _configuration["Discord:BotToken"])
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);
        }

        public async Task<List<GuildResult>> GetAvailableRolesForGuild(ulong guildId)
        {
            return await string.Format(Constants.DiscordApi.GuildRolesEndpoint, guildId).WithHeader("Authorization", "Bot " + _configuration["Discord:BotToken"])
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);
        }
    }
}
