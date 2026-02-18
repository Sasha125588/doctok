using System.Security.Claims;
using Api.Auth;

namespace Api.Features.Session.Me;

public static class MeEndpoint
{
    public static IEndpointRouteBuilder MapMe(this IEndpointRouteBuilder app)
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

        return app;
    }
}
