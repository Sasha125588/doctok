using Infrastructure.Sources.GitHub;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnArchiveManager(MdnTarballOptions opts, GitHubTarballClient tarballClient) : IDisposable
{
    private readonly SemaphoreSlim _lock = new (1, 1);
    private bool _initialized;

    public string ContentExtractRoot => Path.Combine(opts.DataRoot, "content", "extract");

    public string TranslatedExtractRoot => Path.Combine(opts.DataRoot, "translated", "extract");

    public async Task EnsureFreshAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_initialized) return;

            await EnsureRepoFreshAsync(opts.Content, "content", ContentExtractRoot, ct);
            await EnsureRepoFreshAsync(opts.Translated, "translated", TranslatedExtractRoot, ct);

            _initialized = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureRepoFreshAsync(MdnRepoOptions repo, string key, string extractRoot, CancellationToken ct)
    {
        var tarPath = Path.Combine(opts.DataRoot, key, "repo.tar.gz");
        var stampPath = Path.Combine(opts.DataRoot, key, "last_update.txt");

        var needsUpdate = true;

        if (File.Exists(tarPath) && File.Exists(stampPath) && Directory.Exists(extractRoot))
        {
            var txt = await File.ReadAllTextAsync(stampPath, ct);
            if (DateTimeOffset.TryParse(txt, out var last))
            {
                needsUpdate = (DateTimeOffset.UtcNow - last) > TimeSpan.FromHours(opts.RefreshHours);
            }
        }

        if (!needsUpdate) return;

        Directory.CreateDirectory(Path.GetDirectoryName(tarPath)!);

        await tarballClient.DownloadTarballAsync(repo.Owner, repo.Repo, repo.Ref, tarPath, ct);
        TarGzExtractor.Extract(tarPath, extractRoot);

        await File.WriteAllTextAsync(stampPath, DateTimeOffset.UtcNow.ToString("O"), ct);
    }

    public void Dispose()
    {
      _lock.Dispose();
    }
}
