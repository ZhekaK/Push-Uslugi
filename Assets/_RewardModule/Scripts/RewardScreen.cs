using System;
using System.Collections.Generic;
using PushPelmesh.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.RewardModule
{
    public class RewardScreen : MonoBehaviour
    {
        private enum RewardTab
        {
            Championships,
            GovernmentAwards
        }

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Tabs")]
        [SerializeField] private Button championshipsTabButton;
        [SerializeField] private Button governmentAwardsTabButton;
        [SerializeField] private Image championshipsTabBackground;
        [SerializeField] private Image governmentAwardsTabBackground;

        [Header("Table")]
        [SerializeField] private Text tableTitleText;
        [SerializeField] private Text firstHeaderText;
        [SerializeField] private Text secondHeaderText;
        [SerializeField] private Text thirdHeaderText;
        [SerializeField] private Text statusText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private GameObject rowPrefab;

        private readonly List<RewardRecordDto> championships = new List<RewardRecordDto>();
        private readonly List<RewardRecordDto> governmentAwards = new List<RewardRecordDto>();
        private readonly List<GameObject> spawnedRows = new List<GameObject>();
        private RewardTab currentTab = RewardTab.Championships;

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();

            if (backButton != null)
                backButton.onClick.AddListener(BackToMainMenu);

            if (championshipsTabButton != null)
                championshipsTabButton.onClick.AddListener(ShowChampionships);

            if (governmentAwardsTabButton != null)
                governmentAwardsTabButton.onClick.AddListener(ShowGovernmentAwards);

            if (rowPrefab != null)
                rowPrefab.SetActive(false);
        }

        private async void Start()
        {
            await LoadAsync();
        }

        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(BackToMainMenu);

            if (championshipsTabButton != null)
                championshipsTabButton.onClick.RemoveListener(ShowChampionships);

            if (governmentAwardsTabButton != null)
                governmentAwardsTabButton.onClick.RemoveListener(ShowGovernmentAwards);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            SetStatus("Загрузка наград...");

            try
            {
                championships.Clear();
                championships.AddRange(await RewardApi.GetChampionshipsAsync());

                governmentAwards.Clear();
                governmentAwards.AddRange(await RewardApi.GetGovernmentAwardsAsync());

                RenderCurrentTab();
            }
            catch (Exception exception)
            {
                SetStatus("Ошибка загрузки наград: " + exception.Message);
            }
        }

        private void ShowChampionships()
        {
            currentTab = RewardTab.Championships;
            RenderCurrentTab();
        }

        private void ShowGovernmentAwards()
        {
            currentTab = RewardTab.GovernmentAwards;
            RenderCurrentTab();
        }

        private void RenderCurrentTab()
        {
            ClearRows();
            UpdateTabVisuals();

            bool showChampionships = currentTab == RewardTab.Championships;
            List<RewardRecordDto> source = showChampionships ? championships : governmentAwards;

            if (tableTitleText != null)
                tableTitleText.text = showChampionships ? "Чемпионаты" : "Гос. награды";

            if (firstHeaderText != null)
                firstHeaderText.text = "ФИО";

            if (secondHeaderText != null)
                secondHeaderText.text = showChampionships ? "Название события" : "Тип события";

            if (thirdHeaderText != null)
                thirdHeaderText.text = showChampionships ? "Место" : "Название события";

            for (int i = 0; i < source.Count; i++)
            {
                RewardRecordDto record = source[i];
                RewardRowView row = CreateRow();

                if (row == null)
                    continue;

                row.Setup(
                    record.fullName,
                    showChampionships ? record.eventName : record.eventType,
                    showChampionships ? record.place : record.eventName);
            }

            SetStatus(source.Count == 0 ? "Записей пока нет" : "Загружено записей: " + source.Count);
        }

        private RewardRowView CreateRow()
        {
            if (rowPrefab == null || rowsRoot == null)
                return null;

            GameObject rowObject = Instantiate(rowPrefab, rowsRoot);
            rowObject.SetActive(true);
            spawnedRows.Add(rowObject);
            return rowObject.GetComponent<RewardRowView>();
        }

        private void ClearRows()
        {
            for (int i = 0; i < spawnedRows.Count; i++)
            {
                if (spawnedRows[i] != null)
                    Destroy(spawnedRows[i]);
            }

            spawnedRows.Clear();
        }

        private void UpdateTabVisuals()
        {
            Color active = new Color(0.12f, 0.42f, 0.74f);
            Color inactive = new Color(0.36f, 0.42f, 0.5f);

            if (championshipsTabBackground != null)
                championshipsTabBackground.color = currentTab == RewardTab.Championships ? active : inactive;

            if (governmentAwardsTabBackground != null)
                governmentAwardsTabBackground.color = currentTab == RewardTab.GovernmentAwards ? active : inactive;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void BackToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
