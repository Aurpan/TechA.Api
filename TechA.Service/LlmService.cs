using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using TechA.Core.DTOs;
using TechA.Core.Interfaces.Domain;

namespace TechA.Services;

public class LlmService : ILlmService
{
    private readonly LlmStream _options;
    private readonly ILogger<LlmService> _logger;

    public LlmService(HttpClient httpClient, IOptions<LlmStream> options, ILogger<LlmService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> StreamToClientAsync(string sessionId, string userText, string language, WebSocket clientSocket, CancellationToken cancellationToken, string tokenToUse)
    {
        var endpoint = _options.GenerateEndpoint.TrimStart('/');
        var url = $"{_options.BaseUrl.TrimEnd('/')}/{endpoint}";
        _logger.LogInformation("Calling LLM for session {SessionId}: {Url}", sessionId, url);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("X-Internal-Token", tokenToUse);

        if (!string.IsNullOrEmpty(_options.ApiToken))
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

        using var response = await http.PostAsJsonAsync(
            url,
            new { session_id = sessionId, user_text = userText, language },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[4096];
        int bytesRead;
        var responseBuilder = new StringBuilder();

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (clientSocket.State != WebSocketState.Open)
            {
                _logger.LogWarning("Client WebSocket closed during LLM streaming for session {SessionId}.", sessionId);
                break;
            }

            await clientSocket.SendAsync(
                buffer.AsMemory(0, bytesRead),
                WebSocketMessageType.Text,
                endOfMessage: false,
                cancellationToken);

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            responseBuilder.Append(chunk);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("LLM chunk for session {SessionId}: {Chunk}", sessionId, chunk);
            }
        }

        if (clientSocket.State == WebSocketState.Open)
        {
            await clientSocket.SendAsync(
                ReadOnlyMemory<byte>.Empty,
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }

        _logger.LogInformation("LLM streaming completed for session {SessionId}.", sessionId);
        return responseBuilder.ToString();
    }
}
