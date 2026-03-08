using Microsoft.EntityFrameworkCore;
using TechA.Core.Enums;
using TechA.Core.Interfaces.Service;
using TechA.Core.ResponseObjects;
using TechA.Repository.Data;

namespace TechA.Service;

public class HealthCheckService : IHealthCheckService
{
    private readonly ApplicationDbContext _dbContext;

    public HealthCheckService(ApplicationDbContext dbContext)
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
