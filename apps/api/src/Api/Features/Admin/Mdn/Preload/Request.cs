namespace Api.Features.Admin.Mdn.Preload;

public sealed record Request(string Lang, int Count, int? Seed, string? Prefix);
