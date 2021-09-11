using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Flurl.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Services.Exceptions;
using UmbracoDiscord.Core.Services.Models;

namespace UmbracoDiscord.Core.Services
{
    public class DiscordAuthService : IDiscordAuthService
    {
        private const string TokenEndpoint = "https://discord.com/api/v8/oauth2/token";
        private const string UserEndpoint = "https://discord.com/api/v8/users/@me";
        private const string GuildEndpoint = "https://discord.com/api/v8/users/@me/guilds";

        private readonly ILogger<DiscordAuthService> _logger;
        private readonly IMemberService _memberService;
        private static Dictionary<string, Guid> _stateTracker = new(); // todo change this to a repository

        public DiscordAuthService(ILogger<DiscordAuthService> logger,
            IMemberService memberService)
        {
            _logger = logger;
            _memberService = memberService;
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

        public async Task<Attempt<string>> HandleRedirect(HttpContext httpContext, DiscordSection settings)
        {
            // get bearer token from redirect code
            var bearerTokenResult = await ExchangeRedirectCode((string)httpContext.Request.Query["code"],settings);

            // get user
            var userResult = await GetUser(bearerTokenResult.AccessToken);
            if (userResult.Verified == false)
            {
                return Attempt<string>.Fail(new EmailUnverifiedException());
            }

            // get guilds and check they are still a member of the guild specified
            var guildResult = await GetGuilds(bearerTokenResult.AccessToken);

            // if userId exists on any member, update member and log them in
            var existingMember = _memberService.GetByEmail(userResult.Email);
            if (existingMember != null)
            {
                var updateMemberResult = UpdateMember(existingMember, userResult, guildResult, settings);
                if (updateMemberResult.Success == false)
                {
                    return Attempt<string>.Fail(updateMemberResult.Exception);
                }
                return Attempt<string>.Succeed(userResult.Username);
            }

            // if no member exists, create member and log them in
            var newMemberResult = CreateMember(userResult, guildResult, settings);
            if (newMemberResult.Success == false)
            {
                return Attempt<string>.Fail(newMemberResult.Exception);
            }
            return Attempt<string>.Succeed(userResult.Email);

            // contact the authbot and update member permissions based on roles

        }

        private async Task<BearerTokenResult> ExchangeRedirectCode(string code, DiscordSection settings)
        {
            return await TokenEndpoint.PostUrlEncodedAsync(new
            {
                client_id = settings.ClientId,
                client_secret = settings.ClientSecret,
                grant_type = "authorization_code",
                code = code,
                redirect_uri = settings.FirstChild<DiscordLoginRedirectHandler>().Url(mode: UrlMode.Absolute)
            }).ReceiveJson<BearerTokenResult>().ConfigureAwait(false);
        }

        private async Task<UserResult> GetUser(string accessToken)
        {
            return await UserEndpoint.WithOAuthBearerToken(accessToken)
                .GetAsync().ReceiveJson<UserResult>().ConfigureAwait(false);
        }

        private async Task<List<GuildResult>> GetGuilds(string accessToken)
        {
            return await GuildEndpoint.
                WithOAuthBearerToken(accessToken)
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);
        }

        private Attempt<bool> UpdateMember(IMember member, UserResult userResult, List<GuildResult> guilds, DiscordSection settings)
        {
            if (RequiredGuildsValidated(userResult, guilds, settings) == false)
            {
                member.IsApproved = false;
                _memberService.Save(member);
                return Attempt<bool>.Fail(new FailedRequiredGuildsException());
            }
            _memberService.Save(member);
            return Attempt<bool>.Succeed();
            
        }

        private Attempt<bool> CreateMember(UserResult userResult, List<GuildResult> guilds, DiscordSection settings)
        {
            if (RequiredGuildsValidated(userResult, guilds, settings) == false)
            {
                return Attempt<bool>.Fail(new FailedRequiredGuildsException());
            }

            var newMember = _memberService.CreateMember(userResult.Email, userResult.Email, userResult.Username, "member");
            UpdateUserDetails(newMember,userResult);
            
            _memberService.Save(newMember);
            return Attempt<bool>.Succeed();
        }

        private void UpdateUserDetails(IMember member, UserResult userResult)
        {
            member.AdditionalData["discordId"] = userResult.Id;
            member.AdditionalData["discordUserName"] = userResult.Username;
            member.AdditionalData["discordDiscriminator"] = userResult.Discriminator;
        }

        private bool RequiredGuildsValidated(UserResult userResult, List<GuildResult> guilds, DiscordSection section)
        {
            if (section.RequiredGuildIds.IsNullOrWhiteSpace())
            {
                return true;
            }

            var requiredGuildStrings = section.RequiredGuildIds.Split(",");
            requiredGuildStrings.RemoveAll(i => i.IsNullOrWhiteSpace());
            var requiredGuildIds = requiredGuildStrings.Select(id => Convert.ToInt64(id));

            if (requiredGuildStrings.Any() == false)
            {
                return true;
            }

            return guilds.Any(g => requiredGuildIds.Any(gi => gi == g.Id));
        }
        #endregion
    }
}
