using System.Text.Json;

namespace CookScores;

public static class Tests
{
    public static async Task<int> Run(string baseUrl)
    {
        var uri = new Uri(baseUrl.TrimEnd('/') + "/");
        if (!uri.IsLoopback)
        {
            Console.Error.WriteLine($"Refusing to run against non-local host: {uri.Host}");
            return 1;
        }

        using var http = new HttpClient { BaseAddress = uri };
        var failed = 0;

        void Check(string name, bool ok)
        {
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}");
            if (!ok) failed++;
        }

        async Task<(int Code, JsonElement Body)> Call(string path, object? body = null)
        {
            using var res = body is null ? await http.GetAsync(path) : await http.PostAsJsonAsync(path, body);
            var text = await res.Content.ReadAsStringAsync();
            return ((int)res.StatusCode, text.Length > 0 ? JsonDocument.Parse(text).RootElement : default);
        }

        static bool Has(JsonElement e, string key) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out _);
        static string? Str(JsonElement e, string key) => Has(e, key) ? e.GetProperty(key).ToString() : null;

        var game = "test" + string.Concat(Enumerable.Range(0, 10).Select(_ => (char)Random.Shared.Next('a', 'z' + 1)));
        var pid = "p" + Guid.NewGuid().ToString("N")[..16];

        var (code, res) = await Call("api/heartbeat");
        Check("heartbeat returns version", code == 200 && Has(res, "version"));

        (code, _) = await Call("api/score", new { game, pid });
        Check("score without score is 400", code == 400);

        (code, _) = await Call("api/score", new { game, pid, score = "abc" });
        Check("non-numeric score is 400", code == 400);

        var (_, top) = await Call("api/score", new { game, pid, score = 100, level = "1", replay = "replay-data", pname = "tester" });
        Check("score is accepted", Has(top, "sid"));
        Check("top score is position 1", Str(top, "position") == "1");
        Check("nulls are omitted", !Has(top, "data"));

        var (_, second) = await Call("api/score", new { game, pid = "other" + pid, score = 50, level = "1" });
        Check("lower score is position 2", Str(second, "position") == "2");

        (code, var board) = await Call($"api/scoreboard?game={game}&level=1");
        Check("scoreboard lists both scores", code == 200 && board.GetArrayLength() == 2);
        Check("scoreboard is in order with names", Str(board[0], "sid") == Str(top, "sid") && Str(board[0], "pname") == "TESTER");
        Check("scoreboard hides pid", !Has(board[0], "pid"));

        (code, var replay) = await Call($"api/replay?game={game}&scoreId={Str(top, "sid")}");
        Check("replay is returned", code == 200 && Str(replay, "replay") == "replay-data");

        (code, _) = await Call($"api/replay?game={game}&scoreId=0");
        Check("missing replay is 404", code == 404);

        await Call("api/name", new { game, pid, pname = "renamed" });
        (_, board) = await Call($"api/scoreboard?game={game}&level=1");
        Check("name is updated", Str(board[0], "pname") == "RENAMED");

        (code, var stats) = await Call($"api/stats?game={game}&playerId={pid}");
        Check("stats returns best score", code == 200 && Str(stats[0], "best") == "100");

        (code, _) = await Call("api/nosuchmethod");
        Check("unknown method is 404", code == 404);

        Console.WriteLine(failed == 0 ? "All tests passed." : $"{failed} test(s) failed.");
        return failed == 0 ? 0 : 1;
    }
}
