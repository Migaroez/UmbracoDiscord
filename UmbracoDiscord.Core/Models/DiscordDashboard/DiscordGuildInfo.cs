namespace UmbracoDiscord.Core.Models.DiscordDashboard
{
    public class DiscordRoleInfo
    {
        // this needs to be a string because JS cuts off ulong numbers
        public string Id { get; set; }
        public string Name { get; set; }
    }
}