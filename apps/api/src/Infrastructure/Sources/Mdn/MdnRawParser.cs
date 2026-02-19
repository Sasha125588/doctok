using System.Text;

namespace Infrastructure.Sources.Mdn;

public sealed record MdnParsedDoc(string? Title, string ExternalRef, string Content);

public sealed class MdnRawParser
{
    public async Task<MdnParsedDoc> ParseAsync(string filePath, CancellationToken ct)
    {
        var text = await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct);

        string? title = null;
        string? slug = null;

        if (text.StartsWith("---"))
        {
            var end = text.IndexOf("\n---", 3, StringComparison.Ordinal);
            if (end > 0)
            {
                var fm = text.Substring(3, end - 3);
                foreach (var rawLine in fm.Split('\n'))
                {
                    var line = rawLine.Trim();
                    if (line.StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                        title = line["title:".Length..].Trim().Trim('"');
                    else if (line.StartsWith("slug:", StringComparison.OrdinalIgnoreCase))
                        slug = line["slug:".Length..].Trim();
                }

                var bodyStart = end + "\n---".Length;
                text = text[bodyStart..].TrimStart();
            }
        }

        if (string.IsNullOrWhiteSpace(slug))
            throw new InvalidOperationException($"MDN doc missing slug in front matter: {filePath}");

        return new MdnParsedDoc(title, slug!, text);
    }
}