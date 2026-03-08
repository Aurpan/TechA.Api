using System.Net.WebSockets;

namespace TechA.Core.Interfaces.Service;

public interface IAudioStreamService
{
    Task RelayAsync(WebSocket clientSocket, CancellationToken cancellationToken);
}
