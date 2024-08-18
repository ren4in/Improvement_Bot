using System.Collections.Concurrent;

namespace Improvement_Bot
{
    public static class MenuStateManager
    {
        // Словарь для хранения состояния меню пользователей
        private static readonly ConcurrentDictionary<long, string> UserMenuStates = new ConcurrentDictionary<long, string>();

        public static void SetMenuState(long chatId, string state)
        {
            UserMenuStates[chatId] = state;
        }

        public static string GetMenuState(long chatId)
        {
            return UserMenuStates.TryGetValue(chatId, out var state) ? state : null;
        }

        public static void ClearMenuState(long chatId)
        {
            UserMenuStates.TryRemove(chatId, out _);
        }
    }
}
