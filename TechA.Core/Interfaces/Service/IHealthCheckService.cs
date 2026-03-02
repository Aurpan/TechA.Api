using TechA.Core.ResponseObjects;

namespace TechA.Core.Interfaces.Service
{
    public interface IHealthCheckService
    {
        Task<HealthCheckResponse> GetReadinessAsync();
    }
}
