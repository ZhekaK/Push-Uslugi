using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.Durak
{
    public static class DurakApi
    {
        private const string BasePath = "/api/durak/rooms";

        public static async Task<DurakRoomsResponse> GetRoomsAsync()
        {
            string json = await ApiClient.GetAsync(BasePath, withAuth: true);
            DurakRoomsResponse response = JsonUtility.FromJson<DurakRoomsResponse>(json);
            return response ?? new DurakRoomsResponse();
        }

        public static async Task<DurakRoomDto> GetRoomAsync(int roomId)
        {
            string json = await ApiClient.GetAsync(BasePath + "/" + roomId, withAuth: true);
            return ParseRoom(json);
        }

        public static async Task<DurakRoomDto> CreateRoomAsync(CreateDurakRoomRequest request)
        {
            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath, json, withAuth: true);
            return ParseRoom(responseJson);
        }

        public static async Task<DurakRoomDto> JoinRoomAsync(int roomId, string password)
        {
            JoinDurakRoomRequest request = new JoinDurakRoomRequest
            {
                password = password
            };

            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/join", json, withAuth: true);
            return ParseRoom(responseJson);
        }

        public static Task<DurakRoomDto> StartAsync(int roomId)
        {
            return PostEmptyAsync(roomId, "start");
        }

        public static Task<DurakRoomDto> TakeAsync(int roomId)
        {
            return PostEmptyAsync(roomId, "take");
        }

        public static Task<DurakRoomDto> PassAsync(int roomId)
        {
            return PostEmptyAsync(roomId, "pass");
        }

        public static Task<DurakRoomDto> AttackAsync(int roomId, string cardCode)
        {
            return PostCardAsync(roomId, "attack", cardCode, string.Empty);
        }

        public static Task<DurakRoomDto> TransferAsync(int roomId, string cardCode)
        {
            return PostCardAsync(roomId, "transfer", cardCode, string.Empty);
        }

        public static Task<DurakRoomDto> DefendAsync(int roomId, string attackCardCode, string defenseCardCode)
        {
            return PostCardAsync(roomId, "defend", defenseCardCode, attackCardCode);
        }

        private static async Task<DurakRoomDto> PostEmptyAsync(int roomId, string action)
        {
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/" + action, "{}", withAuth: true);
            return ParseRoom(responseJson);
        }

        private static async Task<DurakRoomDto> PostCardAsync(int roomId, string action, string cardCode, string attackCardCode)
        {
            DurakCardActionRequest request = new DurakCardActionRequest
            {
                cardCode = cardCode,
                attackCardCode = attackCardCode
            };

            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + roomId + "/" + action, json, withAuth: true);
            return ParseRoom(responseJson);
        }

        private static DurakRoomDto ParseRoom(string json)
        {
            DurakRoomResponse response = JsonUtility.FromJson<DurakRoomResponse>(json);
            return response != null ? response.room : null;
        }
    }
}
