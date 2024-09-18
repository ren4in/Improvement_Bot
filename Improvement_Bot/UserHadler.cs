using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Improvement_Bot
{
    public static class UserHandler
    {
        public static async Task HandleUserCommands(ITelegramBotClient botClient, long chatId, string command)
        {
            switch (command)
            {
                case "user_list_users":
                    await Api.LoadUsers();  // Ждем завершения загрузки перед использованием данных
                    await ShowUserDetails(botClient, chatId, 0);  // Начинаем с первого пользователя
                    break;

                case "user_prev_user":
                    await ShowPreviousUser(botClient, chatId);
                    break;

                case "user_next_user":
                    await ShowNextUser(botClient, chatId);
                    break;

                case "user_assign_task":
                    await StartOrderCreation(botClient, chatId);
                    break;
                case "tasks":
                    var executorId = UserSessionManager.GetExecutorId(chatId);
                    await botClient.SendTextMessageAsync(chatId, $"ID исполнителя: {executorId}");

                    break;

                case "user_back":
                    if (Api.LoadUserData(out _, out string role, out _))
                    {
                        await MenuManager.ShowMenu(botClient, chatId, role);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Ошибка при возврате в меню.");
                    }
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId, "Неизвестная команда по пользователям.");
                    break;
            }
        }

        // Начинаем процесс создания поручения
        private static async Task StartOrderCreation(ITelegramBotClient botClient, long chatId)
        {
            // Устанавливаем статус сессии как создание поручения
            UserSessionManager.SetOrderCreationState(chatId, UserSessionManager.OrderCreationState.Header);

            // Предлагаем пользователю ввести заголовок
            await botClient.SendTextMessageAsync(chatId, "Введите заголовок поручения:");
        }


        private static async Task ShowUserDetails(ITelegramBotClient botClient, long chatId, int userIndex)
        {
            if (UserDataStore.allUsers == null || UserDataStore.allUsers.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, "Список пользователей пуст.");
                return;
            }

            UserSessionManager.SetCurrentUserIndex(chatId, userIndex);

            var user = UserDataStore.allUsers[userIndex];
            var userInfo = $"ФИО: {user.LastName} {user.FirstName} {user.MiddleName}\n" +
                           $"Телефон: {user.Phone}\n" +
                           $"Логин: {user.Login}\n" +
                           $"Роль: {user.id_RoleNavigation.Name}\n" +
                           $"ID: {user.id_User}";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("⬅️", "user_prev_user"),
            InlineKeyboardButton.WithCallbackData("Дать поручение", $"assign_task_{user.id_User}"),
             InlineKeyboardButton.WithCallbackData("Все поручения", $"tasks_{user.id_User}"),

            InlineKeyboardButton.WithCallbackData("➡️", "user_next_user")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Назад", "user_back")
        }
    });

            await botClient.SendTextMessageAsync(chatId, userInfo, replyMarkup: inlineKeyboard);
        }

        private static async Task ShowPreviousUser(ITelegramBotClient botClient, long chatId)
        {
            var users = UserDataStore.allUsers;
            int currentIndex = UserSessionManager.GetCurrentUserIndex(chatId);
            int prevIndex = (currentIndex - 1 + users.Count) % users.Count;

            await ShowUserDetails(botClient, chatId, prevIndex);
        }

        private static async Task ShowNextUser(ITelegramBotClient botClient, long chatId)
        {
            var users = UserDataStore.allUsers;
            int currentIndex = UserSessionManager.GetCurrentUserIndex(chatId);
            int nextIndex = (currentIndex + 1) % users.Count;

            await ShowUserDetails(botClient, chatId, nextIndex);
        }

    }
    }
