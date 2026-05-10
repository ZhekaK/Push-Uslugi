using UnityEngine;

namespace PushPelmesh.App.Auth
{
    public static class TokenStorage
    {
        private const string TokenKey = "jwt_token";

        public static void SaveToken(string token)
        {
            PlayerPrefs.SetString(TokenKey, token);
            PlayerPrefs.Save();
        }

        public static string GetToken()
        {
            return PlayerPrefs.GetString(TokenKey, "");
        }

        public static bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(GetToken());
        }

        public static void ClearToken()
        {
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.Save();
        }
    }
}