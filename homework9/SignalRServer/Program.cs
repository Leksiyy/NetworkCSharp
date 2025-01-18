using Microsoft.AspNetCore.SignalR;
using SignalRServer.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<CurrencyHub>("/currencyHub");

app.Run();

