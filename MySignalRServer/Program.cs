using MySignalRServer.Hubs;

namespace MySignalRServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR();
        var app = builder.Build();
        
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        
        app.MapHub<ChatHub>("/chat");

        app.Run();
    }
}