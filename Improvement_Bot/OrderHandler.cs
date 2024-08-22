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
                currentOrder.id_Executor = executorId;  // Устанавливаем ID Исполнителя
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

                    // Отправляем информацию о поручении с кнопками
                    await SendOrderSummaryWithButtons(botClient, chatId, currentOrder);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Неверный формат даты. Попробуйте снова:");
                }
                break;
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

    public static async Task SaveOrderAsync(Order order, ITelegramBotClient botClient, long chatId)
    {
        var orderJson = JsonConvert.SerializeObject(order);
        var content = new StringContent(orderJson, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        if (order.id_Order == 0) // Если ID заказа отсутствует, создаем новый заказ
        {
            response = await Api.client.PostAsync(Api.APP_PATH + "/api/Orders", content);
            Console.WriteLine("ПЛОХО");

        }
        else
        {
            response = await Api.client.PostAsync(Api.APP_PATH + "/api/orders", content);
            Console.WriteLine("ПЛОХО");

        }

        Console.WriteLine("контент" + content);
        Console.WriteLine("жопа" + orderJson);

        Console.WriteLine("Путь " + (Api.APP_PATH + "/api/orders/"));
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status Code: {response.StatusCode}");
        Console.WriteLine($"Response Body: {responseBody}");

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
            // Возвращаемся в меню в зависимости от роли
                await MenuManager.ShowMenu(botClient, chatId, menuState);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Не удалось определить состояние меню.");
        }
    }
}
