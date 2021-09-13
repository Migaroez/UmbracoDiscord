using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class SectionComposer : IUserComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<DiscordSection>();
        }
    }
}
