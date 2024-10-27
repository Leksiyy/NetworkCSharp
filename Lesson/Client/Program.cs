using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Clinet;

class Program
{
    static string uri = "http://localhost:5000/api/users";
    private static HttpClient _httpClient = new HttpClient();
    static async Task Main()
    {
        List<Person>? persons = await _httpClient.GetFromJsonAsync<List<Person>>(uri);
        if (persons is not null)
        {
            foreach (var person in persons)
            {
                Console.WriteLine(person.Name);
            }
        }
    }
}

class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
}