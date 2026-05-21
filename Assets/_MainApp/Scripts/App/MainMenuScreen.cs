using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using PushPelmesh.App.Notifications;
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
            public string ModuleKey;
            public Image NotificationIcon;
            public bool GuestAvailable;
        }
        [Header("UI")]
        [SerializeField] private Text welcomeText;
        [SerializeField] private Button logoutButton;
        [SerializeField] private ServiceModule[] serviceModules;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button enableNotificationsButton;
        [SerializeField] private Button disableNotificationsButton;
        [SerializeField] private float moduleNotificationRefreshInterval = 60f;

        [Header("Navigation")]
        [SerializeField] private string loginSceneName = "LoginScene";

        private bool canUseNotifications;
        private bool notificationRequestInProgress;

        private void Awake()
        {
            PushPelmesh.App.ScreenOrientationPolicy.AllowAnyOrientation();
            logoutButton.onClick.AddListener(OnLogoutClicked);
            foreach (ServiceModule serviceModule in serviceModules)
            {
                ServiceModule module = serviceModule;
                SetModuleNotificationIcon(module, false);

                if (module.Button != null)
                    module.Button.onClick.AddListener(() => OnServiceButtonClicked(module));
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
            {
                notificationRequestInProgress = false;
                SetNotificationButtonsVisible(false, false);
                StopModuleNotificationsPolling();
            }

            foreach (ServiceModule serviceModule in serviceModules)
            {
                if (serviceModule.Button != null)
                    serviceModule.Button.interactable = !isGuest || serviceModule.GuestAvailable;

                if (isGuest)
                    SetModuleNotificationIcon(serviceModule, false);
            }
        }

        private void OnDestroy()
        {
            StopModuleNotificationsPolling();

            logoutButton.onClick.RemoveListener(OnLogoutClicked);
            foreach (ServiceModule serviceModule in serviceModules)
            {
                if (serviceModule.Button != null)
                    serviceModule.Button.onClick.RemoveAllListeners();
            }
        }

        private void OnServiceButtonClicked(ServiceModule module)
        {
            if (module == null || string.IsNullOrWhiteSpace(module.SceneName))
                return;

            SetModuleNotificationIcon(module, false);
            MarkModuleNotificationsRead(module);
            SceneManager.LoadScene(module.SceneName);
        }

        public void EnableNotifications()
        {
            if (!TryBeginNotificationRequest())
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebPushService.Register(gameObject.name, nameof(OnPushSubscriptionStateChanged));
#else
            notificationRequestInProgress = false;
            SetNotificationButtonsVisible(true, false);
            WebPushService.Register(gameObject.name, nameof(OnPushSubscriptionStateChanged));
#endif
        }

        public void DisableNotifications()
        {
            if (!TryBeginNotificationRequest())
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebPushService.RequestSubscriptionEndpoint(
                gameObject.name,
                nameof(OnPushUnsubscribeEndpointReceived),
                string.Empty);
#else
            notificationRequestInProgress = false;
            SetNotificationButtonsVisible(false, true);
            WebPushService.RequestSubscriptionEndpoint(
                gameObject.name,
                nameof(OnPushUnsubscribeEndpointReceived),
                string.Empty);
#endif
        }

        public void RefreshNotificationButtons()
        {
            if (!canUseNotifications)
            {
                notificationRequestInProgress = false;
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
            notificationRequestInProgress = false;

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
                notificationRequestInProgress = false;
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
                RestartModuleNotificationsPolling();
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
            {
                enableNotificationsButton.gameObject.SetActive(showEnableButton);
                enableNotificationsButton.interactable = showEnableButton && !notificationRequestInProgress;
            }

            if (disableNotificationsButton != null)
            {
                disableNotificationsButton.gameObject.SetActive(showDisableButton);
                disableNotificationsButton.interactable = showDisableButton && !notificationRequestInProgress;
            }
        }

        private async void RefreshModuleNotifications()
        {
            if (!TokenStorage.HasToken())
            {
                SetAllModuleNotificationIcons(false);
                return;
            }

            try
            {
                ModuleNotificationsResponse response = await ModuleNotificationsApi.GetUnreadAsync();
                HashSet<string> unreadModuleKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (response != null && response.notifications != null)
                {
                    for (int i = 0; i < response.notifications.Count; i++)
                    {
                        ModuleNotificationDto notification = response.notifications[i];

                        if (notification != null && !string.IsNullOrWhiteSpace(notification.moduleKey))
                            unreadModuleKeys.Add(notification.moduleKey);
                    }
                }

                foreach (ServiceModule serviceModule in serviceModules)
                    SetModuleNotificationIcon(serviceModule, unreadModuleKeys.Contains(GetModuleKey(serviceModule)));
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void RestartModuleNotificationsPolling()
        {
            StopModuleNotificationsPolling();

            if (!canUseNotifications || !TokenStorage.HasToken())
            {
                SetAllModuleNotificationIcons(false);
                return;
            }

            RefreshModuleNotifications();

            if (moduleNotificationRefreshInterval > 0f)
                InvokeRepeating(nameof(RefreshModuleNotifications), moduleNotificationRefreshInterval, moduleNotificationRefreshInterval);
        }

        private void StopModuleNotificationsPolling()
        {
            CancelInvoke(nameof(RefreshModuleNotifications));
        }

        private async void MarkModuleNotificationsRead(ServiceModule module)
        {
            string moduleKey = GetModuleKey(module);

            if (string.IsNullOrWhiteSpace(moduleKey) || !TokenStorage.HasToken())
                return;

            try
            {
                await ModuleNotificationsApi.MarkReadAsync(moduleKey);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void SetAllModuleNotificationIcons(bool visible)
        {
            foreach (ServiceModule serviceModule in serviceModules)
                SetModuleNotificationIcon(serviceModule, visible);
        }

        private static void SetModuleNotificationIcon(ServiceModule module, bool visible)
        {
            if (module != null && module.NotificationIcon != null)
                module.NotificationIcon.gameObject.SetActive(visible);
        }

        private static string GetModuleKey(ServiceModule module)
        {
            if (module == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(module.ModuleKey))
                return module.ModuleKey.Trim();

            return string.IsNullOrWhiteSpace(module.SceneName) ? string.Empty : module.SceneName.Trim();
        }

        private bool TryBeginNotificationRequest()
        {
            if (!canUseNotifications || notificationRequestInProgress)
                return false;

            notificationRequestInProgress = true;
            SetNotificationButtonsInteractable(false);
            return true;
        }

        private void SetNotificationButtonsInteractable(bool interactable)
        {
            if (enableNotificationsButton != null)
                enableNotificationsButton.interactable = interactable && enableNotificationsButton.gameObject.activeSelf;

            if (disableNotificationsButton != null)
                disableNotificationsButton.interactable = interactable && disableNotificationsButton.gameObject.activeSelf;
        }

        private void OnLogoutClicked()
        {
            SessionManager.Logout();

            SceneManager.LoadScene(loginSceneName);
        }
    }
}
