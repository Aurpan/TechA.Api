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