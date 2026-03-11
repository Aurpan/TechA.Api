using System.Net.WebSockets;

namespace TechA.Core.Interfaces.Service;

public interface ILlmService
{
    Task StreamToClientAsync(string sessionId, string userText, string language, WebSocket clientSocket, CancellationToken cancellationToken);
}
