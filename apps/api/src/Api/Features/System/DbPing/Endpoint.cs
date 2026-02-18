using Dapper;
using Infrastructure.Persistence.Db;

namespace Api.Features.System.DbPing;

public static class DbPingEndpoint
{
    public static IEndpointRouteBuilder MapDbPing(this IEndpointRouteBuilder app)
    {
        app.MapGet("/db/ping", async (IDbConnectionFactory dbf, CancellationToken ct = default) =>
        {
            using var db = dbf.Create();

            var one = await db.ExecuteScalarAsync<int>(
                new CommandDefinition("select 1", cancellationToken: ct));

            return Results.Ok(new { ok = one == 1 });
        }).RequireAuthorization()
        .WithTags("System")
        .WithSummary("Checks DB connectivity")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
