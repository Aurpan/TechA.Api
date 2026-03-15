using TechA.Core.Enums;
using TechA.Core.Interfaces.Domain;
using TechA.Core.Responses;
using TechA.DataManagement.DbContext;

namespace TechA.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly TechADbContext _dbContext;

    public HealthCheckService(TechADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResponse> GetReadinessAsync()
    {
        var response = new HealthCheckResponse { Status = ConnectivityStatus.Healthy };

        try
        {
            var dbConnected = await _dbContext.Database.CanConnectAsync();

            if (!dbConnected)
            {
                response.Status = ConnectivityStatus.Disconnected;
                response.Checks.Add("database", ConnectivityStatus.Disconnected.ToString());
                return response;
            }

            response.Checks.Add("database", ConnectivityStatus.Healthy.ToString());

            // Add check for downstream services (STT + LLM)
        }
        catch (OperationCanceledException)
        {
            response.Status = ConnectivityStatus.Timeout;
            response.Checks.Add("database", ConnectivityStatus.Timeout.ToString());

        }
        catch (Exception)
        {
            response.Status = ConnectivityStatus.Error;
            response.Checks.Add("database", ConnectivityStatus.Error.ToString());
        }

        return response;
    }
}
