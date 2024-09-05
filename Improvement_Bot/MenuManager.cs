using System.ComponentModel.Design;
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
            var orderCreationState = UserSessionManager.GetOrderCreationState(chatId);
            var reportCreationState = UserSessionManager.GetReportCreationState(chatId);

            // Проверяем состояния создания заказа и отчета
            if (orderCreationState != UserSessionManager.OrderCreationState.None ||
                reportCreationState != UserSessionManager.ReportCreationState.None)
            {

                // Обработка команды сохранения или редактирования отчета
                /*   if (data == "save_report" || data == "edit_report")
                   {
                            await ReportHandler.HandleCallbackQuery(botClient, callbackQuery);
                           return;
                   }
                */
                // Если текущее состояние активно, но команда не относится к сохранению/редактированию, показываем сообщение
                UserSessionManager.ClearOrderCreationState(chatId);
                UserSessionManager.ClearReportCreationState(chatId);

                return;
            }

            // Обработка других коллбеков
            switch (data)
            {
                case var s when s.StartsWith("assign_task_"):
                    int executorId;
                    if (int.TryParse(data.Substring("assign_task_".Length), out executorId))
                    {
                        UserSessionManager.SetExecutorId(chatId, executorId);
                        await botClient.SendTextMessageAsync(chatId, "Исполнитель выбран. Пожалуйста, введите заголовок поручения.");
                        UserSessionManager.SetOrderCreationState(chatId, UserSessionManager.OrderCreationState.Header);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Ошибка выбора исполнителя.");
                    }
                    break;

                case var s when s.StartsWith("save_order_"):
                    var currentOrder = UserSessionManager.GetCurrentOrder(chatId);
                    if (currentOrder != null)
                    {
                        await OrderHandler.SaveOrderAsync(currentOrder, botClient, chatId);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Текущий заказ не найден.");
                    }
                    break;
                case var s when s.StartsWith("_photo_report_"):
                    var currentReport = UserSessionManager.GetCurrentReport(chatId);
                    if (currentReport != null)
                    {
                        await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте фото для добавления в отчет.");
                        UserSessionManager.SetReportCreationState(chatId, UserSessionManager.ReportCreationState.AddingPhoto);
                        return;

                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Текущий заказ не найден.");
                    }
                    break;


                case var s when s.StartsWith("edit_order_"):
                    await botClient.SendTextMessageAsync(chatId, "Редактирование поручения пока не реализовано.");
                    break;

                case "admin_users":
                    await UsersMenuHandler.ShowUsersMenu(botClient, chatId);
                    break;

                case "admin_tasks":
                    await ShowTasksMenu(botClient, chatId);
                    break;

                case "task_list_tasks":
                case "task_employee_tasks":
                case "user_prev_order":
                case "user_next_order":
                case "task_add_task":
                case "task_back":
                    await OrderHandler.HandleOrderCommands(botClient, chatId, data);
                    break;

                case "user_list_users":
                case "user_find_user":
                case "user_back":
                case "user_assign_task":
                case "user_prev_user":
                case "user_next_user":
                    await UserHandler.HandleUserCommands(botClient, chatId, data);
                    break;

                case "report_write_report":
                case "report_back":
                    await ReportHandler.HandleReportCommands(botClient, chatId, data);
                    break;

                case var s when s.StartsWith("make_report"):
                    await ReportHandler.HandleReportCommands(botClient, chatId, s);
                    break;


                case "add_more_photo":
                case "finish_report":
                    await PhotoHandler.HandleCallbackQuery(botClient, callbackQuery);
                    break;

                case "save_report":
                case "edit_report":
                    await ReportHandler.HandleCallbackQuery(botClient, callbackQuery);
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

        public static async Task ShowUserMenu(ITelegramBotClient botClient, long chatId)
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