using System;
using System.Collections.Generic;

namespace PushPelmesh.CalendarEvents
{
    [Serializable]
    public class CalendarEventsResponse
    {
        public List<CalendarEventDto> events = new List<CalendarEventDto>();
    }

    [Serializable]
    public class CalendarEventDto
    {
        public int id;
        public int type;
        public string typeName;
        public string title;
        public string description;
        public string date;
        public bool isSystemEvent;
        public int createdByUserId;
        public string createdByUser;
    }

    [Serializable]
    public class CreateCalendarEventRequest
    {
        public int Type;
        public string Title;
        public string Description;
        public string Date;
    }
}
