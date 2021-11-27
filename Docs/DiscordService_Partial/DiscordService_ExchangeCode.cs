private async Task<BearerTokenResult> ExchangeRedirectCode(string code, DiscordSection settings)
{
    return await Constants.DiscordApi.TokenEndpoint.PostUrlEncodedAsync(new
    {
        client_id = _configuration["Discord:ClientId"],
        client_secret = _configuration["Discord:ClientSecret"],
        grant_type = "authorization_code",
        code = code,
        redirect_uri = settings.FirstChild<DiscordLoginRedirectHandler>().Url(mode: UrlMode.Absolute)
    }).ReceiveJson<BearerTokenResult>().ConfigureAwait(false);
}