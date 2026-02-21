using Api.Features.Admin.Preload;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Sources.Mdn;

namespace Api.Features.Admin.Mdn.Preload;

public sealed class PreloadMdnHandler(MdnIndex index, JobsRepository jobs)
{
  public async Task<PreloadMdnResult> Handle(PreloadMdnCommand cmd, CancellationToken ct)
  {
    var lang = (cmd.Lang ?? "en").ToLowerInvariant();
    var count = Math.Clamp(cmd.Count, 1, 200);
    var seed = cmd.Seed ?? Environment.TickCount;

    await index.BuildOnceAsync(ct);

    var all = index.GetAllExternalRefs(lang);

    if (!string.IsNullOrWhiteSpace(cmd.Prefix))
    {
      var pref = cmd.Prefix.Trim().Trim('/');
      all = all.Where(x => x.StartsWith(pref, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // shuffle
    var rnd = new Random(seed);
    var chosen = all.OrderBy(_ => rnd.Next()).Take(count).ToList();

    var enqueued = 0;
    foreach (var externalRef in chosen)
    {
      var key = $"fetch_raw:mdn:{lang}:{externalRef}";
      await jobs.Enqueue(
        jobType: "fetch_raw",
        jobKey: key,
        payload: new { provider = "mdn", lang, externalRef },
        ct: ct);

      enqueued++;
    }

    return new PreloadMdnResult(enqueued, chosen);
  }
}

public sealed record PreloadMdnResult(int Enqueued, IReadOnlyList<string> Sample);
