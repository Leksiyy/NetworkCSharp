using Microsoft.AspNetCore.SignalR.Client;

namespace ConsoleClient;

class Program
{
    static void Main(string[] args)
    {
        HubConnection connection = new HubConnectionBuilder().WithUrl("https://localhost:7212").Build();
        Console.WriteLine("Enter your username: ");
        string userName = Console.ReadLine();
        while (true)
        {
            Console.WriteLine("Enter message: ");
            string message = Console.ReadLine();
            connection.On<string, string>("Received", (userName, message) =>
            {

            });
        }
    }
}