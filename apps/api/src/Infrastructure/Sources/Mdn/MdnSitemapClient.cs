using System.IO.Compression;
using System.Xml;
using Domain.Mdn;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnSitemapClient(HttpClient http)
{
  public async Task<Dictionary<string, List<string>>> GetAllSlugsAsync(CancellationToken ct = default)
  {
    var tasks = MdnConstants.KnownLocales.Select(locale => FetchLocaleAsync(locale, ct));
    var results = await Task.WhenAll(tasks);

    var index = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    foreach (var (locale, slugs) in results)
      index[locale] = slugs;

    return index;
  }

  private async Task<(string Locale, List<string> Slugs)> FetchLocaleAsync(string locale, CancellationToken ct = default)
  {
    var url = $"sitemaps/{locale}/sitemap.xml.gz";

    using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
    if (!response.IsSuccessStatusCode) return (locale, []);

    await using var compressed = await response.Content.ReadAsStreamAsync(ct);
    await using var gzip = new GZipStream(compressed, CompressionMode.Decompress);

    var slugs = await ParseSitemapXml(gzip);

    return (locale, slugs);
  }

  private static async Task<List<string>> ParseSitemapXml(Stream xml)
  {
    var slugs = new List<string>();

    var docsMarker = "/docs/";

    using var reader = XmlReader.Create(xml,
      new XmlReaderSettings
      {
        Async = true, DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true, IgnoreComments = true
      });

    while (await reader.ReadAsync())
    {
      if(reader.NodeType != XmlNodeType.Element || reader.LocalName != "loc") continue;

      var url = await reader.ReadElementContentAsStringAsync();
      var docsIdx = url.IndexOf(docsMarker, StringComparison.Ordinal);
      if(docsIdx < 0) continue;

      var slug = url[(docsIdx + docsMarker.Length)..];
      if(!string.IsNullOrEmpty(slug))
        slugs.Add(slug);
    }

    return slugs;
  }
}
