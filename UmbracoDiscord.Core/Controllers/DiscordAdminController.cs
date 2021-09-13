using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Controllers;
using UmbracoDiscord.Core.Components;
using UmbracoDiscord.Core.Models.DiscordApi;
using UmbracoDiscord.Core.Models.DiscordDashboard;
using UmbracoDiscord.Core.Repositories;
using UmbracoDiscord.Core.Repositories.Dtos;

namespace UmbracoDiscord.Core.Controllers
{
    [IsBackOffice]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    public class DiscordAdminController : UmbracoApiController
    {
        private readonly IConfiguration _configuration;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly DiscordRoleRepository _discordRoleRepository;
        private readonly IScopeProvider _scopeProvider;
        private readonly IMemberGroupService _memberGroupService;

        public DiscordAdminController(IConfiguration configuration, 
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            DiscordRoleRepository discordRoleRepository,
            IScopeProvider scopeProvider,
            IMemberGroupService memberGroupService)
        {
            _configuration = configuration;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _discordRoleRepository = discordRoleRepository;
            _scopeProvider = scopeProvider;
            _memberGroupService = memberGroupService;
        }

        public async Task<IActionResult> Guilds()
        {
            if (_backOfficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess("discordSection",
                _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser) == false)
            {
                return Unauthorized();
            }

            var availbableGuilds = await Constants.DiscordApi.GuildEndpoint.WithHeader("Authorization", "Bot " + _configuration["Discord:Token"])
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);
            return Ok(availbableGuilds.Select(g => new DiscordGuildInfo { Id = g.Id.ToString(), Name = g.Name }).ToList());
        }

        public async Task<IActionResult> Roles(ulong guildId)
        {
            if (_backOfficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess("discordSection",
                _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser) == false)
            {
                return Unauthorized();
            }

            var roles = await string.Format(Constants.DiscordApi.GuildRolesEndpoint,guildId).WithHeader("Authorization", "Bot " + _configuration["Discord:Token"])
                .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);

            return Ok(roles.Select(r => new DiscordRoleInfo() {Id = r.Id.ToString(), Name = r.Name}));
        }

        public IEnumerable<SyncedDiscordRole> Syncs(ulong guildId, ulong roleId)
        {
            using (_scopeProvider.CreateScope(autoComplete:true))
            {
                return _discordRoleRepository.GetAll().Where(s => s.GuildId == guildId && s.RoleId == roleId);
            }
        }

        public IEnumerable<string> MemberShipGroups()
        {
            return _memberGroupService.GetAll().Select(g => g.Name);
        }

        [HttpPost]
        public int RegisterRoleToMemberGroup([FromBody] AddSyncModel model)
        {
            using var scope = _scopeProvider.CreateScope();

            // this is not optimal, but we don't expect to get enough items for this to be an issue
            var existingItems = _discordRoleRepository.GetAll().Where(i => i.GuildId == model.GuildId && i.RoleId == model.RoleId).ToList();
            if (!existingItems.Any())
            {
                var item = AddItem(model.GuildId, model.RoleId, model.MembershipGroupAlias);
                scope.Complete();
                return item;
            }

            var activeItem = existingItems.FirstOrDefault(i => i.SyncRemoval == false);
            if (activeItem == null)
            {
                var item = AddItem(model.GuildId, model.RoleId, model.MembershipGroupAlias);
                scope.Complete();
                return item;
            }

            if (model.SyncRemoval)
            {
                activeItem.SyncRemoval = true;
                _discordRoleRepository.Save(activeItem);
            }
            else
            {
                _discordRoleRepository.Delete(activeItem.Id);
            }

            var newItem = AddItem(model.GuildId, model.RoleId, model.MembershipGroupAlias);
            scope.Complete();
            return newItem;
        }

        private int AddItem(decimal guildId, decimal roleId, string membershipGroupAlias)
        {
            var newItem = new SyncedDiscordRole
            {
                GuildId = guildId,
                RoleId = roleId,
                MembershipGroupAlias = membershipGroupAlias,
            };
            _discordRoleRepository.Save(newItem);
            return newItem.Id;
        }

        [HttpPost]
        public bool RemoveMemberGroupFromRole(int id, bool syncRemoval)
        {
            using var scope = _scopeProvider.CreateScope();
            var existing = _discordRoleRepository.Get(id);
            if (existing == null)
            {
                return false;
            }

            if (syncRemoval)
            {
                existing.SyncRemoval = true;
            }
            else
            {
                _discordRoleRepository.Delete(existing.Id);
            }

            scope.Complete();
            return true;
        }

        
    }
}