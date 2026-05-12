using System;
using System.Threading.Tasks;
using PushPelmesh.App;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.App.Auth
{
    public class LoginScreen : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputField seriesInput;
        [SerializeField] private InputField numberInput;

        [Header("Buttons")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button guestButton;

        [Header("Status")]
        [SerializeField] private Text statusText;

        [Header("Navigation")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        private void Awake()
        {
            PushPelmesh.App.ScreenOrientationPolicy.AllowAnyOrientation();
            loginButton.onClick.AddListener(OnLoginButtonClicked);
            guestButton.onClick.AddListener(OnGuestButtonClicked);
        }

        private void OnDestroy()
        {
            loginButton.onClick.RemoveListener(OnLoginButtonClicked);
            guestButton.onClick.RemoveListener(OnGuestButtonClicked);
        }

        private async void OnLoginButtonClicked()
        {
            string series = seriesInput.text.Trim();
            string number = numberInput.text.Trim();

            if (string.IsNullOrWhiteSpace(series))
            {
                SetStatus("Введите серию ключа");
                return;
            }

            if (string.IsNullOrWhiteSpace(number))
            {
                SetStatus("Введите номер ключа");
                return;
            }

            await RunAuthRequestAsync(async () =>
            {
                AuthResponse response =
                    await AuthService.LoginByKeyAsync(series, number);

                UserProfileResponse profile = await AuthService.GetProfileAsync();
                SessionManager.SetProfile(profile);

                SetStatus($"Вход выполнен: {response.user.displayName}");

                SceneManager.LoadScene(mainMenuSceneName);
            });
        }

        private async void OnGuestButtonClicked()
        {
            await RunAuthRequestAsync(async () =>
            {
                AuthResponse response = await AuthService.LoginAsGuestAsync();

                UserProfileResponse profile = await AuthService.GetProfileAsync();

                SessionManager.SetProfile(profile);

                SetStatus($"Гостевой вход: {response.user.displayName}");

                SceneManager.LoadScene(mainMenuSceneName);
            });
        }

        private async Task RunAuthRequestAsync(Func<Task> request)
        {
            SetInteractable(false);

            SetStatus("Подключение к серверу...");

            try
            {
                await request();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);

                SetStatus("Ошибка входа");
            }
            finally
            {
                SetInteractable(true);
            }
        }

        private void SetInteractable(bool value)
        {
            loginButton.interactable = value;
            guestButton.interactable = value;

            seriesInput.interactable = value;
            numberInput.interactable = value;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
