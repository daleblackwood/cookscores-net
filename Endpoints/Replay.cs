using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public class Replay : Api.Endpoint
{
    [HttpGet("replay")]
    public async Task<Row> Get(string? game, int? scoreId)
    {
        game = UrlSafe(Need(game, 404, "No game"));
        if (scoreId is null) throw new ApiError(401, "No score id");
        return await Context.Db.One(@"
            SELECT r.sid, r.replay, s.game, s.score, s.level, s.seed, s.time, s.ts, s.data
            FROM replays r
            JOIN scores s ON s.sid = r.sid
            WHERE r.sid = @sid AND s.game = @game", new { sid = scoreId, game }) ?? throw new ApiError(404, "Replay not found");
    }
}
