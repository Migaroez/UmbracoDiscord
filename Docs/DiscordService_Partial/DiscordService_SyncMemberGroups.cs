private async Task SyncMemberGroups(IMember member, UserResult userResult, List<GuildResult> guilds)
{
    using var scope = _scopeProvider.CreateScope(autoComplete: true);

    var availableGuilds = await GetAvailableGuilds();

    var syncRules = _discordRoleRepository.GetAll().ToList();
    // we are not in the guild of the rule OR we no longer have access to the guild OR the rule has been marked as syncRemove
    var groupsToRemove = syncRules.Where(r => guilds.Any(g => g.Id == r.GuildId) == false || availableGuilds.Any(g => g.Id == r.GuildId) || r.SyncRemoval)
        .Select(r => r.MembershipGroupAlias).Distinct().ToList();

    // we need to filter out unavailable guilds else fetching the discord information in the loop below will throw an error
    var activeGuilds = syncRules.Where(r => r.SyncRemoval == false && availableGuilds.Any(g => g.Id == r.GuildId)).Select(r => r.GuildId).Distinct();

    var groupsToAdd = new List<string>();
    foreach (var guildId in activeGuilds)
    {
        var guildMember = await string.Format(Constants.DiscordApi.GuildPermissionsEndpoint, guildId, userResult.Id)
            .WithHeader("Authorization", "Bot " + _configuration["Discord:BotToken"])
            .GetAsync().ReceiveJson<GuildUserResult>().ConfigureAwait(false);
        var validGroups = syncRules
            .Where(s => s.SyncRemoval == false && s.GuildId == guildId &&
                        guildMember.Roles.Any(r => r == s.RoleId)).Select(s => s.MembershipGroupAlias)
            .Distinct();
        foreach (var validGroup in validGroups)
        {
            if (groupsToAdd.Contains(validGroup) == false)
            {
                groupsToAdd.Add(validGroup);
            }
        }
    }

    // no need to delete rolls we are going to add
    foreach (var role in groupsToAdd)
    {
        if (groupsToRemove.Contains(role))
        {
            groupsToRemove.Remove(role);
        }
    }

    if (groupsToRemove.Any())
    {
        _memberService.DissociateRoles(new[] { member.Id }, groupsToRemove.ToArray());
    }

    if (groupsToAdd.Any())
    {
        _memberService.AssignRoles(new[] { member.Id }, groupsToAdd.ToArray());
    }
}