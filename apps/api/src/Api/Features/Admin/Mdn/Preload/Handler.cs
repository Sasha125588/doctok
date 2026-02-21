using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Sources.Mdn;

namespace Api.Features.Admin.Mdn.Preload;

public sealed class PreloadMdnHandler(MdnTreeIndex index, JobsRepository jobs)
{
  public async Task<PreloadMdnResult> Handle(PreloadMdnCommand cmd, CancellationToken ct)
  {
    var lang = (cmd.Lang ?? "en").ToLowerInvariant();
    var count = Math.Clamp(cmd.Count, 1, 200);
    var seed = cmd.Seed ?? Environment.TickCount;

    var all = await index.GetAllExternalRefsAsync(lang, ct);

    IEnumerable<string> filtered = all;
    if (!string.IsNullOrWhiteSpace(cmd.Prefix))
    {
      var pref = cmd.Prefix.Trim().Trim('/');
      filtered = all.Where(x => x.StartsWith(pref, StringComparison.OrdinalIgnoreCase));
    }

    var rnd = new Random(seed);
    var chosen = filtered.OrderBy(_ => rnd.Next()).Take(count).ToList();

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
