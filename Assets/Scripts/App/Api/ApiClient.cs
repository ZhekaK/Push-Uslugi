using System;
using System.Text;
using System.Threading.Tasks;
using PushPelmesh.App.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace PushPelmesh.App.Api
{
    public static class ApiClient
    {
        public static async Task<string> GetAsync(string path, bool withAuth = false)
        {
            using var request = UnityWebRequest.Get(ApiConfig.BaseUrl + path);

            if (withAuth)
                AddAuthHeader(request);

            await SendAsync(request);

            return request.downloadHandler.text;
        }

        public static async Task<string> PostJsonAsync(string path, string json, bool withAuth = false)
        {
            using var request = new UnityWebRequest(ApiConfig.BaseUrl + path, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            if (withAuth)
                AddAuthHeader(request);

            await SendAsync(request);

            return request.downloadHandler.text;
        }

        private static void AddAuthHeader(UnityWebRequest request)
        {
            var token = TokenStorage.GetToken();

            if (!string.IsNullOrWhiteSpace(token))
                request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        private static async Task SendAsync(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new ApiException(
                    request.responseCode,
                    request.downloadHandler.text,
                    $"API error: {request.responseCode} {request.error}");
            }
        }
    }
}