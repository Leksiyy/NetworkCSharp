using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Console.WriteLine("Connecting to server...");
                    await client.ConnectAsync("127.0.0.1", 8080);
                    Console.WriteLine("Connected to server.");

                    using (NetworkStream stream = client.GetStream())
                    {
                        
                        bool running = true;
                        while (running)
                        {
                            Console.WriteLine("Enter command (ORDER, CANCEL, EXIT, RECEIVE):");
                            string command = Console.ReadLine()?.ToUpper();
                            
                            switch (command)
                            {
                                case "ORDER":
                                    await SendOrderAsync(stream);
                                    break;

                                case "CANCEL":
                                    await CancelOrderAsync(stream);
                                    break;

                                case "RECEIVE":
                                    await ReceiveServerMessageAsync(stream);
                                    break;
                                
                                case "EXIT":
                                    running = false;
                                    break;

                                default:
                                    Console.WriteLine("Unknown command. Try ORDER, CANCEL, RECEIVE, or EXIT.");
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task SendOrderAsync(NetworkStream stream)
        {
            Console.Write("Enter order ID: ");
            string orderId = Console.ReadLine();

            Console.Write("Enter preparation time (in ms): ");
            if (int.TryParse(Console.ReadLine(), out int prepTime))
            {
                string orderMessage = $"ORDER;{orderId};{prepTime}";
                await SendMessageAsync(stream, orderMessage);
                string response = await ReceiveResponseAsync(stream);
                Console.WriteLine("Server response: " + response);
            }
            else
            {
                Console.WriteLine("Invalid preparation time.");
            }
        }

        static async Task CancelOrderAsync(NetworkStream stream)
        {
            Console.Write("Enter order ID to cancel: ");
            string orderId = Console.ReadLine();

            string cancelMessage = $"CANCEL;{orderId}";
            await SendMessageAsync(stream, cancelMessage);
            string response = await ReceiveResponseAsync(stream);
            Console.WriteLine("Server response: " + response);
        }

        static async Task SendMessageAsync(NetworkStream stream, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }

        static async Task<string> ReceiveResponseAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        
        static async Task ReceiveServerMessageAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Message from server: " + message);
        }
    }
}