using TechA.Repository.Interfaces;
using TechA.Repository.Data;

namespace TechA.Repository
{
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private readonly ApplicationDbContext _context;

        public WeatherForecastRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<WeatherForecast> GetWeatherForecasts(int days)
        {
            return _context.WeatherForecasts
                .OrderBy(wf => wf.Date)
                .Take(days)
                .ToList();
        }
    }
}
