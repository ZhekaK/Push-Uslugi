using System;
using System.Threading.Tasks;
using PushPelmesh.App;
using PushPelmesh.App.Api;
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

        private GameObject rememberPanel;
        private InputField nameInput;
        private Button nameLoginButton;
        private Button useKeyButton;
        private Text rememberStatusText;

        private void Awake()
        {
            PushPelmesh.App.ScreenOrientationPolicy.AllowAnyOrientation();
            loginButton.onClick.AddListener(OnLoginButtonClicked);
            guestButton.onClick.AddListener(OnGuestButtonClicked);
        }

        private void Start()
        {
            if (TokenStorage.HasRememberToken())
                CreateRememberPanel();
        }

        private void OnDestroy()
        {
            loginButton.onClick.RemoveListener(OnLoginButtonClicked);
            guestButton.onClick.RemoveListener(OnGuestButtonClicked);
            if (nameLoginButton != null)
                nameLoginButton.onClick.RemoveListener(OnNameLoginClicked);
            if (useKeyButton != null)
                useKeyButton.onClick.RemoveListener(UseKeyLogin);
        }

        private async void OnNameLoginClicked()
        {
            string name = nameInput != null ? nameInput.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                SetRememberStatus("Введите имя из вашего ID");
                return;
            }

            SetRememberInteractable(false);
            SetRememberStatus("Проверка имени...");

            try
            {
                AuthResponse response = await AuthService.LoginByNameAsync(name);
                UserProfileResponse profile = await AuthService.GetProfileAsync();
                SessionManager.SetProfile(profile);
                SetRememberStatus("Вход выполнен: " + response.user.displayName);
                SceneManager.LoadScene(mainMenuSceneName);
            }
            catch (ApiException exception) when (exception.StatusCode == 401)
            {
                LoginByNameErrorResponse error = null;
                try { error = JsonUtility.FromJson<LoginByNameErrorResponse>(exception.ResponseBody); }
                catch (ArgumentException) { }

                if (error != null && error.requiresKeyLogin)
                {
                    TokenStorage.ClearRememberToken();
                    UseKeyLogin();
                    SetStatus("Три неверные попытки или вход по имени недоступен. Введите серию и номер.");
                }
                else
                {
                    int remaining = error != null ? error.attemptsRemaining : 0;
                    SetRememberStatus("Неверное имя. Осталось попыток: " + remaining);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                SetRememberStatus("Не удалось войти. Проверьте подключение.");
            }
            finally
            {
                SetRememberInteractable(true);
            }
        }

        private void UseKeyLogin()
        {
            TokenStorage.ClearRememberToken();
            if (rememberPanel != null)
                rememberPanel.SetActive(false);
        }

        private void SetRememberInteractable(bool interactable)
        {
            if (nameInput != null)
                nameInput.interactable = interactable;
            if (nameLoginButton != null)
                nameLoginButton.interactable = interactable;
            if (useKeyButton != null)
                useKeyButton.interactable = interactable;
        }

        private void SetRememberStatus(string message)
        {
            if (rememberStatusText != null)
                rememberStatusText.text = message;
        }

        private void CreateRememberPanel()
        {
            Canvas canvas = statusText != null ? statusText.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
                return;

            Font font = statusText.font;
            rememberPanel = new GameObject("RememberLoginOverlay", typeof(RectTransform), typeof(Image));
            RectTransform overlay = rememberPanel.GetComponent<RectTransform>();
            overlay.SetParent(canvas.transform, false);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;
            rememberPanel.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.82f);
            overlay.SetAsLastSibling();

            GameObject panelObject = new GameObject("RememberLoginCard", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            RectTransform card = panelObject.GetComponent<RectTransform>();
            card.SetParent(overlay, false);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = card.anchorMin;
            card.sizeDelta = new Vector2(760f, 620f);
            panelObject.GetComponent<Image>().color = new Color(0.96f, 0.95f, 1f);
            VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(38, 38, 36, 36);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateText(card, "Срок входа истёк", font, 36, 58f, FontStyle.Bold);
            CreateText(card, "Введите имя из вашего ID, чтобы получить новый доступ на 7 дней.", font, 25, 92f, FontStyle.Normal);
            nameInput = CreateInput(card, font);
            nameLoginButton = CreateButton(card, "Войти по имени", font, new Color(0.28f, 0.28f, 0.64f));
            useKeyButton = CreateButton(card, "Войти по серии и номеру", font, new Color(0.38f, 0.4f, 0.48f));
            rememberStatusText = CreateText(card, string.Empty, font, 22, 55f, FontStyle.Normal);
            nameLoginButton.onClick.AddListener(OnNameLoginClicked);
            useKeyButton.onClick.AddListener(UseKeyLogin);
        }

        private static Text CreateText(Transform parent, string value, Font font, int size, float height, FontStyle style)
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<LayoutElement>().preferredHeight = height;
            Text text = obj.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = new Color(0.15f, 0.12f, 0.25f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;
            text.supportRichText = false;
            return text;
        }

        private static InputField CreateInput(Transform parent, Font font)
        {
            GameObject obj = new GameObject("NameInput", typeof(RectTransform), typeof(Image), typeof(InputField), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<LayoutElement>().preferredHeight = 70f;
            Image image = obj.GetComponent<Image>();
            image.color = Color.white;
            InputField input = obj.GetComponent<InputField>();
            input.targetGraphic = image;
            Text value = CreateInputText(obj.transform, font, string.Empty, new Color(0.1f, 0.1f, 0.15f));
            Text placeholder = CreateInputText(obj.transform, font, "Имя", new Color(0.55f, 0.53f, 0.6f));
            input.textComponent = value;
            input.placeholder = placeholder;
            return input;
        }

        private static Text CreateInputText(Transform parent, Font font, string value, Color color)
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20f, 0f);
            rect.offsetMax = new Vector2(-20f, 0f);
            Text text = obj.GetComponent<Text>();
            text.font = font;
            text.fontSize = 27;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = value;
            text.supportRichText = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Font font, Color color)
        {
            GameObject obj = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<LayoutElement>().preferredHeight = 70f;
            Image image = obj.GetComponent<Image>();
            image.color = color;
            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateInputText(obj.transform, font, label, Color.white);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return button;
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
