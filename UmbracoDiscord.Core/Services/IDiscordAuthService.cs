using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.PublishedModels;
using UmbracoDiscord.Core.Models.DiscordApi;

namespace UmbracoDiscord.Core.Services
{
    public interface IDiscordAuthService
    {
        Guid? GetState(HttpContext httpContext, bool renew);
        bool IsValidState(HttpContext httpContext);
        Task<Attempt<string>> HandleRedirect(HttpContext httpContext, DiscordSection settings);
        Task<List<GuildResult>> GetAvailableGuilds();
        Task<List<GuildResult>> GetAvailableRolesForGuild(ulong guildId);
    }
}