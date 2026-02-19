namespace Infrastructure.Sources.Mdn;

public sealed class MdnRepoOptions
{
    public required string Owner { get; init; }        // "mdn"
    public required string Repo { get; init; }         // "content" or "translated-content"
    public string Ref { get; init; } = "main";
}