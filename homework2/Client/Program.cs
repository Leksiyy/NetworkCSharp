using System.Net.Sockets;
using System.Text;

namespace Client;

class Program
{
    static async Task Main(string[] args)
    {
        string serverIp = "127.0.0.1";
        int port = 8080;

        try
        {
            using (TcpClient client = new TcpClient(serverIp, port))
            {
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Connected to server. Type your requests.");

                while (true)
                {
                    Console.Write("Enter request (or type 'exit' to quit): ");
                    string request = Console.ReadLine();

                    if (request.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
                    Console.WriteLine($"Sent: {request}");

                    byte[] buffer = new byte[512];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}