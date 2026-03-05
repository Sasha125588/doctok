using Api.Extensions;
using Dapper;
using Infrastructure.Persistence.Db;

namespace Api.Features.System.DbPing;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/db/ping", async (IDbConnectionFactory dbf, CancellationToken ct = default) =>
        {
            using var db = dbf.Create();

            var one = await db.ExecuteScalarAsync<int>(
                new CommandDefinition("select 1", cancellationToken: ct));

            return Results.Ok(new SystemDbPingResponse(one == 1));
        }).RequireAuthorization()
        .WithTags("System")
        .WithSummary("Checks DB connectivity")
        .WithName("SystemDbPing")
        .Produces<SystemDbPingResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
