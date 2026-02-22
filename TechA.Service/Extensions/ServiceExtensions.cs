using Microsoft.Extensions.DependencyInjection;
using TechA.Service.Interfaces;

namespace TechA.Service.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IWeatherForecastService, WeatherForecastService>();

            return services;
        }
    }
}
