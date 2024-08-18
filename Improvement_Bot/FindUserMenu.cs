using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Improvement_Bot
{
    public static class FindUserMenu
    {
        public static async Task ShowFindUserMenu(ITelegramBotClient botClient, long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Поиск по фамилии", "search_by_lastname"),
                    InlineKeyboardButton.WithCallbackData("Назад", "back")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Меню поиска пользователя:", replyMarkup: inlineKeyboard);
        }
    }
}
