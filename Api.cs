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
    }
}
