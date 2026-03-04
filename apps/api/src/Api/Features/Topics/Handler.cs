using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public sealed class Handler(TopicReadRepository topicRepo) : IHandler
{
    public async Task<TopicPostsResponse?> Handle(Query query, CancellationToken ct)
    {
        var lang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");
        var items = await topicRepo.GetPosts(query.Slug, lang, query.UserId, ct);

        if (items.Count == 0)
        {
            return null;
        }

        return new TopicPostsResponse(items);
    }
}
