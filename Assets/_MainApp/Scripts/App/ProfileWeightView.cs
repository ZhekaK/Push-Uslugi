using System;
using System.Globalization;
using PushPelmesh.App.Auth;
using PushPelmesh.App.MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.App.Profile
{
    public class ProfileWeightView : MonoBehaviour
    {
        [SerializeField] private InputField weightInput;
        [SerializeField] private Button saveWeightButton;
        [SerializeField] private Text statusText;
        [SerializeField] private MainMenuScreen mainMenuScreen;

        private void Awake()
        {
            saveWeightButton.onClick.AddListener(OnSaveClicked);
        }

        private void Start()
        {
            var profile = SessionManager.CurrentProfile;

            if (profile != null && profile.weightKg > 0)
            {
                weightInput.text = profile.weightKg.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void OnDestroy()
        {
            saveWeightButton.onClick.RemoveListener(OnSaveClicked);
        }

        private async void OnSaveClicked()
        {
            string input = weightInput.text.Trim().Replace(',', '.');

            if (!float.TryParse(
                    input,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float weight))
            {
                SetStatus("Введите корректный вес");
                return;
            }

            if (weight <= 0 || weight > 500)
            {
                SetStatus("Вес должен быть от 1 до 500 кг");
                return;
            }

            saveWeightButton.interactable = false;
            SetStatus("Сохранение...");

            try
            {
                await AuthService.UpdateWeightAsync(weight);

                SetStatus("Вес сохранён");
                mainMenuScreen.LoadProfile();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                SetStatus("Ошибка сохранения веса");
            }
            finally
            {
                saveWeightButton.interactable = true;
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}