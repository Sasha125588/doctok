using System.Text.Json;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnApiClient(HttpClient http, MdnApiOptions options)
{
    public async Task<MdnApiDoc> FetchAsync(string lang, string slug, CancellationToken ct)
    {
        var mdnLang = ToMdnLang(lang);
        var url = $"{options.BaseUrl}/{mdnLang}/docs/{slug}/index.json";

        using var resp = await http.GetAsync(new Uri(url), ct);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var doc = json.RootElement.GetProperty("doc");

        var title = doc.GetProperty("title").GetString() ?? slug;
        var docSlug = doc.GetProperty("mdn_url").GetString() ?? slug;

        var pageType = doc.TryGetProperty("pageType", out var pageTypeEl) ? pageTypeEl.GetString() : null;

        double? popularity = null;
        if (doc.TryGetProperty("popularity", out var popEl) && popEl.ValueKind == JsonValueKind.Number)
            popularity = popEl.GetDouble();

        DateTimeOffset? sourceModifiedAt = null;
        if (doc.TryGetProperty("modified", out var modEl) && modEl.GetString() is { } modStr)
            sourceModifiedAt = DateTimeOffset.TryParse(modStr, out var dt) ? dt : null;

        var otherLocales = new List<string>();
        if (doc.TryGetProperty("other_translations", out var otEl) && otEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var t in otEl.EnumerateArray())
            {
                if (t.TryGetProperty("locale", out var locEl) && locEl.GetString() is { } loc)
                    otherLocales.Add(loc);
            }
        }

        var sections = new List<MdnApiSection>();

        if (doc.TryGetProperty("body", out var bodyEl) && bodyEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in bodyEl.EnumerateArray())
            {
                if (!item.TryGetProperty("type", out var typeEl)) continue;
                if (typeEl.GetString() != "prose") continue;

                if (!item.TryGetProperty("value", out var valEl)) continue;

                var id = valEl.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                var sectionTitle = valEl.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;
                var content = valEl.TryGetProperty("content", out var contentEl) ? contentEl.GetString() ?? "" : "";

                sections.Add(new MdnApiSection(id, sectionTitle, content));
            }
        }

        // strip leading /en-US/docs/ prefix from slug
        var slugValue = docSlug;
        var docsIdx = slugValue.IndexOf("/docs/", StringComparison.OrdinalIgnoreCase);
        if (docsIdx >= 0)
            slugValue = slugValue[(docsIdx + "/docs/".Length)..].Trim('/');

        return new MdnApiDoc(title, slugValue, sections, pageType, popularity, sourceModifiedAt, otherLocales);
    }

    private static string ToMdnLang(string lang) => lang switch
    {
        "en" => "en-US",
        "zh-cn" => "zh-CN",
        "zh-tw" => "zh-TW",
        "pt-br" => "pt-BR",
        _ => lang,
    };
}
