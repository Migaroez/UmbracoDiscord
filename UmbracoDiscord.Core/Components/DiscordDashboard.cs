using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Dashboards;

namespace UmbracoDiscord.Core.Components
{
    public class DiscordDashboard : IDashboard
    {
        public string Alias => "discordDashboard";
        public string View => "/App_Plugins/Discord/Dashboard.html";
        public string[] Sections => new[] { "discordsection" };

        public IAccessRule[] AccessRules => new IAccessRule[]
        {
            new AccessRule {Type = AccessRuleType.Grant, Value = Umbraco.Cms.Core.Constants.Security.AdminGroupAlias}
        };
    }
}
