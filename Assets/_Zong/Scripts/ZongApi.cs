using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.Zong
{
    public static class ZongApi
    {
        private const string BasePath = "/api/zong/rooms";

        public static async Task<ZongRoomsResponse> GetRoomsAsync()
        {
            string json = await ApiClient.GetAsync(BasePath, withAuth: true);
            ZongRoomsResponse response = JsonUtility.FromJson<ZongRoomsResponse>(json);
            return response ?? new ZongRoomsResponse();
        }

        public static async Task<ZongRoomDto> GetRoomAsync(int roomId)
        {
            string json = await ApiClient.GetAsync(BasePath + "/" + roomId, withAuth: true);
            return ParseRoom(json);
        }

        public static async Task<ZongRoomDto> CreateRoomAsync(CreateZongRoomRequest request)
        {
            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath, json, withAuth: true);
            return ParseRoom(responseJson);
        }

        public static async Task<ZongRoomDto> JoinRoomAsync(int roomId, string password)
        {
            JoinZongRoomRequest request = new JoinZongRoomRequest
            {
                password = password
            };

            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/join", json, withAuth: true);
            return ParseRoom(responseJson);
        }

        public static async Task<ZongRoomDto> StartRoomAsync(int roomId)
        {
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/start", "{}", withAuth: true);
            return ParseRoom(responseJson);
        }

        public static async Task<ZongRoomDto> RollAsync(int roomId)
        {
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/roll", "{}", withAuth: true);
            return ParseRoom(responseJson);
        }

        public static async Task<ZongRoomDto> BankAsync(int roomId)
        {
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/bank", "{}", withAuth: true);
            return ParseRoom(responseJson);
        }

        private static ZongRoomDto ParseRoom(string json)
        {
            ZongRoomResponse response = JsonUtility.FromJson<ZongRoomResponse>(json);
            return response != null ? response.room : null;
        }
    }
}
