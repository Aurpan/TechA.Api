using Microsoft.AspNetCore.Mvc;
using TechA.Service.Interfaces;

namespace TechA.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;

        public WeatherForecastController(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var forecasts = _weatherForecastService.GetWeatherForecasts(5);

            return forecasts.Select(f => new WeatherForecast
            {
                Date = f.Date,
                TemperatureC = f.TemperatureC,
                Summary = f.Summary
            });
        }
    }
}
