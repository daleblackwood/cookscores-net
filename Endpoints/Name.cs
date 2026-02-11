using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public record NameIn(string? Game, string? Pid, string? Pname);

public class Name : Api.Endpoint
{
    [HttpPost("name")]
    public async Task<object> Post(NameIn n)
    {
        Need(UrlSafe(n.Game), 404, "No game");
        var pid = Need(n.Pid, 401, "No player id");
        var pname = await Set(Context, pid, Need(n.Pname, 401, "No player name"));
        return new { pid, pname };
    }

    internal static async Task<string> Set(Api.Context ctx, string pid, string pname)
    {
        pname = pname.ToUpperInvariant();
        await ctx.Db.Exec(@"
            INSERT INTO players (pid, pname) VALUES (@pid, @pname)
            ON CONFLICT (pid) DO UPDATE SET pname = excluded.pname", new { pid, pname });
        return pname;
    }
}
