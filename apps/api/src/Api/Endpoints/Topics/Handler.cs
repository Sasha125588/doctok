using Api.Extensions;
using Domain.Posts;
using Domain.Shared;
using Domain.Sources;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Topics;

public sealed class Handler(TopicsRepository topicRepo) : IHandler
{
    public async Task<ErrorOr<TopicPostsResponse>> Handle(Query query, CancellationToken ct)
    {
        var take = Math.Clamp(query.Limit ?? 5, 1, 50);
        var slug = ExternalRefHelpers.Normalize(query.Slug);
        var lang = LanguageHelpers.NormalizeLang(query.Lang);

        var cursor = CursorCodec.Decode<TopicPostsCursor>(query.Cursor);

        var page = await topicRepo.GetPosts(cursor, slug, lang, query.UserId, take + 1, ct);

        if (page.Count == 0)
        {
            return Error.NotFound(
                code: "Topics.NotFound",
                description: $"Topic '{slug}' was not found.");
        }

        var hasNextPage = page.Count > take;
        var items = hasNextPage ? page.Take(take).ToList() : page;

        var nextCursor = hasNextPage
          ? CursorCodec.Encode(ToCursor(items[^1]))
          : null;

        return new TopicPostsResponse(items, nextCursor);
    }

    private static TopicPostsCursor ToCursor(TopicPostView item)
      => new(GetKindRank(item.Kind), item.Position, item.Id);

    private static int GetKindRank(string kind)
      => kind switch
      {
          "summary" => 0,
          "concept" => 1,
          "example" => 2,
          "tip" => 3,
          _ => 4,
      };
}
