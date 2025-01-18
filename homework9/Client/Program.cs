using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using SignalRServer.Models;

namespace Client;

class Program
{
    static async Task Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5080/currencyHub")
            .Build();

        connection.On<CurrencyRate>("UpdateCurrencies", currencyRate =>
        {
            Console.WriteLine($"USD/EUR: {currencyRate.USD_EUR}, GBP/EUR: {currencyRate.GBP_EUR}");
        });
        
        await connection.StartAsync();

        Console.WriteLine("Подключено к серверу SignalR.");

        Console.ReadLine();

        await connection.DisposeAsync();
    }
}

