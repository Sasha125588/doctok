namespace Api.Features.Resolve.Mdn;

public sealed record ResolveMdnResponse(string Status, string? TopicSlug, string? Lang, long? JobId)
{
    public static ResolveMdnResponse Ready(string topicSlug, string lang) => new ("ready", topicSlug, lang, null);

    public static ResolveMdnResponse Pending(long jobId) => new ("pending", null, null, jobId);
}
