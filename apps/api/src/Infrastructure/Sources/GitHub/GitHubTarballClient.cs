using System.Net.Http.Headers;

namespace Infrastructure.Sources.GitHub;

public sealed class GitHubTarballClient
{
    private readonly HttpClient _http;
    private readonly GitHubOptions _gitHubOptions;

    public GitHubTarballClient(HttpClient http, GitHubOptions gitHubOptions)
    {
        _http = http;
        _gitHubOptions = gitHubOptions;

        _http.DefaultRequestHeaders.UserAgent.Clear();
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_gitHubOptions.UserAgent, "1.0"));

        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _gitHubOptions.Token);
    }

    public async Task DownloadTarballAsync(
        string owner,
        string repo,
        string @ref,
        string destGzPath,
        CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destGzPath)!);

        var url = new Uri($"{_gitHubOptions.ApiBaseUrl}/repos/{owner}/{repo}/tarball/{@ref}");
        using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        await using var fs = File.Create(destGzPath);
        await resp.Content.CopyToAsync(fs, ct);
    }
}
