using System.Threading.Tasks;
using PushPelmesh.App.Api;
using PushPelmesh.App.Models;
using UnityEngine;

namespace PushPelmesh.App.Auth
{
    public static class AuthService
    {
        public static async Task<AuthResponse> LoginAsGuestAsync()
        {
            string json = "{}";

            string responseJson = await ApiClient.PostJsonAsync(
                "/api/auth/guest",
                json);

            var response = JsonUtility.FromJson<AuthResponse>(responseJson);

            TokenStorage.SaveToken(response.token);

            return response;
        }

        public static async Task<AuthResponse> LoginByKeyAsync(string series, string number)
        {
            var request = new LoginByKeyRequest
            {
                series = series,
                number = number
            };

            string json = JsonUtility.ToJson(request);

            string responseJson = await ApiClient.PostJsonAsync(
                "/api/auth/login-by-key",
                json);

            var response = JsonUtility.FromJson<AuthResponse>(responseJson);

            TokenStorage.SaveToken(response.token);

            return response;
        }

        public static async Task<UserProfileResponse> GetProfileAsync()
        {
            string responseJson = await ApiClient.GetAsync(
                "/api/user/profile",
                withAuth: true);

            return JsonUtility.FromJson<UserProfileResponse>(responseJson);
        }

        public static async Task<UserRoleResponse> GetUserRolesAsync()
        {
            string responseJson = await ApiClient.GetAsync(
                "/api/user/role",
                withAuth: true);

            return JsonUtility.FromJson<UserRoleResponse>(responseJson);
        }

        public static async Task<UpdateWeightResponse> UpdateWeightAsync(float weightKg)
        {
            var request = new UpdateWeightRequest
            {
                weightKg = weightKg
            };

            string json = JsonUtility.ToJson(request);

            string responseJson = await ApiClient.PutJsonAsync(
                "/api/user/profile/weight",
                json,
                withAuth: true);

            var response = JsonUtility.FromJson<UpdateWeightResponse>(responseJson);

            UserProfileResponse profile = await GetProfileAsync();
            SessionManager.SetProfile(profile);

            return response;
        }
    }
}