using System;
using System.Collections.Generic;

namespace PushPelmesh.App.Notifications
{
    [Serializable]
    public class ModuleNotificationsResponse
    {
        public List<ModuleNotificationDto> notifications = new List<ModuleNotificationDto>();
    }

    [Serializable]
    public class ModuleNotificationDto
    {
        public string moduleKey;
        public int count;
        public string latestTitle;
        public string latestCreatedAt;
    }
}
