using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Improvement_Bot
{
    public static class TaskHandler
    {
        public static async Task HandleTaskCommands(ITelegramBotClient botClient, long chatId, string command)
        {
            switch (command)
            {
                case "task_list_tasks":
                    await botClient.SendTextMessageAsync(chatId, "Список поручений: [Тут будет список]");
                    break;

                case "task_employee_tasks":
                    await botClient.SendTextMessageAsync(chatId, "Введите ID сотрудника для получения поручений:");
                    break;

                case "task_add_task":
                    await botClient.SendTextMessageAsync(chatId, "Введите данные нового поручения:");
                    break;

                case "task_back":
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
                    await botClient.SendTextMessageAsync(chatId, "Неизвестная команда по задачам.");
                    break;
            }
        }
    }
}
