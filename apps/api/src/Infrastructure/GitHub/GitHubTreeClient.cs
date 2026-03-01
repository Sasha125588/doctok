using System.Text.Json;

namespace Infrastructure.GitHub;

public sealed class GitHubTreeClient(HttpClient http)
{
    public async Task<IReadOnlyList<string>> GetFilePathsAsync(
        string owner,
        string repo,
        string @ref,
        CancellationToken ct)
    {
        var encodedRef = Uri.EscapeDataString(@ref);
        var url = $"/repos/{owner}/{repo}/git/trees/{encodedRef}?recursive=1";
        var requestUri = new Uri(url, UriKind.Relative);

        using var resp = await http.GetAsync(requestUri, ct);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var paths = new List<string>();

        if (json.RootElement.TryGetProperty("tree", out var tree) && tree.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in tree.EnumerateArray())
            {
                if (item.TryGetProperty("path", out var pathEl) && pathEl.ValueKind == JsonValueKind.String)
                {
                    var path = pathEl.GetString();
                    if (path != null)
                        paths.Add(path);
                }
            }
        }

        return paths;
    }
}
