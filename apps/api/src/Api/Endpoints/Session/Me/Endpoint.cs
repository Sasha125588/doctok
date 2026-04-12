using System.Security.Claims;
using Api.Auth;
using Api.Extensions;

namespace Api.Endpoints.Session.Me;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var userId = CurrentUser.GetUserIdOrThrow(user);
            var email = CurrentUser.GetEmail(user);

            return Results.Ok(new SessionMeResponse(userId, email));
        }).RequireAuthorization()
        .WithTags("Session")
        .WithSummary("Returns the current user id from Supabase JWT")
        .WithName("SessionMeGet")
        .Produces<SessionMeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
