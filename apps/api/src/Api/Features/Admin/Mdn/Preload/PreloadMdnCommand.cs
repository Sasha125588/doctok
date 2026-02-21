namespace Api.Features.Admin.Mdn.Preload;

public sealed record PreloadMdnCommand(string Lang, int Count, int? Seed, string? Prefix);
