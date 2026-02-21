namespace Domain.Rules;

public static class PathRules
{
    public static string GetMdnCacheRoot()
    {
        var baseCache = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Caches", "DocTok", "mdn");
        }

        if (OperatingSystem.IsLinux())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".cache", "doctok", "mdn");
        }

        return Path.Combine(baseCache, "DocTok", "mdn");
    }
}
