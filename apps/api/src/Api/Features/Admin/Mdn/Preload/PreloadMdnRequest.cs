namespace Api.Features.Admin.Mdn.Preload;

public sealed record PreloadMdnRequest(string Lang, int Count, int? Seed, string? Prefix);
