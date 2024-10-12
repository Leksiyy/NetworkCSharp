using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Server;

class Program
{
    private static string Rates { get; set; }

    static async Task Main(string[] args)
    {
        using (TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080))
        {
            try
            {
                listener.Start();
                Console.WriteLine("Server successfully started");
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                }
            }
            catch
            {
                Console.WriteLine("Server failed to start");
            }
            finally
            {
                listener.Stop();
            }
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        Console.WriteLine($"Log: Client {client.Client.RemoteEndPoint} connected at {DateTime.Now}");
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[512];
        int bytesRead;
        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string request = Encoding.ASCII.GetString(buffer, 0, bytesRead).ToUpper();
                await RefreshRates();
                if (request == "REFRESH")
                {
                    await RefreshRates();
                }
                else if (request.Contains(' '))
                {
                    string[] requests = request.Split(' ');
                    
                    JArray parsedRates = JArray.Parse(Rates);
                    JToken rate1 = parsedRates.FirstOrDefault(rate => rate["cc"].ToString() == requests[0]);
                    JToken rate2 = parsedRates.FirstOrDefault(rate => rate["cc"].ToString() == requests[1]);


                    if (rate1 == null && rate2 == null) // если курс не обновился
                    {
                        byte[] responseBytes = Encoding.ASCII.GetBytes("Rate service not available");
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    else if (requests[1] == "UAH") // так как в базе храняться только соотношения к UAH, а сомого UAH нет
                    {
                        Console.WriteLine($"Log: rate of {requests[0]} is {rate1}\nrate of {requests[1]} is 1"); // курс гривны - 1,
                                                                                                                 // а на клиент передаю соотношение
                        byte[] responseBytes = Encoding.ASCII.GetBytes(rate1["rate"].ToString());
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    else if (requests[0] == "UAH") // так как в базе храняться только соотношения к UAH, а сомого UAH нет
                    {
                        Console.WriteLine($"Log: rate of {requests[0]} is 1\nrate of {requests[1]} is {rate2}"); // курс гривны - 1,
                                                                                                                 // а на клиент передаю соотношение
                        decimal exchangeRate = 1 / decimal.Parse(rate2["rate"].ToString());
                        byte[] responseBytes = Encoding.ASCII.GetBytes(exchangeRate.ToString());
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    else if (rate1 == null || rate2 == null) // если курс не найден в базе
                    {
                        byte[] responseBytes = Encoding.ASCII.GetBytes("An error occurred due to a possibly incorrect request");
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    else // если все нормально
                    {
                        Console.WriteLine($"Log: rate of {requests[0]} is {rate1}\nrate of {requests[1]} is {rate2}");

                        decimal rateValue1 = (decimal)rate1["rate"];
                        decimal rateValue2 = (decimal)rate2["rate"];
                        decimal exchangeRate = rateValue1 / rateValue2;

                        byte[] responseBytes = Encoding.ASCII.GetBytes(exchangeRate.ToString());
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Console.WriteLine($"Log: Client {client.Client.RemoteEndPoint} disconnected at {DateTime.Now}");
            client.Close();
        }
    }
    
    public static async Task RefreshRates()
    {
        HttpClient client = new HttpClient();

        string apiUrl = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";

        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            Rates = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}