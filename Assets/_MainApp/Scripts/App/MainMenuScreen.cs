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
        [SerializeField] private Text welcomeText;
        [SerializeField] private Button logoutButton;
        [SerializeField] private ServiceModule[] serviceModules;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button enableNotificationsButton;
        [SerializeField] private Button disableNotificationsButton;

        [Header("Navigation")]
        [SerializeField] private string loginSceneName = "LoginScene";

        private bool canUseNotifications;

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

            canUseNotifications = !isGuest;

            if (!canUseNotifications)
                SetNotificationButtonsVisible(false, false);

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

        public void EnableNotifications()
        {
            if (!canUseNotifications)
                return;

            WebPushService.Register(gameObject.name, nameof(OnPushSubscriptionStateChanged));
        }

        public void DisableNotifications()
        {
            if (!canUseNotifications)
                return;

            WebPushService.RequestSubscriptionEndpoint(
                gameObject.name,
                nameof(OnPushUnsubscribeEndpointReceived),
                nameof(OnPushSubscriptionStateChanged));
        }

        public void RefreshNotificationButtons()
        {
            if (!canUseNotifications)
            {
                SetNotificationButtonsVisible(false, false);
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            WebPushService.RefreshSubscriptionState(gameObject.name, nameof(OnPushSubscriptionStateChanged));
#else
            SetNotificationButtonsVisible(true, false);
#endif
        }

        public void OnPushSubscriptionStateChanged(string isSubscribedText)
        {
            if (!canUseNotifications)
            {
                SetNotificationButtonsVisible(false, false);
                return;
            }

            bool isSubscribed =
                isSubscribedText == "1" ||
                string.Equals(isSubscribedText, "true", StringComparison.OrdinalIgnoreCase);

            SetNotificationButtonsVisible(!isSubscribed, isSubscribed);
        }

        public async void OnPushUnsubscribeEndpointReceived(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                WebPushService.UnsubscribeLocally(gameObject.name, nameof(OnPushSubscriptionStateChanged));
                return;
            }

            try
            {
                await WebPushService.UnsubscribeEndpointAsync(endpoint);
                WebPushService.UnsubscribeLocally(gameObject.name, nameof(OnPushSubscriptionStateChanged));
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                RefreshNotificationButtons();
            }
        }

        public async void LoadProfile()
        {
            try
            {
                UserProfileResponse profile = SessionManager.CurrentProfile;

                if (profile == null)
                {
                    profile = await AuthService.GetProfileAsync();
                    SessionManager.SetProfile(profile);
                }
                welcomeText.text = $"Добро пожаловать, {profile.firstName}!";

                SessionManager.userRole = await AuthService.GetUserRolesAsync();

                ApplyAccess(profile);
                RefreshNotificationButtons();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);

                ApplyAccess(null);
            }
        }

        private void SetNotificationButtonsVisible(bool showEnableButton, bool showDisableButton)
        {
            if (enableNotificationsButton != null)
                enableNotificationsButton.gameObject.SetActive(showEnableButton);

            if (disableNotificationsButton != null)
                disableNotificationsButton.gameObject.SetActive(showDisableButton);
        }

        private void OnLogoutClicked()
        {
            SessionManager.Logout();

            SceneManager.LoadScene(loginSceneName);
        }
    }
}
