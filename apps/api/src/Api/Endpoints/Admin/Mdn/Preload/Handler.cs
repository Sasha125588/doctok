using Api.Extensions;
using Domain.Jobs;
using Domain.Shared;
using Domain.Sources;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Sources.Mdn;

namespace Api.Endpoints.Admin.Mdn.Preload;

public sealed class Handler(MdnSitemapIndex index, JobsRepository jobs) : IHandler
{
  public async Task<PreloadMdnResponse> Handle(Command cmd, CancellationToken ct)
  {
    var lang = LanguageHelpers.NormalizeLang(cmd.Lang);
    var count = Math.Clamp(cmd.Count ?? 5, 1, 100);
    var seed = cmd.Seed ?? Environment.TickCount;

    var all = await index.GetAllSlugsAsync(lang, ct);

    IEnumerable<string> filtered = all;
    if (!string.IsNullOrWhiteSpace(cmd.Prefix))
    {
      var pref = cmd.Prefix.Trim().Trim('/');
      filtered = all.Where(x => x.StartsWith(pref, StringComparison.OrdinalIgnoreCase));
    }

    var rnd = new Random(seed);
    var chosen = filtered.OrderBy(_ => rnd.Next()).Take(count).ToList();

    foreach (var externalRef in chosen)
    {
      var key = $"{JobTypes.FetchRaw}:{SourceCodes.Mdn}:{lang}:{externalRef}";
      await jobs.Enqueue(
        jobType: JobTypes.FetchRaw,
        jobKey: key,
        payload: new { provider = SourceCodes.Mdn, lang, externalRef },
        ct);
    }

    return new PreloadMdnResponse(chosen.Select(slug => SourceCodes.Mdn + "/" + slug).ToArray());
  }
}
