using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoDiscord.Core.Controllers
{
    public class AddSyncModel
    {
        public decimal GuildId { get; set; }
        public decimal RoleId { get; set; }
        public string MembershipGroupAlias { get; set; }
        public bool SyncRemoval { get; set; }
    }
}
