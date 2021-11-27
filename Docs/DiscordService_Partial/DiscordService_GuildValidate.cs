private bool RequiredGuildsValidated(UserResult userResult, List<GuildResult> guilds, DiscordSection section)
{
    if (section.RequiredGuildIds.IsNullOrWhiteSpace())
    {
        return true;
    }

    var requiredGuildStrings = section.RequiredGuildIds.Split(",");
    requiredGuildStrings.RemoveAll(i => i.IsNullOrWhiteSpace());
    var requiredGuildIds = requiredGuildStrings.Select(id => Convert.ToUInt64(id));

    if (requiredGuildStrings.Any() == false)
    {
        return true;
    }

    return guilds.Any(g => requiredGuildIds.Any(gi => gi == g.Id));
}