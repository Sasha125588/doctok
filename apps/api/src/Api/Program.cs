using Api.Auth;
using Api.Features.Session.Me;
using Api.Features.System.DbPing;
using Api.Features.System.Health;
using Infrastructure.Persistence.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<SupabaseJwtOptions>(builder.Configuration.GetSection("Supabase"));

var supabaseJwtOptions = 
    builder.Configuration.GetSection("Supabase").Get<SupabaseJwtOptions>() 
    ?? throw new InvalidOperationException("Supabase config missing");

builder.Services
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

builder.Services.AddAuthorization();

var connStr = builder.Configuration["Database:ConnectionString"]
              ?? throw new InvalidOperationException("Database:ConnectionString missing");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connStr));
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{  
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealth();
app.MapMe();
app.MapDbPing();

app.Run();
