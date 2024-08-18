using Improvement_Bot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;

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
                    await AuthManager.HandleAuthorization(botClient, message);
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
