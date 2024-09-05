using Improvement_Bot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using static Improvement_Bot.UserSessionManager;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    static async Task Main()
    {
        _botClient = new TelegramBotClient("7493796715:AAHSS8QGmqB1Y3S0YTuYs-U8fvBr-fiU0nY");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            ThrowPendingUpdates = true,
        };

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
                if (message != null)
                {
                    // Проверяем, находится ли пользователь в процессе создания поручения
                    var chatId = message.Chat.Id;
                    var orderCreationState = UserSessionManager.GetOrderCreationState(chatId);
                    var reportCreationState = UserSessionManager.GetReportCreationState(chatId);

                    if (orderCreationState != OrderCreationState.None)
                    {
                        // Обрабатываем шаги создания поручения
                        await OrderHandler.HandleOrderCreation(botClient, chatId, message.Text);
                    }
                    else if (reportCreationState!=ReportCreationState.None)
                    {
                        await ReportHandler.HandleReportCreation(botClient, chatId, message.Text, message);

                    }
                    else
                    {
                        // Если процесс создания поручения не активен, выполняем авторизацию
                        await AuthManager.HandleAuthorization(botClient, message);
                    }
                   
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                await MenuManager.HandleCallbackQuery(botClient, callbackQuery);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var errorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
