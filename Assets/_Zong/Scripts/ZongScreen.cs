using System;
using System.Collections.Generic;
using System.Linq;
using PushPelmesh.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.Zong
{
    public class ZongScreen : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Mode")]
        [SerializeField] private GameObject modePanel;
        [SerializeField] private Button botModeButton;
        [SerializeField] private Button multiplayerModeButton;

        [Header("Bot")]
        [SerializeField] private GameObject botPanel;
        [SerializeField] private Text botScoreText;
        [SerializeField] private Text botDiceText;
        [SerializeField] private Text botStatusText;
        [SerializeField] private Image[] botDiceImages;
        [SerializeField] private Button botRollButton;
        [SerializeField] private Button botBankButton;
        [SerializeField] private Button botBackButton;

        [Header("Rooms")]
        [SerializeField] private GameObject roomsPanel;
        [SerializeField] private Transform roomsRoot;
        [SerializeField] private GameObject roomRowPrefab;
        [SerializeField] private Button refreshRoomsButton;
        [SerializeField] private Button openCreateRoomButton;
        [SerializeField] private Button roomsBackButton;
        [SerializeField] private InputField joinPasswordInput;
        [SerializeField] private Text roomsStatusText;

        [Header("Create Room")]
        [SerializeField] private GameObject createRoomPanel;
        [SerializeField] private InputField roomNameInput;
        [SerializeField] private InputField roomPasswordInput;
        [SerializeField] private InputField maxPlayersInput;
        [SerializeField] private InputField targetScoreInput;
        [SerializeField] private Button submitCreateRoomButton;
        [SerializeField] private Button cancelCreateRoomButton;
        [SerializeField] private Text createRoomStatusText;

        [Header("Room")]
        [SerializeField] private GameObject roomPanel;
        [SerializeField] private Text roomTitleText;
        [SerializeField] private Text roomMetaText;
        [SerializeField] private Text roomDiceText;
        [SerializeField] private Image[] roomDiceImages;
        [SerializeField] private Text roomTurnScoreText;
        [SerializeField] private Text roomStatusText;
        [SerializeField] private Transform playersRoot;
        [SerializeField] private GameObject playerRowPrefab;
        [SerializeField] private Button startRoomButton;
        [SerializeField] private Button rollRoomButton;
        [SerializeField] private Button bankRoomButton;
        [SerializeField] private Button refreshRoomButton;
        [SerializeField] private Button leaveRoomButton;

        [Header("Dice Animation")]
        [SerializeField] private ZongDiceAnimator diceAnimator;

        private readonly List<GameObject> spawnedRoomRows = new List<GameObject>();
        private readonly List<GameObject> spawnedPlayerRows = new List<GameObject>();
        private int selectedJoinRoomId;
        private int currentRoomId;
        private float nextRoomRefreshAt;

        private int botPlayerScore;
        private int botOpponentScore;
        private int botTurnScore;
        private int botRemainingDice = 6;
        private bool botPlayerTurn;
        private bool botGameOver;

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();

            if (roomRowPrefab != null)
                roomRowPrefab.SetActive(false);

            if (playerRowPrefab != null)
                playerRowPrefab.SetActive(false);

            AddListener(backButton, BackToMainMenu);
            AddListener(botModeButton, OpenBotMode);
            AddListener(multiplayerModeButton, OpenRooms);
            AddListener(botRollButton, BotRoll);
            AddListener(botBankButton, BotBank);
            AddListener(botBackButton, ShowMode);
            AddListener(refreshRoomsButton, RefreshRooms);
            AddListener(openCreateRoomButton, OpenCreateRoom);
            AddListener(roomsBackButton, ShowMode);
            AddListener(submitCreateRoomButton, CreateRoom);
            AddListener(cancelCreateRoomButton, OpenRooms);
            AddListener(startRoomButton, StartRoom);
            AddListener(rollRoomButton, RollRoom);
            AddListener(bankRoomButton, BankRoom);
            AddListener(refreshRoomButton, RefreshCurrentRoom);
            AddListener(leaveRoomButton, OpenRooms);
        }

        private void Start()
        {
            if (diceAnimator == null)
                diceAnimator = GetComponent<ZongDiceAnimator>();

            ShowMode();
        }

        private void Update()
        {
            if (currentRoomId <= 0 || roomPanel == null || !roomPanel.activeSelf)
                return;

            if (Time.unscaledTime < nextRoomRefreshAt)
                return;

            nextRoomRefreshAt = Time.unscaledTime + 3f;
            RefreshCurrentRoom();
        }

        public async void OpenJoinRoom(int roomId)
        {
            selectedJoinRoomId = roomId;
            SetRoomsStatus("Вход в комнату...");

            try
            {
                string password = joinPasswordInput != null ? joinPasswordInput.text : string.Empty;
                ZongRoomDto room = await ZongApi.JoinRoomAsync(roomId, password);
                RenderRoom(room);
                ShowRoom();
            }
            catch (Exception exception)
            {
                SetRoomsStatus("Не удалось войти: " + exception.Message);
            }
        }

        private async void OpenRooms()
        {
            currentRoomId = 0;
            selectedJoinRoomId = 0;
            ShowRooms();
            await LoadRoomsAsync();
        }

        private void OpenCreateRoom()
        {
            if (roomNameInput != null)
                roomNameInput.text = string.Empty;

            if (roomPasswordInput != null)
                roomPasswordInput.text = string.Empty;

            if (maxPlayersInput != null)
                maxPlayersInput.text = "2";

            if (targetScoreInput != null)
                targetScoreInput.text = "5000";

            SetCreateRoomStatus(string.Empty);
            ShowCreateRoom();
        }

        private async void RefreshRooms()
        {
            await LoadRoomsAsync();
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            SetRoomsStatus("Загрузка комнат...");
            ClearRows(spawnedRoomRows);

            try
            {
                ZongRoomsResponse response = await ZongApi.GetRoomsAsync();
                List<ZongRoomListItemDto> rooms = response != null && response.rooms != null
                    ? response.rooms
                    : new List<ZongRoomListItemDto>();

                for (int i = 0; i < rooms.Count; i++)
                    CreateRoomRow(rooms[i]);

                SetRoomsStatus(rooms.Count == 0 ? "Комнат пока нет" : "Комнат: " + rooms.Count);
            }
            catch (Exception exception)
            {
                SetRoomsStatus("Ошибка загрузки комнат: " + exception.Message);
            }
        }

        private async void CreateRoom()
        {
            CreateZongRoomRequest request = new CreateZongRoomRequest
            {
                name = roomNameInput != null ? roomNameInput.text.Trim() : string.Empty,
                password = roomPasswordInput != null ? roomPasswordInput.text : string.Empty,
                maxPlayers = ParseInt(maxPlayersInput != null ? maxPlayersInput.text : null, 2),
                targetScore = ParseInt(targetScoreInput != null ? targetScoreInput.text : null, 5000)
            };

            if (string.IsNullOrWhiteSpace(request.name))
            {
                SetCreateRoomStatus("Введите имя комнаты");
                return;
            }

            SetCreateRoomStatus("Создание комнаты...");
            SetCreateRoomInteractable(false);

            try
            {
                ZongRoomDto room = await ZongApi.CreateRoomAsync(request);
                RenderRoom(room);
                ShowRoom();
            }
            catch (Exception exception)
            {
                SetCreateRoomStatus("Ошибка создания: " + exception.Message);
            }
            finally
            {
                SetCreateRoomInteractable(true);
            }
        }

        private async void StartRoom()
        {
            if (currentRoomId <= 0)
                return;

            await RunRoomAction(() => ZongApi.StartRoomAsync(currentRoomId));
        }

        private async void RollRoom()
        {
            if (currentRoomId <= 0)
                return;

            SetRoomButtonsInteractable(false);

            try
            {
                ZongRoomDto room = await ZongApi.RollAsync(currentRoomId);
                RenderRoom(room, animateDice: true);
            }
            catch (Exception exception)
            {
                SetRoomStatus("РћС€РёР±РєР°: " + exception.Message);
            }
            finally
            {
                SetRoomButtonsFromState();
            }
        }

        private async void BankRoom()
        {
            if (currentRoomId <= 0)
                return;

            await RunRoomAction(() => ZongApi.BankAsync(currentRoomId));
        }

        private async void RefreshCurrentRoom()
        {
            if (currentRoomId <= 0)
                return;

            try
            {
                ZongRoomDto room = await ZongApi.GetRoomAsync(currentRoomId);
                RenderRoom(room);
            }
            catch (Exception exception)
            {
                SetRoomStatus("Ошибка обновления: " + exception.Message);
            }
        }

        private async System.Threading.Tasks.Task RunRoomAction(Func<System.Threading.Tasks.Task<ZongRoomDto>> action)
        {
            SetRoomButtonsInteractable(false);

            try
            {
                ZongRoomDto room = await action();
                RenderRoom(room);
            }
            catch (Exception exception)
            {
                SetRoomStatus("Ошибка: " + exception.Message);
            }
            finally
            {
                SetRoomButtonsFromState();
            }
        }

        private void RenderRoom(ZongRoomDto room, bool animateDice = false)
        {
            if (room == null)
                return;

            currentRoomId = room.id;

            if (roomTitleText != null)
                roomTitleText.text = room.name;

            if (roomMetaText != null)
                roomMetaText.text = $"{room.status} | цель {room.targetScore} | игроков {room.players.Count}/{room.maxPlayers}";

            if (roomDiceText != null)
                roomDiceText.text = string.IsNullOrWhiteSpace(room.lastDice) ? "Кости: -" : "Кости: " + room.lastDice;

            List<int> roomDice = ParseDice(room.lastDice);

            if (animateDice)
                AnimateDice(roomDiceImages, roomDice);
            else
                ShowDice(roomDiceImages, roomDice);

            if (roomTurnScoreText != null)
                roomTurnScoreText.text = $"Очки хода: {room.currentTurnScore} | осталось костей: {room.remainingDice}";

            SetRoomStatus(string.IsNullOrWhiteSpace(room.lastRollMessage) ? (room.isMyTurn ? "Ваш ход" : "Ожидание хода другого игрока") : room.lastRollMessage);
            RenderPlayers(room);

            SetButton(startRoomButton, room.canStart);
            SetButton(rollRoomButton, room.canRoll);
            SetButton(bankRoomButton, room.canBank);
        }

        private void RenderPlayers(ZongRoomDto room)
        {
            ClearRows(spawnedPlayerRows);

            if (playersRoot == null || playerRowPrefab == null || room.players == null)
                return;

            for (int i = 0; i < room.players.Count; i++)
            {
                GameObject rowObject = Instantiate(playerRowPrefab, playersRoot);
                PrepareSpawnedRow(rowObject);
                rowObject.SetActive(true);
                spawnedPlayerRows.Add(rowObject);

                ZongPlayerRowView row = rowObject.GetComponent<ZongPlayerRowView>();

                if (row != null)
                    row.Setup(room.players[i]);
            }
        }

        private void CreateRoomRow(ZongRoomListItemDto room)
        {
            if (roomsRoot == null || roomRowPrefab == null)
                return;

            GameObject rowObject = Instantiate(roomRowPrefab, roomsRoot);
            PrepareSpawnedRow(rowObject);
            rowObject.SetActive(true);
            spawnedRoomRows.Add(rowObject);

            ZongRoomRowView row = rowObject.GetComponent<ZongRoomRowView>();

            if (row != null)
                row.Setup(this, room);
        }

        private void OpenBotMode()
        {
            botPlayerScore = 0;
            botOpponentScore = 0;
            botTurnScore = 0;
            botRemainingDice = 6;
            botPlayerTurn = true;
            botGameOver = false;
            ShowBot();
            ShowDice(botDiceImages, new List<int>());
            RenderBot("Ваш ход. Бросайте кости.");
        }

        private void BotRoll()
        {
            if (!botPlayerTurn || botGameOver)
                return;

            List<int> dice = ZongScoring.RollDice(botRemainingDice);
            ZongRollScore score = ZongScoring.CalculateScore(dice);
            AnimateDice(botDiceImages, dice);

            if (score.Score <= 0)
            {
                botTurnScore = 0;
                botRemainingDice = 6;
                RenderBot("Зонг. Очки хода сгорели. Ход бота.");
                RunBotTurn();
                return;
            }

            botTurnScore += score.Score;
            botRemainingDice -= score.UsedDice;

            if (botRemainingDice <= 0)
                botRemainingDice = 6;

            RenderBot($"Выпало: {string.Join(",", dice)}. +{score.Score}. Можно бросить ещё или забрать.");
        }

        private void BotBank()
        {
            if (!botPlayerTurn || botGameOver || botTurnScore <= 0)
                return;

            botPlayerScore += botTurnScore;
            botTurnScore = 0;
            botRemainingDice = 6;

            if (botPlayerScore >= 5000)
            {
                botGameOver = true;
                RenderBot("Вы победили.");
                return;
            }

            RenderBot("Вы забрали очки. Ход бота.");
            RunBotTurn();
        }

        private void RunBotTurn()
        {
            botPlayerTurn = false;
            int turnScore = 0;
            int remaining = 6;
            string lastMessage = string.Empty;

            for (int safety = 0; safety < 8; safety++)
            {
                List<int> dice = ZongScoring.RollDice(remaining);
                ZongRollScore score = ZongScoring.CalculateScore(dice);

                if (score.Score <= 0)
                {
                    lastMessage = $"Бот выбросил {string.Join(",", dice)}: зонг.";
                    turnScore = 0;
                    break;
                }

                turnScore += score.Score;
                remaining -= score.UsedDice;

                if (remaining <= 0)
                    remaining = 6;

                lastMessage = $"Бот выбросил {string.Join(",", dice)} и набрал за ход {turnScore}.";

                if (turnScore >= 450)
                    break;
            }

            botOpponentScore += turnScore;
            botTurnScore = 0;
            botRemainingDice = 6;
            botPlayerTurn = true;

            if (botOpponentScore >= 5000)
            {
                botGameOver = true;
                RenderBot(lastMessage + " Бот победил.");
                return;
            }

            RenderBot(lastMessage + " Ваш ход.");
        }

        private void RenderBot(string message)
        {
            if (botScoreText != null)
                botScoreText.text = $"Вы: {botPlayerScore} | Бот: {botOpponentScore} | цель: 5000";

            if (botDiceText != null)
                botDiceText.text = $"Очки хода: {botTurnScore} | осталось костей: {botRemainingDice}";

            if (botStatusText != null)
                botStatusText.text = message;

            SetButton(botRollButton, botPlayerTurn && !botGameOver);
            SetButton(botBankButton, botPlayerTurn && !botGameOver && botTurnScore > 0);
        }

        private void ShowMode()
        {
            SetPanel(modePanel, true);
            SetPanel(botPanel, false);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, false);
            SetPanel(roomPanel, false);
        }

        private void ShowBot()
        {
            SetPanel(modePanel, false);
            SetPanel(botPanel, true);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, false);
            SetPanel(roomPanel, false);
        }

        private void ShowRooms()
        {
            SetPanel(modePanel, false);
            SetPanel(botPanel, false);
            SetPanel(roomsPanel, true);
            SetPanel(createRoomPanel, false);
            SetPanel(roomPanel, false);
        }

        private void ShowCreateRoom()
        {
            SetPanel(modePanel, false);
            SetPanel(botPanel, false);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, true);
            SetPanel(roomPanel, false);
        }

        private void ShowRoom()
        {
            SetPanel(modePanel, false);
            SetPanel(botPanel, false);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, false);
            SetPanel(roomPanel, true);
            nextRoomRefreshAt = Time.unscaledTime + 3f;
        }

        private void SetRoomButtonsInteractable(bool interactable)
        {
            SetButton(startRoomButton, interactable);
            SetButton(rollRoomButton, interactable);
            SetButton(bankRoomButton, interactable);
        }

        private void SetRoomButtonsFromState()
        {
            SetButton(refreshRoomButton, true);
            SetButton(leaveRoomButton, true);
        }

        private void SetCreateRoomInteractable(bool interactable)
        {
            if (submitCreateRoomButton != null)
                submitCreateRoomButton.interactable = interactable;

            if (cancelCreateRoomButton != null)
                cancelCreateRoomButton.interactable = interactable;
        }

        private void SetRoomsStatus(string value)
        {
            if (roomsStatusText != null)
                roomsStatusText.text = value;
        }

        private void SetCreateRoomStatus(string value)
        {
            if (createRoomStatusText != null)
                createRoomStatusText.text = value;
        }

        private void SetRoomStatus(string value)
        {
            if (roomStatusText != null)
                roomStatusText.text = value;
        }

        private void BackToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        private static void SetButton(Button button, bool interactable)
        {
            if (button != null)
                button.interactable = interactable;
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, out int result) ? result : fallback;
        }

        private void AnimateDice(Image[] diceImages, List<int> diceValues)
        {
            if (diceAnimator != null)
                diceAnimator.AnimateToResult(diceImages, diceValues);
            else
                ShowDice(diceImages, diceValues);
        }

        private void ShowDice(Image[] diceImages, List<int> diceValues)
        {
            if (diceAnimator != null)
                diceAnimator.ShowResult(diceImages, diceValues);
        }

        private static List<int> ParseDice(string value)
        {
            List<int> result = new List<int>();

            if (string.IsNullOrWhiteSpace(value))
                return result;

            string[] parts = value.Split(',');

            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int dice) && dice >= 1 && dice <= 6)
                    result.Add(dice);
            }

            return result;
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
        }

        private static void ClearRows(List<GameObject> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] != null)
                    Destroy(rows[i]);
            }

            rows.Clear();
        }
    }
}
