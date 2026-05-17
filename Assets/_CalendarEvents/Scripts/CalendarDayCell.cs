using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.CalendarEvents
{
    public class CalendarDayCell : MonoBehaviour
    {
        [SerializeField] private Button dateButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text dayText;
        [SerializeField] private RectTransform eventsRoot;

        private readonly List<GameObject> spawnedBadges = new List<GameObject>();
        private DateTime date;
        private Action<DateTime> onDateClicked;

        private void Awake()
        {
            if (dateButton != null)
                dateButton.onClick.AddListener(HandleDateClicked);
        }

        private void OnDestroy()
        {
            if (dateButton != null)
                dateButton.onClick.RemoveListener(HandleDateClicked);
        }

        public void Setup(
            DateTime value,
            bool isCurrentMonth,
            IReadOnlyList<CalendarEventDto> events,
            GameObject eventBadgePrefab,
            Action<CalendarEventDto> onEventClicked,
            Action<DateTime> onDayClicked)
        {
            date = value;
            onDateClicked = onDayClicked;

            if (dayText != null)
            {
                dayText.text = value.Day.ToString();
                dayText.color = isCurrentMonth
                    ? new Color(0.1f, 0.12f, 0.16f)
                    : new Color(0.58f, 0.62f, 0.68f);
            }

            if (backgroundImage != null)
            {
                bool hasEvents = events != null && events.Count > 0;
                bool isToday = value.Date == DateTime.Today;

                if (isToday)
                    backgroundImage.color = new Color(1f, 0.86f, 0.86f);
                else if (hasEvents)
                    backgroundImage.color = new Color(0.86f, 0.94f, 1f);
                else
                    backgroundImage.color = isCurrentMonth
                        ? Color.white
                        : new Color(0.94f, 0.95f, 0.96f);
            }

            ClearBadges();

            if (events == null || eventBadgePrefab == null || eventsRoot == null)
                return;

            for (int i = 0; i < events.Count; i++)
            {
                CreateEventBadge(events[i], eventBadgePrefab, onEventClicked);
            }
        }

        private void CreateEventBadge(
            CalendarEventDto calendarEvent,
            GameObject eventBadgePrefab,
            Action<CalendarEventDto> onEventClicked)
        {
            GameObject badge = Instantiate(eventBadgePrefab, eventsRoot);
            badge.name = "EventBadge";
            badge.SetActive(true);
            spawnedBadges.Add(badge);

            Text label = badge.GetComponentInChildren<Text>();
            if (label != null)
            {
                string title = string.IsNullOrWhiteSpace(calendarEvent.title) ? "Без названия" : calendarEvent.title;
                label.text = GetEventPrefix(calendarEvent) + title;
            }

            Image image = badge.GetComponent<Image>();
            if (image != null)
                image.color = GetEventColor(calendarEvent);

            Button button = badge.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onEventClicked?.Invoke(calendarEvent));
            }
        }

        private void ClearBadges()
        {
            for (int i = 0; i < spawnedBadges.Count; i++)
            {
                if (spawnedBadges[i] != null)
                    Destroy(spawnedBadges[i]);
            }

            spawnedBadges.Clear();
        }

        private void HandleDateClicked()
        {
            onDateClicked?.Invoke(date);
        }

        private static string GetEventPrefix(CalendarEventDto calendarEvent)
        {
            if (calendarEvent.type == 0)
                return "ДР: ";

            if (calendarEvent.type == 1)
                return "";

            return string.Empty;
        }

        private static Color GetEventColor(CalendarEventDto calendarEvent)
        {
            if (calendarEvent.type == 0)
                return new Color(0.86f, 0.28f, 0.42f);

            if (calendarEvent.type == 1)
                return new Color(0.14f, 0.43f, 0.75f);

            return new Color(0.34f, 0.38f, 0.45f);
        }
    }
}
