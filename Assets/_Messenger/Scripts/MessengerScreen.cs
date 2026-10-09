using System;
using System.Collections.Generic;
using System.Globalization;
using PushPelmesh.App;
using PushPelmesh.App.Auth;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace PushPelmesh.Messenger
{
    // Scene UI and row templates are serialized; runtime only fills them with data.
    public class MessengerScreen : MonoBehaviour
    {
        private readonly Color background = new Color(0.95f, 0.94f, 0.98f);
        private readonly Color accent = new Color(0.42f, 0.33f, 0.67f);
        private readonly Color ink = new Color(0.16f, 0.13f, 0.25f);
        private Font font;
        [Header("Scene UI")]
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform chatsContent;
        [SerializeField] private RectTransform messagesContent;
        [SerializeField] private RectTransform usersContent;
        [SerializeField] private ScrollRect messagesScroll;
        [SerializeField] private GameObject picker;
        [SerializeField] private InputField groupTitleInput;
        [SerializeField] private InputField messageInput;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button directButton;
        [SerializeField] private Button groupButton;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button createGroupButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button muteButton;
        [SerializeField] private Text chatTitleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text pickerTitleText;
        [SerializeField] private Text pickerStatusText;

        [Header("Editable row templates")]
        [SerializeField] private Button chatRowTemplate;
        [SerializeField] private Button userRowTemplate;
        [SerializeField] private MessengerMessageRowView messageRowTemplate;
        private readonly HashSet<int> selectedUsers = new HashSet<int>();
        private readonly HashSet<long> displayedMessageIds = new HashSet<long>();
        private List<MessengerChatDto> chats = new List<MessengerChatDto>();
        private int currentChatId;
        private long lastMessageId;
        private bool refreshingMessages;
        private bool pickerBusy;
        private bool groupMode;
        private bool refreshingChats;
        private bool changingMute;

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();
            if (root == null || chatsContent == null || messagesContent == null || usersContent == null ||
                messagesScroll == null || picker == null || groupTitleInput == null || messageInput == null ||
                backButton == null || refreshButton == null || directButton == null || groupButton == null ||
                sendButton == null || createGroupButton == null || cancelButton == null || muteButton == null ||
                chatTitleText == null || statusText == null || pickerTitleText == null || pickerStatusText == null ||
                chatRowTemplate == null || userRowTemplate == null || messageRowTemplate == null)
            {
                Debug.LogError("MessengerScene has missing UI references. Open the saved scene and reconnect them.");
                enabled = false;
                return;
            }

            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenuScene"));
            refreshButton.onClick.AddListener(async () => await LoadChatsAsync());
            directButton.onClick.AddListener(() => OpenPicker(false));
            groupButton.onClick.AddListener(() => OpenPicker(true));
            sendButton.onClick.AddListener(SendMessage);
            createGroupButton.onClick.AddListener(CreateGroupChat);
            cancelButton.onClick.AddListener(() => picker.SetActive(false));
            muteButton.onClick.AddListener(ToggleMute);
        }

#if UNITY_EDITOR
        public void BakeEditableUiInEditor()
        {
            if (root != null)
                throw new InvalidOperationException("Messenger UI already exists; existing scene content was preserved.");
            font = LoadUiFont();
            BuildUi();
        }
#endif

        private static Font LoadUiFont()
        {
            Font result = Resources.Load<Font>("Font/JACOBSSANS-REGULAR");
            if (result == null)
                result = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (result == null)
                result = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return result;
        }

        private async void Start()
        {
            await LoadChatsAsync();
            OpenChat(chats.Find(x => x.id == 0) ?? new MessengerChatDto { id = 0, title = "Общий чат" });
            InvokeRepeating(nameof(RefreshMessages), 3f, 3f);
            InvokeRepeating(nameof(RefreshChats), 5f, 5f);
        }

        private void OnDestroy()
        {
            CancelInvoke(nameof(RefreshMessages));
            CancelInvoke(nameof(RefreshChats));
        }

        private async void RefreshChats()
        {
            if (refreshingChats)
                return;
            refreshingChats = true;
            try
            {
                List<MessengerChatDto> latest = await MessengerApi.GetChatsAsync();
                bool changed = latest.Count != chats.Count;
                if (!changed)
                {
                    for (int i = 0; i < latest.Count; i++)
                    {
                        if (latest[i].id != chats[i].id || latest[i].muted != chats[i].muted ||
                            latest[i].unread != chats[i].unread || latest[i].title != chats[i].title)
                        {
                            changed = true;
                            break;
                        }
                    }
                }
                chats = latest;
                if (changed)
                    RenderChats();
                UpdateMuteButton();
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception);
            }
            finally
            {
                refreshingChats = false;
            }
        }

        private async System.Threading.Tasks.Task LoadChatsAsync()
        {
            try
            {
                chats = await MessengerApi.GetChatsAsync();
                RenderChats();
                SetStatus(string.Empty);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                SetStatus("Не удалось загрузить чаты. Мессенджер доступен пользователям с ID.");
            }
        }

        private void RenderChats()
        {
            ClearChildren(chatsContent);
            foreach (MessengerChatDto chat in chats)
            {
                MessengerChatDto selected = chat;
                string caption = (chat.unread ? "● " : string.Empty) + chat.title +
                    (chat.muted ? "  (без звука)" : string.Empty);
                Button button = Instantiate(chatRowTemplate, chatsContent);
                button.gameObject.name = "Chat " + chat.id;
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>(true).text = caption;
                if (chat.id == currentChatId)
                    button.GetComponent<Image>().color = new Color(0.86f, 0.8f, 0.97f);
                button.onClick.AddListener(() => OpenChat(selected));
            }
        }

        private void OpenChat(MessengerChatDto chat)
        {
            if (chat == null)
                return;

            currentChatId = chat.id;
            lastMessageId = 0;
            displayedMessageIds.Clear();
            ClearChildren(messagesContent);
            chatTitleText.text = chat.title;
            UpdateMuteButton();
            messageInput.text = string.Empty;
            SetStatus(string.Empty);
            RenderChats();
            RefreshMessages();
        }

        private async void RefreshMessages()
        {
            if (refreshingMessages)
                return;

            refreshingMessages = true;
            int chatId = currentChatId;
            try
            {
                List<MessengerMessageDto> messages = await MessengerApi.GetMessagesAsync(chatId, lastMessageId);
                if (chatId != currentChatId)
                    return;

                foreach (MessengerMessageDto message in messages)
                {
                    if (message == null || !displayedMessageIds.Add(message.id))
                        continue;

                    AppendMessage(message);
                    lastMessageId = Math.Max(lastMessageId, message.id);
                }

                if (messages.Count > 0)
                {
                    Canvas.ForceUpdateCanvases();
                    messagesScroll.verticalNormalizedPosition = 0f;
                }
                if (messages.Count > 0 && lastMessageId > 0)
                {
                    await MessengerApi.MarkReadAsync(chatId, lastMessageId);
                    MessengerChatDto current = chats.Find(x => x.id == chatId);
                    if (current != null && current.unread)
                    {
                        current.unread = false;
                        RenderChats();
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                if (chatId == currentChatId)
                    SetStatus("Не удалось обновить сообщения");
            }
            finally
            {
                refreshingMessages = false;
            }
        }

        private void AppendMessage(MessengerMessageDto message)
        {
            int lineCount = Math.Max(1, (message.text ?? string.Empty).Length / 42 + 1);
            string[] lines = (message.text ?? string.Empty).Split('\n');
            lineCount += Math.Max(0, lines.Length - 1);
            float height = Mathf.Max(92f, 62f + lineCount * 31f);
            MessengerMessageRowView row = Instantiate(messageRowTemplate, messagesContent);
            row.gameObject.name = "Message " + message.id;
            row.gameObject.SetActive(true);
            row.GetComponent<LayoutElement>().preferredHeight = height;
            DateTimeOffset.TryParse(message.createdAt, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out DateTimeOffset created);
            string time = created == default ? string.Empty : created.ToLocalTime().ToString("dd.MM HH:mm", CultureInfo.InvariantCulture);
            row.SetText(message.senderName + "  •  " + time, message.text);
        }

        private async void SendMessage()
        {
            string text = messageInput.text.Trim();
            if (text.Length == 0 || text.Length > 2000)
            {
                SetStatus("Сообщение должно содержать от 1 до 2000 символов");
                return;
            }

            sendButton.interactable = false;
            int chatId = currentChatId;
            try
            {
                MessengerMessageDto message = await MessengerApi.SendMessageAsync(chatId, text);
                if (chatId == currentChatId)
                {
                    messageInput.text = string.Empty;
                    SetStatus(string.Empty);
                    if (message != null && displayedMessageIds.Add(message.id))
                    {
                        AppendMessage(message);
                        lastMessageId = Math.Max(lastMessageId, message.id);
                        Canvas.ForceUpdateCanvases();
                        messagesScroll.verticalNormalizedPosition = 0f;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                SetStatus("Сообщение не отправлено");
            }
            finally
            {
                sendButton.interactable = true;
            }
        }

        private async void OpenPicker(bool forGroup)
        {
            groupMode = forGroup;
            selectedUsers.Clear();
            groupTitleInput.text = string.Empty;
            groupTitleInput.gameObject.SetActive(forGroup);
            createGroupButton.gameObject.SetActive(forGroup);
            pickerTitleText.text = forGroup ? "Новый групповой чат" : "Написать пользователю";
            pickerStatusText.text = "Загрузка пользователей...";
            picker.SetActive(true);
            ClearChildren(usersContent);

            try
            {
                List<MessengerUserDto> users = await MessengerApi.GetUsersAsync();
                if (picker == null || !picker.activeSelf || groupMode != forGroup)
                    return;

                int ownId = SessionManager.CurrentProfile != null ? SessionManager.CurrentProfile.id : -1;
                foreach (MessengerUserDto user in users)
                {
                    if (user.id == ownId)
                        continue;

                    MessengerUserDto selected = user;
                    string displayName = user.displayName;
                    Button button = Instantiate(userRowTemplate, usersContent);
                    button.gameObject.name = "User " + user.id;
                    button.gameObject.SetActive(true);
                    Text label = button.GetComponentInChildren<Text>(true);
                    if (label == null)
                    {
                        Debug.LogError("User row template has no Text component.");
                        continue;
                    }
                    if (forGroup)
                    {
                        label.text = "[ ] " + displayName;
                        button.onClick.AddListener(() =>
                        {
                            if (!selectedUsers.Add(selected.id))
                                selectedUsers.Remove(selected.id);
                            label.text = (selectedUsers.Contains(selected.id) ? "[x] " : "[ ] ") + displayName;
                        });
                    }
                    else
                    {
                        label.text = displayName;
                        button.onClick.AddListener(() => CreateDirectChat(selected.id));
                    }
                }

                pickerStatusText.text = users.Count == 0 ? "Зарегистрированных пользователей пока нет" : string.Empty;
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                pickerStatusText.text = "Не удалось загрузить пользователей";
            }
        }

        private async void CreateDirectChat(int userId)
        {
            if (pickerBusy)
                return;

            pickerBusy = true;
            pickerStatusText.text = "Создание чата...";
            try
            {
                MessengerChatDto chat = await MessengerApi.CreateDirectChatAsync(userId);
                picker.SetActive(false);
                await LoadChatsAsync();
                OpenChat(chats.Find(x => x.id == chat.id) ?? chat);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                pickerStatusText.text = "Не удалось создать личный чат";
            }
            finally
            {
                pickerBusy = false;
            }
        }

        private async void CreateGroupChat()
        {
            if (pickerBusy)
                return;

            string title = groupTitleInput.text.Trim();
            if (title.Length == 0 || selectedUsers.Count == 0)
            {
                pickerStatusText.text = "Введите название и выберите участников";
                return;
            }

            pickerBusy = true;
            createGroupButton.interactable = false;
            pickerStatusText.text = "Создание чата...";
            try
            {
                MessengerChatDto chat = await MessengerApi.CreateGroupChatAsync(title, new List<int>(selectedUsers));
                picker.SetActive(false);
                await LoadChatsAsync();
                OpenChat(chats.Find(x => x.id == chat.id) ?? chat);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                pickerStatusText.text = "Не удалось создать групповой чат";
            }
            finally
            {
                pickerBusy = false;
                createGroupButton.interactable = true;
            }
        }

        private void SetStatus(string value) { statusText.text = value; }

        private void UpdateMuteButton()
        {
            if (muteButton == null)
                return;
            MessengerChatDto current = chats.Find(x => x.id == currentChatId);
            Text label = muteButton.GetComponentInChildren<Text>();
            if (label != null)
                label.text = current != null && current.muted ? "Включить" : "Без звука";
            muteButton.interactable = current != null && !changingMute;
        }

        private async void ToggleMute()
        {
            MessengerChatDto current = chats.Find(x => x.id == currentChatId);
            if (current == null || changingMute)
                return;
            changingMute = true;
            int chatId = current.id;
            UpdateMuteButton();
            try
            {
                bool muted = !current.muted;
                await MessengerApi.SetMutedAsync(chatId, muted);
                MessengerChatDto updated = chats.Find(x => x.id == chatId);
                if (updated != null)
                {
                    updated.muted = muted;
                    updated.unread = false;
                }
                RenderChats();
                SetStatus(string.Empty);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                SetStatus("Не удалось изменить уведомления чата");
            }
            finally
            {
                changingMute = false;
                UpdateMuteButton();
            }
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("MessengerCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            root = Rect("Background", canvasObject.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<Image>().color = background;

            RectTransform header = Rect("Header", root, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -105f), Vector2.zero);
            header.gameObject.AddComponent<Image>().color = new Color(0.23f, 0.18f, 0.38f);
            backButton = ButtonAt(header, "В меню", new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(18f, 18f), new Vector2(185f, -18f), new Color(0.42f, 0.36f, 0.55f));
            Text title = Label(header, "Мессенджер", 34, Color.white, TextAnchor.MiddleCenter);
            Stretch(title.rectTransform, new Vector2(210f, 0f), new Vector2(-195f, 0f));
            refreshButton = ButtonAt(header, "Обновить", new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-180f, 18f), new Vector2(-18f, -18f), new Color(0.42f, 0.36f, 0.55f));

            RectTransform sidebar = Rect("ChatsPanel", root, new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(16f, 16f), new Vector2(320f, -121f));
            sidebar.gameObject.AddComponent<Image>().color = Color.white;
            Text chatsLabel = Label(sidebar, "Чаты", 27, ink, TextAnchor.MiddleCenter);
            Place(chatsLabel.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(10f, -55f), new Vector2(-10f, -5f));
            directButton = ButtonAt(sidebar, "Личное", new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                new Vector2(10f, -125f), new Vector2(-5f, -60f), accent);
            groupButton = ButtonAt(sidebar, "Группа", new Vector2(0.5f, 1f), new Vector2(1f, 1f),
                new Vector2(5f, -125f), new Vector2(-10f, -60f), accent);
            CreateScroll(sidebar, new Vector2(0f, 0f), Vector2.one,
                new Vector2(10f, 10f), new Vector2(-10f, -138f), out chatsContent);

            RectTransform conversation = Rect("Conversation", root, Vector2.zero, Vector2.one,
                new Vector2(336f, 16f), new Vector2(-16f, -121f));
            conversation.gameObject.AddComponent<Image>().color = Color.white;
            chatTitleText = Label(conversation, "Общий чат", 30, ink, TextAnchor.MiddleLeft);
            Place(chatTitleText.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(20f, -58f), new Vector2(-190f, -6f));
            muteButton = ButtonAt(conversation, "Без звука", new Vector2(1f, 1f), Vector2.one,
                new Vector2(-176f, -58f), new Vector2(-15f, -6f), accent);
            muteButton.interactable = false;
            statusText = Label(conversation, string.Empty, 19, new Color(0.65f, 0.18f, 0.22f), TextAnchor.MiddleLeft);
            Place(statusText.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(20f, -87f), new Vector2(-20f, -59f));
            messagesScroll = CreateScroll(conversation, Vector2.zero, Vector2.one,
                new Vector2(15f, 106f), new Vector2(-15f, -94f), out messagesContent);
            messageInput = InputAt(conversation, "Сообщение", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(15f, 18f), new Vector2(-158f, 91f));
            sendButton = ButtonAt(conversation, "Отправить", new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-150f, 18f), new Vector2(-15f, 91f), accent);

            BuildPicker();
            BuildRowTemplates();

            bool hasEventSystem = false;
            foreach (GameObject sceneRoot in gameObject.scene.GetRootGameObjects())
            {
                if (sceneRoot.GetComponentInChildren<EventSystem>(true) != null)
                {
                    hasEventSystem = true;
                    break;
                }
            }
            if (!hasEventSystem)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                eventSystem.AddComponent<InputSystemUIInputModule>();
#else
                eventSystem.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        private void BuildPicker()
        {
            RectTransform overlay = Rect("UserPicker", root, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            overlay.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.05f, 0.13f, 0.75f);
            picker = overlay.gameObject;
            RectTransform card = Rect("PickerCard", overlay, new Vector2(0.12f, 0.12f), new Vector2(0.88f, 0.88f),
                Vector2.zero, Vector2.zero);
            card.gameObject.AddComponent<Image>().color = new Color(0.98f, 0.97f, 1f);
            pickerTitleText = Label(card, string.Empty, 30, ink, TextAnchor.MiddleCenter);
            Place(pickerTitleText.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(20f, -66f), new Vector2(-20f, -10f));
            groupTitleInput = InputAt(card, "Название чата", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(25f, -140f), new Vector2(-25f, -76f));
            CreateScroll(card, Vector2.zero, Vector2.one,
                new Vector2(25f, 125f), new Vector2(-25f, -150f), out usersContent);
            pickerStatusText = Label(card, string.Empty, 20, new Color(0.65f, 0.18f, 0.22f), TextAnchor.MiddleCenter);
            Place(pickerStatusText.rectTransform, Vector2.zero, new Vector2(1f, 0f),
                new Vector2(25f, 85f), new Vector2(-25f, 120f));
            createGroupButton = ButtonAt(card, "Создать чат", Vector2.zero, new Vector2(0.5f, 0f),
                new Vector2(25f, 20f), new Vector2(-5f, 82f), accent);
            cancelButton = ButtonAt(card, "Отмена", new Vector2(0.5f, 0f), new Vector2(1f, 0f),
                new Vector2(5f, 20f), new Vector2(-25f, 82f), new Color(0.45f, 0.45f, 0.52f));
            picker.SetActive(false);
        }

        private void BuildRowTemplates()
        {
            chatRowTemplate = ListButton(chatsContent, "Пример чата", 72f);
            chatRowTemplate.gameObject.name = "ChatRowTemplate";
            chatRowTemplate.gameObject.SetActive(false);

            userRowTemplate = ListButton(usersContent, "Пример пользователя", 64f);
            userRowTemplate.gameObject.name = "UserRowTemplate";
            userRowTemplate.gameObject.SetActive(false);

            RectTransform row = Row(messagesContent, "MessageRowTemplate", 92f, new Color(0.98f, 0.97f, 1f));
            Text header = Label(row, "Имя  •  дата", 20, ink, TextAnchor.MiddleLeft);
            header.gameObject.name = "Header";
            Place(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(15f, -36f), new Vector2(-15f, -5f));
            Text body = Label(row, "Пример сообщения", 23, ink, TextAnchor.UpperLeft);
            body.gameObject.name = "Body";
            body.verticalOverflow = VerticalWrapMode.Overflow;
            Stretch(body.rectTransform, new Vector2(15f, 7f), new Vector2(-15f, -39f));
            messageRowTemplate = row.gameObject.AddComponent<MessengerMessageRowView>();
            messageRowTemplate.SetReferences(header, body);
            row.gameObject.SetActive(false);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            Place(rect, Vector2.zero, Vector2.one, offsetMin, offsetMax);
        }

        private static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private Text Label(Transform parent, string value, int size, Color color, TextAnchor align)
        {
            RectTransform rect = Rect("Text", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.text = value ?? string.Empty;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private Button ButtonAt(Transform parent, string caption, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            RectTransform rect = Rect(caption + " Button", parent, anchorMin, anchorMax, offsetMin, offsetMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text label = Label(rect, caption, 21, Color.white, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform, new Vector2(4f, 0f), new Vector2(-4f, 0f));
            return button;
        }

        private Button ListButton(Transform parent, string caption, float height)
        {
            RectTransform row = Row(parent, caption, height, new Color(0.94f, 0.92f, 0.98f));
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = row.GetComponent<Image>();
            Text label = Label(row, caption, 21, ink, TextAnchor.MiddleLeft);
            Stretch(label.rectTransform, new Vector2(14f, 0f), new Vector2(-10f, 0f));
            return button;
        }

        private static RectTransform Row(Transform parent, string name, float height, Color color)
        {
            RectTransform row = Rect(name, parent, new Vector2(0f, 1f), Vector2.one,
                Vector2.zero, Vector2.zero);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            row.gameObject.AddComponent<Image>().color = color;
            return row;
        }

        private InputField InputAt(Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = Rect("Input", parent, anchorMin, anchorMax, offsetMin, offsetMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.96f, 0.95f, 0.98f);
            InputField input = rect.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;
            Text value = Label(rect, string.Empty, 23, ink, TextAnchor.MiddleLeft);
            Stretch(value.rectTransform, new Vector2(16f, 0f), new Vector2(-16f, 0f));
            Text hint = Label(rect, placeholder, 23, new Color(0.54f, 0.52f, 0.6f), TextAnchor.MiddleLeft);
            Stretch(hint.rectTransform, new Vector2(16f, 0f), new Vector2(-16f, 0f));
            input.textComponent = value;
            input.placeholder = hint;
            return input;
        }

        private ScrollRect CreateScroll(Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, out RectTransform content)
        {
            RectTransform scrollRect = Rect("Scroll", parent, anchorMin, anchorMax, offsetMin, offsetMax);
            scrollRect.gameObject.AddComponent<Image>().color = new Color(0.98f, 0.98f, 1f);
            ScrollRect scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
            RectTransform viewport = Rect("Viewport", scrollRect, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect("Content", viewport, new Vector2(0f, 1f), Vector2.one,
                Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 35f;
            return scroll;
        }

        private void ClearChildren(Transform parent)
        {
            if (parent == null)
                return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if ((chatRowTemplate != null && child == chatRowTemplate.transform) ||
                    (userRowTemplate != null && child == userRowTemplate.transform) ||
                    (messageRowTemplate != null && child == messageRowTemplate.transform))
                    continue;
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }
}
