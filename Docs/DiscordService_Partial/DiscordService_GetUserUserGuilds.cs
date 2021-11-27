private async Task<UserResult> GetUser(string accessToken)
{
    return await Constants.DiscordApi.UserEndpoint.WithOAuthBearerToken(accessToken)
        .GetAsync().ReceiveJson<UserResult>().ConfigureAwait(false);
}

private async Task<List<GuildResult>> GetUserGuilds(string accessToken)
{
    return await Constants.DiscordApi.GuildEndpoint.
        WithOAuthBearerToken(accessToken)
        .GetAsync().ReceiveJson<List<GuildResult>>().ConfigureAwait(false);
}