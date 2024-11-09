using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiceGame;

class Program
{
    private const string ApiKey = "01a70e39-fdb5-47a8-a0a0-9c7a2a65b805";
    private const string ApiUrl = "https://api.random.org/json-rpc/4/invoke";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в игру 'Кости'!");
        Console.WriteLine("Выберите режим игры: ");
        Console.WriteLine("1. Человек-человек");
        Console.WriteLine("2. Человек-компьютер");

        int gameMode = int.Parse(Console.ReadLine());
        if (gameMode == 1)
        {
            await PlayHumanVsHuman();
        }
        else if (gameMode == 2)
        {
            await PlayHumanVsComputer();
        }
        else
        {
            Console.WriteLine("Неверный выбор. Завершение программы.");
        }
    }

    private static async Task PlayHumanVsHuman()
    {
        Console.WriteLine("Режим: Человек-человек");
        int player1Score = await RollDice("Игрок 1", false);
        int player2Score = await RollDice("Игрок 2", false);

        Console.WriteLine($"Игрок 1 набрал: {player1Score}");
        Console.WriteLine($"Игрок 2 набрал: {player2Score}");

        if (player1Score > player2Score)
        {
            Console.WriteLine("Игрок 1 побеждает!");
        }
        else if (player2Score > player1Score)
        {
            Console.WriteLine("Игрок 2 побеждает!");
        }
        else
        {
            Console.WriteLine("Ничья!");
        }
    }

    private static async Task PlayHumanVsComputer()
    {
        Console.WriteLine("Режим: Человек-компьютер");
        int playerScore = await RollDice("Игрок", false);
        int computerScore = await RollDice("Компьютер", true);

        Console.WriteLine($"Игрок набрал: {playerScore}");
        Console.WriteLine($"Компьютер набрал: {computerScore}");

        if (playerScore > computerScore)
        {
            Console.WriteLine("Вы победили!");
        }
        else if (computerScore > playerScore)
        {
            Console.WriteLine("Компьютер победил!");
        }
        else
        {
            Console.WriteLine("Ничья!");
        }
    }

    private static async Task<int> RollDice(string playerName, bool isRobot)
    {
        if (isRobot)
        {
            Console.WriteLine(playerName + " бросает кости..");
            Thread.Sleep(1000);
            
            int[] rolls = await GetRandomNumbers(2, 1, 6);
            int sum = rolls[0] + rolls[1];

            Console.WriteLine($"{playerName} выбросил {rolls[0]} и {rolls[1]}, сумма: {sum}");
            return sum;
        }
        else
        {
            Console.WriteLine($"{playerName}, нажмите Enter для броска костей...");
            Console.ReadLine();

            int[] rolls = await GetRandomNumbers(2, 1, 6);
            int sum = rolls[0] + rolls[1];

            Console.WriteLine($"{playerName} выбросил {rolls[0]} и {rolls[1]}, сумма: {sum}");
            return sum;
        }
    }

    private static async Task<int[]> GetRandomNumbers(int num, int min, int max)
    {
        using (var client = new HttpClient())
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "generateIntegers",
                @params = new
                {
                    apiKey = ApiKey,
                    n = num,
                    min = min,
                    max = max,
                    replacement = true
                },
                id = 1
            };

            string jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiUrl, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonResponse);
            JsonElement resultElement = doc.RootElement.GetProperty("result").GetProperty("random").GetProperty("data");
            
            int[] numbers = new int[num];
            for (int i = 0; i < num; i++)
            {
                numbers[i] = resultElement[i].GetInt32();
            }
            return numbers;
        }
    }
}