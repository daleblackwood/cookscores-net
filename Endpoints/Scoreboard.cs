using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public class Scoreboard : Api.Endpoint
{
    [HttpGet("scoreboard")]
    public async Task<IEnumerable<Row>> Get(string? game, string? level, int? sid, int? seed, int? target)
    {
        game = UrlSafe(Need(game, 404, "No game"));
        Need(level, 404, "No level");
        long position = target ?? 1;
        if (sid is not null && await Entry(game, sid.Value) is { } entry)
            position = (long)entry["position"];

        var rows = await Context.Db.All(@"
            SELECT * FROM (
                SELECT s.*, p.pname, ROW_NUMBER() OVER (ORDER BY s.score DESC, s.sid) AS position
                FROM scores s
                LEFT JOIN players p ON p.pid = s.pid
                WHERE s.game = @game AND s.level = @level
                AND (@seed::int IS NULL OR s.seed = @seed)
            ) ranked
            ORDER BY ABS(position - @position), position
            LIMIT 10", new { game, level, seed, position });
        rows.ForEach(r => r.Remove("pid"));
        return rows.OrderBy(r => (long)r["position"]);
    }
}
