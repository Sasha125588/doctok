namespace Domain.Common;

public static class LanguageHelpers
{
  public static string NormalizeLang(string lang)
    => lang.Trim().ToLowerInvariant() switch
    {
      "en" or "en-us" => "en",
      "ru" => "ru",
      _ => lang.Trim().ToLowerInvariant()
    };
}
