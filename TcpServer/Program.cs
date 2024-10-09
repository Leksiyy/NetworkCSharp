using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServer;

class Program
{
    static void Main()
    {
        StartServer();
    }
 
    public static void StartServer()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8080;
        TcpListener listener = new TcpListener(ipAddress, port);
 
        try
        {
            listener.Start();
            Console.WriteLine("Сервер запущен...");

            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Новый клиент подключен.");

            NetworkStream stream = client.GetStream();

            byte[] data = new byte[256];
            int bytesRead = stream.Read(data, 0, data.Length);
            
            string message = Encoding.ASCII.GetString(data, 0, bytesRead);

            string response;
            if (message == "1")
            {
                response = DateTime.Now.ToString("HH:mm:ss");
            }
            else if (message == "2")
            {
                response = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else throw new Exception("Accepted message is corrupted");
            
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            
            stream.Write(responseBytes, 0, responseBytes.Length);
            
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        finally
        {
            listener.Stop();
        }
    }
}