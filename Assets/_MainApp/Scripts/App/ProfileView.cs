using System;
using System.Globalization;
using PushPelmesh.App.Auth;
using PushPelmesh.App.MainMenu;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.App.Profile
{
    public class ProfileView : MonoBehaviour
    {
        UserProfileResponse profile;

        [Header("Weight")]
        [SerializeField] private InputField weightInput;
        [SerializeField] private Button saveWeightButton;
        [SerializeField] private Text statusText;
        [Header("Presonal ID")]
        [SerializeField] private Text personalIdSeriesAndNumber;
        [SerializeField] private Text GivePlace;
        [SerializeField] private Text GiveDate;
        [SerializeField] private Text Name;
        [SerializeField] private Text Sex;
        [SerializeField] private Text BirthDate;
        [Header("Role ID")]
        [SerializeField] private GameObject ShowRoleIdButton;
        [SerializeField] private Transform RoleIdButtonsParent;
        [SerializeField] private GameObject RoleIdPanel;
        [SerializeField] private Text RoleIdNumber;
        [SerializeField] private Text RoleIdPostName;
        [SerializeField] private Text RoleIdGivePlace;
        [SerializeField] private Text RoleIdStartDate;

        private void Awake()
        {
            saveWeightButton.onClick.AddListener(OnSaveClicked);
        }

        private void Start()
        {
            LoadDocuments();
            if (SessionManager.userRole?.roles == null)
                return;

            for (int i = 0; i < SessionManager.userRole.roles.Count; i++)
            {
                int roleIndex = i;

                var btnObject = Instantiate(ShowRoleIdButton, RoleIdButtonsParent);
                var button = btnObject.GetComponent<Button>();

                button.onClick.AddListener(() => LoadRoles(roleIndex));
                button.onClick.AddListener(() => RoleIdPanel.SetActive(true));
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
                LoadDocuments();
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

        private async void LoadDocuments()
        {
            try
            {
                profile = SessionManager.CurrentProfile;

                if (profile == null)
                {
                    profile = await AuthService.GetProfileAsync();
                    SessionManager.SetProfile(profile);
                }

                personalIdSeriesAndNumber.text = $"{profile.series} {profile.number}";
                GivePlace.text = profile.givePlace;
                GiveDate.text = profile.giveDate;
                Name.text = $"{profile.middleName} {profile.firstName} {profile.lastName}";
                Sex.text = profile.sex;
                BirthDate.text = profile.birthDate;

                if (profile != null && profile.weightKg > 0)
                {
                    weightInput.text = profile.weightKg.ToString(CultureInfo.InvariantCulture);
                }

                SetStatus("Документы загружены");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                SetStatus("Ошибка загрузки документов");
            }
        }

        public void LoadRoles(int id)
        {
            RoleIdNumber.text = SessionManager.userRole.roles[id].number;
            RoleIdPostName.text = SessionManager.userRole.roles[id].postName;
            RoleIdGivePlace.text = SessionManager.userRole.roles[id].givePlace;
            RoleIdStartDate.text = SessionManager.userRole.roles[id].startDate;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}