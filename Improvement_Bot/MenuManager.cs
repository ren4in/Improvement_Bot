using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Improvement_Bot
{
    public static class MenuManager
    {
        public static async Task ShowMenu(ITelegramBotClient botClient, long chatId, string role)
        {
            MenuStateManager.SetMenuState(chatId, role);

            if (role == "Администратор")
            {
                await ShowAdminMenu(botClient, chatId);
            }
            else if (role == "Пользователь")
            {
                await ShowUserMenu(botClient, chatId);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Роль не определена.");
            }
        }

        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var data = callbackQuery.Data;
            var chatId = callbackQuery.Message.Chat.Id;

            // Логирование данных для отладки
            Console.WriteLine($"Received callback data: {data}");

            switch (data)
            {
                case "admin_users":
                    await UsersMenuHandler.ShowUsersMenu(botClient, chatId);
                    break;

                case "admin_tasks":
                    await ShowTasksMenu(botClient, chatId);
                    break;

                case "task_list_tasks":
                case "task_employee_tasks":
                case "task_add_task":
                case "task_back":
                    await TaskHandler.HandleTaskCommands(botClient, chatId, data);
                    break;

                case "user_list_users":
                case "user_find_user":
                case "user_back":
                    await UserHandler.HandleUserCommands(botClient, chatId, data);
                    break;

                case "report_write_report":
                case "report_back":
                    await ReportHandler.HandleReportCommands(botClient, chatId, data);
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId, $"Неизвестная команда: {data}");
                    break;
            }

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        private static async Task ShowAdminMenu(ITelegramBotClient botClient, long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Пользователи", "admin_users"),
                    InlineKeyboardButton.WithCallbackData("Поручения", "admin_tasks")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: inlineKeyboard);
        }

        private static async Task ShowUserMenu(ITelegramBotClient botClient, long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Поручения", "task_list_tasks"),
                    InlineKeyboardButton.WithCallbackData("Написать отчет", "report_write_report")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: inlineKeyboard);
        }

        public static async Task ShowUsersMenu(ITelegramBotClient botClient, long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все пользователи", "user_list_users"),
                    InlineKeyboardButton.WithCallbackData("Найти пользователя по фамилии", "user_find_user")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "user_back")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Меню пользователей:", replyMarkup: inlineKeyboard);
        }

        public static async Task ShowTasksMenu(ITelegramBotClient botClient, long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все поручения", "task_list_tasks"),
                    InlineKeyboardButton.WithCallbackData("Поручения сотрудника", "task_employee_tasks"),
                    InlineKeyboardButton.WithCallbackData("Добавить поручение", "task_add_task")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "task_back")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Меню поручений:", replyMarkup: inlineKeyboard);
        }
    }
}
