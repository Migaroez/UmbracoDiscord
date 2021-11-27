public bool IsValidState(HttpContext httpContext)
{
    if (httpContext?.Connection.RemoteIpAddress == null)
    {
        _logger.LogWarning("Could not validate State, httpContext or RemoteIpAddress unavailable");
        return false;
    }

    var ip = httpContext.Connection.RemoteIpAddress.ToString();

    return _stateTracker.ContainsKey(ip) && _stateTracker[ip].ToString() == (string)httpContext.Request.Query["state"];
}