using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public record ScoreIn(string? Game, string? Pid, int? Score, string? Level, int? Seed, int? Time, string? Replay, string? Pname);

public class Score : Api.Endpoint
{
    [HttpPost("score")]
    public async Task<Row?> Post(ScoreIn s)
    {
        var game = UrlSafe(Need(s.Game, 400, "Requires game"));
        var pid = Need(s.Pid, 400, "Requires pid");
        if (s.Score is null) throw new ApiError(400, "Requires score");
        if (s.Replay?.Length > 1048576) throw new ApiError(400, "Replay too large");

        var sid = await Context.Db.Scalar<int>(@"
            INSERT INTO scores (game, pid, score, level, seed, time)
            VALUES (@game, @pid, @score, @level, @seed, @time) RETURNING sid",
            new { game, pid, score = s.Score, level = s.Level, seed = s.Seed ?? 0, time = s.Time ?? 0 });
        if (s.Replay is not null)
            await Context.Db.Exec("INSERT INTO replays (sid, replay) VALUES (@sid, @replay)", new { sid, replay = s.Replay });
        if (!string.IsNullOrEmpty(s.Pname))
            await Name.Set(Context, pid, s.Pname);
        return await Entry(game, sid);
    }
}
