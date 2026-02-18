namespace Api.Auth;

using System.Security.Claims;

public static class CurrentUser
{
    public static Guid GetUserIdOrThrow(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : throw new UnauthorizedAccessException("Missing or invalid 'sub' claim");
    }

    public static string? GetEmail(ClaimsPrincipal user) => 
        user.FindFirstValue(ClaimTypes.Email) 
        ?? user.FindFirstValue("email");
}