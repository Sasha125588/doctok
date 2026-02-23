namespace Api.Features.Admin.Mdn.Preload;

public sealed record Response(int Enqueued, IReadOnlyList<string> Sample);
