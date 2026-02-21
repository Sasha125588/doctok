namespace Api.Features.Resolve.Mdn;

public sealed record ResolveMdnResult(string Status, string? TopicSlug, string? Lang, long? JobId)
{
    public static ResolveMdnResult Ready(string topicSlug, string lang) => new ("ready",  topicSlug, lang, null);
    public static ResolveMdnResult Pending(long jobId) => new ("pending", null, null, jobId);
}
