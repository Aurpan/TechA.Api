using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TechA.Core.DTOs;
using TechA.Core.Interfaces.Service;

namespace TechA.Service;

public class AudioStreamService : IAudioStreamService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly AudioStream _audioStream;
    private readonly ILlmService _llmService;
    private readonly ILogger<AudioStreamService> _logger;

    public AudioStreamService(IOptions<AudioStream> options, ILlmService llmService, ILogger<AudioStreamService> logger)
    {
        _audioStream = options.Value;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task RelayAsync(WebSocket clientSocket, string sttToken, string sessionId, CancellationToken cancellationToken)
    {
        using var downstream = new ClientWebSocket();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var tokenToUse = string.IsNullOrEmpty(_audioStream.SttApiToken) ? sttToken : _audioStream.SttApiToken;
            var sttUrl = $"{_audioStream.DownstreamServiceUrl}?token={Uri.EscapeDataString(tokenToUse)}";
            await downstream.ConnectAsync(new Uri(sttUrl), cts.Token);
            _logger.LogInformation("Connected to STT service at {Url} for session {SessionId}.", _audioStream.DownstreamServiceUrl, sessionId);

            var clientToDownstream = ForwardClientToDownstreamAsync(clientSocket, downstream, sessionId, cts.Token);
            var downstreamToClient = ForwardDownstreamToClientAsync(downstream, clientSocket, sessionId, cts.Token);

            var completed = await Task.WhenAny(clientToDownstream, downstreamToClient);

            if (completed == clientToDownstream)
            {
                _logger.LogInformation("Client audio stream ended for session {SessionId}. Waiting for stt.final from downstream.", sessionId);
                await downstreamToClient;
            }

            var sttResult = downstreamToClient.Result;

            if (sttResult is not null)
            {
                _logger.LogInformation("Calling LLM for session {SessionId} with text: \"{Text}\".", sessionId, sttResult.Text);
                await _llmService.StreamToClientAsync(
                    sessionId,
                    sttResult.Text ?? string.Empty,
                    sttResult.Language ?? "en",
                    clientSocket,
                    cancellationToken);
            }

            await cts.CancelAsync();

            await Task.WhenAll(clientToDownstream, downstreamToClient)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Audio relay cancelled for session {SessionId}.", sessionId);
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket error during audio relay for session {SessionId}.", sessionId);
        }
        finally
        {
            await TryCloseAsync(clientSocket);
            await TryCloseAsync(downstream);
        }
    }

    private async Task ForwardClientToDownstreamAsync(WebSocket client, WebSocket downstream, string sessionId, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(_audioStream.BufferSize);
        var audioBytesReceived = 0L;
        var audioChunksReceived = 0;
        var startReceived = false;
        var endReceived = false;

        try
        {
            while (client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await client.ReceiveAsync(buffer.AsMemory(), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Client closed connection for session {SessionId}. Audio chunks: {AudioChunks}, bytes: {AudioBytes}.",
                        sessionId, audioChunksReceived, audioBytesReceived);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var messageType = doc.RootElement.GetProperty("type").GetString();
                        var clientSessionId = doc.RootElement.TryGetProperty("sessionId", out var sidProp)
                            ? sidProp.GetString()
                            : null;

                        if (messageType == "start")
                        {
                            startReceived = true;
                            _logger.LogInformation("Received START for session {SessionId} (client: {ClientSessionId}).",
                                sessionId, clientSessionId);

                            if (doc.RootElement.TryGetProperty("audio", out var audioProp))
                            {
                                var encoding = audioProp.TryGetProperty("encoding", out var enc) ? enc.GetString() : "unknown";
                                var sampleRate = audioProp.TryGetProperty("sampleRate", out var sr) ? sr.GetInt32() : 0;
                                var channels = audioProp.TryGetProperty("channels", out var ch) ? ch.GetInt32() : 0;

                                _logger.LogInformation("  Audio config: encoding={Encoding}, sampleRate={SampleRate}Hz, channels={Channels}.",
                                    encoding, sampleRate, channels);
                            }
                        }
                        else if (messageType == "end")
                        {
                            endReceived = true;
                            _logger.LogInformation("Received END for session {SessionId}. Total: {AudioChunks} chunks, {AudioBytes} bytes.",
                                sessionId, audioChunksReceived, audioBytesReceived);
                        }
                        else
                        {
                            _logger.LogWarning("Unknown message type '{MessageType}' for session {SessionId}.", messageType, sessionId);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Invalid JSON in control message for session {SessionId}: {Json}.", sessionId, json);
                    }

                    var patched = PatchSessionId(json, sessionId);
                    var patchedBytes = Encoding.UTF8.GetBytes(patched);

                    if (downstream.State == WebSocketState.Open)
                    {
                        await downstream.SendAsync(
                            patchedBytes.AsMemory(),
                            WebSocketMessageType.Text,
                            result.EndOfMessage,
                            cancellationToken);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    audioBytesReceived += result.Count;
                    audioChunksReceived++;

                    if (audioChunksReceived == 1)
                        _logger.LogInformation("First audio chunk received for session {SessionId}.", sessionId);

                    if (downstream.State == WebSocketState.Open)
                    {
                        await downstream.SendAsync(
                            buffer.AsMemory(0, result.Count),
                            result.MessageType,
                            result.EndOfMessage,
                            cancellationToken);
                    }

                    if (audioChunksReceived % 100 == 0)
                    {
                        _logger.LogDebug("Forwarded {ChunkCount} chunks ({ByteCount} bytes) for session {SessionId}.",
                            audioChunksReceived, audioBytesReceived, sessionId);
                    }
                }
            }

            _logger.LogInformation("Session {SessionId} summary: Start={StartReceived}, End={EndReceived}, Chunks={AudioChunks}, Bytes={AudioBytes}.",
                sessionId, startReceived, endReceived, audioChunksReceived, audioBytesReceived);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static string PatchSessionId(string json, string sessionId)
    {
        using var doc = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name == "sessionId")
                {
                    writer.WriteString("sessionId", sessionId);
                }
                else
                {
                    prop.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private async Task<SttFinalResult?> ForwardDownstreamToClientAsync(WebSocket source, WebSocket destination, string sessionId, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(_audioStream.BufferSize);
        SttFinalResult? sttResult = null;

        try
        {
            while (source.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await source.ReceiveAsync(buffer.AsMemory(), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                if (destination.State == WebSocketState.Open)
                {
                    await destination.SendAsync(
                        buffer.AsMemory(0, result.Count),
                        result.MessageType,
                        result.EndOfMessage,
                        cancellationToken);
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("type", out var typeProp) &&
                            typeProp.GetString() == "stt.final")
                        {
                            var text = doc.RootElement.TryGetProperty("text", out var textProp)
                                ? textProp.GetString()
                                : null;
                            var language = doc.RootElement.TryGetProperty("language", out var langProp)
                                ? langProp.GetString()
                                : null;

                            _logger.LogInformation(
                                "Received stt.final for session {SessionId}: lang={Language}, text=\"{Text}\".",
                                sessionId, language, text);

                            sttResult = new SttFinalResult { Text = text, Language = language };
                            break;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Non-JSON text message from downstream for session {SessionId}.", sessionId);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return sttResult;
    }

    private async Task TryCloseAsync(WebSocket socket)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            try
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Stream ended",
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing WebSocket gracefully.");
            }
        }
    }
}
