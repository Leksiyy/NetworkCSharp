using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        private static Queue<Order> _orderQueue = new Queue<Order>();
        private static object _orderQueueLock = new object();

        //связка заказ-токен
        private static Dictionary<string, CancellationTokenSource> _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        private static object _cancellationTokensLock = new object(); // для разделения ответственности

        static async Task Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            server.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                while (client.Connected)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + request);

                    string response = await ProcessClientRequest(request, client);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        private static async Task<string> ProcessClientRequest(string request, TcpClient client)
        {
            string[] parts = request.Split(";");
            if (parts.Length < 2)
            {
                return "Invalid format.";
            }

            string command = parts[0];
            string orderId = parts[1];

            switch (command.ToUpper())
            {
                case "ORDER":
                    if (parts.Length < 3)
                        return "Invalid order format.";

                    if (_cancellationTokens.ContainsKey(orderId))
                        return $"Order {orderId} already exists.";

                    int orderTime = Convert.ToInt16(parts[2]);
                    return await AddOrder(orderId, orderTime, client);

                case "CANCEL":
                    return CancelOrder(orderId);

                default:
                    return "Unknown command.";
            }
        }

        private static async Task<string> AddOrder(string orderId, int orderTime, TcpClient client)
        {
            var newOrder = new Order(orderId, orderTime);
            var cts = new CancellationTokenSource();

            lock (_orderQueueLock)
            {
                _orderQueue.Enqueue(newOrder);
            }

            lock (_cancellationTokensLock)
            {
                _cancellationTokens[orderId] = cts;
            }

            _ = ProcessOrderInBackgroundAsync(newOrder, cts.Token, client);

            return $"Order {orderId} received and processing started.";
        }

        private static string CancelOrder(string orderId)
        {
            lock (_cancellationTokensLock)
            {
                if (_cancellationTokens.TryGetValue(orderId, out var cts))
                {
                    cts.Cancel();
                    _cancellationTokens.Remove(orderId);

                    lock (_orderQueueLock)
                    {
                        var order = _orderQueue.FirstOrDefault(o => o.Id == orderId);
                        if (order != null)
                        {
                            _orderQueue = new Queue<Order>(_orderQueue.Where(o => o.Id != orderId));
                        }
                    }

                    return $"Order {orderId} has been cancelled.";
                }
                else
                {
                    return $"Order {orderId} not found.";
                }
            }
        }

        private static async Task ProcessOrderInBackgroundAsync(Order order, CancellationToken token, TcpClient client)
        {
            try
            {
                await Task.Delay(order.Time, token);

                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"Order {order.Id} was cancelled.");
                    return;
                }

                order.Status = "Ready to receive.";
                await NotifyClientOrderReady(order, client);


                lock (_cancellationTokensLock)
                {
                    _cancellationTokens.Remove(order.Id);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Order {order.Id} was cancelled during processing.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing order {order.Id}: {e.Message}");
            }
        }

        private static async Task NotifyClientOrderReady(Order order, TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                string response = $"Order {order.Id} is ready.";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes);
                Console.WriteLine($"Order {order.Id} is ready, response sent.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending response for order {order.Id}: {e.Message}");
            }
        }
    }

    class Order
    {
        public string Id { get; set; }
        public string Status { get; set; } = "In progress";
        public int Time { get; set; }

        public Order(string id, int time)
        {
            Id = id;
            Time = time;
        }
    }
}