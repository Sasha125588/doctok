using System.Collections.Concurrent;
using System.Text;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnIndex(MdnArchiveManager mdnArchiveManager)
{

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _map = new();
    private int _built;


    public async Task BuildOnceAsync(CancellationToken ct = default)
    {
        if (Interlocked.Exchange(ref _built, 1) == 1) return;

        await mdnArchiveManager.EnsureFreshAsync(ct);

        var contentRepoRoot = GetSingleTopFolder(mdnArchiveManager.ContentExtractRoot);
        var translatedRepoRoot = GetSingleTopFolder(mdnArchiveManager.TranslatedExtractRoot);

        await BuildForContentAsync(contentRepoRoot, ct);
        await BuildForTranslatedAsync(translatedRepoRoot, ct);
    }

    public string? TryGetPath(string lang, string externalRef)
    {
        lang = NormalizeLang(lang);

        if (_map.TryGetValue(lang, out var dict) && dict.TryGetValue(externalRef, out var path))
            return path;

        if (lang != "en" && _map.TryGetValue("en", out var enDict) && enDict.TryGetValue(externalRef, out var enPath))
            return enPath;

        return null;
    }

    private static string GetSingleTopFolder(string extractRoot)
    {
        var dirs = Directory.GetDirectories(extractRoot);
        if (dirs.Length != 1)
            throw new InvalidOperationException($"Unexpected tarball extract structure in {extractRoot}");
        return dirs[0];
    }

    private static string NormalizeLang(string? lang)
        => (lang ?? "en").Trim().ToLowerInvariant() switch
        {
            "en" or "en-us" => "en",
            "ru" => "ru",
            _ => lang!.Trim().ToLowerInvariant()
        };

    private async Task BuildForContentAsync(string repoRoot, CancellationToken ct)
    {
        var dict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _map["en"] = dict;

        var filesRoot = Path.Combine(repoRoot, "files");
        if (!Directory.Exists(filesRoot)) return;

        foreach (var file in Directory.EnumerateFiles(filesRoot, "index.md", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            var slug = await TryReadFrontMatterSlugAsync(file, ct);
            if (slug is null) continue;
            dict.TryAdd(slug, file);
        }
    }

    private async Task BuildForTranslatedAsync(string repoRoot, CancellationToken ct)
    {
        var filesRoot = Path.Combine(repoRoot, "files");
        if (!Directory.Exists(filesRoot)) return;

        foreach (var file in Directory.EnumerateFiles(filesRoot, "index.md", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();

            var lang = InferLangFromPath(file);
            if (lang is null) continue;

            var dict = _map.GetOrAdd(lang, _ => new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            var slug = await TryReadFrontMatterSlugAsync(file, ct);
            if (slug is null) continue;
            dict.TryAdd(slug, file);
        }
    }

    private static string? InferLangFromPath(string path)
    {
        var p = path.Replace('\\', '/').ToLowerInvariant();
        var idx = p.IndexOf("/files/", StringComparison.Ordinal);
        if (idx < 0) return null;

        var rest = p[(idx + "/files/".Length)..];
        var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : null;
    }

    private static async Task<string?> TryReadFrontMatterSlugAsync(string file, CancellationToken ct)
    {
        using var fs = File.OpenRead(file);
        using var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096);

        var first = await sr.ReadLineAsync(ct);
        if (first is null || first.Trim() != "---") return null;

        while (true)
        {
            var line = await sr.ReadLineAsync(ct);
            if (line is null) return null;
            var t = line.Trim();
            if (t == "---") break;

            if (t.StartsWith("slug:", StringComparison.OrdinalIgnoreCase))
            {
                var slug = t["slug:".Length..].Trim();
                return string.IsNullOrWhiteSpace(slug) ? null : slug;
            }
        }
        return null;
    }
}
