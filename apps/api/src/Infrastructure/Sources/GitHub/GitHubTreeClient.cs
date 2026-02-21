using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.Sources.GitHub;

public sealed class GitHubTreeClient
{
    private readonly HttpClient _http;
    private readonly GitHubOptions _options;

    public GitHubTreeClient(HttpClient http, GitHubOptions options)
    {
        _http = http;
        _options = options;

        _http.DefaultRequestHeaders.UserAgent.Clear();
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_options.UserAgent, "1.0"));

        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
    }

    public async Task<IReadOnlyList<string>> GetFilePathsAsync(
        string owner,
        string repo,
        string @ref,
        CancellationToken ct)
    {
        var url = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{@ref}?recursive=1";

        using var resp = await _http.GetAsync(new Uri(url), ct);
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
