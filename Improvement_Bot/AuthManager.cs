using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Improvement_Bot
{
    public static class AuthManager
    {
        private static ConcurrentDictionary<long, string> userStates = new ConcurrentDictionary<long, string>();
        private static ConcurrentDictionary<long, string> userLogins = new ConcurrentDictionary<long, string>();
        private static ConcurrentDictionary<long, UserInfo> userInfoDict = new ConcurrentDictionary<long, UserInfo>();

        static AuthManager()
        {
            // Загрузка состояний пользователей и информации о пользователях из файла
            userStates = Api.LoadUserStates();
            userInfoDict = Api.LoadUserInfo();
        }

        public static async Task HandleAuthorization(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            var userMessage = message.Text;

            if (userMessage == "/exit")
            {
                // Обработка команды выхода
                await HandleExit(botClient, chatId);
                return;
            }

            if (!userStates.ContainsKey(chatId))
            {
                userStates[chatId] = "awaiting_login";
                Api.SaveUserStates(userStates);
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
                        Api.SaveUserStates(userStates);
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
                            Api.SaveUserStates(userStates);
                            Api.SaveUserInfo(userInfoDict);
                            await botClient.SendTextMessageAsync(chatId, $"Вы успешно вошли в систему! Ваш ID: {userId}");
                            await MenuManager.ShowMenu(botClient, chatId, role);
                        }
                        catch (HttpRequestException ex)
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Ошибка авторизации: {ex.Message}");
                            userStates[chatId] = "awaiting_login";
                            Api.SaveUserStates(userStates);
                        }
                        break;

                    case "authenticated":
                        if (userInfoDict.TryGetValue(chatId, out UserInfo userInfo))
                        {
                            await MenuManager.ShowMenu(botClient, chatId, userInfo.Role);
                        }
                        break;
                }
            }
        }

        public static async Task HandleExit(ITelegramBotClient botClient, long chatId)
        {
            userStates.TryRemove(chatId, out _);
            userLogins.TryRemove(chatId, out _);
            userInfoDict.TryRemove(chatId, out _);
            Api.ClearUserData();
            Api.SaveUserStates(userStates);
            Api.SaveUserInfo(userInfoDict);

            await botClient.SendTextMessageAsync(chatId, "Вы успешно вышли из аккаунта.");
        }
    }
}
