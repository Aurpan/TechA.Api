using TechA.Core.Enums;

namespace TechA.Core.Responses
{
    public class HealthCheckResponse
    {
        public ConnectivityStatus Status { get; set; }
        public Dictionary<string, string> Checks { get; set; } = [];
    }
}
