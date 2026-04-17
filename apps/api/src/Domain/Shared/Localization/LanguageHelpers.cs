namespace Domain.Shared;

public static class LanguageHelpers
{
  public static string NormalizeLang(string? lang)
  {
    if (string.IsNullOrWhiteSpace(lang))
      return "en";

    var normalized = lang.Trim().ToLowerInvariant();

    return normalized switch
    {
      "en-us" => "en",
      "es" => "es",
      "fr" => "fr",
      "ja" => "ja",
      "ko" => "ko",
      "pt-br" => "pt-br",
      "ru" => "ru",
      "zh-cn" => "zh-cn",
      "zh-tw" => "zh-tw",
      "de" => "de",
      _ => "en"
    };
  }

  public static string ToMdnLang(string lang)
  {
    var normalized = NormalizeLang(lang);

    return normalized switch
    {
      "en" => "en-US",
      "es" => "es",
      "fr" => "fr",
      "ja" => "ja",
      "ko" => "ko",
      "pt-br" => "pt-BR",
      "ru" => "ru",
      "zh-cn" => "zh-CN",
      "zh-tw" => "zh-TW",
      "de" => "de",
      _ => "en-US"
    };
  }

  public static string ToLangName(string lang) =>
    lang.ToLowerInvariant() switch
    {
      "en"             => "English",
      "ru"             => "Russian",
      "ja"             => "Japanese",
      "ko"             => "Korean",
      "zh-cn" or "zh"  => "Chinese (Simplified)",
      "zh-tw"          => "Chinese (Traditional)",
      "fr"             => "French",
      "de"             => "German",
      "es"             => "Spanish",
      "pt" or "pt-br"  => "Portuguese",
      _                => lang,
    };
}
