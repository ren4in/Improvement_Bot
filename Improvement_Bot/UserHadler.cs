using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Improvement_Bot
{
    public static class UserHandler
    {
        public static async Task HandleUserCommands(ITelegramBotClient botClient, long chatId, string command)
        {
            switch (command)
            {
                case "user_list_users":
                    await botClient.SendTextMessageAsync(chatId, "Список пользователей: [Тут будет список]");
                    break;

                case "user_find_user":
                    await botClient.SendTextMessageAsync(chatId, "Введите фамилию пользователя:");
                    break;

                case "user_back":
                    // Возврат в предыдущее меню
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
    }
}
