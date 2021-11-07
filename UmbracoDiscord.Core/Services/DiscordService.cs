//todo: The state tracking is currently based on the IP address passed in the request object
// it is unlikely but not impossible that 2 requests for the same IP will happen around the same time
// when this happens, the first request might get invalidated because the LoginController requests a refresh
//todo: move state tracking into its own class

using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Models.DiscordApi;
using UmbracoDiscord.Core.Repositories;
using UmbracoDiscord.Core.Services.Exceptions;

namespace UmbracoDiscord.Core.Services
{
    public class DiscordService : IDiscordService
    {
        private readonly ILogger<DiscordService> _logger;
        private readonly IMemberService _memberService;
        private readonly DiscordRoleRepository _discordRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly IScopeProvider _scopeProvider;
        private static Dictionary<string, Guid> _stateTracker = new(); // todo change this to a repository so it survives app recycles

        public DiscordService(ILogger<DiscordService> logger,
            IMemberService memberService,
            DiscordRoleRepository discordRoleRepository,
            IConfiguration configuration,
            IScopeProvider scopeProvider)
        {
            _logger = logger;
            _memberService = memberService;
            _discordRoleRepository = discordRoleRepository;
            _configuration = configuration;
            _scopeProvider = scopeProvider;
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
            var bearerTokenResult = await ExchangeRedirectCode((string)httpContext.Request.Query["code"], settings);

            // get user
            var userResult = await GetUser(bearerTokenResult.AccessToken);
            if (userResult.Verified == false)
            {
                return Attempt<string>.Fail(new EmailUnverifiedException());
            }

            // get guilds and check they are still a member of the guild specified
            var guildResult = await GetUserGuilds(bearerTokenResult.AccessToken);

            // if userId exists update them
            var existingMember = _memberService.GetByEmail(userResult.Email);
            if (existingMember != null)
            {
                var updateMemberResult = await UpdateMember(existingMember, userResult, guildResult, settings).ConfigureAwait(false);
                if (updateMemberResult.Success == false)
                {
                    return Attempt<string>.Fail(updateMemberResult.Exception);
                }
                return Attempt<string>.Succeed(userResult.Email);
            }

            // if no member exists create them
            var newMemberResult = CreateMember(userResult, guildResult, settings);
            if (newMemberResult.Success == false)
            {
                return Attempt<string>.Fail(newMemberResult.Exception);
            }
            return Attempt<string>.Succeed(userResult.Email);
        }

        private async Task<BearerTokenResult> ExchangeRedirectCode(string code, DiscordSection settings)
        {
            return await Constants.DiscordApi.TokenEndpoint.PostUrlEncodedAsync(new
            {
                client_id = _configuration["Discord:ClientId"],
                client_secret = _configuration["Discord:ClientSecret"],
                grant_type = "authorization_code",
                code = code,
                redirect_uri = settings.FirstChild<DiscordLoginRedirectHandler>().Url(mode: UrlMode.Absolute)
            }).ReceiveJson<BearerTokenResult>().ConfigureAwait(false);
        }
        #endregion


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

        private async Task<UserResult> GetUser(string accessToken)
        {
            return await Constants.DiscordApi.UserEndpoint.WithOAuthBearerToken(accessToken)
                .GetAsync().ReceiveJson<UserResult>().ConfigureAwait(false);
        }

        private async Task<List<GuildResult>> GetUserGuilds(string accessToken)
        {
            return await Constants.DiscordApi.GuildEndpoint.
                WithOAuthBearerToken(accessToken)
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);
        }

        private async Task<Attempt<bool>> UpdateMember(IMember member, UserResult userResult, List<GuildResult> guilds, DiscordSection settings)
        {
            if (RequiredGuildsValidated(userResult, guilds, settings) == false)
            {
                member.IsApproved = false;
                _memberService.Save(member);
                return Attempt<bool>.Fail(new FailedRequiredGuildsException());
            }
            UpdateUserDetails(member, userResult);
            _memberService.Save(member);
            await SyncMemberGroups(member, userResult, guilds);
            return Attempt<bool>.Succeed();
        }

        private async Task SyncMemberGroups(IMember member, UserResult userResult, List<GuildResult> guilds)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            var availableGuilds = await GetAvailableGuilds();

            var syncRules = _discordRoleRepository.GetAll().ToList();
            // we are not in the guild of the rule OR we no longer have access to the guild OR the rule has been marked as syncRemove
            var groupsToRemove = syncRules.Where(r => guilds.Any(g => g.Id == r.GuildId) == false || availableGuilds.Any(g => g.Id == r.GuildId) || r.SyncRemoval)
                .Select(r => r.MembershipGroupAlias).Distinct().ToList();

            // we need to filter out unavailable guilds else fetching the discord information in the loop below will throw an error
            var activeGuilds = syncRules.Where(r => r.SyncRemoval == false && availableGuilds.Any(g => g.Id == r.GuildId)).Select(r => r.GuildId).Distinct();

            var groupsToAdd = new List<string>();
            foreach (var guildId in activeGuilds)
            {
                var guildMember = await string.Format(Constants.DiscordApi.GuildPermissionsEndpoint,guildId,userResult.Id)
                    .WithHeader("Authorization", "Bot " + _configuration["Discord:BotToken"])
                    .GetAsync().ReceiveJson<GuildUserResult>().ConfigureAwait(false);
                var validGroups = syncRules
                    .Where(s => s.SyncRemoval == false && s.GuildId == guildId &&
                                guildMember.Roles.Any(r => r == s.RoleId)).Select(s => s.MembershipGroupAlias)
                    .Distinct();
                foreach (var validGroup in validGroups)
                {
                    if (groupsToAdd.Contains(validGroup) == false)
                    {
                        groupsToAdd.Add(validGroup);
                    }
                }
            }

            // no need to delete rolls we are going to add
            foreach (var role in groupsToAdd)
            {
                if (groupsToRemove.Contains(role))
                {
                    groupsToRemove.Remove(role);
                }
            }

            if (groupsToRemove.Any())
            {
                _memberService.DissociateRoles(new[] { member.Id }, groupsToRemove.ToArray());
            }

            if (groupsToAdd.Any())
            {
                _memberService.AssignRoles(new[]{member.Id},groupsToAdd.ToArray());
            }

        }

        private Attempt<bool> CreateMember(UserResult userResult, List<GuildResult> guilds, DiscordSection settings)
        {
            if (RequiredGuildsValidated(userResult, guilds, settings) == false)
            {
                return Attempt<bool>.Fail(new FailedRequiredGuildsException());
            }

            var newMember = _memberService.CreateMember(userResult.Email, userResult.Email, userResult.Username, "member");
            UpdateUserDetails(newMember, userResult);

            _memberService.Save(newMember);
            return Attempt<bool>.Succeed();
        }

        private void UpdateUserDetails(IMember member, UserResult userResult)
        {
            member.SetValue("discordId", userResult.Id);
            member.SetValue("discordUserName", userResult.Username);
            member.SetValue("discordDiscriminator", userResult.Discriminator);
        }

        private bool RequiredGuildsValidated(UserResult userResult, List<GuildResult> guilds, DiscordSection section)
        {
            if (section.RequiredGuildIds.IsNullOrWhiteSpace())
            {
                return true;
            }

            var requiredGuildStrings = section.RequiredGuildIds.Split(",");
            requiredGuildStrings.RemoveAll(i => i.IsNullOrWhiteSpace());
            var requiredGuildIds = requiredGuildStrings.Select(id => Convert.ToUInt64(id));

            if (requiredGuildStrings.Any() == false)
            {
                return true;
            }

            return guilds.Any(g => requiredGuildIds.Any(gi => gi == g.Id));
        }
    }
}
