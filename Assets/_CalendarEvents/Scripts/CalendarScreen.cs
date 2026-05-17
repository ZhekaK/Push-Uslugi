using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PushPelmesh.App;
using PushPelmesh.App.Api;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.CalendarEvents
{
    public class CalendarScreen : MonoBehaviour
    {
        private const int MeetingEventType = 1;

        private static readonly CultureInfo RussianCulture = new CultureInfo("ru-RU");
        private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy" };

        [Header("Navigation")]
        [SerializeField] private Text monthTitleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button previousMonthButton;
        [SerializeField] private Button nextMonthButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button addEventButton;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Calendar")]
        [SerializeField] private CalendarDayCell[] dayCells;
        [SerializeField] private GameObject eventBadgePrefab;

        [Header("Add Event Panel")]
        [SerializeField] private GameObject addEventPanel;
        [SerializeField] private InputField titleInput;
        [SerializeField] private InputField descriptionInput;
        [SerializeField] private InputField dateInput;
        [SerializeField] private Text addEventStatusText;
        [SerializeField] private Button submitEventButton;
        [SerializeField] private Button cancelAddEventButton;

        [Header("Details Panel")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Text detailsTitleText;
        [SerializeField] private Text detailsDescriptionText;
        [SerializeField] private Text detailsDateText;
        [SerializeField] private Text detailsCreatedByText;
        [SerializeField] private Button deleteEventButton;
        [SerializeField] private Button closeDetailsButton;

        private readonly List<CalendarEventDto> loadedEvents = new List<CalendarEventDto>();
        private DateTime currentMonth;
        private DateTime selectedDate;
        private CalendarEventDto selectedEvent;
        private bool canCreateEvents;
        private bool canDeleteAnyEvents;
        private bool canDeleteOwnEvents;
        private int loadVersion;

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();
            BindEvents();

            if (addEventPanel != null)
                addEventPanel.SetActive(false);

            if (detailsPanel != null)
                detailsPanel.SetActive(false);

            SetDeleteButtonVisible(false);

            if (eventBadgePrefab != null)
                eventBadgePrefab.SetActive(false);
        }

        private async void Start()
        {
            DateTime today = DateTime.Today;
            currentMonth = new DateTime(today.Year, today.Month, 1);
            selectedDate = today;

            await RefreshPermissionsAsync();
            await LoadCurrentMonthAsync();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void BindEvents()
        {
            if (previousMonthButton != null)
                previousMonthButton.onClick.AddListener(ShowPreviousMonth);

            if (nextMonthButton != null)
                nextMonthButton.onClick.AddListener(ShowNextMonth);

            if (backButton != null)
                backButton.onClick.AddListener(BackToMainMenu);

            if (addEventButton != null)
                addEventButton.onClick.AddListener(OpenAddEventPanel);

            if (submitEventButton != null)
                submitEventButton.onClick.AddListener(SubmitEvent);

            if (cancelAddEventButton != null)
                cancelAddEventButton.onClick.AddListener(CloseAddEventPanel);

            if (closeDetailsButton != null)
                closeDetailsButton.onClick.AddListener(CloseDetailsPanel);
        }

        private void UnbindEvents()
        {
            if (previousMonthButton != null)
                previousMonthButton.onClick.RemoveListener(ShowPreviousMonth);

            if (nextMonthButton != null)
                nextMonthButton.onClick.RemoveListener(ShowNextMonth);

            if (backButton != null)
                backButton.onClick.RemoveListener(BackToMainMenu);

            if (addEventButton != null)
                addEventButton.onClick.RemoveListener(OpenAddEventPanel);

            if (submitEventButton != null)
                submitEventButton.onClick.RemoveListener(SubmitEvent);

            if (cancelAddEventButton != null)
                cancelAddEventButton.onClick.RemoveListener(CloseAddEventPanel);

            if (closeDetailsButton != null)
                closeDetailsButton.onClick.RemoveListener(CloseDetailsPanel);
        }

        private async Task RefreshPermissionsAsync()
        {
            canCreateEvents = false;
            canDeleteAnyEvents = false;
            canDeleteOwnEvents = false;
            SetAddButtonVisible(false);
            SetDeleteButtonVisible(false);

            try
            {
                UserRoleResponse roles = SessionManager.userRole;

                if (roles == null)
                {
                    roles = await AuthService.GetUserRolesAsync();
                    SessionManager.userRole = roles;
                }

                canCreateEvents = HasMeetingCreationRole(roles);
                canDeleteAnyEvents = HasRole(roles, "President");
                canDeleteOwnEvents = HasRole(roles, "Minister");
                SetAddButtonVisible(canCreateEvents);
            }
            catch (Exception exception)
            {
                canCreateEvents = false;
                canDeleteAnyEvents = false;
                canDeleteOwnEvents = false;
                SetAddButtonVisible(false);
                SetDeleteButtonVisible(false);
                SetStatus("Не удалось проверить права на создание событий: " + exception.Message);
            }
        }

        private async Task LoadCurrentMonthAsync()
        {
            int version = ++loadVersion;
            UpdateMonthTitle();
            RenderCalendar();
            SetStatus("Загрузка событий...");

            DateTime gridStart = GetGridStartDate();
            DateTime gridEnd = gridStart.AddDays(Mathf.Max(0, GetCellCount() - 1));

            try
            {
                List<CalendarEventDto> events = await CalendarEventsApi.GetEventsAsync(gridStart, gridEnd);

                if (version != loadVersion)
                    return;

                loadedEvents.Clear();
                loadedEvents.AddRange(events);
                RenderCalendar();
                SetStatus(events.Count == 0 ? "Событий нет" : "События загружены");
            }
            catch (Exception exception)
            {
                if (version != loadVersion)
                    return;

                SetStatus("Ошибка загрузки событий: " + exception.Message);
            }
        }

        private void RenderCalendar()
        {
            if (dayCells == null || dayCells.Length == 0)
                return;

            DateTime gridStart = GetGridStartDate();

            for (int i = 0; i < dayCells.Length; i++)
            {
                if (dayCells[i] == null)
                    continue;

                DateTime cellDate = gridStart.AddDays(i);
                List<CalendarEventDto> cellEvents = GetEventsForDate(cellDate);
                bool isCurrentMonth = cellDate.Month == currentMonth.Month && cellDate.Year == currentMonth.Year;

                dayCells[i].Setup(
                    cellDate,
                    isCurrentMonth,
                    cellEvents,
                    eventBadgePrefab,
                    ShowEventDetails,
                    SelectDate);
            }
        }

        private List<CalendarEventDto> GetEventsForDate(DateTime date)
        {
            List<CalendarEventDto> result = new List<CalendarEventDto>();

            for (int i = 0; i < loadedEvents.Count; i++)
            {
                if (TryParseDate(loadedEvents[i].date, out DateTime eventDate) && eventDate.Date == date.Date)
                    result.Add(loadedEvents[i]);
            }

            return result;
        }

        private DateTime GetGridStartDate()
        {
            DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            int mondayOffset = ((int)firstDay.DayOfWeek + 6) % 7;
            return firstDay.AddDays(-mondayOffset);
        }

        private int GetCellCount()
        {
            return dayCells == null || dayCells.Length == 0 ? 42 : dayCells.Length;
        }

        private void UpdateMonthTitle()
        {
            if (monthTitleText != null)
                monthTitleText.text = currentMonth.ToString("MMMM yyyy", RussianCulture);
        }

        private async void ShowPreviousMonth()
        {
            currentMonth = currentMonth.AddMonths(-1);
            await LoadCurrentMonthAsync();
        }

        private async void ShowNextMonth()
        {
            currentMonth = currentMonth.AddMonths(1);
            await LoadCurrentMonthAsync();
        }

        private void BackToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void SelectDate(DateTime date)
        {
            selectedDate = date;

            if (dateInput != null)
                dateInput.text = CalendarEventsApi.FormatDate(date);
        }

        private void OpenAddEventPanel()
        {
            if (!canCreateEvents)
                return;

            if (titleInput != null)
                titleInput.text = string.Empty;

            if (descriptionInput != null)
                descriptionInput.text = string.Empty;

            if (dateInput != null)
                dateInput.text = CalendarEventsApi.FormatDate(selectedDate);

            if (addEventStatusText != null)
                addEventStatusText.text = string.Empty;

            if (addEventPanel != null)
                addEventPanel.SetActive(true);
        }

        private void CloseAddEventPanel()
        {
            if (addEventPanel != null)
                addEventPanel.SetActive(false);
        }

        private async void SubmitEvent()
        {
            if (!canCreateEvents)
                return;

            string title = titleInput != null ? titleInput.text.Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                SetAddEventStatus("Введите название события");
                return;
            }

            string dateText = dateInput != null ? dateInput.text.Trim() : string.Empty;

            if (!TryParseDate(dateText, out DateTime date))
            {
                SetAddEventStatus("Дата должна быть в формате yyyy-MM-dd");
                return;
            }

            CreateCalendarEventRequest request = new CreateCalendarEventRequest
            {
                Type = MeetingEventType,
                Title = title,
                Description = descriptionInput != null ? descriptionInput.text : string.Empty,
                Date = CalendarEventsApi.FormatDate(date)
            };

            SetSubmitInteractable(false);
            SetAddEventStatus("Отправка...");

            try
            {
                await CalendarEventsApi.CreateEventAsync(request);
                CloseAddEventPanel();
                currentMonth = new DateTime(date.Year, date.Month, 1);
                selectedDate = date;
                await LoadCurrentMonthAsync();
                SetStatus("Событие создано");
            }
            catch (ApiException exception)
            {
                SetAddEventStatus(GetApiErrorMessage(exception));
            }
            catch (Exception exception)
            {
                SetAddEventStatus("Ошибка создания события: " + exception.Message);
            }
            finally
            {
                SetSubmitInteractable(true);
            }
        }

        private void ShowEventDetails(CalendarEventDto calendarEvent)
        {
            selectedEvent = calendarEvent;

            if (detailsPanel != null)
                detailsPanel.SetActive(true);

            SetDeleteButtonVisible(CanDeleteEvent(calendarEvent));

            if (detailsTitleText != null)
            {
                detailsTitleText.text = string.IsNullOrWhiteSpace(calendarEvent.title)
                    ? "Без названия"
                    : calendarEvent.title;
            }

            if (detailsDescriptionText != null)
            {
                detailsDescriptionText.text = string.IsNullOrWhiteSpace(calendarEvent.description)
                    ? "Описание не указано"
                    : calendarEvent.description;
            }

            if (detailsDateText != null)
            {
                string dateText = calendarEvent.date;
                if (TryParseDate(calendarEvent.date, out DateTime date))
                    dateText = date.ToString("dd.MM.yyyy", RussianCulture);

                detailsDateText.text = "Дата: " + dateText;
            }

            if (detailsCreatedByText != null)
            {
                string creator = string.IsNullOrWhiteSpace(calendarEvent.createdByUser)
                    ? "неизвестно"
                    : calendarEvent.createdByUser;

                detailsCreatedByText.text = "Создал: " + creator;
            }
        }

        private void CloseDetailsPanel()
        {
            selectedEvent = null;
            SetDeleteButtonVisible(false);

            if (detailsPanel != null)
                detailsPanel.SetActive(false);
        }

        public async void DeleteSelectedEvent()
        {
            if (selectedEvent == null)
                return;

            if (!CanDeleteEvent(selectedEvent))
            {
                SetDeleteButtonVisible(false);
                SetStatus("Недостаточно прав для удаления события");
                return;
            }

            try
            {
                await CalendarEventsApi.DeleteEventAsync(selectedEvent.id);
                CloseDetailsPanel();
                await LoadCurrentMonthAsync();
                SetStatus("Событие удалено");
            }
            catch (ApiException exception)
            {
                SetStatus(GetDeleteApiErrorMessage(exception));
            }
            catch (Exception exception)
            {
                SetStatus("Ошибка удаления события: " + exception.Message);
            }
        }

        private void SetAddButtonVisible(bool visible)
        {
            if (addEventButton != null)
                addEventButton.gameObject.SetActive(visible);
        }

        private void SetDeleteButtonVisible(bool visible)
        {
            if (deleteEventButton != null)
                deleteEventButton.gameObject.SetActive(visible);
        }

        private void SetSubmitInteractable(bool value)
        {
            if (submitEventButton != null)
                submitEventButton.interactable = value;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void SetAddEventStatus(string message)
        {
            if (addEventStatusText != null)
                addEventStatusText.text = message;
        }

        private static bool TryParseDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private bool CanDeleteEvent(CalendarEventDto calendarEvent)
        {
            if (calendarEvent == null || calendarEvent.isSystemEvent)
                return false;

            if (canDeleteAnyEvents)
                return true;

            UserProfileResponse profile = SessionManager.CurrentProfile;
            return canDeleteOwnEvents &&
                   profile != null &&
                   profile.id > 0 &&
                   calendarEvent.createdByUserId == profile.id;
        }

        private static bool HasMeetingCreationRole(UserRoleResponse roles)
        {
            return HasRole(roles, "Minister") || HasRole(roles, "President");
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

        private static string GetApiErrorMessage(ApiException exception)
        {
            if (exception.StatusCode == 403)
                return "Недостаточно прав для создания события";

            if (exception.StatusCode == 400 && !string.IsNullOrWhiteSpace(exception.ResponseBody))
                return exception.ResponseBody;

            return "Ошибка сервера: " + exception.StatusCode;
        }
        private static string GetDeleteApiErrorMessage(ApiException exception)
        {
            if (exception.StatusCode == 403)
                return "Недостаточно прав для удаления события";

            if (exception.StatusCode == 404)
                return "Событие не найдено";

            if (exception.StatusCode == 400 && !string.IsNullOrWhiteSpace(exception.ResponseBody))
                return exception.ResponseBody;

            return "Ошибка удаления события: " + exception.StatusCode;
        }
    }
}
