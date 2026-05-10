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
        [Header("UI")]
        [SerializeField] private Text profileText;
        [SerializeField] private Text welcomeText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button logoutButton;

        [Header("Navigation")]
        [SerializeField] private string loginSceneName = "LoginScene";

        private void Awake()
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }

        private void Start()
        {
            LoadProfile();
        }

        private void OnDestroy()
        {
            logoutButton.onClick.RemoveListener(OnLogoutClicked);
        }

        private async void LoadProfile()
        {
            SetStatus("Загрузка профиля...");

            try
            {
                UserProfileResponse profile = SessionManager.CurrentProfile;

                welcomeText.text = $"Добро пожаловать, {profile.firstName}!";

                profileText.text =
                    $"ID: {profile.id}\n" +
                    $"Тип: {ShowType(profile.type)}\n" +
                    $"Имя: {profile.firstName}\n" +
                    $"Фамилия: {profile.middleName}\n" +
                    $"Отчество: {profile.lastName}\n" +
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