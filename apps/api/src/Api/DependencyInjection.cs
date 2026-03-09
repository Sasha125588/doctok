using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Auth;
using Api.Errors;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Api;

public static class WebServiceRegistration
{
  public static IServiceCollection AddWebServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var corsOrigins = configuration.GetSection("Cors:Origins").Get<string[]>()
                      ?? ["http://localhost:5173"];

    services.AddCors(options =>
    {
      options.AddDefaultPolicy(policy =>
      {
        policy
          .WithOrigins(corsOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
      });
    });

    services.AddOpenApi(options =>
    {
      options.AddDocumentTransformer((document, context, ct) =>
      {
        document.Info = new OpenApiInfo
        {
          Title = "DocTok API",
          Version = "v1",
          Description = "DocTok HTTP API. JWT authentication uses Supabase bearer tokens."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
          ["BearerAuth"] = new OpenApiSecurityScheme
          {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
          }
        };
        return Task.CompletedTask;
      });

      options.AddOperationTransformer((operation, context, ct) =>
      {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var allowAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

        if (!allowAnonymous && requiresAuthorization)
        {
          operation.Security ??= [];
          operation.Security.Add(new OpenApiSecurityRequirement
          {
            [new OpenApiSecuritySchemeReference("BearerAuth", context.Document, null)] = []
          });

          operation.Responses ??= new OpenApiResponses();
          operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
          operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }

        return Task.CompletedTask;
      });
    });

    services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

    services.AddValidation();

    services.AddProblemDetails(options =>
    {
      options.CustomizeProblemDetails = context =>
      {
        if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
        {
          context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        }

        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
      };
    });

    services.AddExceptionHandler<ApiExceptionHandler>();

    services.AddValidatedOptions<SupabaseJwtOptions>("Supabase");

    var supabaseJwtOptions = configuration.GetSection("Supabase").Get<SupabaseJwtOptions>()
                             ?? throw new InvalidOperationException("Supabase config missing");

    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = true;
        options.MetadataAddress = supabaseJwtOptions.MetadataUrl;

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidIssuer = supabaseJwtOptions.Issuer,
          ValidateAudience = true,
          ValidAudience = supabaseJwtOptions.JwtAudience,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.FromMinutes(2),
          ValidateIssuerSigningKey = true,
        };
      });

    services.AddAuthorizationBuilder()
      .AddPolicy("Admin", policy =>
        policy.RequireClaim("user_role", "admin"));

    return services;
  }
}
