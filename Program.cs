using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CookScores;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            })
            .ConfigureApiBehaviorOptions(o =>
                o.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(new { error = "Invalid request" }));
        builder.Services.AddSingleton(new Db(builder.Configuration.GetConnectionString("CookScores")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:CookScores - copy appsettings.example.json or run install.sh.")));

        var app = builder.Build();

        if (args.Length > 0 && args[0] == "install")
        {
            await Install.Run(app.Services.GetRequiredService<Db>());
            return 0;
        }

        app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.CacheControl = "no-store";
            ctx.Response.Headers["Time"] = Api.Now();
            try
            {
                await next();
            }
            catch (Exception e)
            {
                var (status, error) = e is ApiError api ? (api.Status, api.Message) : (500, "Internal error");
                if (status == 500) app.Logger.LogError(e, "Request failed");
                ctx.Response.StatusCode = status;
                await ctx.Response.WriteAsJsonAsync(new { error });
            }
        });

        Api.Map(app);
        await app.RunAsync();
        return 0;
    }
}
