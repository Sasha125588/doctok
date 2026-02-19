using System.Net.Http.Headers;

namespace Infrastructure.Sources.GitHub;

public sealed class GitHubTarballClient
{
    private readonly HttpClient _http;
    private readonly GitHubOptions _opt;

    public GitHubTarballClient(HttpClient http, GitHubOptions opt)
    {
        _http = http;
        _opt = opt;

        _http.DefaultRequestHeaders.UserAgent.Clear();
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_opt.UserAgent, "1.0"));

        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _opt.Token);
    }

    public async Task DownloadTarballAsync(
        string owner, 
        string repo, 
        string @ref, 
        string destGzPath, 
        CancellationToken ct
        )
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destGzPath)!);
        
        var url = $"{_opt.ApiBaseUrl}/repos/{owner}/{repo}/tarball/{@ref}";
        using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        await using var fs = File.Create(destGzPath);
        await resp.Content.CopyToAsync(fs, ct);
    }
}