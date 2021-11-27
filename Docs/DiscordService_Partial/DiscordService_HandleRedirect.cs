public async Task<Attempt<string>> HandleRedirect(HttpContext httpContext, DiscordSection settings)
{
    // get bearer token from redirect code
    var bearerTokenResult = await ExchangeRedirectCode((string)httpContext.Request.Query["code"], settings);

    // get user
    var userResult = await GetUser(bearerTokenResult.AccessToken);
    if (userResult.Verified == false)
    {
        return Attempt<string>.Fail(new EmailUnverifiedException());
    }

    // get guilds and check they are still a member of the guild specified
    var guildResult = await GetUserGuilds(bearerTokenResult.AccessToken);

    // if userId exists update them
    var existingMember = _memberService.GetByEmail(userResult.Email);
    if (existingMember != null)
    {
        var updateMemberResult = await UpdateMember(existingMember, userResult, guildResult, settings).ConfigureAwait(false);
        if (updateMemberResult.Success == false)
        {
            return Attempt<string>.Fail(updateMemberResult.Exception);
        }
        return Attempt<string>.Succeed(userResult.Email);
    }

    // if no member exists create them
    var newMemberResult = await CreateMember(userResult, guildResult, settings);
    if (newMemberResult.Success == false)
    {
        return Attempt<string>.Fail(newMemberResult.Exception);
    }
    return Attempt<string>.Succeed(userResult.Email);
}