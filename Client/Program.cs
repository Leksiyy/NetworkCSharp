using System.Net.Sockets;
using System.Text;

namespace Client;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 8080);
            Console.WriteLine("Подключение успешно");

            NetworkStream networkStream = tcpClient.GetStream();

            Console.WriteLine("Вывести время - 1\nВывести дату - 2\n");
            string input;
            do
            {
                input = Console.ReadLine();
            } while (input != "1" && input != "2");
            
            byte[] data = Encoding.ASCII.GetBytes(input);
            networkStream.Write(data, 0, data.Length);
            
            byte[] buffer = new byte[256];
            int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            
            string response = Encoding.ASCII.GetString(buffer);
            Console.WriteLine(input == "1" ? $"Время на сервере: {response}" : $"Дата на сервере: {response}");
            
            tcpClient.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}