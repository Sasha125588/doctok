namespace Domain.Sources;

public static class ExternalRefHelpers
{
    public static string Normalize(string? externalRef)
        => (externalRef ?? string.Empty)
            .Trim()
            .TrimStart('/')
            .Replace('\\', '/')
            .ToLowerInvariant();
}
