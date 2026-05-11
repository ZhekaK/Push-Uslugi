namespace PushPelmesh.App.Api
{
    public static class ApiConfig
    {
#if UNITY_EDITOR
        public const string BaseUrl = "http://localhost:5028";
#else
        public const string BaseUrl = "https://push-pelmesh.ru";
#endif
    }
}