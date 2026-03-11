using TechA.Api.WebSockets;
using TechA.DependencyInjection.Extensions;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddApplicationServices(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            
            // Serve the audio test page in development only
            app.MapGet("/audio-test", async context =>
            {
                var filePath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "audio-test.html");
                
                if (!File.Exists(filePath))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Audio test page not found.");
                    return;
                }
                
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(filePath);
            });
        }

        app.UseHttpsRedirection();
        app.UseWebSockets();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAudioStreamEndpoints();
        app.MapControllers();

        app.Run();
    }
}