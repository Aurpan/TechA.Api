using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TechA.Core.DTOs;
using TechA.Core.Interfaces.Domain;
using TechA.Core.Interfaces.Persistence;
using TechA.DataManagement.DbContext;
using TechA.DataManagement.Persistence.Repositories;
using TechA.Services;

namespace TechA.DependencyInjection.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TechADbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(TechADbContext).Assembly.FullName)
            )
            .UseSnakeCaseNamingConvention()
        );

        var jwtSection = configuration.GetSection("Jwt");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSection["Key"]!))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken)
                        && context.HttpContext.WebSockets.IsWebSocketRequest)
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.Configure<AudioStream>(configuration.GetSection(AudioStream.SectionName));
        services.Configure<LlmStream>(configuration.GetSection(LlmStream.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILlmResponseRepository, LlmResponseRepository>();

        services.AddScoped<IHealthCheckService, HealthCheckService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddTransient<IAudioStreamService, AudioStreamService>();

        var llmConfig = configuration.GetSection(LlmStream.SectionName);
        services.AddHttpClient<ILlmService, LlmService>(client =>
        {
            client.BaseAddress = new Uri(llmConfig["BaseUrl"] ?? "https://api.techabd.live");
        });

        return services;
    }
}
