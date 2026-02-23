namespace Api.Features.Admin.Mdn.Preload;

public sealed record Command(string? Lang, int? Count, int? Seed, string? Prefix);
