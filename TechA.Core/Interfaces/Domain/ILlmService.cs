using System.Net.WebSockets;

namespace TechA.Core.Interfaces.Domain;

public interface ILlmService
{
    Task<string> StreamToClientAsync(string sessionId, string userText, string language, WebSocket clientSocket, CancellationToken cancellationToken, string tokenToUse);
}
