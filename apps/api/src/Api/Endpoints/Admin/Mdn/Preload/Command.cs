namespace Api.Endpoints.Admin.Mdn.Preload;

public sealed record Command(string? Lang, int? Count, int? Seed, string? Prefix);
