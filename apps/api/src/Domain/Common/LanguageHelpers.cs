namespace Domain.Common;

public static class LanguageHelpers
{
  public static string NormalizeLang(string lang)
  {
    if (string.IsNullOrWhiteSpace(lang))
    {
      return "en";
    }

    var normalized = lang.Trim().ToLowerInvariant();
    return normalized switch
    {
      "en" or "en-us" => "en",
      "ru" => "ru",
      _ => normalized
    };
  }

  public static string ToMdnLang(string lang)
  {
    var normalized = NormalizeLang(lang);

    return normalized switch
    {
      "en" => "en-US",
      "zh-cn" => "zh-CN",
      "zh-tw" => "zh-TW",
      "pt-br" => "pt-BR",
      _ => normalized
    };
  }
}
