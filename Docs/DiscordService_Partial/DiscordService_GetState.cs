public Guid? GetState(HttpContext httpContext, bool renew)
{
    if (httpContext?.Connection.RemoteIpAddress == null)
    {
        _logger.LogWarning("Could not issue State, httpContext or RemoteIpAddress unavailable");
        return null;
    }

    var ip = httpContext.Connection.RemoteIpAddress.ToString();
    if (renew || _stateTracker.ContainsKey(ip) == false)
    {
        return SetState(ip);
    }

    return _stateTracker[ip];
}

private Guid SetState(string ip)
{
    var state = Guid.NewGuid();
    _stateTracker[ip] = state;
    return state;
}