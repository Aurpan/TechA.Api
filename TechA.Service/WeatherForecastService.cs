using TechA.Repository.Interfaces;
using TechA.Service.Interfaces;

namespace TechA.Service
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IWeatherForecastRepository _repository;

        public WeatherForecastService(IWeatherForecastRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<WeatherForecast> GetWeatherForecasts(int days)
        {
            return _repository.GetWeatherForecasts(days);
        }
    }
}
