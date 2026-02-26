using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public class Heartbeat : Api.Endpoint
{
    [HttpGet("heartbeat")]
    public object Get() => new { time = Api.Now(), version = Api.Version };
}
