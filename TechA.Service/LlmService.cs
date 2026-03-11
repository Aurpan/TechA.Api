using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TechA.Core.DTOs;
using TechA.Core.Interfaces.Service;

namespace TechA.Service;

public class LlmService : ILlmService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HttpClient _httpClient;
    private readonly LlmStream _options;
    private readonly ILogger<LlmService> _logger;

    public LlmService(HttpClient httpClient, IOptions<LlmStream> options, ILogger<LlmService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StreamToClientAsync(string sessionId, string userText, string language, WebSocket clientSocket, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            sessionId,
            userText,
            language,
            generation = new
            {
                temperature = _options.Temperature,
                maxOutputTokens = _options.MaxOutputTokens
            }
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var endpoint = _options.GenerateEndpoint.TrimStart('/');
        _logger.LogInformation("Calling LLM for session {SessionId}: {Endpoint}", sessionId, endpoint);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

        if (!string.IsNullOrEmpty(_options.ApiToken))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[4096];
        int bytesRead;

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

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
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
    }
}
