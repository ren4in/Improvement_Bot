using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Improvement_Bot
{
    public static class UsersMenuHandler
    {
        public static async Task ShowUsersMenu(ITelegramBotClient botClient, long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все пользователи", "user_list_users"),
                    InlineKeyboardButton.WithCallbackData("Найти пользователя по фамилии", "find_user")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "back")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Меню пользователей:", replyMarkup: inlineKeyboard);
        }
    }
}
