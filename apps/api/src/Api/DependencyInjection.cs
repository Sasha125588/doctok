using Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    });

    services.AddProblemDetails();

    services
      .AddOptions<SupabaseJwtOptions>()
      .BindConfiguration("Supabase")
      .ValidateDataAnnotations()
      .ValidateOnStart();

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

    services.AddSingleton<Features.Resolve.Mdn.ResolveMdnHandler>();
    services.AddSingleton<Features.Admin.Mdn.Preload.PreloadMdnHandler>();
    services.AddSingleton<Features.Posts.Comments.Create.CreateCommentHandler>();

    return services;
  }
}
