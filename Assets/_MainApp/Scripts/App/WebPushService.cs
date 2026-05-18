using System.Runtime.InteropServices;
using PushPelmesh.App.Auth;
using UnityEngine;

public static class WebPushService
{
    private const string VapidPublicKey = "ПУБЛИЧНЫЙ_КЛЮЧ";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterWebPushFromUnity(string publicKey, string jwt);
#endif

    public static void Register()
    {
        string token = TokenStorage.GetToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            Debug.LogWarning("JWT token is empty. Cannot register Web Push.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        RegisterWebPushFromUnity(VapidPublicKey, token);
#else
        Debug.Log("Web Push registration works only in WebGL build.");
#endif
    }
}