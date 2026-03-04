using Api.Extensions;
using Domain.Common;
using ErrorOr;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public sealed class Handler(TopicReadRepository topicRepo) : IHandler
{
    public async Task<ErrorOr<TopicPostsResponse>> Handle(Query query, CancellationToken ct)
    {
        var lang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");
        var items = await topicRepo.GetPosts(query.Slug, lang, query.UserId, ct);

        if (items.Count == 0)
        {
            return Error.NotFound(
                code: "Topics.NotFound",
                description: $"Topic '{query.Slug}' was not found.");
        }

        return new TopicPostsResponse(items);
    }
}
