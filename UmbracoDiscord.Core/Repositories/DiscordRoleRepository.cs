using System;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Scoping;
using UmbracoDiscord.Core.Repositories.Dtos;

namespace UmbracoDiscord.Core.Repositories
{
    public class DiscordRoleRepository : UmbracoRepository<SyncedDiscordRole>
    {
        public DiscordRoleRepository(IScopeAccessor scopeAccessor, IProfilingLogger logger)
            : base(scopeAccessor, logger)
        {
            this.tableName = Constants.Database.SyncedDiscordRoleTableName;
        }
    }
}
