using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace Improvement_Bot
{
    public static class Api
    {
        public const string APP_PATH = "http://localhost:44731";
        public static readonly HttpClient client = new HttpClient();
        public static string role = "";
        public static int? userId;
        private const string UserDataFile = "userData.json";
        private const string UserStatesFile = "userStates.json";
        private const string UserInfoFile = "userInfoDict.json";

        // Словарь для хранения состояний пользователей
        public static ConcurrentDictionary<long, string> UserStates = LoadUserStates();
        // Словарь для хранения информации о пользователях
        public static ConcurrentDictionary<long, UserInfo> UserInfoDict = LoadUserInfo();

        public static async Task<(int, string)> UserAuthAsync(string email, string password)
        {
            var values = new Dictionary<string, string>
            {
                { "Login", email },
                { "password", password }
            };
            var content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await client.PostAsync($"{APP_PATH}/token/?Login={email}&Password={password}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                userId = tokenResponse.id_User;
                string token = tokenResponse.access_token;
                role = tokenResponse.role;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                SaveUserData(token, role, userId.Value);
                return (userId.Value, role);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Unauthorized", null, response.StatusCode);
            }
            else
            {
                throw new HttpRequestException("Error", null, response.StatusCode);
            }
        }
        public static async Task SaveOrder(Order order)
        {
            var jsonContent = JsonConvert.SerializeObject(order);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(APP_PATH + "/api/Orders", content);

            if (!response.IsSuccessStatusCode)
            {
                // Обработка ошибок
            }
        }

        public static async Task LoadUsers()
        {
            HttpResponseMessage response = await Api.client.GetAsync(Api.APP_PATH + "/api/Users");

            if (response.IsSuccessStatusCode)
            {
                var usersJson = await response.Content.ReadAsStringAsync();
                UserDataStore.allUsers = JsonConvert.DeserializeObject<List<User>>(usersJson);
            }
            else
            {
                Console.WriteLine("Ошибка загрузки пользователей.");
            }
        }

        public static async Task LoadUsers(string searchText)
        {
            HttpResponseMessage response = await Api.client.GetAsync(Api.APP_PATH + "/api/Users/search?searchText=" + searchText);

            if (response.IsSuccessStatusCode)
            {
                var usersJson = await response.Content.ReadAsStringAsync();
                UserDataStore.allUsers = JsonConvert.DeserializeObject<List<User>>(usersJson);
            }
            else
            {
                Console.WriteLine("Ошибка загрузки пользователей.");
            }
        }
        public static void SaveUserData(string token, string role, int userId)
        {
            var userData = new { Token = token, Role = role, UserId = userId };
            File.WriteAllText(UserDataFile, JsonConvert.SerializeObject(userData));
        }

          public  static   bool LoadUserData(out string token, out string role, out int? userId)
        {
            if (File.Exists(UserDataFile))
            {
                var userData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(UserDataFile));
                token = (string)userData.Token;
                role = (string)userData.Role;
                userId = (int)userData.UserId;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return true;
            }
            token = null;
            role = null;
            userId = null;
            return false;
        }

        public static void ClearUserData()
        {
            if (File.Exists(UserDataFile))
            {
                File.Delete(UserDataFile);
            }
        }

        public static void SaveUserStates(ConcurrentDictionary<long, string> userStates)
        {
            File.WriteAllText(UserStatesFile, JsonConvert.SerializeObject(userStates));
        }

        public static ConcurrentDictionary<long, string> LoadUserStates()
        {
            if (File.Exists(UserStatesFile))
            {
                return JsonConvert.DeserializeObject<ConcurrentDictionary<long, string>>(File.ReadAllText(UserStatesFile));
            }
            return new ConcurrentDictionary<long, string>();
        }

        public static void SaveUserInfo(ConcurrentDictionary<long, UserInfo> userInfoDict)
        {
            File.WriteAllText(UserInfoFile, JsonConvert.SerializeObject(userInfoDict));
        }

        public static ConcurrentDictionary<long, UserInfo> LoadUserInfo()
        {
            if (File.Exists(UserInfoFile))
            {
                return JsonConvert.DeserializeObject<ConcurrentDictionary<long, UserInfo>>(File.ReadAllText(UserInfoFile));
            }
            return new ConcurrentDictionary<long, UserInfo>();
        }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string Role { get; set; }
    }
}
