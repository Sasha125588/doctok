using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Extensions;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}

public interface IAdminEndpoint : IEndpoint { }

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
      var serviceDescriptors = assembly.DefinedTypes
        .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
        .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type));

      services.TryAddEnumerable(serviceDescriptors);

      return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        var apiGroup = app.MapGroup("/api");
        var adminGroup = apiGroup.MapGroup("/admin")
            .RequireAuthorization("Admin")
            .WithTags("Admin");

        foreach (var endpoint in endpoints)
        {
          endpoint.Map(endpoint is IAdminEndpoint ? adminGroup : apiGroup);
        }

        return app;
    }
}
