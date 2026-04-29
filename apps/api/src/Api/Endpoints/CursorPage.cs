using Domain.Shared;

namespace Api.Endpoints;

public sealed record CursorPage<TItem>(IReadOnlyList<TItem> Items, string? NextCursor);

public static class CursorPage
{
  public static CursorPage<TItem> From<TItem, TCursor>(
    IReadOnlyList<TItem> page,
    int take,
    Func<TItem, TCursor> toCursor)
  {
    var hasNextPage = page.Count > take;
    var items = hasNextPage ? page.Take(take).ToList() : page;
    var nextCursor = hasNextPage
      ? CursorCodec.Encode(toCursor(items[^1]))
      : null;

    return new CursorPage<TItem>(items, nextCursor);
  }
}
