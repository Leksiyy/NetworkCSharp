using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace homework8;

static class Program
{
    private const string Token = "8024308133:AAENMdjPK4yr1Va5E-TKnUCIt11EAZkEjDg";
    static ITelegramBotClient _bot = new TelegramBotClient(Token);
    static readonly Dictionary<long, int> UserScores = new Dictionary<long, int>();
    static readonly Dictionary<long, int> UserQuestionsIndex = new Dictionary<long, int>();
    static readonly List<Question> Questions = new List<Question>();
    static readonly CancellationTokenSource Cts = new CancellationTokenSource();

    static async Task Main()
    {
        _bot = new TelegramBotClient(Token);

        FillQuestions();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _bot.StartReceiving(
            HandleUpdateAsync, // логика
            HandleErrorAsync, // логгер
            receiverOptions,
            Cts.Token
        );

        Console.WriteLine("Bot started");
        Console.ReadLine();
        await Cts.CancelAsync();
    }

    // для удобного добавления вопросов
    private static void FillQuestions()
    {
        Questions.Add(new Question
        {
            Text = "Кто написал картину 'Мона Лиза'?",
            Options = ["Леонардо да Винчи", "Винсент Ван Гог", "Пабло Пикассо", "Клод Моне"],
            CorrectOptionIndex = 0
        });
        Questions.Add(new Question
        {
            Text = "Какой художник известен своими картинами из кувшинов и ренессансных шедевров?",
            Options = ["Рафаэль", "Диего Веласкес", "Микеланджело", "Сандро Боттичелли"],
            CorrectOptionIndex = 1
        });
        Questions.Add(new Question
        {
            Text = "В каком году был создан 'Звездная ночь' Винсента Ван Гога?",
            Options = ["1889", "1895", "1883", "1879"],
            CorrectOptionIndex = 0
        });
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;
            string messageText = update.Message.Text ?? string.Empty;

            if (messageText == "/start")
            {
                UserScores[chatId] = 0;
                UserQuestionsIndex[chatId] = 0;
                await botClient.SendMessage(chatId, "Привет! Давай сыграем в викторину по искусству!", cancellationToken: cancellationToken);
                await AskQuestion(botClient, chatId, cancellationToken);
            }
            else
            {
                if (UserQuestionsIndex.ContainsKey(chatId))
                {
                    int questionIndex = UserQuestionsIndex[chatId];
                    Question currentQuestion = Questions[questionIndex];

                    if (int.TryParse(messageText, out var selectedOption) && selectedOption > 0 && selectedOption <= currentQuestion.Options.Count)
                    {
                        if (selectedOption - 1 == currentQuestion.CorrectOptionIndex)
                        {
                            UserScores[chatId] += 1;
                            await botClient.SendMessage(chatId, "Правильно!", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await botClient.SendMessage(chatId, $"Неправильно! Правильный ответ: {currentQuestion.Options[currentQuestion.CorrectOptionIndex]}", cancellationToken: cancellationToken);
                        }

                        UserQuestionsIndex[chatId]++;

                        if (UserQuestionsIndex[chatId] < Questions.Count)
                        {
                            await AskQuestion(botClient, chatId, cancellationToken);
                        }
                        else
                        {
                            int score = UserScores[chatId];
                            await botClient.SendMessage(chatId, $"Игра окончена! Ваш результат: {score}/{Questions.Count} правильных ответов.", cancellationToken: cancellationToken);
                            UserScores.Remove(chatId);
                            UserQuestionsIndex.Remove(chatId);
                        }
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Пожалуйста, введите номер ответа.", cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    await botClient.SendMessage(chatId, "Напишите /start для начала игры.", cancellationToken: cancellationToken);
                }
            }
        }
    }

    private static async Task AskQuestion(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        int questionIndex = UserQuestionsIndex[chatId];
        Question question = Questions[questionIndex];

        string optionsText = "";
        for (int i = 0; i < question.Options.Count; i++)
        {
            optionsText += $"{i + 1}. {question.Options[i]}\n";
        }

        string questionText = $"{question.Text}\n\n{optionsText}";
        await botClient.SendMessage(chatId, questionText, cancellationToken: cancellationToken);
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}

class Question
{
    public required string Text { get; init; }
    public required List<string> Options { get; init; }
    public required int CorrectOptionIndex { get; init; }
}
