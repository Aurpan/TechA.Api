using System.Security.Claims;
using TechA.Core.Interfaces.Domain;

namespace TechA.Api.WebSockets;

public static class AudioStreamEndpoints
{
    public static IEndpointRouteBuilder MapAudioStreamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/audio", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("WebSocket connection required.");
                return;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authentication required.");
                return;
            }

            var sttToken = context.Request.Query["access_token"].ToString();

            if (string.IsNullOrEmpty(sttToken))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("access_token query parameter is required.");
                return;
            }

            var sessionId = Guid.NewGuid().ToString();
            var service = context.RequestServices.GetRequiredService<IAudioStreamService>();

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            await service.RelayAsync(socket, sttToken, sessionId, userId, context.RequestAborted);
        });

        return endpoints;
    }
}
