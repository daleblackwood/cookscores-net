using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public class Test : Api.Endpoint
{
    [HttpGet("test")]
    public Task<List<Row>> Get() =>
        IsDebug(Context.Http) ? Context.Db.All("SELECT * FROM scores") : throw new ApiError(404, "Method not found");

    static bool IsDebug(HttpContext http) =>
        Environment.GetEnvironmentVariable("COOKSCORES_DEBUG") == "1"
        || (http.Connection.RemoteIpAddress is { } ip && IPAddress.IsLoopback(ip));
}
