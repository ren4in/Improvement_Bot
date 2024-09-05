using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Improvement_Bot;
using static Improvement_Bot.UserSessionManager;
using Telegram.Bot.Requests;
using static System.Runtime.InteropServices.JavaScript.JSType;
public static class OrderHandler
{
    public static async Task HandleOrderCreation(ITelegramBotClient botClient, long chatId, string userInput)
    {
        if (!Api.LoadUserData(out string token, out string role, out int? userId))
        {
            await botClient.SendTextMessageAsync(chatId, "Ошибка: не удалось загрузить данные пользователя.");
            return;
        }

        var orderCreationState = UserSessionManager.GetOrderCreationState(chatId);
        var currentOrder = UserSessionManager.GetCurrentOrder(chatId);
        var executorId = UserSessionManager.GetExecutorId(chatId);

        switch (orderCreationState)
        {
            case UserSessionManager.OrderCreationState.Header:
                currentOrder.Header = userInput;
                currentOrder.id_Supervisor = userId;
                currentOrder.id_Executor = executorId;
                currentOrder.Accepted = false;

                UserSessionManager.SetOrderCreationState(chatId, UserSessionManager.OrderCreationState.Text);
                await botClient.SendTextMessageAsync(chatId, "Введите текст поручения:");
                break;

            case UserSessionManager.OrderCreationState.Text:
                currentOrder.Text = userInput;
                UserSessionManager.SetOrderCreationState(chatId, UserSessionManager.OrderCreationState.Deadline);
                await botClient.SendTextMessageAsync(chatId, "Введите срок выполнения (в формате ГГГГ-ММ-ДД):");
                break;

            case UserSessionManager.OrderCreationState.Deadline:
                if (DateTime.TryParse(userInput, out DateTime deadline))
                {
                    currentOrder.Deadline = deadline;
                    currentOrder.Date_of_Issue = DateTime.Now;

                    // Отправляем информацию о поручении с кнопками (как новое сообщение)
                    await SendOrderSummaryWithButtons(botClient, chatId, currentOrder);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Неверный формат даты. Попробуйте снова:");
                }
                break;
        }
    }

    private static async Task ShowOrderDetails(ITelegramBotClient botClient, long chatId, int orderIndex)
    {
        if (OrderDataStore.allOrders == null || OrderDataStore.allOrders.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Список поручений пуст.");
            return;
        }

        orderIndex = (orderIndex + OrderDataStore.allOrders.Count) % OrderDataStore.allOrders.Count;

        UserSessionManager.SetCurrentUserIndex(chatId, orderIndex);

        var order = OrderDataStore.allOrders[orderIndex];
        var orderInfo = $"Заголовок: {order.Header}\n" +
                        $"Дата выдачи: {order.Date_of_Issue}\n" +
                        $"Содержание: {order.Text}\n" +
                        $"Срок выполнения: {order.Deadline}\n" +
                        $"ID: {order.id_Executor}";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⬅️", "user_prev_order"),
                InlineKeyboardButton.WithCallbackData("Написать отчет", $"make_report{order.id_Order}"),
                InlineKeyboardButton.WithCallbackData("➡️", "user_next_order")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "user_back")
            }
        });

        int? messageId = UserSessionManager.GetReportMessageId(chatId);

        if (messageId == null)
        {
            var sentMessage = await botClient.SendTextMessageAsync(chatId, orderInfo, replyMarkup: inlineKeyboard);
            UserSessionManager.SetReportMessageId(chatId, sentMessage.MessageId);
        }
        else
        {
            try
            {
              await botClient.EditMessageTextAsync(chatId, messageId.Value, orderInfo, replyMarkup: inlineKeyboard);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.Message.Contains("message to edit not found"))
            {
                var sentMessage = await botClient.SendTextMessageAsync(chatId, orderInfo, replyMarkup: inlineKeyboard);
                UserSessionManager.SetReportMessageId(chatId, sentMessage.MessageId);
            }
        }
    }

    private static async Task SendOrderSummaryWithButtons(ITelegramBotClient botClient, long chatId, Order order)
    {
        var summaryMessage = $"Поручение:\n\n" +
                             $"ID Руководителя: {order.id_Supervisor}\n" +
                             $"ID Исполнителя: {order.id_Executor}\n" +
                             $"Заголовок: {order.Header}\n" +
                             $"Дата выдачи: {order.Date_of_Issue?.ToString("yyyy-MM-dd")}\n" +
                             $"Срок выполнения: {order.Deadline?.ToString("yyyy-MM-dd")}\n" +
                             $"Текст поручения: {order.Text}";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Сохранить", $"save_order_{order.id_Order}"),
                InlineKeyboardButton.WithCallbackData("Изменить", $"edit_order_{order.id_Order}")
            }
        });

        await botClient.SendTextMessageAsync(chatId, summaryMessage, replyMarkup: inlineKeyboard);
    }

    public static async Task HandleOrderCommands(ITelegramBotClient botClient, long chatId, string command)
    {
        string role;

        switch (command)
        {
            case "task_list_tasks":
                if (Api.LoadUserData(out _, out role, out int? userId))
                {
                    await Api.LoadOrders(userId);
                    await ShowOrderDetails(botClient, chatId, 0);
                }
                break;

            case "task_employee_tasks":
                await botClient.SendTextMessageAsync(chatId, "Введите ID сотрудника для получения поручений:");
                break;

            case "task_add_task":
                await botClient.SendTextMessageAsync(chatId, "Введите данные нового поручения:");
                break;

            case "user_prev_order":
                int prevIndex = UserSessionManager.GetCurrentUserIndex(chatId) - 1;
                await ShowOrderDetails(botClient, chatId, prevIndex);
                break;

            case "user_next_order":
                int nextIndex = UserSessionManager.GetCurrentUserIndex(chatId) + 1;
                await ShowOrderDetails(botClient, chatId, nextIndex);
                break;

            case "task_back":
                if (Api.LoadUserData(out _, out role, out _))
                {
                    await MenuManager.ShowMenu(botClient, chatId, role);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Не удалось загрузить данные пользователя.");
                }
                break;

            default:
                await botClient.SendTextMessageAsync(chatId, "Неизвестная команда.");
                break;
        }
    }

    public static async Task SaveOrderAsync(Order order, ITelegramBotClient botClient, long chatId)
    {
        var orderJson = JsonConvert.SerializeObject(order);
        var content = new StringContent(orderJson, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        if (order.id_Order == null)
        {
            response = await Api.client.PostAsync(Api.APP_PATH + "/api/Orders", content);
        }
        else
        {
            response = await Api.client.PostAsync(Api.APP_PATH + "/api/orders", content);
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            await botClient.SendTextMessageAsync(chatId, "Поручение успешно сохранено.");
            await ReturnToPreviousMenu(botClient, chatId);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, $"Ошибка при сохранении поручения: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }

    private static async Task ReturnToPreviousMenu(ITelegramBotClient botClient, long chatId)
    {
        var menuState = MenuStateManager.GetMenuState(chatId);
        if (!string.IsNullOrEmpty(menuState))
        {
            await MenuManager.ShowMenu(botClient, chatId, menuState);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Не удалось определить состояние меню.");
        }
    }
}
