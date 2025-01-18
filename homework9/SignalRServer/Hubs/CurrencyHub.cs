using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;

namespace SignalRServer.Hubs;

public class CurrencyHub : Hub
{
    private static Timer? _timer;
    private static readonly Random _random = new Random();
    private readonly IHubContext<CurrencyHub> _hubContext;

    public CurrencyHub(IHubContext<CurrencyHub> hubContext)
    {
        _hubContext = hubContext;

        if (_timer == null)
        {
            _timer = new Timer(SendCurrencyUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }
    }

    private void SendCurrencyUpdates(object? state)
    {
        var currencyRate = new CurrencyRate
        {
            USD_EUR = Math.Round(0.85 + _random.NextDouble() * 0.1, 4),
            GBP_EUR = Math.Round(1.12 + _random.NextDouble() * 0.1, 4)
        };
        
        _hubContext.Clients.All.SendAsync("UpdateCurrencies", currencyRate);
    }

}