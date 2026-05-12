using System;
using System.Runtime.InteropServices;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.App
{
    public static class ScreenOrientationPolicy
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PushUslugiSetOrientationMode(string mode);
#endif

        public static void AllowAnyOrientation()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            SetWebGlOrientationMode("any");
        }

        public static void UseLandscapeOnly()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            SetWebGlOrientationMode("landscape");
        }

        private static void SetWebGlOrientationMode(string mode)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                PushUslugiSetOrientationMode(mode);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"WebGL orientation request failed: {exception.Message}");
            }
#endif
        }
    }
}

namespace PushPelmesh.App.MainMenu
{
    public class MainMenuScreen : MonoBehaviour
    {
        [Serializable]
        public class ServiceModule
        {
            public Button Button;
            public string SceneName;
            public bool GuestAvailable;
        }
        [Header("UI")]
        [SerializeField] private Text profileText;
        [SerializeField] private Text welcomeText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button logoutButton;
        [SerializeField] private ServiceModule[] serviceModules;
        [SerializeField] private Button profileButton;

        [Header("Navigation")]
        [SerializeField] private string loginSceneName = "LoginScene";

        private void Awake()
        {
            PushPelmesh.App.ScreenOrientationPolicy.AllowAnyOrientation();
            logoutButton.onClick.AddListener(OnLogoutClicked);
            foreach (ServiceModule serviceModule in serviceModules)
            {
                serviceModule.Button.onClick.AddListener(() => OnServiceButtonClicked(serviceModule.SceneName));
            }
        }

        private void Start()
        {
            LoadProfile();
        }

        private void ApplyAccess(UserProfileResponse profile)
        {
            bool isGuest = profile == null || profile.type == "Guest";

            if (profileButton != null)
                profileButton.interactable = !isGuest;

            foreach (ServiceModule serviceModule in serviceModules)
            {
                if (serviceModule.Button != null)
                    serviceModule.Button.interactable = !isGuest || serviceModule.GuestAvailable;
            }
        }

        private void OnDestroy()
        {
            logoutButton.onClick.RemoveListener(OnLogoutClicked);
            foreach (ServiceModule serviceModule in serviceModules)
            {
                serviceModule.Button.onClick.RemoveAllListeners();
            }
        }

        private void OnServiceButtonClicked(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public async void LoadProfile()
        {
            SetStatus("Загрузка профиля...");

            try
            {
                UserProfileResponse profile = SessionManager.CurrentProfile;

                if (profile == null)
                {
                    profile = await AuthService.GetProfileAsync();
                    SessionManager.SetProfile(profile);
                }
                welcomeText.text = $"Добро пожаловать, {profile.firstName}!";

                profileText.text =
                    $"Тип: {ShowType(profile.type)}\n" +
                    $"Имя: {profile.firstName}\n" +
                    $"Фамилия: {profile.middleName}\n" +
                    $"Отчество: {profile.lastName}\n" +
                    $"Вес: {profile.weightKg} кг\n" +
                    $"Дата рождения: {profile.birthDate}";

                SetStatus("Профиль загружен");
                ApplyAccess(profile);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);

                ApplyAccess(null);
                SetStatus("Ошибка загрузки профиля. Выполните вход заново.");
            }
        }

        private string ShowType(string type)
        {
            switch (type)
            {
                case "Guest":
                    return "Гость";
                case "KeyUser":
                    return "Гражданин";
                case "Admin":
                    return "Администратор";
                default:
                    return $"Добро пожаловать, {type}!";
            }
        }

        private void OnLogoutClicked()
        {
            SessionManager.Logout();

            SceneManager.LoadScene(loginSceneName);
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
