using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class OptionsExtensions
{
  public static IServiceCollection AddValidatedOptions<T>(
    this IServiceCollection services,
    string sectionPath)
    where T : class => services
      .AddOptions<T>()
      .BindConfiguration(sectionPath)
      .ValidateDataAnnotations()
      .ValidateOnStart()
      .Services;
}
