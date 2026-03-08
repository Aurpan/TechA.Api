using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Net.WebSockets;
using TechA.Core.DTOs;
using TechA.Core.Interfaces.Service;

namespace TechA.Service;

public class AudioStreamService : IAudioStreamService
{
    private readonly AudioStream _options;
    private readonly ILogger<AudioStreamService> _logger;

    public AudioStreamService(IOptions<AudioStream> options, ILogger<AudioStreamService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task RelayAsync(WebSocket clientSocket, CancellationToken cancellationToken)
    {
        using var downstream = new ClientWebSocket();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await downstream.ConnectAsync(new Uri(_options.DownstreamServiceUrl), cts.Token);
            _logger.LogInformation("Connected to downstream audio service at {Url}.", _options.DownstreamServiceUrl);

            var clientToDownstream = ForwardAsync(clientSocket, downstream, cts.Token);
            var downstreamToClient = ForwardAsync(downstream, clientSocket, cts.Token);

            await Task.WhenAny(clientToDownstream, downstreamToClient);
            await cts.CancelAsync();

            await Task.WhenAll(clientToDownstream, downstreamToClient)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Audio relay cancelled by client disconnect.");
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket error during audio relay.");
        }
        finally
        {
            await TryCloseAsync(clientSocket);
            await TryCloseAsync(downstream);
        }
    }

    private async Task ForwardAsync(WebSocket source, WebSocket destination, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(_options.BufferSize);

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
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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
