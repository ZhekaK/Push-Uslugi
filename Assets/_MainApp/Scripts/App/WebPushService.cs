using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PushPelmesh.App.Api;
using PushPelmesh.App.Auth;
using UnityEngine;

public static class WebPushService
{
    private const string VapidPublicKey = "BD_F8Ia34MoD3m4svbMmNya0mvM2XGtqwqVgyrYequ8JKeCevsiZZa9ensaw3IkyMmhHvh2iEATow9rOYxeTGJU";
    private const string UnsubscribePath = "/api/push/unsubscribe";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterWebPushFromUnity(
        string publicKey,
        string jwt,
        string apiBaseUrl,
        string gameObjectName,
        string stateCallbackMethodName);

    [DllImport("__Internal")]
    private static extern void GetWebPushSubscriptionStateFromUnity(string gameObjectName, string stateCallbackMethodName);

    [DllImport("__Internal")]
    private static extern void GetWebPushSubscriptionEndpointFromUnity(string gameObjectName, string endpointCallbackMethodName, string stateCallbackMethodName);

    [DllImport("__Internal")]
    private static extern void UnsubscribeWebPushLocallyFromUnity(string gameObjectName, string stateCallbackMethodName);
#endif

    [Serializable]
    private class UnsubscribePushRequest
    {
        public string Endpoint;
    }

    public static void Register(string gameObjectName = null, string stateCallbackMethodName = null)
    {
        string token = TokenStorage.GetToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            Debug.LogWarning("JWT token is empty. Cannot register Web Push.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        RegisterWebPushFromUnity(VapidPublicKey, token, ApiConfig.BaseUrl, gameObjectName, stateCallbackMethodName);
#else
        Debug.Log("Web Push registration works only in WebGL build.");
#endif
    }

    public static void RefreshSubscriptionState(string gameObjectName, string stateCallbackMethodName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetWebPushSubscriptionStateFromUnity(gameObjectName, stateCallbackMethodName);
#else
        Debug.Log("Web Push subscription state is available only in WebGL build.");
#endif
    }

    public static void RequestSubscriptionEndpoint(
        string gameObjectName,
        string endpointCallbackMethodName,
        string stateCallbackMethodName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetWebPushSubscriptionEndpointFromUnity(gameObjectName, endpointCallbackMethodName, stateCallbackMethodName);
#else
        Debug.Log("Web Push subscription endpoint is available only in WebGL build.");
#endif
    }

    public static void UnsubscribeLocally(string gameObjectName, string stateCallbackMethodName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnsubscribeWebPushLocallyFromUnity(gameObjectName, stateCallbackMethodName);
#else
        Debug.Log("Web Push local unsubscribe works only in WebGL build.");
#endif
    }

    public static async Task UnsubscribeEndpointAsync(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Push endpoint is empty.", nameof(endpoint));

        UnsubscribePushRequest request = new UnsubscribePushRequest
        {
            Endpoint = endpoint
        };

        string json = JsonUtility.ToJson(request);
        await ApiClient.PostJsonAsync(UnsubscribePath, json, withAuth: true);
    }
}
