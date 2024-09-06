    using Improvement_Bot;
    using Telegram.Bot.Types.ReplyMarkups;
    using Telegram.Bot.Types;
    using Telegram.Bot;
    using Newtonsoft.Json;
    using System.Text;

    public static class ReportHandler
    {

        public static async Task HandleReportCreation(ITelegramBotClient botClient, long chatId, string userInput, Message message)
        {
            var reportCreationState = UserSessionManager.GetReportCreationState(chatId);
            var currentReport = UserSessionManager.GetCurrentReport(chatId);

            switch (reportCreationState)
            {
                case UserSessionManager.ReportCreationState.Header:
                    currentReport.Header = userInput;
                    UserSessionManager.SetReportCreationState(chatId, UserSessionManager.ReportCreationState.Text);
                    await botClient.SendTextMessageAsync(chatId, "Введите текст отчета:");
                    break;

                case UserSessionManager.ReportCreationState.Text:
                    currentReport.Text = userInput;

                    var reportInfo = $"Отчет:\n\n" +
                                     $"Заголовок: {currentReport.Header}\n" +
                                     $"Текст: {currentReport.Text}\n" +
                                     $"ID Поручения: {currentReport.id_Order}";

                    await ReportHandler.SaveReportAsync(botClient, chatId);

                UserSessionManager.ClearOrderCreationState(chatId);
                UserSessionManager.ClearReportCreationState(chatId);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Фото", $"_photo_report_{currentReport.id_Report}"),
                        InlineKeyboardButton.WithCallbackData("Изменить", $"edit_report_{currentReport.id_Report}")
                    }
                });

                    await botClient.SendTextMessageAsync(chatId, reportInfo, replyMarkup: inlineKeyboard);
             //       UserSessionManager.SetReportCreationState(chatId, UserSessionManager.ReportCreationState.AddingPhoto);
                    break;

                case UserSessionManager.ReportCreationState.AddingPhoto:
                PhotoHandler.HandlePhotoMessage(botClient, message );
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId, "Неизвестное состояние создания отчета.");
                    break;
            }
        }

        public static async Task HandleReportCommands(ITelegramBotClient botClient, long chatId, string command)
        {
            if (command.StartsWith("make_report"))
            {
                await StartReportCreation(botClient, chatId, command);
                return;
            }

            if (UserSessionManager.GetReportCreationState(chatId) != UserSessionManager.ReportCreationState.None)
            {
                await HandleReportCreation(botClient, chatId, command, null);
                return;
            }

            await botClient.SendTextMessageAsync(chatId, "Неизвестная команда или не начат процесс создания отчета.");
        }

        public static async Task SaveReportAsync(ITelegramBotClient botClient, long chatId)
        {
            var currentReport = UserSessionManager.GetCurrentReport(chatId);

            var reportJson = JsonConvert.SerializeObject(currentReport);
            var content = new StringContent(reportJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (currentReport.id_Report == null)
            {
                response = await Api.client.PostAsync(Api.APP_PATH + "/api/reports", content);

            if (response.IsSuccessStatusCode)
            {
              var  responseB = await response.Content.ReadAsStringAsync();

                // Предположим, что сервер возвращает созданный отчет с id_Report
                var savedReport = JsonConvert.DeserializeObject<Report>(responseB);

                if (savedReport != null && savedReport.id_Report.HasValue)
                {
                    // Обновляем id_Report в текущем отчете
                    currentReport.id_Report = savedReport.id_Report;

                    // Сохраняем обновленный отчет в UserSessionManager
                    UserSessionManager.SetCurrentReport(chatId, currentReport);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Ошибка при создании отчета.");
            }


        }
        else
            {
                response = await Api.client.PutAsync(Api.APP_PATH + $"/api/reports/{currentReport.id_Report}", content);
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await botClient.SendTextMessageAsync(chatId, "Отчет успешно сохранен.");
            //    UserSessionManager.ClearCurrentReport(chatId); // Очистка текущего отчета
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"Ошибка при сохранении отчета: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        private static async Task StartReportCreation(ITelegramBotClient botClient, long chatId, string command)
        {
            UserSessionManager.SetReportCreationState(chatId, UserSessionManager.ReportCreationState.Header);
            int idOrder = int.Parse(command.Substring("make_report".Length).Trim());

            var currentReport = new Report
            {
                Date_Of_Writing = DateTime.Now,
                Accepted = false,
                id_Order = idOrder
            };

            UserSessionManager.SetCurrentReport(chatId, currentReport);
            await botClient.SendTextMessageAsync(chatId, "Введите заголовок отчета:");
        }

        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;


            if (data.StartsWith("_photo_report_"))
            {
                var reportId = int.Parse(data.Split('_')[2]);
                UserSessionManager.SetReportCreationState(chatId, UserSessionManager.ReportCreationState.AddingPhoto);
                await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте фотографию для добавления в отчет.");
                return;
            }

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
    }

