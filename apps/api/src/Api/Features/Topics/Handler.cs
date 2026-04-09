using Api.Extensions;
using Domain.Common;
using ErrorOr;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public sealed class Handler(TopicReadRepository topicRepo) : IHandler
{
    public async Task<ErrorOr<TopicPostsResponse>> Handle(Query query, CancellationToken ct)
    {
        var slug = query.Slug.Trim().Trim('/');

        var lang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");
        var items = await topicRepo.GetPosts(slug, lang, query.UserId, ct);

        if (items.Count == 0)
        {
            return Error.NotFound(
                code: "Topics.NotFound",
                description: $"Topic '{slug}' was not found.");
        }

        return new TopicPostsResponse(items);
    }
}
