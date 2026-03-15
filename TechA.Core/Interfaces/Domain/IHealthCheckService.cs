using TechA.Core.Responses;

namespace TechA.Core.Interfaces.Domain
{
    public interface IHealthCheckService
    {
        Task<HealthCheckResponse> GetReadinessAsync();
    }
}
