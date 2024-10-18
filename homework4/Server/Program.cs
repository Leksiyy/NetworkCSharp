using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

class Program
{
    static async Task Main(string[] args)
    {
        const int maxClientsCount = 100;
        Dictionary<IPEndPoint, Client> clients = new Dictionary<IPEndPoint, Client>();
        UdpClient udpClient = new UdpClient(8080);
        Dictionary<string, int> prices = new Dictionary<string, int>()
        {
            { "процессор", 300 },
            { "видеокарта", 500 },
            { "озу", 100 },
            { "блок питания", 250 }
        };

        while (true)
        {
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.Receive(ref client);
            string request = Encoding.UTF8.GetString(data);
            Console.WriteLine("Here is new client");
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var client1 in clients.Keys.ToList())
                    {
                        if ((DateTime.Now - clients[client1].LastRequest).TotalMinutes > 10) clients.Remove(client1);
                    }
                    await Task.Delay(60000);
                }
            });
            
            string response;
            if (clients.Count > maxClientsCount)
            {
                response = "Server is running at maximum number of clients. Please try again.";
            }
            else if (clients.ContainsKey(client))
            {
                Client clientData = clients[client];
                if ((DateTime.Now - clientData.LastRequest).TotalHours < 1 && clientData.RequestCounter >= 10)
                {
                    response = "Request counter exceeded, try again later.";
                }
                else
                {
                    if ((DateTime.Now - clientData.LastRequest).TotalHours >= 1)
                    {
                        clientData.RequestCounter = 0;
                    }
                    clientData.LastRequest = DateTime.Now;
                    clientData.RequestCounter++;
                    clients[client] = clientData;
                    
                    response = prices.ContainsKey(request.ToLower())
                        ? $"Price for {request}: {prices[request.ToLower()]}$"
                        : $"Sorry, we don't have information about {request}.";
                }
            }
            else
            {
                clients[client] = new Client {RequestCounter = 1, LastRequest = DateTime.Now};
                
                response = prices.ContainsKey(request.ToLower())
                    ? $"Price for {request}: {prices[request.ToLower()]}$"
                    : $"Sorry, we don't have information about {request}.";
            }
            byte[] dataToClient = Encoding.UTF8.GetBytes(response);
            udpClient.Send(dataToClient, dataToClient.Length, client);
        }
    }
}

class Client
{
    public int RequestCounter { get; set; }
    public DateTime LastRequest { get; set; }
}