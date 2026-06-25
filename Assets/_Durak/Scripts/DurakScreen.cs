using System;
using System.Collections.Generic;
using System.Linq;
using PushPelmesh.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.Durak
{
    public class DurakScreen : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Cards")]
        [SerializeField] private DurakCardSkinSet cardSkinSet;

        [Header("Mode")]
        [SerializeField] private GameObject modePanel;
        [SerializeField] private Button botModeButton;
        [SerializeField] private Button multiplayerModeButton;

        [Header("Bot Setup")]
        [SerializeField] private GameObject botSetupPanel;
        [SerializeField] private InputField botCardCountInput;
        [SerializeField] private InputField botCountInput;
        [SerializeField] private Button startBotGameButton;
        [SerializeField] private Button botSetupBackButton;
        [SerializeField] private Text botSetupStatusText;

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
        [SerializeField] private InputField roomCardCountInput;
        [SerializeField] private Button submitCreateRoomButton;
        [SerializeField] private Button cancelCreateRoomButton;
        [SerializeField] private Text createRoomStatusText;

        [Header("Game")]
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private Text gameTitleText;
        [SerializeField] private Text gameMetaText;
        [SerializeField] private Text gameStatusText;
        [SerializeField] private Text myTurnText;
        [SerializeField] private Transform playersRoot;
        [SerializeField] private GameObject playerRowPrefab;
        [SerializeField] private Transform tableRoot;
        [SerializeField] private Transform handRoot;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Image deckBackImage;
        [SerializeField] private Text deckCountText;
        [SerializeField] private Image trumpCardImage;
        [SerializeField] private Text trumpSuitText;
        [SerializeField] private Button startRoomButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button defendButton;
        [SerializeField] private Button transferButton;
        [SerializeField] private Button takeButton;
        [SerializeField] private Button passButton;
        [SerializeField] private Button refreshGameButton;
        [SerializeField] private Button leaveGameButton;

        private readonly List<GameObject> spawnedRooms = new List<GameObject>();
        private readonly List<GameObject> spawnedPlayers = new List<GameObject>();
        private readonly List<GameObject> spawnedCards = new List<GameObject>();
        private readonly Dictionary<string, RectTransform> attackDropTargets = new Dictionary<string, RectTransform>();
        private int currentRoomId;
        private string selectedCardCode;
        private string selectedAttackCardCode;
        private bool localBotMode;
        private float nextRefreshAt;
        private DurakRoomDto latestServerRoom;
        private RectTransform dragLayer;

        private LocalDurakGame localGame;

        public Vector3 TableCenter => tableRoot != null ? tableRoot.position : transform.position;
        public Transform DragLayer => dragLayer;

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();

            if (roomRowPrefab != null)
                roomRowPrefab.SetActive(false);

            if (playerRowPrefab != null)
                playerRowPrefab.SetActive(false);

            if (cardPrefab != null)
                cardPrefab.SetActive(false);

            PrepareGameLayers();

            AddListener(backButton, BackToMainMenu);
            AddListener(botModeButton, OpenBotSetup);
            AddListener(multiplayerModeButton, OpenRooms);
            AddListener(startBotGameButton, StartBotGame);
            AddListener(botSetupBackButton, ShowMode);
            AddListener(refreshRoomsButton, RefreshRooms);
            AddListener(openCreateRoomButton, OpenCreateRoom);
            AddListener(roomsBackButton, ShowMode);
            AddListener(submitCreateRoomButton, CreateRoom);
            AddListener(cancelCreateRoomButton, OpenRooms);
            AddListener(startRoomButton, StartRoom);
            AddListener(attackButton, Attack);
            AddListener(defendButton, Defend);
            AddListener(transferButton, Transfer);
            AddListener(takeButton, Take);
            AddListener(passButton, Pass);
            AddListener(refreshGameButton, RefreshGame);
            AddListener(leaveGameButton, OpenRooms);
        }

        private void Start()
        {
            ShowMode();
        }

        private void Update()
        {
            if (localBotMode || currentRoomId <= 0 || gamePanel == null || !gamePanel.activeSelf)
                return;

            if (Time.unscaledTime < nextRefreshAt)
                return;

            nextRefreshAt = Time.unscaledTime + 3f;
            RefreshGame();
        }

        public async void OpenJoinRoom(int roomId)
        {
            currentRoomId = roomId;
            SetRoomsStatus("Вход в комнату...");

            try
            {
                string password = joinPasswordInput != null ? joinPasswordInput.text : string.Empty;
                DurakRoomDto room = await DurakApi.JoinRoomAsync(roomId, password);
                RenderServerRoom(room);
                ShowGame();
            }
            catch (Exception exception)
            {
                SetRoomsStatus("Не удалось войти: " + exception.Message);
            }
        }

        public void OnCardClicked(string cardCode)
        {
            if (localBotMode)
            {
                selectedCardCode = cardCode;
                RenderLocalGame();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedAttackCardCode) && IsAttackCardOnTable(cardCode))
                selectedAttackCardCode = cardCode;
            else
                selectedCardCode = cardCode;

            SetGameStatus(BuildSelectionMessage());
        }

        public bool TryDropCard(string cardCode, Vector2 screenPosition, Camera eventCamera)
        {
            RectTransform tableRect = tableRoot as RectTransform;

            if (tableRect == null || !RectTransformUtility.RectangleContainsScreenPoint(tableRect, screenPosition, eventCamera))
                return false;

            string attackCode = FindAttackCardAt(screenPosition, eventCamera);

            if (localBotMode)
            {
                if (localGame == null)
                    return false;

                if (localGame.CanPlayerDefend && !string.IsNullOrWhiteSpace(attackCode))
                {
                    localGame.PlayerDefend(attackCode, cardCode);
                    RenderLocalGame();
                    return true;
                }

                if (localGame.CanPlayerTransfer && string.IsNullOrWhiteSpace(attackCode))
                {
                    localGame.PlayerTransfer(cardCode);
                    RenderLocalGame();
                    return true;
                }

                if (localGame.CanPlayerAttack)
                {
                    localGame.PlayerAttack(cardCode);
                    RenderLocalGame();
                    return true;
                }

                return false;
            }

            if (latestServerRoom == null)
                return false;

            if (latestServerRoom.canDefend && !string.IsNullOrWhiteSpace(attackCode))
            {
                selectedAttackCardCode = attackCode;
                selectedCardCode = cardCode;
                Defend();
                return true;
            }

            if (latestServerRoom.canTransfer && string.IsNullOrWhiteSpace(attackCode))
            {
                selectedCardCode = cardCode;
                Transfer();
                return true;
            }

            if (latestServerRoom.canAttack)
            {
                selectedCardCode = cardCode;
                Attack();
                return true;
            }

            return false;
        }

        private void OpenBotSetup()
        {
            if (botCardCountInput != null)
                botCardCountInput.text = "36";

            if (botCountInput != null)
                botCountInput.text = "1";

            SetBotSetupStatus("24 карты: до 3 ботов. 36/52 карты: до 5 ботов.");
            ShowBotSetup();
        }

        private void StartBotGame()
        {
            int cardCount = NormalizeCardCount(ParseInt(botCardCountInput != null ? botCardCountInput.text : null, 36));
            int botCount = Mathf.Clamp(ParseInt(botCountInput != null ? botCountInput.text : null, 1), 1, MaxOpponents(cardCount));

            localBotMode = true;
            currentRoomId = 0;
            selectedCardCode = string.Empty;
            selectedAttackCardCode = string.Empty;
            localGame = LocalDurakGame.Create(cardCount, botCount);
            RenderLocalGame();
            ShowGame();
        }

        private async void OpenRooms()
        {
            localBotMode = false;
            currentRoomId = 0;
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

            if (roomCardCountInput != null)
                roomCardCountInput.text = "36";

            SetCreateRoomStatus("24 карты: до 4 игроков. 36/52 карты: до 6 игроков.");
            ShowCreateRoom();
        }

        private async void RefreshRooms()
        {
            await LoadRoomsAsync();
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            ClearRows(spawnedRooms);
            SetRoomsStatus("Загрузка комнат...");

            try
            {
                DurakRoomsResponse response = await DurakApi.GetRoomsAsync();
                List<DurakRoomListItemDto> rooms = response != null && response.rooms != null
                    ? response.rooms
                    : new List<DurakRoomListItemDto>();

                for (int i = 0; i < rooms.Count; i++)
                    CreateRoomRow(rooms[i]);

                SetRoomsStatus(rooms.Count == 0 ? "Комнат пока нет" : "Комнат: " + rooms.Count);
            }
            catch (Exception exception)
            {
                SetRoomsStatus("Ошибка загрузки: " + exception.Message);
            }
        }

        private async void CreateRoom()
        {
            int cardCount = NormalizeCardCount(ParseInt(roomCardCountInput != null ? roomCardCountInput.text : null, 36));
            int maxPlayers = Mathf.Clamp(ParseInt(maxPlayersInput != null ? maxPlayersInput.text : null, 2), 2, MaxPlayers(cardCount));

            CreateDurakRoomRequest request = new CreateDurakRoomRequest
            {
                name = roomNameInput != null ? roomNameInput.text.Trim() : string.Empty,
                password = roomPasswordInput != null ? roomPasswordInput.text : string.Empty,
                maxPlayers = maxPlayers,
                cardCount = cardCount
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
                DurakRoomDto room = await DurakApi.CreateRoomAsync(request);
                RenderServerRoom(room);
                ShowGame();
            }
            catch (Exception exception)
            {
                SetCreateRoomStatus("Ошибка: " + exception.Message);
            }
            finally
            {
                SetCreateRoomInteractable(true);
            }
        }

        private async void StartRoom()
        {
            if (localBotMode || currentRoomId <= 0)
                return;

            await RunServerAction(() => DurakApi.StartAsync(currentRoomId));
        }

        private async void Attack()
        {
            if (localBotMode)
            {
                localGame.PlayerAttack(selectedCardCode);
                selectedCardCode = string.Empty;
                RenderLocalGame();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedCardCode))
            {
                SetGameStatus("Выберите карту для атаки.");
                return;
            }

            await RunServerAction(() => DurakApi.AttackAsync(currentRoomId, selectedCardCode));
            selectedCardCode = string.Empty;
        }

        private async void Defend()
        {
            if (localBotMode)
            {
                localGame.PlayerDefend(selectedAttackCardCode, selectedCardCode);
                selectedAttackCardCode = string.Empty;
                selectedCardCode = string.Empty;
                RenderLocalGame();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedAttackCardCode) || string.IsNullOrWhiteSpace(selectedCardCode))
            {
                SetGameStatus("Выберите карту на столе и карту из руки.");
                return;
            }

            await RunServerAction(() => DurakApi.DefendAsync(currentRoomId, selectedAttackCardCode, selectedCardCode));
            selectedAttackCardCode = string.Empty;
            selectedCardCode = string.Empty;
        }

        private async void Transfer()
        {
            if (localBotMode)
            {
                localGame.PlayerTransfer(selectedCardCode);
                selectedCardCode = string.Empty;
                RenderLocalGame();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedCardCode))
            {
                SetGameStatus("Выберите карту для перевода.");
                return;
            }

            await RunServerAction(() => DurakApi.TransferAsync(currentRoomId, selectedCardCode));
            selectedCardCode = string.Empty;
        }

        private async void Take()
        {
            if (localBotMode)
            {
                localGame.PlayerTake();
                selectedCardCode = string.Empty;
                selectedAttackCardCode = string.Empty;
                RenderLocalGame();
                return;
            }

            await RunServerAction(() => DurakApi.TakeAsync(currentRoomId));
        }

        private async void Pass()
        {
            if (localBotMode)
            {
                localGame.PlayerPass();
                RenderLocalGame();
                return;
            }

            await RunServerAction(() => DurakApi.PassAsync(currentRoomId));
        }

        private async void RefreshGame()
        {
            if (localBotMode || currentRoomId <= 0)
                return;

            try
            {
                DurakRoomDto room = await DurakApi.GetRoomAsync(currentRoomId);
                RenderServerRoom(room);
            }
            catch (Exception exception)
            {
                SetGameStatus("Ошибка обновления: " + exception.Message);
            }
        }

        private async System.Threading.Tasks.Task RunServerAction(Func<System.Threading.Tasks.Task<DurakRoomDto>> action)
        {
            SetGameButtonsInteractable(false);

            try
            {
                DurakRoomDto room = await action();
                RenderServerRoom(room);
            }
            catch (Exception exception)
            {
                if (latestServerRoom != null)
                    RenderServerRoom(latestServerRoom);

                SetGameStatus("Ошибка: " + exception.Message);
            }
            finally
            {
                SetGameButtonsInteractable(true);
            }
        }

        private void RenderServerRoom(DurakRoomDto room)
        {
            if (room == null)
                return;

            localBotMode = false;
            currentRoomId = room.id;
            latestServerRoom = room;
            selectedCardCode = string.Empty;
            selectedAttackCardCode = string.Empty;

            if (gameTitleText != null)
                gameTitleText.text = room.name;

            if (gameMetaText != null)
                gameMetaText.text = $"{room.status} | {room.cardCount} карт | колода: {room.deckCount} | козырь: {DurakCardText.Format(new DurakCardDto { code = room.trumpCardCode, rank = RankFromCode(room.trumpCardCode), suit = room.trumpSuit })}";

            SetGameStatus(string.IsNullOrWhiteSpace(room.message) ? "Ожидание действия" : room.message);
            RenderDeck(room.deckCount, room.trumpCardCode, room.trumpSuit);
            bool hasOpenAttack = room.table != null && room.table.Any(x => x.attack != null && x.defense == null);
            int currentTurnPlayerId = hasOpenAttack ? room.defenderPlayerId : room.attackerPlayerId;
            bool isMyTurn = string.Equals(room.status, "InProgress", StringComparison.OrdinalIgnoreCase) && currentTurnPlayerId == room.myPlayerId;
            RenderPlayers(room.players, room.myPlayerId, currentTurnPlayerId);
            SetMyTurnIndicator(isMyTurn);
            RenderTable(room.table, room.canDefend);
            RenderHand(room.myHand, room.canAttack || room.canDefend || room.canTransfer, room.trumpSuit);

            SetButton(startRoomButton, room.canStart);
            SetButton(attackButton, room.canAttack);
            SetButton(defendButton, room.canDefend);
            SetButton(transferButton, room.canTransfer);
            SetButton(takeButton, room.canTake);
            SetButton(passButton, room.canPass);
            nextRefreshAt = Time.unscaledTime + 3f;
        }

        private void RenderLocalGame()
        {
            if (localGame == null)
                return;

            if (gameTitleText != null)
                gameTitleText.text = "Дурак с ботами";

            if (gameMetaText != null)
                gameMetaText.text = $"{localGame.CardCount} карт | колода: {localGame.Deck.Count} | козырь: {DurakCardText.Format(localGame.TrumpCard)}";

            SetGameStatus(localGame.Message);
            RenderDeck(localGame.Deck.Count, localGame.TrumpCard.code, localGame.TrumpCard.suit);
            bool localHasOpenAttack = localGame.Table.Any(x => x.attack != null && x.defense == null);
            int localCurrentPlayerId = localHasOpenAttack ? localGame.DefenderIndex : localGame.AttackerIndex;
            RenderPlayers(localGame.ToPlayers(), 0, localCurrentPlayerId);
            SetMyTurnIndicator(!localGame.GameOver && localCurrentPlayerId == 0);
            RenderTable(localGame.Table, localGame.CanPlayerDefend);
            RenderHand(
                localGame.PlayerHand,
                localGame.CanPlayerAttack || localGame.CanPlayerDefend || localGame.CanPlayerTransfer,
                localGame.TrumpCard.suit);

            SetButton(startRoomButton, false);
            SetButton(attackButton, localGame.CanPlayerAttack);
            SetButton(defendButton, localGame.CanPlayerDefend);
            SetButton(transferButton, localGame.CanPlayerTransfer);
            SetButton(takeButton, localGame.CanPlayerDefend);
            SetButton(passButton, localGame.CanPlayerPass);
        }

        private void RenderPlayers(List<DurakPlayerDto> players, int myPlayerId, int currentTurnPlayerId)
        {
            ClearRows(spawnedPlayers);

            if (playersRoot == null || playerRowPrefab == null || players == null)
                return;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null || players[i].id == myPlayerId)
                    continue;

                GameObject rowObject = Instantiate(playerRowPrefab, playersRoot);
                PrepareSpawnedRow(rowObject);
                rowObject.SetActive(true);
                spawnedPlayers.Add(rowObject);

                DurakPlayerRowView row = rowObject.GetComponent<DurakPlayerRowView>();

                if (row != null)
                    row.Setup(players[i], cardSkinSet, true, players[i].id == currentTurnPlayerId);
            }
        }

        private void RenderTable(List<DurakTableCardDto> table, bool attackCardsSelectable)
        {
            ClearRows(spawnedCards);
            attackDropTargets.Clear();

            if (tableRoot == null || cardPrefab == null || table == null)
                return;

            for (int i = 0; i < table.Count; i++)
            {
                DurakTableCardDto pair = table[i];
                RectTransform pairRect = CreateTablePair(i);
                GameObject attackCard = CreateCard(pairRect, pair.attack, attackCardsSelectable && pair.defense == null, false, false);
                PositionTableCard(attackCard, new Vector2(-17f, 10f), 0f);

                if (pair.attack != null && !string.IsNullOrWhiteSpace(pair.attack.code))
                    attackDropTargets[pair.attack.code] = pairRect;

                if (pair.defense != null)
                {
                    GameObject defenseCard = CreateCard(pairRect, pair.defense, false, false, false);
                    PositionTableCard(defenseCard, new Vector2(18f, -16f), -5f);
                }
            }
        }

        private void RenderHand(List<DurakCardDto> cards, bool selectable, string trumpSuit)
        {
            if (handRoot == null || cardPrefab == null || cards == null)
                return;

            List<DurakCardDto> sortedCards = cards
                .OrderByDescending(x => string.Equals(x.suit, trumpSuit, StringComparison.OrdinalIgnoreCase))
                .ThenBy(x => x.suit)
                .ThenBy(x => x.value)
                .ToList();

            for (int i = 0; i < sortedCards.Count; i++)
            {
                GameObject cardObject = CreateCard(handRoot, sortedCards[i], selectable, selectable);
                float t = sortedCards.Count <= 1 ? 0.5f : i / (float)(sortedCards.Count - 1);
                float angle = Mathf.Lerp(8f, -8f, t);
                cardObject.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private GameObject CreateCard(Transform parent, DurakCardDto card, bool selectable, bool draggable, bool track = true)
        {
            GameObject cardObject = Instantiate(cardPrefab, parent);
            PrepareSpawnedRow(cardObject);
            cardObject.SetActive(true);
            if (track)
                spawnedCards.Add(cardObject);

            DurakCardView view = cardObject.GetComponent<DurakCardView>();

            if (view != null)
                view.Setup(this, card, cardSkinSet, selectable, draggable);

            return cardObject;
        }

        private RectTransform CreateTablePair(int index)
        {
            GameObject pairObject = new GameObject("Table Pair " + (index + 1), typeof(RectTransform), typeof(LayoutElement));
            pairObject.transform.SetParent(tableRoot, false);
            RectTransform pairRect = pairObject.GetComponent<RectTransform>();
            pairRect.sizeDelta = new Vector2(160f, 142f);

            LayoutElement layout = pairObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 160f;
            layout.preferredHeight = 142f;
            spawnedCards.Add(pairObject);
            return pairRect;
        }

        private static void PositionTableCard(GameObject cardObject, Vector2 position, float rotation)
        {
            if (cardObject == null)
                return;

            LayoutElement layout = cardObject.GetComponent<LayoutElement>();

            if (layout != null)
                layout.ignoreLayout = true;

            RectTransform rect = cardObject.transform as RectTransform;

            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(88f, 126f);
            rect.anchoredPosition = position;
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private string FindAttackCardAt(Vector2 screenPosition, Camera eventCamera)
        {
            foreach (KeyValuePair<string, RectTransform> target in attackDropTargets)
            {
                if (target.Value != null && RectTransformUtility.RectangleContainsScreenPoint(target.Value, screenPosition, eventCamera))
                    return target.Key;
            }

            return string.Empty;
        }

        private void RenderDeck(int deckCount, string trumpCardCode, string trumpSuit)
        {
            bool hasDeck = deckCount > 0;

            if (deckBackImage != null)
            {
                deckBackImage.gameObject.SetActive(hasDeck);
                deckBackImage.sprite = cardSkinSet != null ? cardSkinSet.CardBackSprite : null;
                deckBackImage.color = deckBackImage.sprite != null ? Color.white : new Color(0.2f, 0.32f, 0.46f);
                deckBackImage.preserveAspect = true;
            }

            if (deckCountText != null)
                deckCountText.text = hasDeck ? deckCount.ToString() : string.Empty;

            DurakCardDto trumpCard = new DurakCardDto
            {
                code = trumpCardCode,
                rank = RankFromCode(trumpCardCode),
                suit = trumpSuit
            };
            Sprite trumpSprite = cardSkinSet != null ? cardSkinSet.GetSprite(trumpCardCode) : null;

            if (trumpCardImage != null)
            {
                trumpCardImage.gameObject.SetActive(hasDeck);
                trumpCardImage.sprite = trumpSprite;
                trumpCardImage.color = trumpSprite != null ? Color.white : new Color(0.98f, 0.98f, 0.95f);
                trumpCardImage.preserveAspect = true;
            }

            if (trumpSuitText != null)
            {
                trumpSuitText.gameObject.SetActive(true);
                trumpSuitText.text = hasDeck && trumpSprite == null
                    ? DurakCardText.Format(trumpCard)
                    : hasDeck ? string.Empty : DurakCardText.SuitSymbol(trumpSuit);
            }
        }

        private void CreateRoomRow(DurakRoomListItemDto room)
        {
            if (roomsRoot == null || roomRowPrefab == null)
                return;

            GameObject rowObject = Instantiate(roomRowPrefab, roomsRoot);
            PrepareSpawnedRow(rowObject);
            rowObject.SetActive(true);
            spawnedRooms.Add(rowObject);

            DurakRoomRowView row = rowObject.GetComponent<DurakRoomRowView>();

            if (row != null)
                row.Setup(this, room);
        }

        private bool IsAttackCardOnTable(string cardCode)
        {
            if (localBotMode && localGame != null)
                return localGame.Table.Any(x => x.attack != null && x.attack.code == cardCode);

            return latestServerRoom != null &&
                   latestServerRoom.table != null &&
                   latestServerRoom.table.Any(x => x.attack != null && x.attack.code == cardCode && x.defense == null);
        }

        private string BuildSelectionMessage()
        {
            if (!string.IsNullOrWhiteSpace(selectedAttackCardCode) && !string.IsNullOrWhiteSpace(selectedCardCode))
                return $"Выбрано: {selectedAttackCardCode} -> {selectedCardCode}";

            if (!string.IsNullOrWhiteSpace(selectedCardCode))
                return "Выбрана карта: " + selectedCardCode;

            return "Выберите карту.";
        }

        private void ShowMode()
        {
            localBotMode = false;
            SetPanel(modePanel, true);
            SetPanel(botSetupPanel, false);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, false);
            SetPanel(gamePanel, false);
        }

        private void ShowBotSetup()
        {
            SetPanel(modePanel, false);
            SetPanel(botSetupPanel, true);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, false);
            SetPanel(gamePanel, false);
        }

        private void ShowRooms()
        {
            SetPanel(modePanel, false);
            SetPanel(botSetupPanel, false);
            SetPanel(roomsPanel, true);
            SetPanel(createRoomPanel, false);
            SetPanel(gamePanel, false);
        }

        private void ShowCreateRoom()
        {
            SetPanel(modePanel, false);
            SetPanel(botSetupPanel, false);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, true);
            SetPanel(gamePanel, false);
        }

        private void ShowGame()
        {
            SetPanel(modePanel, false);
            SetPanel(botSetupPanel, false);
            SetPanel(roomsPanel, false);
            SetPanel(createRoomPanel, false);
            SetPanel(gamePanel, true);
        }

        private void SetGameButtonsInteractable(bool interactable)
        {
            SetButton(startRoomButton, interactable);
            SetButton(attackButton, interactable);
            SetButton(defendButton, interactable);
            SetButton(transferButton, interactable);
            SetButton(takeButton, interactable);
            SetButton(passButton, interactable);
        }

        private void SetCreateRoomInteractable(bool interactable)
        {
            if (submitCreateRoomButton != null)
                submitCreateRoomButton.interactable = interactable;

            if (cancelCreateRoomButton != null)
                cancelCreateRoomButton.interactable = interactable;
        }

        private void SetBotSetupStatus(string value)
        {
            if (botSetupStatusText != null)
                botSetupStatusText.text = value;
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

        private void SetGameStatus(string value)
        {
            if (gameStatusText != null)
                gameStatusText.text = value;
        }

        private void SetMyTurnIndicator(bool visible)
        {
            if (myTurnText == null)
                return;

            myTurnText.gameObject.SetActive(visible);
            myTurnText.text = visible ? "ВАШ ХОД" : string.Empty;
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

        private static int NormalizeCardCount(int value)
        {
            if (value <= 24)
                return 24;

            return value <= 36 ? 36 : 52;
        }

        private static int MaxOpponents(int cardCount)
        {
            return cardCount == 24 ? 3 : 5;
        }

        private static int MaxPlayers(int cardCount)
        {
            return cardCount == 24 ? 4 : 6;
        }

        private static string RankFromCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "";

            return code.Length > 2 ? code.Substring(0, code.Length - 1) : code.Substring(0, 1);
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

        private void PrepareGameLayers()
        {
            if (tableRoot != null)
            {
                Image tableCover = tableRoot.GetComponent<Image>();

                if (tableCover != null)
                {
                    Color color = tableCover.color;
                    color.a = 0f;
                    tableCover.color = color;
                    tableCover.raycastTarget = false;
                }
            }

            Canvas rootCanvas = handRoot != null ? handRoot.GetComponentInParent<Canvas>() : null;

            if (rootCanvas == null)
                return;

            Transform existing = rootCanvas.transform.Find("DurakDragLayer");

            if (existing != null)
            {
                dragLayer = existing as RectTransform;
                return;
            }

            GameObject layerObject = new GameObject("DurakDragLayer", typeof(RectTransform), typeof(Canvas));
            layerObject.layer = rootCanvas.gameObject.layer;
            layerObject.transform.SetParent(rootCanvas.transform, false);
            dragLayer = layerObject.GetComponent<RectTransform>();
            dragLayer.anchorMin = Vector2.zero;
            dragLayer.anchorMax = Vector2.one;
            dragLayer.offsetMin = Vector2.zero;
            dragLayer.offsetMax = Vector2.zero;
            dragLayer.SetAsLastSibling();

            Canvas layerCanvas = layerObject.GetComponent<Canvas>();
            layerCanvas.overrideSorting = true;
            layerCanvas.sortingOrder = 1000;
        }

        private sealed class LocalDurakGame
        {
            public int CardCount;
            public List<DurakCardDto> Deck = new List<DurakCardDto>();
            public DurakCardDto TrumpCard;
            public List<DurakCardDto> PlayerHand = new List<DurakCardDto>();
            public List<List<DurakCardDto>> BotHands = new List<List<DurakCardDto>>();
            public List<DurakTableCardDto> Table = new List<DurakTableCardDto>();
            public int AttackerIndex;
            public int DefenderIndex = 1;
            public bool FirstBout = true;
            public bool GameOver;
            public string Message = "";

            public bool CanPlayerAttack => !GameOver && AttackerIndex == 0 && CanStillAddCards;
            public bool CanPlayerDefend => !GameOver && DefenderIndex == 0 && Table.Any(x => x.defense == null);
            public bool CanPlayerTransfer => !GameOver && DefenderIndex == 0 && Table.Count > 0 && Table.All(x => x.defense == null);
            public bool CanPlayerPass => !GameOver && AttackerIndex == 0 && Table.Count > 0 && AllDefended;
            private bool AllDefended => Table.Count > 0 && Table.All(x => x.defense != null);
            private bool CanStillAddCards => Table.Count < (FirstBout ? 5 : 6);

            public static LocalDurakGame Create(int cardCount, int botCount)
            {
                LocalDurakGame game = new LocalDurakGame { CardCount = cardCount };
                game.Deck = CreateDeck(cardCount).OrderBy(_ => UnityEngine.Random.value).ToList();
                game.TrumpCard = game.Deck[game.Deck.Count - 1];

                for (int i = 0; i < botCount; i++)
                    game.BotHands.Add(new List<DurakCardDto>());

                game.DrawUpToSix(game.PlayerHand);

                for (int i = 0; i < game.BotHands.Count; i++)
                    game.DrawUpToSix(game.BotHands[i]);

                game.AttackerIndex = game.FindFirstAttacker();
                game.DefenderIndex = game.NextActive(game.AttackerIndex);
                game.Message = game.AttackerIndex == 0 ? "Ваш ход. Подкиньте карту." : "Бот ходит первым.";
                game.RunBotsIfNeeded();
                return game;
            }

            public void PlayerAttack(string code)
            {
                if (!CanPlayerAttack || string.IsNullOrWhiteSpace(code))
                    return;

                DurakCardDto card = RemoveCard(PlayerHand, code);

                if (card == null || !CanAddAttack(card))
                {
                    if (card != null)
                        PlayerHand.Add(card);

                    Message = "Эту карту нельзя подкинуть.";
                    return;
                }

                Table.Add(new DurakTableCardDto { attack = card });
                Message = "Карта на столе.";
                RunBotsIfNeeded();
            }

            public void PlayerDefend(string attackCode, string defenseCode)
            {
                if (!CanPlayerDefend)
                    return;

                DurakTableCardDto pair = Table.FirstOrDefault(x => x.attack != null && x.attack.code == attackCode && x.defense == null) ?? Table.FirstOrDefault(x => x.defense == null);
                DurakCardDto defense = RemoveCard(PlayerHand, defenseCode);

                if (pair == null || defense == null || !Beats(defense, pair.attack, TrumpCard.suit))
                {
                    if (defense != null)
                        PlayerHand.Add(defense);

                    Message = "Эта карта не бьет атаку.";
                    return;
                }

                pair.defense = defense;
                Message = "Отбито. Атакующий может подкинуть еще или нажать «Бито».";
                RunBotsIfNeeded();
            }

            public void PlayerTransfer(string code)
            {
                if (!CanPlayerTransfer)
                    return;

                DurakCardDto card = RemoveCard(PlayerHand, code);

                if (card == null || Table.All(x => x.attack.rank != card.rank))
                {
                    if (card != null)
                        PlayerHand.Add(card);

                    Message = "Переводить можно только той же величиной.";
                    return;
                }

                Table.Add(new DurakTableCardDto { attack = card });
                AttackerIndex = DefenderIndex;
                DefenderIndex = NextActive(DefenderIndex);
                Message = "Перевод.";
                RunBotsIfNeeded();
            }

            public void PlayerTake()
            {
                if (!CanPlayerDefend)
                    return;

                TakeTable(PlayerHand);
                EndBout(NextActive(DefenderIndex));

                if (!GameOver)
                    RunBotsIfNeeded();
            }

            public void PlayerPass()
            {
                if (!CanPlayerPass)
                    return;

                DiscardTable();
                EndBout(DefenderIndex);

                if (!GameOver)
                    RunBotsIfNeeded();
            }

            public List<DurakPlayerDto> ToPlayers()
            {
                List<DurakPlayerDto> players = new List<DurakPlayerDto>
                {
                    new DurakPlayerDto
                    {
                        id = 0,
                        displayName = "Вы",
                        handCount = PlayerHand.Count,
                        turnOrder = 1,
                        isAttacker = AttackerIndex == 0,
                        isDefender = DefenderIndex == 0,
                        isOut = PlayerHand.Count == 0 && Deck.Count == 0
                    }
                };

                for (int i = 0; i < BotHands.Count; i++)
                {
                    int index = i + 1;
                    players.Add(new DurakPlayerDto
                    {
                        id = index,
                        displayName = "Бот " + index,
                        handCount = BotHands[i].Count,
                        turnOrder = index + 1,
                        isBot = true,
                        isAttacker = AttackerIndex == index,
                        isDefender = DefenderIndex == index,
                        isOut = BotHands[i].Count == 0 && Deck.Count == 0
                    });
                }

                return players;
            }

            private void RunBotsIfNeeded()
            {
                if (GameOver)
                    return;

                for (int safety = 0; safety < 24; safety++)
                {
                    if (GameOver)
                        break;

                    if (AttackerIndex == 0 && (Table.Count == 0 || AllDefended))
                        break;

                    if (DefenderIndex == 0 && Table.Any(x => x.defense == null))
                        break;

                    if (AttackerIndex > 0 && (Table.Count == 0 || AllDefended))
                    {
                        DurakCardDto attack = BotHands[AttackerIndex - 1].FirstOrDefault(CanAddAttack);

                        if (attack == null)
                        {
                            if (AllDefended)
                            {
                                DiscardTable();
                                EndBout(DefenderIndex);

                                if (GameOver)
                                    break;
                            }

                            break;
                        }

                        BotHands[AttackerIndex - 1].Remove(attack);
                        Table.Add(new DurakTableCardDto { attack = attack });
                    }

                    if (DefenderIndex > 0 && Table.Any(x => x.defense == null))
                    {
                        List<DurakCardDto> defenderHand = BotHands[DefenderIndex - 1];
                        DurakTableCardDto pair = Table.First(x => x.defense == null);
                        DurakCardDto defense = defenderHand.Where(x => Beats(x, pair.attack, TrumpCard.suit)).OrderBy(x => x.value).FirstOrDefault();

                        if (defense == null)
                        {
                            TakeTable(defenderHand);
                            EndBout(NextActive(DefenderIndex));

                            if (GameOver)
                                break;

                            continue;
                        }

                        defenderHand.Remove(defense);
                        pair.defense = defense;
                    }

                    if (AllDefended)
                    {
                        if (AttackerIndex == 0)
                            break;

                        DiscardTable();
                        EndBout(DefenderIndex);
                    }
                }

                if (GameOver)
                    return;

                if (AttackerIndex == 0)
                    Message = "Ваш ход. Подкиньте карту.";
                else if (DefenderIndex == 0)
                    Message = "Вы защищаетесь.";
            }

            private void EndBout(int nextAttacker)
            {
                DrawUpToSix(PlayerHand);

                for (int i = 0; i < BotHands.Count; i++)
                    DrawUpToSix(BotHands[i]);

                AttackerIndex = IsActive(nextAttacker) ? nextAttacker : NextActive(nextAttacker);
                DefenderIndex = NextActive(AttackerIndex);
                FirstBout = false;
                CheckGameOver();
            }

            private bool CheckGameOver()
            {
                if (Deck.Count > 0)
                    return false;

                if (PlayerHand.Count == 0)
                {
                    GameOver = true;
                    Message = "Вы победили.";
                    return true;
                }

                if (BotHands.All(x => x.Count == 0))
                {
                    GameOver = true;
                    Message = "Вы остались дураком.";
                    return true;
                }

                return false;
            }

            private bool IsActive(int index)
            {
                if (index == 0)
                    return PlayerHand.Count > 0 || Deck.Count > 0;

                return index > 0 && index <= BotHands.Count && (BotHands[index - 1].Count > 0 || Deck.Count > 0);
            }

            private void DrawUpToSix(List<DurakCardDto> hand)
            {
                while (hand.Count < 6 && Deck.Count > 0)
                {
                    hand.Add(Deck[0]);
                    Deck.RemoveAt(0);
                }
            }

            private void TakeTable(List<DurakCardDto> hand)
            {
                for (int i = 0; i < Table.Count; i++)
                {
                    hand.Add(Table[i].attack);

                    if (Table[i].defense != null)
                        hand.Add(Table[i].defense);
                }

                Table.Clear();
            }

            private void DiscardTable()
            {
                Table.Clear();
            }

            private bool CanAddAttack(DurakCardDto card)
            {
                int limit = FirstBout ? 5 : 6;

                if (Table.Count >= limit)
                    return false;

                return Table.Count == 0 || Table.Any(x => x.attack.rank == card.rank || (x.defense != null && x.defense.rank == card.rank));
            }

            private int FindFirstAttacker()
            {
                List<List<DurakCardDto>> hands = new List<List<DurakCardDto>> { PlayerHand };
                hands.AddRange(BotHands);
                int result = 0;
                int best = int.MaxValue;

                for (int i = 0; i < hands.Count; i++)
                {
                    DurakCardDto trump = hands[i].Where(x => x.suit == TrumpCard.suit).OrderBy(x => x.value).FirstOrDefault();

                    if (trump != null && trump.value < best)
                    {
                        best = trump.value;
                        result = i;
                    }
                }

                return result;
            }

            private int NextActive(int from)
            {
                int count = BotHands.Count + 1;

                for (int offset = 1; offset <= count; offset++)
                {
                    int index = (from + offset) % count;
                    List<DurakCardDto> hand = index == 0 ? PlayerHand : BotHands[index - 1];

                    if (hand.Count > 0 || Deck.Count > 0)
                        return index;
                }

                return from;
            }

            private static DurakCardDto RemoveCard(List<DurakCardDto> hand, string code)
            {
                DurakCardDto card = hand.FirstOrDefault(x => x.code == code);

                if (card != null)
                    hand.Remove(card);

                return card;
            }

            private static bool Beats(DurakCardDto defense, DurakCardDto attack, string trumpSuit)
            {
                if (defense.suit == attack.suit && defense.value > attack.value)
                    return true;

                return defense.suit == trumpSuit && attack.suit != trumpSuit;
            }

            private static List<DurakCardDto> CreateDeck(int cardCount)
            {
                string[] suits = { "Clubs", "Diamonds", "Hearts", "Spades" };
                string[] ranks = cardCount == 24
                    ? new[] { "9", "10", "J", "Q", "K", "A" }
                    : cardCount == 36
                        ? new[] { "6", "7", "8", "9", "10", "J", "Q", "K", "A" }
                        : new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

                List<DurakCardDto> deck = new List<DurakCardDto>();

                for (int s = 0; s < suits.Length; s++)
                {
                    for (int r = 0; r < ranks.Length; r++)
                    {
                        deck.Add(new DurakCardDto
                        {
                            code = ranks[r] + suits[s][0],
                            rank = ranks[r],
                            suit = suits[s],
                            value = r + 2
                        });
                    }
                }

                return deck;
            }
        }
    }
}
