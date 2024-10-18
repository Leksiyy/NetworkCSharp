using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

class Program
{
    static void Main(string[] args)
    {
        UdpClient client = new UdpClient();
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
        while (true)
        {
            Console.WriteLine("Enter request to find price or type \"exit\" to exit: ");
            string request = Console.ReadLine();
            if (request == "exit") break;
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            client.Send(requestBytes, requestBytes.Length, endPoint);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] serverBytes = client.Receive(ref serverEndPoint);
            Console.WriteLine(Encoding.UTF8.GetString(serverBytes));
        }
    }
}