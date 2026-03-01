using System.ComponentModel.DataAnnotations;

namespace Api.Auth;

public sealed class SupabaseJwtOptions
{
    [Required]
    public string ProjectRef { get; init; } = default!;

    public string JwtAudience { get; init; } = "authenticated";

    public string Issuer => $"https://{ProjectRef}.supabase.co/auth/v1";

    public string JwksUrl => $"{Issuer}/.well-known/jwks.json";

    public string MetadataUrl => $"{Issuer}/.well-known/openid-configuration";
}
