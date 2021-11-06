namespace UmbracoDiscord.Core.Controllers.SubmitModels
{
    public class AddSyncModel
    {
        public decimal GuildId { get; set; }
        public decimal RoleId { get; set; }
        public string MembershipGroupAlias { get; set; }
        public bool SyncRemoval { get; set; }
    }
}
