using Domain.Common;
using Infrastructure.Sources.GitHub;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnTreeIndex(GitHubTreeClient treeClient) : IDisposable
{
    private const string MdnOwner = "mdn";
    private const string MdnContentRepo = "content";
    private const string MdnTranslatedRepo = "translated-content";
    private const string MdnRef = "main";

    private readonly SemaphoreSlim _lock = new (1, 1);

    // key = мова, значення(List<string>) = slugs  ( чому це називається "slug"? )
    // "en" -> ["web/API/Fetch_API", ...]
    private Task<Dictionary<string, List<string>>>? _buildTask;

    private DateTimeOffset _builtAt = DateTimeOffset.MinValue;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(12);

    public void Dispose() => _lock.Dispose();

    public async Task<IReadOnlyList<string>> GetAllExternalRefsAsync(string lang, CancellationToken ct)
    {
        lang = LanguageHelpers.NormalizeLang(lang);

        var cache = await GetOrBuildAsync(ct);

        if (cache.TryGetValue(lang, out var list))
            return list;

        return [];
    }

    private async Task<Dictionary<string, List<string>>> GetOrBuildAsync(CancellationToken ct)
    {
        var task = _buildTask;
        if (task != null && !task.IsFaulted && !task.IsCanceled && DateTimeOffset.UtcNow - _builtAt < CacheTtl)
            return await task.WaitAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            task = _buildTask;
            if (task != null && !task.IsFaulted && !task.IsCanceled && DateTimeOffset.UtcNow - _builtAt < CacheTtl)
                return await task.WaitAsync(ct);

            _buildTask = BuildIndexAsync(CancellationToken.None);
            _builtAt = DateTimeOffset.UtcNow;

            return await _buildTask.WaitAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<Dictionary<string, List<string>>> BuildIndexAsync(CancellationToken ct)
    {
        var contentTask = treeClient.GetFilePathsAsync(MdnOwner, MdnContentRepo, MdnRef, ct);
        var translatedTask = treeClient.GetFilePathsAsync(MdnOwner, MdnTranslatedRepo, MdnRef, ct);

        var content = await contentTask;
        var translated = await translatedTask;

        var index = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in content.Concat(translated))
        {
            if (!path.EndsWith("/index.md", StringComparison.OrdinalIgnoreCase)) continue;

            var slug = PathToSlug(path);
            if (slug is null) continue;

            var lang = InferLang(path);

            if (!index.TryGetValue(lang, out var list))
            {
                list = new List<string>();
                index[lang] = list;
            }

            list.Add(slug);
        }

        return index;
    }

    private static string InferLang(string path)
    {
        var normalized = path.Replace('\\', '/');
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || !string.Equals(parts[0], "files", StringComparison.OrdinalIgnoreCase))
            return "en";

        var rawLang = parts[1].ToLowerInvariant();
        return rawLang switch
        {
            "en-us" => "en",
            _ => rawLang
        };
    }

    private static string? PathToSlug(string path)
    {
        var normalized = path.Replace('\\', '/');
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4) return null;
        if (!string.Equals(parts[0], "files", StringComparison.OrdinalIgnoreCase)) return null;
        if (!string.Equals(parts[^1], "index.md", StringComparison.OrdinalIgnoreCase)) return null;

        var slugParts = parts[2..^1];
        return string.Join("/", slugParts);
    }
}
