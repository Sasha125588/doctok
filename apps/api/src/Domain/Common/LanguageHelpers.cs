namespace Domain.Common;

public static class LanguageHelpers
{
  public static string NormalizeLang(string lang)
    => lang.Trim().ToLowerInvariant() switch
    {
      "en" or "en-US" => "en",
      "ru" => "ru",
      _ => lang.Trim().ToLowerInvariant()
    };

  public static string ToMdnLang(string lang) => lang switch
  {
    "en" => "en-US",
    "zh-cn" => "zh-CN",
    "zh-tw" => "zh-TW",
    "pt-br" => "pt-BR",
    _ => lang.Trim().ToLowerInvariant()
  };
}
