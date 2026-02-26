using System.Security.Claims;
using Api.Auth;
using Api.Extensions;

namespace Api.Features.Session.Me;

public sealed class MeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var userId = CurrentUser.GetUserIdOrThrow(user);
            var email = CurrentUser.GetEmail(user);

            return Results.Ok(new Response(userId, email));
        }).RequireAuthorization()
        .WithTags("Session")
        .WithSummary("Returns the current user id from Supabase JWT")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
