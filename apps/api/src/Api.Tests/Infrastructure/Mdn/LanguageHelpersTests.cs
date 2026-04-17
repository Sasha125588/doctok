using Domain.Shared;
using Xunit;

namespace Api.Tests.Infrastructure.Mdn;

public sealed class LanguageHelpersTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    [InlineData("en-us", "en")]
    [InlineData("EN-US", "en")]
    [InlineData("ru", "ru")]
    [InlineData("RU", "ru")]
    [InlineData("zh-CN", "zh-cn")]
    [InlineData("zh-TW", "zh-tw")]
    [InlineData("pt-BR", "pt-br")]
    [InlineData("fr", "fr")]
    [InlineData("de", "de")]
    [InlineData("ja", "ja")]
    [InlineData("ko", "ko")]
    [InlineData("  en-US  ", "en")]
    [InlineData("", "en")]
    [InlineData("   ", "en")]
    public void NormalizeLangReturnsExpectedValue(string input, string expected)
    {
        Assert.Equal(expected, LanguageHelpers.NormalizeLang(input));
    }

    [Theory]
    [InlineData("en", "en-US")]
    [InlineData("en-us", "en-US")]
    [InlineData("en-US", "en-US")]
    [InlineData("ru", "ru")]
    [InlineData("zh-cn", "zh-CN")]
    [InlineData("zh-tw", "zh-TW")]
    [InlineData("pt-br", "pt-BR")]
    [InlineData("fr", "fr")]
    public void ToMdnLangReturnsExpectedValue(string input, string expected)
    {
        Assert.Equal(expected, LanguageHelpers.ToMdnLang(input));
    }
}
