using System.Formats.Tar;
using System.IO.Compression;

namespace Infrastructure.Sources.GitHub;

public static class TarGzExtractor
{
    public static void Extract(string tarGzPath, string destinationDir)
    {
        if (Directory.Exists(destinationDir))
            Directory.Delete(destinationDir, recursive: true);

        Directory.CreateDirectory(destinationDir);

        using var fs = File.OpenRead(tarGzPath);
        using var gzip = new GZipStream(fs, CompressionMode.Decompress);

        TarFile.ExtractToDirectory(gzip, destinationDir, overwriteFiles: true);
    }
}