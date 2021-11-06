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

    public class SectionComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<DiscordSection>();
        }
    }
}
