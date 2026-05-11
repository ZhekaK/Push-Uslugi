using System;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.App.MainMenu
{
    public class MainMenuScreen : MonoBehaviour
    {
        [Serializable]
        public class ServiceModule
        {
            public Button Button;
            public string SceneName;
        }
        [Header("UI")]
        [SerializeField] private Text profileText;
        [SerializeField] private Text welcomeText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button logoutButton;
        [SerializeField] private ServiceModule[] serviceModules;

        [Header("Navigation")]
        [SerializeField] private string loginSceneName = "LoginScene";

        private void Awake()
        {
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
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);

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