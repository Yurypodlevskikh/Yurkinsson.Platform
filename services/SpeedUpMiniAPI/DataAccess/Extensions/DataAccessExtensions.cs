using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Extensions
{
    public static class DataAccessExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IMetronomeSettingsRepo, MetronomeSettingsRepo>();
            services.AddScoped<IMetronomeCollectNameRepo, MetronomeCollectNameRepo>();

            services.AddDbContext<AppDbContext>(options =>
            {
                // options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 4, 14)));
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 5, 25)));
            });
            services.AddScoped<IUserInfoRepo, UserInfoRepo>();
            return services;
        }
    }
}