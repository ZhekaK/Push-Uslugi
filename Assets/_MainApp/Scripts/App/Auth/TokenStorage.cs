using UnityEngine;

namespace PushPelmesh.App.Auth
{
    public static class TokenStorage
    {
        private const string TokenKey = "jwt_token";
        private const string RememberTokenKey = "remember_login_token";

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

        public static void SaveRememberToken(string token)
        {
            PlayerPrefs.SetString(RememberTokenKey, token);
            PlayerPrefs.Save();
        }

        public static string GetRememberToken()
        {
            return PlayerPrefs.GetString(RememberTokenKey, "");
        }

        public static bool HasRememberToken()
        {
            return !string.IsNullOrWhiteSpace(GetRememberToken());
        }

        public static void ClearRememberToken()
        {
            PlayerPrefs.DeleteKey(RememberTokenKey);
            PlayerPrefs.Save();
        }
    }
}
