private async Task<Attempt<bool>> CreateMember(UserResult userResult, List<GuildResult> guilds, DiscordSection settings)
{
    if (RequiredGuildsValidated(userResult, guilds, settings) == false)
    {
        return Attempt<bool>.Fail(new FailedRequiredGuildsException());
    }

    var newMember = _memberService.CreateMember(userResult.Email, userResult.Email, userResult.Username, "member");
    UpdateUserDetails(newMember, userResult);

    _memberService.Save(newMember);
    await SyncMemberGroups(newMember, userResult, guilds);
    return Attempt<bool>.Succeed();
}
private async Task<Attempt<bool>> UpdateMember(IMember member, UserResult userResult, List<GuildResult> guilds, DiscordSection settings)
{
    if (RequiredGuildsValidated(userResult, guilds, settings) == false)
    {
        member.IsApproved = false;
        _memberService.Save(member);
        return Attempt<bool>.Fail(new FailedRequiredGuildsException());
    }
    UpdateUserDetails(member, userResult);
    _memberService.Save(member);
    await SyncMemberGroups(member, userResult, guilds);
    return Attempt<bool>.Succeed();
}
private void UpdateUserDetails(IMember member, UserResult userResult)
{
    member.SetValue("discordId", userResult.Id);
    member.SetValue("discordUserName", userResult.Username);
    member.SetValue("discordDiscriminator", userResult.Discriminator);
}