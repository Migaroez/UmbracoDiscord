using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoDiscord.Core.Services
{
    public interface IDiscordAuthService
    {
        Guid? GetState(HttpContext httpContext, bool renew);
        bool IsValidState(HttpContext httpContext);
        Task<Attempt<string>> HandleRedirect(HttpContext httpContext, DiscordSection settings);
    }
}