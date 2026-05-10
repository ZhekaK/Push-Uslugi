using System;
using PushPelmesh.App.Api;
using PushPelmesh.App.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.App.Startup
{
    public class StartupScreen : MonoBehaviour
    {
        [SerializeField] private Text statusText;

        [SerializeField] private string loginSceneName = "LoginScene";

        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        private async void Start()
        {
            SetStatus("Проверка авторизации...");

            try
            {
                bool success =
                    await SessionManager.TryAutoLoginAsync();

                if (success)
                {
                    SetStatus("Автовход выполнен");

                    SceneManager.LoadScene(mainMenuSceneName);
                }
                else
                {
                    SetStatus("Требуется вход");

                    SceneManager.LoadScene(loginSceneName);
                }
            }
            catch (ApiException exception)
            {
                Debug.LogError(exception);

                SetStatus("Ошибка API");

                SceneManager.LoadScene(loginSceneName);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);

                SetStatus("Ошибка подключения");

                SceneManager.LoadScene(loginSceneName);
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}