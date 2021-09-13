using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbracoDiscord.Core.Repositories.Dtos
{
    [TableName(Constants.Database.SyncedDiscordRoleTableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class SyncedDiscordRole
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("GuildId")]
        public decimal GuildId { get; set; }

        [Column("RoleId")]
        public decimal RoleId { get; set; }

        [Column("MembershipGroupAlias")]
        public string MembershipGroupAlias { get; set; }

        [Column("SyncRemoval")]
        public bool SyncRemoval { get; set; }

    }
}
