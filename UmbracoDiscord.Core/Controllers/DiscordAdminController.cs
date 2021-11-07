using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;
using UmbracoDiscord.Core.Controllers.SubmitModels;
using UmbracoDiscord.Core.Models.DiscordDashboard;
using UmbracoDiscord.Core.Repositories;
using UmbracoDiscord.Core.Repositories.Dtos;
using UmbracoDiscord.Core.Services;

namespace UmbracoDiscord.Core.Controllers
{
    [IsBackOffice]
    [Authorize(Policy = Constants.Backoffice.DiscordSectionAccessPolicy)]
    public class DiscordAdminController : UmbracoApiController
    {
        private readonly DiscordRoleRepository _discordRoleRepository;
        private readonly IScopeProvider _scopeProvider;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IDiscordService _discordAuthService;

        public DiscordAdminController(DiscordRoleRepository discordRoleRepository,
            IScopeProvider scopeProvider,
            IMemberGroupService memberGroupService,
            IDiscordService discordAuthService)
        {
            _discordRoleRepository = discordRoleRepository;
            _scopeProvider = scopeProvider;
            _memberGroupService = memberGroupService;
            _discordAuthService = discordAuthService;
        }

        public async Task<IEnumerable<DiscordGuildInfo>> Guilds()
        {
            var availableGuilds = await _discordAuthService.GetAvailableGuilds();
            return availableGuilds.Select(g => new DiscordGuildInfo { Id = g.Id.ToString(CultureInfo.InvariantCulture), Name = g.Name });
        }

        public async Task<IEnumerable<DiscordRoleInfo>> Roles(ulong guildId)
        {
            var roles = await _discordAuthService.GetAvailableRolesForGuild(guildId);

            return roles.Select(r => new DiscordRoleInfo() {Id = r.Id.ToString(CultureInfo.InvariantCulture), Name = r.Name});
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
            var existingItems = _discordRoleRepository.GetAll().Where(i => i.GuildId == model.GuildId && i.RoleId == model.RoleId && i.MembershipGroupAlias == model.MembershipGroupAlias).ToList();
            if (!existingItems.Any())
            {
                var item = AddItem(model.GuildId, model.RoleId, model.MembershipGroupAlias);
                scope.Complete();
                return item;
            }

            return existingItems.First().Id;
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
        public bool RemoveMemberGroupFromRole(RemoveSyncModel model)
        {
            using var scope = _scopeProvider.CreateScope();
            var existing = _discordRoleRepository.Get(model.Id);
            if (existing == null)
            {
                return false;
            }

            if (model.SyncRemoval)
            {
                existing.SyncRemoval = true;
                _discordRoleRepository.Save(existing);
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