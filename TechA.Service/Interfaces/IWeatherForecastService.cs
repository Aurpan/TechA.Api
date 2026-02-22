using TechA.Repository.Interfaces;

namespace TechA.Service.Interfaces
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> GetWeatherForecasts(int days);
    }
}
