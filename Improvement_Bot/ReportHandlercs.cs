using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Improvement_Bot
{
    public static class ReportHandler
    {
        public static async Task HandleReportCommands(ITelegramBotClient botClient, long chatId, string command)
        {
            switch (command)
            {
                case "write_report":
                    await botClient.SendTextMessageAsync(chatId, "Введите текст отчета:");
                    break;
                // Добавьте другие команды по мере необходимости
                default:
                    await botClient.SendTextMessageAsync(chatId, "Неизвестная команда по отчетам.");
                    break;
            }
        }
    }
}
