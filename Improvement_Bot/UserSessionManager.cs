using System.Collections.Concurrent;

namespace Improvement_Bot
{
    public static class UserSessionManager
    {
        private static readonly ConcurrentDictionary<long, int> UserIndices = new ConcurrentDictionary<long, int>();
        private static Dictionary<long, OrderCreationState> orderCreationStates = new();
        private static Dictionary<long, string> orderHeaders = new();
        private static Dictionary<long, string> orderTexts = new();
        private static Dictionary<long, DateTime> orderDeadlines = new();
        private static ConcurrentDictionary<long, Order> currentOrders = new ConcurrentDictionary<long, Order>();

        private static Dictionary<long, int> executorIds = new();  // Хранение ID исполнителя

        // Методы для управления состоянием создания поручений
        public static void SetExecutorId(long chatId, int executorId) => executorIds[chatId] = executorId;
        public static int GetExecutorId(long chatId) => executorIds.TryGetValue(chatId, out var executorId) ? executorId : 0;

        public static void SetOrderCreationState(long chatId, OrderCreationState state) => orderCreationStates[chatId] = state;
        public static OrderCreationState GetOrderCreationState(long chatId) => orderCreationStates.TryGetValue(chatId, out var state) ? state : OrderCreationState.None;

        public static void SetOrderHeader(long chatId, string header) => orderHeaders[chatId] = header;
        public static string GetOrderHeader(long chatId) => orderHeaders.TryGetValue(chatId, out var header) ? header : string.Empty;

        public static void SetOrderText(long chatId, string text) => orderTexts[chatId] = text;
        public static string GetOrderText(long chatId) => orderTexts.TryGetValue(chatId, out var text) ? text : string.Empty;

        public static void SetOrderDeadline(long chatId, DateTime deadline) => orderDeadlines[chatId] = deadline;
        public static DateTime GetOrderDeadline(long chatId) => orderDeadlines.TryGetValue(chatId, out var deadline) ? deadline : DateTime.MinValue;

        public static void ClearOrderCreationState(long chatId)
        {
            orderCreationStates.Remove(chatId);
            orderHeaders.Remove(chatId);
            orderTexts.Remove(chatId);
            orderDeadlines.Remove(chatId);
        }

        public enum ReportCreationState
        {
            None,       // Нет активного процесса создания отчета
            Header,     // Ввод заголовка отчета
            Text,        // Ввод текста отчета
            AddingPhoto
        }

        private static Dictionary<long, ReportCreationState> reportCreationStates = new();
        private static ConcurrentDictionary<long, Report> currentReports = new ConcurrentDictionary<long, Report>();
        private static readonly Dictionary<long, int> _reportMessageIds = new Dictionary<long, int>();

        public static void SetReportMessageId(long chatId, int messageId)
        {
            _reportMessageIds[chatId] = messageId;
        }

        public static int GetReportMessageId(long chatId)
        {
            return _reportMessageIds.TryGetValue(chatId, out var messageId) ? messageId : 0;
        }
    

    public static void SetReportCreationState(long chatId, ReportCreationState state) => reportCreationStates[chatId] = state;
        public static ReportCreationState GetReportCreationState(long chatId) => reportCreationStates.TryGetValue(chatId, out var state) ? state : ReportCreationState.None;

        public static void ClearReportCreationState(long chatId)
        {
            reportCreationStates.Remove(chatId);
        }

        public static Report GetCurrentReport(long chatId)
        {
            if (!currentReports.TryGetValue(chatId, out var report))
            {
                report = new Report();  // Используем метод для создания нового отчета
                currentReports[chatId] = report;
            }
            return report;
        }

        public static void ClearCurrentReport(long chatId)
        {
            currentReports.TryRemove(chatId, out _);
        }
        public static void ClearCurrentOrder(long chatId)
        {
            currentReports.TryRemove(chatId, out _);
        }

        public enum OrderCreationState
        {
            None,       // Нет активного процесса создания поручения
            Header,     // Ввод заголовка поручения
            Text,       // Ввод текста поручения
            Deadline    // Ввод срока выполнения поручения
        }

        public static int GetCurrentUserIndex(long chatId)
        {
            if (!UserIndices.ContainsKey(chatId))
            {
                UserIndices[chatId] = 0;
            }

            return UserIndices[chatId];
        }

        public static Order GetCurrentOrder(long chatId)
        {
            if (!currentOrders.TryGetValue(chatId, out var order))
            {
                order = new Order();  // Используем метод для получения текущего пользователя
                currentOrders[chatId] = order;
            }
            return order;
        }


        public static void SetCurrentUserIndex(long chatId, int index)
        {
            UserIndices[chatId] = index;
        }

        public static void SetCurrentReport(long chatId, Report report)
        {
            currentReports[chatId] = report;
        }
    }
}
