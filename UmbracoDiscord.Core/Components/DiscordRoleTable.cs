using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using UmbracoDiscord.Core.Repositories;
using UmbracoDiscord.Core.Repositories.Dtos;

namespace UmbracoDiscord.Core.Components
{
    public class DiscordRoleComposer : IUserComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components().Append<DiscordRoleTableComponent>();
            builder.Services.AddScoped<DiscordRoleRepository>();
        }
    }

    public class DiscordRoleTableComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;

        public DiscordRoleTableComponent(
            IScopeProvider scopeProvider,
            IMigrationPlanExecutor migrationPlanExecutor,
            IKeyValueService keyValueService,
            IRuntimeState runtimeState)
        {
            _scopeProvider = scopeProvider;
            _migrationPlanExecutor = migrationPlanExecutor;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }

            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("SyncedDiscordRole");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty)
                .To<AddSyncedDiscordRoleTable>("syncedDiscordRole-db");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_migrationPlanExecutor,_scopeProvider,_keyValueService);
        }

        public class AddSyncedDiscordRoleTable : MigrationBase
        {
            public AddSyncedDiscordRoleTable(IMigrationContext context) : base(context) { }

            protected override void Migrate()
            {
                Logger.LogDebug("Running migration {MigrationStep}", "AddSyncedDiscordRoleTable");

                // Lots of methods available in the MigrationBase class - discover with this.
                if (TableExists(Constants.Database.SyncedDiscordRoleTableName) == false)
                {
                    Create.Table<SyncedDiscordRole>().Do();
                }
                else
                {
                    Logger.LogDebug("The database table {DbTable} already exists, skipping", Constants.Database.SyncedDiscordRoleTableName);
                }
            }
        }

        public void Terminate()
        {
        }
    }
}
