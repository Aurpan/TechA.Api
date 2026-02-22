using Microsoft.EntityFrameworkCore;
using TechA.Repository.Interfaces;

namespace TechA.Repository.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherForecast> WeatherForecasts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WeatherForecast>(entity =>
            {
                entity.HasKey(e => e.Date);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.TemperatureC).IsRequired();
                entity.Property(e => e.Summary).HasMaxLength(100);
            });
        }
    }
}
