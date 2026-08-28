using Microsoft.Extensions.DependencyInjection;
using BusinessLogic.Services;
using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddScoped<IMetronomeCollectNameService, MetronomeCollectNameService>();
        services.AddScoped<IMetronomeSettingsService, MetronomeSettingsService>();

        services.AddScoped<IAuthService, AuthService>();

        services.Configure<IdentityApiSettings>(configuration.GetSection("IdentityApiSettings"));
        services.AddHttpClient<IIdentityApiService, IdentityApiService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<IdentityApiSettings>>().Value;

            if(string.IsNullOrEmpty(settings.BaseUrl))
                throw new Exception("IdentityApiSettings.BaseAddress is not set");
                
            client.BaseAddress = new Uri(settings.BaseUrl);
        });
        services.AddMemoryCache();
        services.AddSingleton<ITokenCacheService, TokenCacheService>();
        // services.AddHttpClient<IIdentityApiService, IdentityApiService>(client =>
        // {
        //     client.BaseAddress = new Uri("https://localhost:7261/");
        // });

        return services;
    }
}