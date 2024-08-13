using Improvement_Bot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Concurrent;
using System.Net;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static ConcurrentDictionary<long, string> userStates;
    private static ConcurrentDictionary<long, string> userLogins = new ConcurrentDictionary<long, string>();
    private static ConcurrentDictionary<long, UserInfo> userInfoDict = new ConcurrentDictionary<long, UserInfo>();

    static async Task Main()
    {
        _botClient = new TelegramBotClient("7493796715:AAHSS8QGmqB1Y3S0YTuYs-U8fvBr-fiU0nY");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message },
            ThrowPendingUpdates = true,
        };

        // Загружаем состояния пользователей и информацию о пользователях из файла
        userStates = Api.LoadUserStates();
        userInfoDict = Api.LoadUserInfo();

        using var cts = new CancellationTokenSource();
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1);
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                var chatId = message.Chat.Id;
                var userMessage = message.Text;

                if (userMessage == "/exit")
                {
                    userStates.TryRemove(chatId, out _);
                    userInfoDict.TryRemove(chatId, out _);
                    Api.ClearUserData();
                    Api.SaveUserStates(userStates);  // Сохраняем состояния пользователей после изменения
                    Api.SaveUserInfo(userInfoDict);  // Сохраняем информацию о пользователях после изменения
                    await botClient.SendTextMessageAsync(chatId, "Вы успешно вышли из аккаунта.");
                    return;
                }

                if (!userStates.ContainsKey(chatId))
                {
                    // Если пользователь еще не авторизован
                    userStates[chatId] = "awaiting_login";
                    Api.SaveUserStates(userStates);  // Сохраняем состояния пользователей после изменения
                    await botClient.SendTextMessageAsync(chatId, "Введите ваш логин:");
                }
                else
                {
                    var state = userStates[chatId];

                    switch (state)
                    {
                        case "awaiting_login":
                            userLogins[chatId] = userMessage;
                            userStates[chatId] = "awaiting_password";
                            Api.SaveUserStates(userStates);  // Сохраняем состояния пользователей после изменения
                            await botClient.SendTextMessageAsync(chatId, "Введите ваш пароль:");
                            break;

                        case "awaiting_password":
                            var login = userLogins[chatId];
                            var password = userMessage;

                            try
                            {
                                var (userId, role) = await Api.UserAuthAsync(login, password);
                                userStates[chatId] = "authenticated";
                                userInfoDict[chatId] = new UserInfo { UserId = userId, Role = role };
                                Api.SaveUserStates(userStates);  // Сохраняем состояния пользователей после изменения
                                Api.SaveUserInfo(userInfoDict);  // Сохраняем информацию о пользователях после авторизации
                                await botClient.SendTextMessageAsync(chatId, $"Вы успешно вошли в систему! Ваш ID: {userId}");
                            }
                            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                await botClient.SendTextMessageAsync(chatId, "Неверный логин или пароль. Попробуйте снова.");
                                userStates[chatId] = "awaiting_login";
                                Api.SaveUserStates(userStates);  // Сохраняем состояния пользователей после изменения
                            }
                            catch (HttpRequestException ex)
                            {
                                await botClient.SendTextMessageAsync(chatId, $"Ошибка авторизации: {ex.Message}");
                                userStates[chatId] = "awaiting_login";
                                Api.SaveUserStates(userStates);  // Сохраняем состояния пользователей после изменения
                            }
                            break;

                        case "authenticated":
                            if (userInfoDict.TryGetValue(chatId, out UserInfo userInfo))
                            {
                                await botClient.SendTextMessageAsync(chatId, $"Ваш ID: {userInfo.UserId}\n{userMessage}");
                            }
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}