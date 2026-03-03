using Domain.Common;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Sources.Mdn;

namespace Api.Features.Admin.Mdn.Preload;

public sealed class Handler(MdnTreeIndex index, JobsRepository jobs)
{
    public async Task<PreloadMdnResponse> Handle(Command cmd, CancellationToken ct)
    {
        var lang = LanguageHelpers.NormalizeLang(cmd.Lang ?? "en");
        var count = Math.Clamp(cmd.Count ?? 5, 1, 200);
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

        foreach (var externalRef in chosen)
        {
            var key = $"{JobTypes.FetchRaw}:{SourceCodes.Mdn}:{lang}:{externalRef}";
            await jobs.Enqueue(
                jobType: JobTypes.FetchRaw,
                jobKey: key,
                payload: new { provider = SourceCodes.Mdn, lang, externalRef },
                ct: ct);
        }

        return new PreloadMdnResponse(chosen);
    }
}
