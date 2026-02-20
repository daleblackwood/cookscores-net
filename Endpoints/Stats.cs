using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public class Stats : Api.Endpoint
{
    [HttpGet("stats")]
    public Task<List<Row>> Get(string? game, string? playerId) => Context.Db.All(@"
        SELECT level, MAX(score) AS best, COUNT(*) AS plays
        FROM scores
        WHERE game = @game AND pid = @pid
        GROUP BY level", new { game = UrlSafe(Need(game, 404, "No game")), pid = Need(playerId, 404, "No player") });
}
