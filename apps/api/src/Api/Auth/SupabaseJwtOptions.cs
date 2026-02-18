namespace Api.Auth;

public sealed class SupabaseJwtOptions
{
    public required string ProjectRef { get; init; }
    public string JwtAudience { get; init; } = "authenticated";

    public string Issuer => $"https://{ProjectRef}.supabase.co/auth/v1";
    public string JwksUrl => $"{Issuer}/.well-known/jwks.json";
    public string MetadataUrl => $"{Issuer}/.well-known/openid-configuration";
}

