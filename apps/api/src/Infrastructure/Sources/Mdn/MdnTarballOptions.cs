namespace Infrastructure.Sources.Mdn;

public sealed class MdnTarballOptions
{
    public string DataRoot { get; set; } = null!;

    public int RefreshHours { get; init; } = 12;

    public required MdnRepoOptions Content { get; init; }

    public required MdnRepoOptions Translated { get; init; }
}
