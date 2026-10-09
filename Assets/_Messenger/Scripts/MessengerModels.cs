using System;
using System.Collections.Generic;

namespace PushPelmesh.Messenger
{
    [Serializable]
    public class MessengerUsersResponse
    {
        public List<MessengerUserDto> users = new List<MessengerUserDto>();
    }

    [Serializable]
    public class MessengerUserDto
    {
        public int id;
        public string displayName;
    }

    [Serializable]
    public class MessengerChatsResponse
    {
        public List<MessengerChatDto> chats = new List<MessengerChatDto>();
    }

    [Serializable]
    public class MessengerChatResponse
    {
        public MessengerChatDto chat;
    }

    [Serializable]
    public class MessengerChatDto
    {
        public int id;
        public string type;
        public string title;
        public List<int> participantIds = new List<int>();
        public bool muted;
        public bool unread;
    }

    [Serializable]
    public class MessengerMessagesResponse
    {
        public List<MessengerMessageDto> messages = new List<MessengerMessageDto>();
    }

    [Serializable]
    public class MessengerMessageResponse
    {
        public MessengerMessageDto message;
    }

    [Serializable]
    public class MessengerMessageDto
    {
        public long id;
        public int chatId;
        public int senderUserId;
        public string senderName;
        public string text;
        public string createdAt;
    }

    [Serializable]
    public class CreateDirectChatRequest
    {
        public int userId;
    }

    [Serializable]
    public class CreateGroupChatRequest
    {
        public string title;
        public List<int> userIds = new List<int>();
    }

    [Serializable]
    public class SendMessengerMessageRequest
    {
        public string text;
    }

    [Serializable]
    public class SetMessengerMuteRequest
    {
        public bool muted;
    }

    [Serializable]
    public class MarkMessengerReadRequest
    {
        public long lastMessageId;
    }
}
