using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Clinet;

class Program
{
    static async Task Main()
    {
        var words = new string[] { "red", "yellow", "blue", "green" };
 
        using (TcpClient tcpClient = new TcpClient())
        {
            await tcpClient.ConnectAsync("127.0.0.1", 8888);
            var stream = tcpClient.GetStream();
 
            var response = new List<byte>();
            int bytesRead = 10; 
            foreach (var word in words)
            {
                // считываем строку в массив байтов
                // при отправке добавляем маркер завершения сообщения - \n
                byte[] data = Encoding.UTF8.GetBytes(word + '\n');
 
                await stream.WriteAsync(data);
 
                // считываем данные до конечного символа
                while ((bytesRead = stream.ReadByte()) != '\n')
                {
                    // добавляем в буфер
                    response.Add((byte)bytesRead);
                }
                var translation = Encoding.UTF8.GetString(response.ToArray());
                Console.WriteLine($"Слово {word}: {translation}");
                response.Clear();
                // имитируем долговременную работу, чтобы одновременно несколько клиентов обрабатывались
                await Task.Delay(2000);
            }
 
            // отправляем маркер завершения подключения - END
            await stream.WriteAsync(Encoding.UTF8.GetBytes("END\n"));
            Console.ReadLine();
        }
    }
}