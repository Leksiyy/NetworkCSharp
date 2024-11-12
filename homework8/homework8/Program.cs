using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ArtQuizBot
{
    class Program
    {
        static ITelegramBotClient bot;
        static Dictionary<long, int> userScores = new Dictionary<long, int>();
        static Dictionary<long, int> userQuestionsIndex = new Dictionary<long, int>();
        static List<Question> questions = new List<Question>();
        static CancellationTokenSource cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            string token = "8024308133:AAENMdjPK4yr1Va5E-TKnUCIt11EAZkEjDg";
            bot = new TelegramBotClient(token);

            // Заполняем список вопросов
            FillQuestions();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cts.Token
            );

            Console.WriteLine("Bot started");
            Console.ReadLine();
            cts.Cancel();
        }

        // для удобного добавления вопросов
        private static void FillQuestions()
        {
            questions.Add(new Question
            {
                Text = "Кто написал картину 'Мона Лиза'?",
                Options = new List<string> { "Леонардо да Винчи", "Винсент Ван Гог", "Пабло Пикассо", "Клод Моне" },
                CorrectOptionIndex = 0
            });
            questions.Add(new Question
            {
                Text = "Какой художник известен своими картинами из кувшинов и ренессансных шедевров?",
                Options = new List<string> { "Рафаэль", "Диего Веласкес", "Микеланджело", "Сандро Боттичелли" },
                CorrectOptionIndex = 1
            });
            questions.Add(new Question
            {
                Text = "В каком году был создан 'Звездная ночь' Винсента Ван Гога?",
                Options = new List<string> { "1889", "1895", "1883", "1879" },
                CorrectOptionIndex = 0
            });
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                long chatId = update.Message.Chat.Id;
                string messageText = update.Message.Text;

                if (messageText == "/start")
                {
                    userScores[chatId] = 0;
                    userQuestionsIndex[chatId] = 0;
                    await botClient.SendTextMessageAsync(chatId, "Привет! Давай сыграем в викторину по искусству!", cancellationToken: cancellationToken);
                    await AskQuestion(botClient, chatId, cancellationToken);
                }
                else
                {
                    if (userQuestionsIndex.ContainsKey(chatId))
                    {
                        int questionIndex = userQuestionsIndex[chatId];
                        Question currentQuestion = questions[questionIndex];
                        int selectedOption;

                        if (int.TryParse(messageText, out selectedOption) && selectedOption > 0 && selectedOption <= currentQuestion.Options.Count)
                        {
                            if (selectedOption - 1 == currentQuestion.CorrectOptionIndex)
                            {
                                userScores[chatId] += 1;
                                await botClient.SendTextMessageAsync(chatId, "Правильно!", cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chatId, $"Неправильно! Правильный ответ: {currentQuestion.Options[currentQuestion.CorrectOptionIndex]}", cancellationToken: cancellationToken);
                            }

                            userQuestionsIndex[chatId]++;

                            if (userQuestionsIndex[chatId] < questions.Count)
                            {
                                await AskQuestion(botClient, chatId, cancellationToken);
                            }
                            else
                            {
                                int score = userScores[chatId];
                                await botClient.SendTextMessageAsync(chatId, $"Игра окончена! Ваш результат: {score}/{questions.Count} правильных ответов.", cancellationToken: cancellationToken);
                                userScores.Remove(chatId);
                                userQuestionsIndex.Remove(chatId);
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите номер ответа.", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Напишите /start для начала игры.", cancellationToken: cancellationToken);
                    }
                }
            }
        }

        private static async Task AskQuestion(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            int questionIndex = userQuestionsIndex[chatId];
            Question question = questions[questionIndex];

            string optionsText = "";
            for (int i = 0; i < question.Options.Count; i++)
            {
                optionsText += $"{i + 1}. {question.Options[i]}\n";
            }

            string questionText = $"{question.Text}\n\n{optionsText}";
            await botClient.SendTextMessageAsync(chatId, questionText, cancellationToken: cancellationToken);
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
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; }
    }
}
