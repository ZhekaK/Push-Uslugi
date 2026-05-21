using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;
using UnityEngine.Networking;

namespace PushPelmesh.App.Notifications
{
    public static class ModuleNotificationsApi
    {
        private const string BasePath = "/api/module-notifications";

        public static async Task<ModuleNotificationsResponse> GetUnreadAsync()
        {
            string json = await ApiClient.GetAsync(BasePath + "/unread", withAuth: true);
            ModuleNotificationsResponse response = JsonUtility.FromJson<ModuleNotificationsResponse>(json);
            return response ?? new ModuleNotificationsResponse();
        }

        public static Task MarkReadAsync(string moduleKey)
        {
            string escapedModuleKey = UnityWebRequest.EscapeURL(moduleKey);
            return ApiClient.PostJsonAsync(BasePath + "/" + escapedModuleKey + "/read", "{}", withAuth: true);
        }
    }
}
