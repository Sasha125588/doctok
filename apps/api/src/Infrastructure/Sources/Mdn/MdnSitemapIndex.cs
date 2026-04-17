using Domain.Shared;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnSitemapIndex(MdnSitemapClient sitemapClient): IDisposable
{
      private readonly SemaphoreSlim _lock = new (1, 1);

      // key = мова, значення(List<string>) = slugs
      // "en" -> ["web/API/Fetch_API", ...]
      private Task<Dictionary<string, List<string>>>? _buildTask;

      private DateTimeOffset _builtAt = DateTimeOffset.MinValue;
      private static readonly TimeSpan _cacheTtl = TimeSpan.FromHours(12);

      public void Dispose() => _lock.Dispose();

      public async Task<IReadOnlyList<string>> GetAllSlugsAsync(string lang, CancellationToken ct)
    {
        lang = LanguageHelpers.ToMdnLang(lang);

        var cache = await GetOrBuildAsync(ct);

        return cache.TryGetValue(lang, out var list) ? list : [];
    }

      private async Task<Dictionary<string, List<string>>> GetOrBuildAsync(CancellationToken ct)
    {
        var task = _buildTask;
        if (task is { IsFaulted: false, IsCanceled: false } && DateTimeOffset.UtcNow - _builtAt < _cacheTtl)
            return await task.WaitAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            task = _buildTask;
            if (task is { IsFaulted: false, IsCanceled: false } && DateTimeOffset.UtcNow - _builtAt < _cacheTtl)
                return await task.WaitAsync(ct);

            _buildTask = sitemapClient.GetAllSlugsAsync(CancellationToken.None);
            _builtAt = DateTimeOffset.UtcNow;

            return await _buildTask.WaitAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }
}
