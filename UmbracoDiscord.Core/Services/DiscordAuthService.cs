using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Flurl.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Services.Models;

namespace UmbracoDiscord.Core.Services
{
    public class DiscordAuthService : IDiscordAuthService
    {
        private const string TokenEndpoint = "https://discord.com/api/v8/oauth2/token";
        private const string UserEndpoint = "https://discord.com/api/v8/users/@me";
        private const string GuildEndpoint = "https://discord.com/api/v8/users/@me/guilds";

        private readonly ILogger<DiscordAuthService> _logger;
        private static Dictionary<string, Guid> _stateTracker = new(); // todo change this to a repository

        public DiscordAuthService(ILogger<DiscordAuthService> logger)
        {
            _logger = logger;
        }

        #region State
        public Guid? GetState(HttpContext httpContext, bool renew)
        {
            if (httpContext?.Connection.RemoteIpAddress == null)
            {
                _logger.LogWarning("Could not issue State, httpContext or RemoteIpAddress unavailable");
                return null;
            }

            var ip = httpContext.Connection.RemoteIpAddress.ToString();
            if (renew || _stateTracker.ContainsKey(ip) == false)
            {
                return SetState(ip);
            }

            return _stateTracker[ip];
        }

        private Guid SetState(string ip)
        {
            var state = Guid.NewGuid();
            _stateTracker[ip] = state;
            return state;
        }

        public bool IsValidState(HttpContext httpContext)
        {
            if (httpContext?.Connection.RemoteIpAddress == null)
            {
                _logger.LogWarning("Could not validate State, httpContext or RemoteIpAddress unavailable");
                return false;
            }

            var ip = httpContext.Connection.RemoteIpAddress.ToString();

            return _stateTracker.ContainsKey(ip) && _stateTracker[ip].ToString() == (string)httpContext.Request.Query["state"];
        }
        #endregion

        #region HandleRedirect

        public async Task<bool> HandleRedirect(HttpContext httpContext, DiscordSection settings)
        {
            // get bearer token from redirect code
            var bearerTokenResult = await TokenEndpoint.AllowAnyHttpStatus().PostUrlEncodedAsync(new
            {
                client_id = settings.ClientId,
                client_secret = settings.ClientSecret,
                grant_type = "authorization_code",
                code = (string) httpContext.Request.Query["code"],
                redirect_uri = settings.FirstChild<DiscordLoginRedirectHandler>().Url(mode: UrlMode.Absolute)
            }).ReceiveJson<BearerTokenResult>().ConfigureAwait(false);

            // get user
            var userResult = await UserEndpoint.
                WithOAuthBearerToken(bearerTokenResult.AccessToken)
                .AllowAnyHttpStatus()
                .GetAsync().ReceiveJson<UserResult>().ConfigureAwait(false);

            // get guilds and check they are still a member of the guild specified
            var guildResult = await GuildEndpoint.
                WithOAuthBearerToken(bearerTokenResult.AccessToken)
                .AllowAnyHttpStatus()
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);

            // if userId exists on any member, update member and log them in

            // if no member exists, create member and log them in

            // contact the authbot and update member permissions based on roles

            return true;
        }

        private void ExchangeRedirectCode(string code)
        {

        }
        #endregion
    }
}
