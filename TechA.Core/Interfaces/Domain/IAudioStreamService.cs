using System.Net.WebSockets;

namespace TechA.Core.Interfaces.Domain;

public interface IAudioStreamService
{
    Task RelayAsync(WebSocket clientSocket, string sttToken, string sessionId, CancellationToken cancellationToken);
}
