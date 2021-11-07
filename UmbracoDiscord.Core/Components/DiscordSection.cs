using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Sections;

namespace UmbracoDiscord.Core.Components
{
    public class DiscordSection : ISection
    {
        public string Alias => "discordSection";
        public string Name => "Discord";
    }

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

    // This will probably be changed in the future so it can be done more easily in the composer
    // See https://github.com/umbraco/Umbraco-CMS/pull/11308
    public class CustomPackageScript : JavaScriptFile
    {
        public CustomPackageScript() : base("/App_Plugins/Discord/Dashboard.controller.js") { }
    }

    public class SectionComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<DiscordSection>();
            builder.BackOfficeAssets().Append<CustomPackageScript>();
        }
    }
}
