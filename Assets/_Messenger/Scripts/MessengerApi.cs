using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.Messenger
{
    public static class MessengerApi
    {
        private const string BasePath = "/api/messenger";

        public static async Task<List<MessengerUserDto>> GetUsersAsync()
        {
            string json = await ApiClient.GetAsync(BasePath + "/users", withAuth: true);
            MessengerUsersResponse response = JsonUtility.FromJson<MessengerUsersResponse>(json);
            return response?.users ?? new List<MessengerUserDto>();
        }

        public static async Task<List<MessengerChatDto>> GetChatsAsync()
        {
            string json = await ApiClient.GetAsync(BasePath + "/chats", withAuth: true);
            MessengerChatsResponse response = JsonUtility.FromJson<MessengerChatsResponse>(json);
            return response?.chats ?? new List<MessengerChatDto>();
        }

        public static async Task<MessengerChatDto> CreateDirectChatAsync(int userId)
        {
            string json = JsonUtility.ToJson(new CreateDirectChatRequest { userId = userId });
            string response = await ApiClient.PostJsonAsync(BasePath + "/chats/direct", json, withAuth: true);
            return JsonUtility.FromJson<MessengerChatResponse>(response)?.chat;
        }

        public static async Task<MessengerChatDto> CreateGroupChatAsync(string title, List<int> userIds)
        {
            string json = JsonUtility.ToJson(new CreateGroupChatRequest { title = title, userIds = userIds });
            string response = await ApiClient.PostJsonAsync(BasePath + "/chats/group", json, withAuth: true);
            return JsonUtility.FromJson<MessengerChatResponse>(response)?.chat;
        }

        public static async Task<List<MessengerMessageDto>> GetMessagesAsync(int chatId, long afterId)
        {
            string path = BasePath + "/chats/" + chatId.ToString(CultureInfo.InvariantCulture) +
                "/messages?afterId=" + afterId.ToString(CultureInfo.InvariantCulture);
            string json = await ApiClient.GetAsync(path, withAuth: true);
            MessengerMessagesResponse response = JsonUtility.FromJson<MessengerMessagesResponse>(json);
            return response?.messages ?? new List<MessengerMessageDto>();
        }

        public static async Task<MessengerMessageDto> SendMessageAsync(int chatId, string text)
        {
            string path = BasePath + "/chats/" + chatId.ToString(CultureInfo.InvariantCulture) + "/messages";
            string json = JsonUtility.ToJson(new SendMessengerMessageRequest { text = text });
            string response = await ApiClient.PostJsonAsync(path, json, withAuth: true);
            return JsonUtility.FromJson<MessengerMessageResponse>(response)?.message;
        }

        public static async Task SetMutedAsync(int chatId, bool muted)
        {
            string path = BasePath + "/chats/" + chatId.ToString(CultureInfo.InvariantCulture) + "/mute";
            await ApiClient.PostJsonAsync(path,
                JsonUtility.ToJson(new SetMessengerMuteRequest { muted = muted }), withAuth: true);
        }

        public static async Task MarkReadAsync(int chatId, long lastMessageId)
        {
            string path = BasePath + "/chats/" + chatId.ToString(CultureInfo.InvariantCulture) + "/read";
            await ApiClient.PostJsonAsync(path,
                JsonUtility.ToJson(new MarkMessengerReadRequest { lastMessageId = lastMessageId }), withAuth: true);
        }
    }
}
