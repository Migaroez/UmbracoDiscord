using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Cms.Web.Common.Authorization;

namespace UmbracoDiscord.Core.Components
{
    public class DiscordSection : ISection
    {
        public string Alias => Constants.Backoffice.DiscordSection;
        public string Name => "Discord";
    }

    public class DiscordDashboard : IDashboard
    {
        public string Alias => Constants.Backoffice.DiscordDashboard;
        public string View => "/App_Plugins/Discord/Dashboard.html";
        public string[] Sections => new[] { Constants.Backoffice.DiscordSection };

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

    public class DiscordSectionComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<DiscordSection>();
            builder.BackOfficeAssets().Append<CustomPackageScript>();
            builder.Services.AddAuthorization(o => AddSecurityPolicies(o, Umbraco.Cms.Core.Constants.Security.BackOfficeAuthenticationType));
        }

        private void AddSecurityPolicies(AuthorizationOptions options, string backOfficeAuthenticationScheme)
        {
            options.AddPolicy(Constants.Backoffice.DiscordSectionAccessPolicy, policy =>
            {
                policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
                policy.Requirements.Add(new SectionRequirement(Constants.Backoffice.DiscordSection));
            });
        }
    }
}
