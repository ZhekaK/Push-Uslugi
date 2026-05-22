using System;
using System.Collections.Generic;
using System.Globalization;
using PushPelmesh.App;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.VoteModule
{
    public class VoteScreen : MonoBehaviour
    {
        private const string MinisterGroup = "Ministers";
        private const string RegularUsersGroup = "RegularUsers";

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("List")]
        [SerializeField] private GameObject listPanel;
        [SerializeField] private Transform pollsRoot;
        [SerializeField] private GameObject pollRowPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button openCreateButton;
        [SerializeField] private Text listStatusText;

        [Header("Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Button closeDetailsButton;
        [SerializeField] private Text detailsTitleText;
        [SerializeField] private Text detailsDescriptionText;
        [SerializeField] private Text detailsMetaText;
        [SerializeField] private Text detailsStatusText;
        [SerializeField] private Transform optionsRoot;
        [SerializeField] private GameObject optionPrefab;

        [Header("Create")]
        [SerializeField] private GameObject createPanel;
        [SerializeField] private Button closeCreateButton;
        [SerializeField] private Button submitCreateButton;
        [SerializeField] private InputField titleInput;
        [SerializeField] private InputField descriptionInput;
        [SerializeField] private InputField endDateInput;
        [SerializeField] private Transform optionInputsRoot;
        [SerializeField] private GameObject optionInputRowPrefab;
        [SerializeField] private Button addOptionButton;
        [SerializeField] private GameObject audiencePanel;
        [SerializeField] private Toggle regularUsersToggle;
        [SerializeField] private Toggle ministersToggle;
        [SerializeField] private Text createStatusText;

        private readonly List<GameObject> spawnedPollRows = new List<GameObject>();
        private readonly List<GameObject> spawnedOptionRows = new List<GameObject>();
        private readonly List<VoteCreateOptionInputView> createOptionInputs = new List<VoteCreateOptionInputView>();
        private bool canChooseAudienceGroups;
        private int selectedPollId;

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();

            if (pollRowPrefab != null)
                pollRowPrefab.SetActive(false);

            if (optionPrefab != null)
                optionPrefab.SetActive(false);

            if (optionInputRowPrefab != null)
                optionInputRowPrefab.SetActive(false);

            if (backButton != null)
                backButton.onClick.AddListener(BackToMainMenu);

            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshPolls);

            if (openCreateButton != null)
                openCreateButton.onClick.AddListener(OpenCreatePanel);

            if (closeDetailsButton != null)
                closeDetailsButton.onClick.AddListener(ShowList);

            if (closeCreateButton != null)
                closeCreateButton.onClick.AddListener(ShowList);

            if (submitCreateButton != null)
                submitCreateButton.onClick.AddListener(SubmitPoll);

            if (addOptionButton != null)
                addOptionButton.onClick.AddListener(AddCreateOptionInput);
        }

        private async void Start()
        {
            await RefreshPermissionsAsync();
            await LoadPollsAsync();
            ShowList();
        }

        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(BackToMainMenu);

            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(RefreshPolls);

            if (openCreateButton != null)
                openCreateButton.onClick.RemoveListener(OpenCreatePanel);

            if (closeDetailsButton != null)
                closeDetailsButton.onClick.RemoveListener(ShowList);

            if (closeCreateButton != null)
                closeCreateButton.onClick.RemoveListener(ShowList);

            if (submitCreateButton != null)
                submitCreateButton.onClick.RemoveListener(SubmitPoll);

            if (addOptionButton != null)
                addOptionButton.onClick.RemoveListener(AddCreateOptionInput);
        }

        public async void OpenPoll(int pollId)
        {
            selectedPollId = pollId;
            SetDetailsStatus("Загрузка голосования...");
            ShowDetails();

            try
            {
                VotePollDto poll = await VoteApi.GetPollAsync(pollId);
                RenderPollDetails(poll);
            }
            catch (Exception exception)
            {
                SetDetailsStatus("Ошибка загрузки голосования: " + exception.Message);
            }
        }

        public async void VoteForOption(int optionId)
        {
            if (selectedPollId <= 0)
                return;

            SetDetailsStatus("Отправка голоса...");
            SetOptionsInteractable(false);

            try
            {
                VotePollDto poll = await VoteApi.VoteAsync(selectedPollId, optionId);
                RenderPollDetails(poll);
                await LoadPollsAsync();
            }
            catch (Exception exception)
            {
                SetDetailsStatus("Не удалось проголосовать: " + exception.Message);
                SetOptionsInteractable(true);
            }
        }

        private async System.Threading.Tasks.Task RefreshPermissionsAsync()
        {
            try
            {
                UserRoleResponse roles = SessionManager.userRole;

                if (roles == null)
                {
                    roles = await AuthService.GetUserRolesAsync();
                    SessionManager.userRole = roles;
                }

                canChooseAudienceGroups = HasRole(roles, "President") || HasRole(roles, "Governor");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                canChooseAudienceGroups = false;
            }

            if (audiencePanel != null)
                audiencePanel.SetActive(canChooseAudienceGroups);
        }

        private async System.Threading.Tasks.Task LoadPollsAsync()
        {
            SetListStatus("Загрузка голосований...");
            ClearPollRows();

            try
            {
                VotePollsResponse response = await VoteApi.GetPollsAsync();
                List<VotePollDto> polls = response != null && response.polls != null
                    ? response.polls
                    : new List<VotePollDto>();

                for (int i = 0; i < polls.Count; i++)
                    CreatePollRow(polls[i]);

                SetListStatus(polls.Count == 0 ? "Голосований пока нет" : "Голосований: " + polls.Count);
            }
            catch (Exception exception)
            {
                SetListStatus("Ошибка загрузки голосований: " + exception.Message);
            }
        }

        private async void RefreshPolls()
        {
            await LoadPollsAsync();
        }

        private void RenderPollDetails(VotePollDto poll)
        {
            ClearOptions();

            if (poll == null)
            {
                SetDetailsStatus("Голосование не найдено");
                return;
            }

            selectedPollId = poll.id;

            if (detailsTitleText != null)
                detailsTitleText.text = string.IsNullOrWhiteSpace(poll.title) ? "Без названия" : poll.title;

            if (detailsDescriptionText != null)
                detailsDescriptionText.text = string.IsNullOrWhiteSpace(poll.description) ? "Описание отсутствует" : poll.description;

            if (detailsMetaText != null)
                detailsMetaText.text = $"До {FormatDate(poll.endDate)} • Автор: {poll.createdByUser}";

            bool canVote = poll.canVote && !poll.isClosed && !poll.hasVoted;

            if (poll.options != null)
            {
                for (int i = 0; i < poll.options.Count; i++)
                    CreateOptionRow(poll.options[i], canVote);
            }

            if (poll.isClosed)
                SetDetailsStatus("Голосование завершено. Всего голосов: " + poll.totalVotes);
            else if (poll.hasVoted)
                SetDetailsStatus("Ваш голос принят. Всего голосов: " + poll.totalVotes);
            else if (poll.canVote)
                SetDetailsStatus("Выберите один из вариантов. Всего голосов: " + poll.totalVotes);
            else
                SetDetailsStatus("Голосование недоступно. Результаты видны.");
        }

        private void CreatePollRow(VotePollDto poll)
        {
            if (pollsRoot == null || pollRowPrefab == null)
                return;

            GameObject rowObject = Instantiate(pollRowPrefab, pollsRoot);
            PrepareSpawnedRow(rowObject);
            rowObject.SetActive(true);
            spawnedPollRows.Add(rowObject);

            VotePollListItemView row = rowObject.GetComponent<VotePollListItemView>();

            if (row != null)
                row.Setup(this, poll);
        }

        private void CreateOptionRow(VoteOptionDto option, bool canVote)
        {
            if (optionsRoot == null || optionPrefab == null)
                return;

            GameObject rowObject = Instantiate(optionPrefab, optionsRoot);
            PrepareSpawnedRow(rowObject);
            rowObject.SetActive(true);
            spawnedOptionRows.Add(rowObject);

            VoteOptionView optionView = rowObject.GetComponent<VoteOptionView>();

            if (optionView != null)
                optionView.Setup(this, option, canVote);
        }

        private async void SubmitPoll()
        {
            CreateVotePollRequest request = BuildCreateRequest();

            if (!ValidateCreateRequest(request))
                return;

            SetCreateInteractable(false);
            SetCreateStatus("Создание голосования...");

            try
            {
                VotePollDto poll = await VoteApi.CreatePollAsync(request);
                ClearCreateInputs();
                await LoadPollsAsync();
                ShowList();

                if (poll != null)
                    OpenPoll(poll.id);
            }
            catch (Exception exception)
            {
                SetCreateStatus("Ошибка создания: " + exception.Message);
            }
            finally
            {
                SetCreateInteractable(true);
            }
        }

        private CreateVotePollRequest BuildCreateRequest()
        {
            CreateVotePollRequest request = new CreateVotePollRequest
            {
                title = titleInput != null ? titleInput.text.Trim() : string.Empty,
                description = descriptionInput != null ? descriptionInput.text.Trim() : string.Empty,
                endDate = endDateInput != null ? endDateInput.text.Trim() : string.Empty
            };

            for (int i = 0; i < createOptionInputs.Count; i++)
            {
                InputField input = createOptionInputs[i] != null ? createOptionInputs[i].Input : null;
                string option = input != null ? input.text.Trim() : string.Empty;

                if (!string.IsNullOrWhiteSpace(option))
                    request.options.Add(option);
            }

            if (canChooseAudienceGroups)
            {
                if (regularUsersToggle == null || regularUsersToggle.isOn)
                    request.audienceGroups.Add(RegularUsersGroup);

                if (ministersToggle == null || ministersToggle.isOn)
                    request.audienceGroups.Add(MinisterGroup);
            }

            return request;
        }

        private bool ValidateCreateRequest(CreateVotePollRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.title))
            {
                SetCreateStatus("Введите название голосования");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.endDate) || !IsDate(request.endDate))
            {
                SetCreateStatus("Введите дату окончания в формате yyyy-MM-dd");
                return false;
            }

            if (request.options.Count < 2)
            {
                SetCreateStatus("Добавьте минимум два варианта ответа");
                return false;
            }

            if (canChooseAudienceGroups && request.audienceGroups.Count == 0)
            {
                SetCreateStatus("Выберите хотя бы одну группу пользователей");
                return false;
            }

            return true;
        }

        private void OpenCreatePanel()
        {
            ClearCreateInputs();
            ShowCreate();
        }

        private void ClearCreateInputs()
        {
            if (titleInput != null)
                titleInput.text = string.Empty;

            if (descriptionInput != null)
                descriptionInput.text = string.Empty;

            if (endDateInput != null)
                endDateInput.text = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            ClearCreateOptionInputs();
            AddCreateOptionInput();
            AddCreateOptionInput();

            if (regularUsersToggle != null)
                regularUsersToggle.isOn = true;

            if (ministersToggle != null)
                ministersToggle.isOn = true;

            SetCreateStatus(string.Empty);
        }

        private void ShowList()
        {
            SetPanel(listPanel, true);
            SetPanel(detailsPanel, false);
            SetPanel(createPanel, false);
        }

        private void ShowDetails()
        {
            SetPanel(listPanel, false);
            SetPanel(detailsPanel, true);
            SetPanel(createPanel, false);
        }

        private void ShowCreate()
        {
            SetPanel(listPanel, false);
            SetPanel(detailsPanel, false);
            SetPanel(createPanel, true);
        }

        private void ClearPollRows()
        {
            DestroyRows(spawnedPollRows);
        }

        private void ClearOptions()
        {
            DestroyRows(spawnedOptionRows);
        }

        private void AddCreateOptionInput()
        {
            if (optionInputsRoot == null || optionInputRowPrefab == null)
                return;

            GameObject rowObject = Instantiate(optionInputRowPrefab, optionInputsRoot);
            PrepareSpawnedRow(rowObject);
            rowObject.SetActive(true);

            VoteCreateOptionInputView input = rowObject.GetComponent<VoteCreateOptionInputView>();

            if (input == null)
                return;

            createOptionInputs.Add(input);
            input.Setup(this, "Вариант " + createOptionInputs.Count);
            RefreshCreateOptionRemoveButtons();
        }

        public void RemoveCreateOptionInput(VoteCreateOptionInputView optionInput)
        {
            if (optionInput == null || createOptionInputs.Count <= 2)
                return;

            createOptionInputs.Remove(optionInput);

            if (optionInput.gameObject != null)
                Destroy(optionInput.gameObject);

            RefreshCreateOptionPlaceholders();
            RefreshCreateOptionRemoveButtons();
        }

        private void ClearCreateOptionInputs()
        {
            for (int i = 0; i < createOptionInputs.Count; i++)
            {
                if (createOptionInputs[i] != null)
                    Destroy(createOptionInputs[i].gameObject);
            }

            createOptionInputs.Clear();
        }

        private void RefreshCreateOptionPlaceholders()
        {
            for (int i = 0; i < createOptionInputs.Count; i++)
            {
                InputField input = createOptionInputs[i] != null ? createOptionInputs[i].Input : null;
                Text placeholder = input != null ? input.placeholder as Text : null;

                if (placeholder != null)
                    placeholder.text = "Вариант " + (i + 1);
            }
        }

        private void RefreshCreateOptionRemoveButtons()
        {
            bool canRemove = createOptionInputs.Count > 2;

            for (int i = 0; i < createOptionInputs.Count; i++)
            {
                if (createOptionInputs[i] != null)
                    createOptionInputs[i].SetRemoveVisible(canRemove);
            }
        }

        private static void PrepareSpawnedRow(GameObject rowObject)
        {
            if (rowObject == null)
                return;

            RectTransform rectTransform = rowObject.transform as RectTransform;

            if (rectTransform == null)
                return;

            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = Vector2.zero;

            LayoutElement layoutElement = rowObject.GetComponent<LayoutElement>();

            if (layoutElement != null)
                rectTransform.sizeDelta = new Vector2(0f, layoutElement.preferredHeight);
        }

        private static void DestroyRows(List<GameObject> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] != null)
                    Destroy(rows[i]);
            }

            rows.Clear();
        }

        private void SetOptionsInteractable(bool interactable)
        {
            for (int i = 0; i < spawnedOptionRows.Count; i++)
            {
                Button button = spawnedOptionRows[i] != null ? spawnedOptionRows[i].GetComponent<Button>() : null;

                if (button != null)
                    button.interactable = interactable;
            }
        }

        private void SetCreateInteractable(bool interactable)
        {
            if (submitCreateButton != null)
                submitCreateButton.interactable = interactable;

            if (closeCreateButton != null)
                closeCreateButton.interactable = interactable;

            if (addOptionButton != null)
                addOptionButton.interactable = interactable;
        }

        private void SetListStatus(string message)
        {
            if (listStatusText != null)
                listStatusText.text = message;
        }

        private void SetDetailsStatus(string message)
        {
            if (detailsStatusText != null)
                detailsStatusText.text = message;
        }

        private void SetCreateStatus(string message)
        {
            if (createStatusText != null)
                createStatusText.text = message;
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        private static bool HasRole(UserRoleResponse roles, string roleType)
        {
            if (roles == null || roles.roles == null)
                return false;

            for (int i = 0; i < roles.roles.Count; i++)
            {
                UserRoleResponse.UserRoleDTO role = roles.roles[i];

                if (role != null && string.Equals(role.roleType, roleType, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool IsDate(string value)
        {
            return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public static string FormatDate(string value)
        {
            if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private void BackToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
