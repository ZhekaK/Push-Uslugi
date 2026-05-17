using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.CalendarEvents
{
    public static class CalendarEventsApi
    {
        private const string EventsPath = "/api/calendar/events";
        private const string DateFormat = "yyyy-MM-dd";

        public static async Task<List<CalendarEventDto>> GetEventsAsync(DateTime from, DateTime to)
        {
            string path = string.Format(
                CultureInfo.InvariantCulture,
                "{0}?from={1}&to={2}",
                EventsPath,
                FormatDate(from),
                FormatDate(to));

            string responseJson = await ApiClient.GetAsync(path, withAuth: true);
            CalendarEventsResponse response = JsonUtility.FromJson<CalendarEventsResponse>(responseJson);

            if (response == null || response.events == null)
                return new List<CalendarEventDto>();

            return response.events;
        }

        public static async Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventRequest request)
        {
            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(EventsPath, json, withAuth: true);
            return JsonUtility.FromJson<CalendarEventDto>(responseJson);
        }

        public static Task DeleteEventAsync(int id)
        {
            return ApiClient.DeleteAsync(EventsPath + "/" + id.ToString(CultureInfo.InvariantCulture), withAuth: true);
        }

        public static string FormatDate(DateTime date)
        {
            return date.ToString(DateFormat, CultureInfo.InvariantCulture);
        }
    }
}
