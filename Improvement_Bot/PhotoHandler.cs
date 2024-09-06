using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Improvement_Bot
{
    public static class PhotoHandler
    {
        public static async Task HandlePhotoMessage(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            var reportCreationState = UserSessionManager.GetReportCreationState(chatId);

            if (reportCreationState == UserSessionManager.ReportCreationState.AddingPhoto)
            {
                await HandleReportPhoto(botClient, chatId, message);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Фотография не требуется в данный момент.");
            }
        }
        private static async Task HandleReportPhoto(ITelegramBotClient botClient, long chatId, Message message)
        {
            if (message.Photo == null || !message.Photo.Any())
            {
                await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте фотографию.");
                return;
            }

            var photo = message.Photo.LastOrDefault();
            var fileId = photo?.FileId;

            if (fileId == null)
            {
                await botClient.SendTextMessageAsync(chatId, "Не удалось получить фото.");
                return;
            }

            try
            {
                var file = await botClient.GetFileAsync(fileId);
                byte[] fileBytes;

                using (var memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(file.FilePath, memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var currentReport = UserSessionManager.GetCurrentReport(chatId);
                if (currentReport != null)
                {
                    var reportImage = new Report_Image
                    {
                        id_Report = currentReport.id_Report,
                        RowGuid = Guid.NewGuid(),
                        Image = fileBytes
                    };

                    SaveReportImage(reportImage);

                    await botClient.SendTextMessageAsync(chatId, "Фотография добавлена к отчету.");

                    // Предлагаем пользователю добавить еще фото
                    UserSessionManager.ClearReportCreationState(chatId);

                    var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                        new[]
                        {
                    new[]
                    {
                        Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Да", "add_more_photo"),
                        Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Нет", "finish_report")
                    }
                        });

                    await botClient.SendTextMessageAsync(chatId, "Добавить еще фото?", replyMarkup: keyboard);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Текущий отчет не найден.");
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, $"Ошибка при обработке фотографии: {ex.Message}");
            }
        }


        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            if (callbackQuery.Data == "add_more_photo")
            {
                // Устанавливаем состояние для добавления еще одной фотографии
                UserSessionManager.SetReportCreationState(chatId, UserSessionManager.ReportCreationState.AddingPhoto);
                await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте еще одну фотографию.");
            }
            else if (callbackQuery.Data == "finish_report")
            {
                // Очищаем состояние и возвращаемся в главное меню
                UserSessionManager.ClearReportCreationState(chatId);
                UserSessionManager.ClearCurrentReport(chatId);
                UserSessionManager.ClearOrderCreationState(chatId);


                await botClient.SendTextMessageAsync(chatId, "Отчет завершен. Возвращаюсь в главное меню.");
                // Здесь можно реализовать возвращение в главное меню (отправка других команд или кнопок)
                MenuManager.ShowUserMenu(botClient, chatId);
            }
        }

        private static async void SaveReportImage(Report_Image reportImage)
        {
            var imageJson = JsonConvert.SerializeObject(reportImage);
            var content = new StringContent(imageJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await Api.client.PostAsync(Api.APP_PATH + "/api/Report_Image", content);
            Console.WriteLine(response.StatusCode.ToString());

        }
    }
}
