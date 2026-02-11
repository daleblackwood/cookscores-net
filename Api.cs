using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public class ApiError : Exception
{
    public ApiError(int status, string message) : base(message) => Status = status;

    public int Status { get; }
}

public static class Api
{
    public static string Now() => DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:sszzz");

    public static void Map(WebApplication app)
    {
        app.MapControllers();
        app.MapFallback(() => Results.Json(new { error = "Method not found" }, statusCode: 404));
    }

    public class Context
    {
        public Context(HttpContext http, Db db)
        {
            Http = http;
            Db = db;
        }

        public HttpContext Http { get; }
        public Db Db { get; }
    }

    [ApiController, Route("api")]
    public abstract class Endpoint : ControllerBase
    {
        Context? context;

        protected Context Context => context ??= new Context(HttpContext, HttpContext.RequestServices.GetRequiredService<Db>());

        static readonly Regex Unsafe = new("[^a-zA-Z_]", RegexOptions.Compiled);

        protected static string UrlSafe(string? s) => Unsafe.Replace(s ?? "", "").ToLowerInvariant();

        protected static string Need(string? value, int status, string error) =>
            string.IsNullOrEmpty(value) ? throw new ApiError(status, error) : value;

        protected Task<Row?> Entry(string game, int sid) => Context.Db.One(@"
            SELECT s.*, p.pname, (
                SELECT COUNT(*) + 1 FROM scores h
                WHERE h.game = s.game
                AND h.level IS NOT DISTINCT FROM s.level
                AND (h.score > s.score OR (h.score = s.score AND h.sid < s.sid))
            ) AS position
            FROM scores s
            LEFT JOIN players p ON p.pid = s.pid
            WHERE s.sid = @sid AND s.game = @game", new { sid, game });
    }
}
