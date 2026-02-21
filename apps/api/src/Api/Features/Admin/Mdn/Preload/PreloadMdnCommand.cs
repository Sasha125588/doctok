namespace Api.Features.Admin.Preload;

public sealed record PreloadMdnCommand(string Lang, int Count, int? Seed, string? Prefix);
