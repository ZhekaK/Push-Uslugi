using PushPelmesh.Durak;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.Durak.EditorTools
{
    public static class DurakUiGenerator
    {
        private const string ScenePath = "Assets/_Durak/Scenes/DurakScene.unity";
        private const string CanvasName = "DurakCanvas";
        private const string ControllerName = "DurakController";

        [MenuItem("Tools/Push Uslugi/Durak Module/Generate DurakScene UI")]
        public static void GenerateDurakSceneUi()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            if (sceneAsset != null)
                EditorSceneManager.OpenScene(ScenePath);
            else
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GenerateUiInCurrentScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        }

        [MenuItem("Tools/Push Uslugi/Durak Module/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);

            if (existingCanvas != null)
            {
                bool replace = Application.isBatchMode || EditorUtility.DisplayDialog("Durak UI", "В сцене уже есть DurakCanvas. Заменить?", "Заменить", "Отмена");

                if (!replace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            DurakScreen screen = FindOrCreateScreen();
            Font font = GetDefaultFont();
            Canvas canvas = CreateCanvas();
            RectTransform root = CreateRoot(canvas.transform);

            RectTransform header = CreateHorizontal(root, "Header", 82f, 12f);
            Button backButton = CreateButton(header, "В меню", font, new Color(0.34f, 0.4f, 0.48f), 150f);
            Text title = CreateText(header, "Дурак", font, 42, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), TextAnchor.MiddleCenter);
            AddLayout(title.gameObject, 0f, 68f, 1f);

            GameObject modePanel = CreatePanel(root, "ModePanel");
            CreateTextBlock(modePanel.transform as RectTransform, "Выберите режим", font, 34, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 72f);
            Button botModeButton = CreateButton(modePanel.transform as RectTransform, "Играть с ботами", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button multiplayerModeButton = CreateButton(modePanel.transform as RectTransform, "Мультиплеер", font, new Color(0.12f, 0.58f, 0.32f), 0f);

            GameObject botSetupPanel = CreatePanel(root, "BotSetupPanel");
            CreateTextBlock(botSetupPanel.transform as RectTransform, "Игра с ботами", font, 32, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 66f);
            InputField botCardCountInput = CreateInput(botSetupPanel.transform as RectTransform, "Карт в колоде", "24 / 36 / 52", font, 64f);
            InputField botCountInput = CreateInput(botSetupPanel.transform as RectTransform, "Количество ботов", "1-5", font, 64f);
            Text botSetupStatusText = CreateTextBlock(botSetupPanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 52f);
            RectTransform botSetupButtons = CreateHorizontal(botSetupPanel.transform as RectTransform, "BotSetupButtons", 76f, 12f);
            Button startBotGameButton = CreateButton(botSetupButtons, "Начать", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Button botSetupBackButton = CreateButton(botSetupButtons, "Назад", font, new Color(0.42f, 0.46f, 0.52f), 0f);

            GameObject roomsPanel = CreatePanel(root, "RoomsPanel");
            RectTransform roomsHeader = CreateHorizontal(roomsPanel.transform as RectTransform, "RoomsHeader", 76f, 12f);
            Button refreshRoomsButton = CreateButton(roomsHeader, "Обновить", font, new Color(0.12f, 0.42f, 0.74f), 160f);
            InputField joinPasswordInput = CreateInput(roomsHeader, "Пароль", "пароль комнаты", font, 0f);
            Button openCreateRoomButton = CreateButton(roomsHeader, "+", font, new Color(0.12f, 0.58f, 0.32f), 70f);
            Button roomsBackButton = CreateButton(roomsHeader, "Назад", font, new Color(0.42f, 0.46f, 0.52f), 130f);
            CreateScroll(roomsPanel.transform as RectTransform, "RoomsScroll", out RectTransform roomsRoot);
            Text roomsStatusText = CreateTextBlock(roomsPanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 44f);

            GameObject createRoomPanel = CreatePanel(root, "CreateRoomPanel");
            CreateTextBlock(createRoomPanel.transform as RectTransform, "Создать комнату", font, 32, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 66f);
            InputField roomNameInput = CreateInput(createRoomPanel.transform as RectTransform, "Имя комнаты", "например: вечерняя партия", font, 64f);
            InputField roomPasswordInput = CreateInput(createRoomPanel.transform as RectTransform, "Пароль", "можно оставить пустым", font, 64f);
            InputField maxPlayersInput = CreateInput(createRoomPanel.transform as RectTransform, "Игроков", "2-6", font, 64f);
            InputField roomCardCountInput = CreateInput(createRoomPanel.transform as RectTransform, "Карт в колоде", "24 / 36 / 52", font, 64f);
            Text createRoomStatusText = CreateTextBlock(createRoomPanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.62f, 0.18f, 0.14f), 48f);
            RectTransform createButtons = CreateHorizontal(createRoomPanel.transform as RectTransform, "CreateButtons", 76f, 12f);
            Button submitCreateRoomButton = CreateButton(createButtons, "Создать", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Button cancelCreateRoomButton = CreateButton(createButtons, "Отмена", font, new Color(0.42f, 0.46f, 0.52f), 0f);

            GameObject gamePanel = CreatePanel(root, "GamePanel");
            VerticalLayoutGroup gameLayout = gamePanel.GetComponent<VerticalLayoutGroup>();
            gameLayout.padding = new RectOffset(16, 16, 12, 12);
            gameLayout.spacing = 8f;
            RectTransform gameHeader = CreateHorizontal(gamePanel.transform as RectTransform, "GameHeader", 70f, 12f);
            Text gameTitleText = CreateText(gameHeader, "Комната", font, 30, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(gameTitleText.gameObject, 0f, 58f, 1f);
            Button refreshGameButton = CreateButton(gameHeader, "Обновить", font, new Color(0.12f, 0.42f, 0.74f), 150f);
            Button leaveGameButton = CreateButton(gameHeader, "Назад", font, new Color(0.42f, 0.46f, 0.52f), 130f);
            Text gameMetaText = CreateTextBlock(gamePanel.transform as RectTransform, string.Empty, font, 21, FontStyle.Normal, new Color(0.16f, 0.18f, 0.22f), 44f);

            CreateScroll(gamePanel.transform as RectTransform, "PlayersScroll", out RectTransform playersRoot, 80f, 0f);

            RectTransform tableBand = CreateHorizontal(gamePanel.transform as RectTransform, "TableBand", 310f, 18f);
            RectTransform deckArea = CreateVertical(tableBand, "DeckArea", 310f, 6f);
            AddLayout(deckArea.gameObject, 180f, 310f, 0f);
            CreateTextBlock(deckArea, "Колода", font, 19, FontStyle.Bold, new Color(0.16f, 0.18f, 0.22f), 32f);
            RectTransform deckCards = CreateHorizontal(deckArea, "DeckCards", 120f, 8f);
            Image deckBackImage = CreateDeckImage(deckCards, "DeckBack", 72f, 105f);
            Image trumpCardImage = CreateDeckImage(deckCards, "TrumpCard", 72f, 105f);
            Text deckCountText = CreateTextBlock(deckArea, "0", font, 22, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 30f);
            deckCountText.alignment = TextAnchor.MiddleCenter;
            Text trumpSuitText = CreateTextBlock(deckArea, string.Empty, font, 38, FontStyle.Bold, new Color(0.72f, 0.18f, 0.2f), 48f);
            trumpSuitText.alignment = TextAnchor.MiddleCenter;

            RectTransform tableRoot = CreateTableGrid(tableBand, "TableRoot");
            AddLayout(tableRoot.gameObject, 0f, 310f, 1f);

            CreateScroll(gamePanel.transform as RectTransform, "HandScroll", out RectTransform handRoot, 170f, 0f);
            HorizontalLayoutGroup handLayout = handRoot.GetComponent<HorizontalLayoutGroup>();

            if (handLayout != null)
                handLayout.spacing = -28f;

            RectTransform gameButtons = CreateHorizontal(gamePanel.transform as RectTransform, "GameButtons", 64f, 10f);
            Button startRoomButton = CreateButton(gameButtons, "Старт", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Button attackButton = CreateButton(gameButtons, "Атака", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button defendButton = CreateButton(gameButtons, "Бить", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button transferButton = CreateButton(gameButtons, "Перевести", font, new Color(0.53f, 0.36f, 0.76f), 0f);
            Button takeButton = CreateButton(gameButtons, "Взять", font, new Color(0.72f, 0.28f, 0.2f), 0f);
            Button passButton = CreateButton(gameButtons, "Бито", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Text myTurnText = CreateTextBlock(gamePanel.transform as RectTransform, "ВАШ ХОД", font, 26, FontStyle.Bold, new Color(0.08f, 0.68f, 0.27f), 42f);
            myTurnText.alignment = TextAnchor.MiddleCenter;
            myTurnText.gameObject.SetActive(false);
            Text gameStatusText = CreateTextBlock(gamePanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 44f);

            GameObject roomRowPrefab = CreateRoomRowTemplate(canvas.transform, font);
            GameObject playerRowPrefab = CreatePlayerRowTemplate(canvas.transform, font);
            GameObject cardPrefab = CreateCardTemplate(canvas.transform, font);

            EnsureEventSystem();
            AssignReferences(
                screen,
                backButton,
                modePanel,
                botModeButton,
                multiplayerModeButton,
                botSetupPanel,
                botCardCountInput,
                botCountInput,
                startBotGameButton,
                botSetupBackButton,
                botSetupStatusText,
                roomsPanel,
                roomsRoot,
                roomRowPrefab,
                refreshRoomsButton,
                openCreateRoomButton,
                roomsBackButton,
                joinPasswordInput,
                roomsStatusText,
                createRoomPanel,
                roomNameInput,
                roomPasswordInput,
                maxPlayersInput,
                roomCardCountInput,
                submitCreateRoomButton,
                cancelCreateRoomButton,
                createRoomStatusText,
                gamePanel,
                gameTitleText,
                gameMetaText,
                gameStatusText,
                myTurnText,
                playersRoot,
                playerRowPrefab,
                tableRoot,
                handRoot,
                cardPrefab,
                deckBackImage,
                deckCountText,
                trumpCardImage,
                trumpSuitText,
                startRoomButton,
                attackButton,
                defendButton,
                transferButton,
                takeButton,
                passButton,
                refreshGameButton,
                leaveGameButton);

            botSetupPanel.SetActive(false);
            roomsPanel.SetActive(false);
            createRoomPanel.SetActive(false);
            gamePanel.SetActive(false);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static DurakScreen FindOrCreateScreen()
        {
            DurakScreen screen = Object.FindFirstObjectByType<DurakScreen>();

            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            Undo.RegisterCreatedObjectUndo(controller, "Create Durak Controller");
            return controller.AddComponent<DurakScreen>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go, "Create Durak Canvas");
            SetUiLayer(go);

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = go.AddComponent<Image>();
            background.color = new Color(0.91f, 0.94f, 0.92f);
            return canvas;
        }

        private static RectTransform CreateRoot(Transform parent)
        {
            RectTransform root = CreateRect("Content", parent);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(36f, 36f);
            root.offsetMax = new Vector2(-36f, -36f);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return root;
        }

        private static GameObject CreatePanel(RectTransform parent, string name)
        {
            RectTransform panel = CreateRect(name, parent);
            Image image = panel.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.99f, 0.97f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 22);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLayout(panel.gameObject, 0f, 0f, 1f, 1f);
            return panel.gameObject;
        }

        private static RectTransform CreateHorizontal(RectTransform parent, string name, float height, float spacing)
        {
            RectTransform rect = CreateRect(name, parent);
            HorizontalLayoutGroup layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            AddLayout(rect.gameObject, 0f, height, 1f);
            return rect;
        }

        private static RectTransform CreateVertical(RectTransform parent, string name, float height, float spacing)
        {
            RectTransform rect = CreateRect(name, parent);
            VerticalLayoutGroup layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            AddLayout(rect.gameObject, 0f, height, 1f);
            return rect;
        }

        private static RectTransform CreateScroll(RectTransform parent, string name, out RectTransform content, float preferredHeight = 0f, float flexibleHeight = 1f)
        {
            RectTransform scroll = CreateRect(name, parent);
            scroll.gameObject.AddComponent<Image>().color = Color.white;
            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            AddLayout(scroll.gameObject, 0f, preferredHeight, 1f, flexibleHeight);

            RectTransform viewport = CreateRect("Viewport", scroll);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<Image>().color = Color.white;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);

            HorizontalLayoutGroup horizontal = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(10, 10, 10, 10);
            horizontal.spacing = 8f;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            return scroll;
        }

        private static RectTransform CreateTableGrid(RectTransform parent, string name)
        {
            RectTransform table = CreateRect(name, parent);
            Image background = table.gameObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.04f);
            background.raycastTarget = true;

            GridLayoutGroup grid = table.gameObject.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(12, 12, 10, 10);
            grid.cellSize = new Vector2(160f, 142f);
            grid.spacing = new Vector2(12f, 8f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            return table;
        }

        private static Image CreateDeckImage(RectTransform parent, string name, float width, float height)
        {
            RectTransform rect = CreateRect(name, parent);
            AddLayout(rect.gameObject, width, height, 0f);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.32f, 0.46f);
            image.preserveAspect = true;
            return image;
        }

        private static GameObject CreateRoomRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("DurakRoomRowTemplate", parent);
            row.gameObject.SetActive(false);
            Image image = row.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.99f, 1f);
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            AddLayout(row.gameObject, 0f, 86f, 1f);

            Text title = CreateText(row, "Комната", font, 24, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(title.gameObject, 0f, 62f, 1f);
            Text meta = CreateText(row, "2/6", font, 19, FontStyle.Normal, new Color(0.34f, 0.4f, 0.48f), TextAnchor.MiddleRight);
            AddLayout(meta.gameObject, 460f, 62f, 0f);

            DurakRoomRowView view = row.gameObject.AddComponent<DurakRoomRowView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "titleText", title);
            SetReference(so, "metaText", meta);
            SetReference(so, "button", button);
            so.ApplyModifiedProperties();
            return row.gameObject;
        }

        private static GameObject CreatePlayerRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("DurakPlayerRowTemplate", parent);
            row.gameObject.SetActive(false);
            Image background = row.gameObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0f);
            background.raycastTarget = false;
            Outline turnOutline = row.gameObject.AddComponent<Outline>();
            turnOutline.effectColor = new Color(0.12f, 0.72f, 0.3f, 1f);
            turnOutline.effectDistance = new Vector2(3f, -3f);
            turnOutline.useGraphicAlpha = false;
            turnOutline.enabled = false;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 6, 6);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            AddLayout(row.gameObject, 270f, 72f, 0f);

            Text name = CreateText(row, "Игрок", font, 21, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(name.gameObject, 86f, 56f, 0f);
            RectTransform cardsRoot = CreateRect("OpponentCards", row);
            AddLayout(cardsRoot.gameObject, 156f, 56f, 0f);

            RectTransform cardBackRect = CreateRect("CardBackTemplate", cardsRoot);
            cardBackRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardBackRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardBackRect.pivot = new Vector2(0.5f, 0.5f);
            cardBackRect.sizeDelta = new Vector2(34f, 48f);
            Image cardBackTemplate = cardBackRect.gameObject.AddComponent<Image>();
            cardBackTemplate.color = new Color(0.2f, 0.32f, 0.46f);
            cardBackTemplate.preserveAspect = true;
            cardBackTemplate.raycastTarget = false;
            cardBackTemplate.gameObject.SetActive(false);

            DurakPlayerRowView view = row.gameObject.AddComponent<DurakPlayerRowView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "nameText", name);
            SetReference(so, "cardsRoot", cardsRoot);
            SetReference(so, "cardBackTemplate", cardBackTemplate);
            SetReference(so, "turnOutline", turnOutline);
            so.ApplyModifiedProperties();
            return row.gameObject;
        }

        private static GameObject CreateCardTemplate(Transform parent, Font font)
        {
            RectTransform card = CreateRect("DurakCardTemplate", parent);
            card.gameObject.SetActive(false);
            Image image = card.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.98f, 0.95f);
            card.gameObject.AddComponent<CanvasGroup>();
            Button button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            AddLayout(card.gameObject, 100f, 142f, 0f);

            Text label = CreateText(card, "A♠", font, 28, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            DurakCardView view = card.gameObject.AddComponent<DurakCardView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "image", image);
            SetReference(so, "labelText", label);
            SetReference(so, "button", button);
            so.ApplyModifiedProperties();
            return card.gameObject;
        }

        private static InputField CreateInput(RectTransform parent, string name, string placeholder, Font font, float preferredHeight)
        {
            RectTransform rect = CreateRect(name + " Input", parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            InputField input = rect.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.lineType = InputField.LineType.SingleLine;
            AddLayout(rect.gameObject, 0f, preferredHeight <= 0f ? 64f : preferredHeight, 1f);

            Text text = CreateText(rect, "Text", font, 21, FontStyle.Normal, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(14f, 0f);
            text.rectTransform.offsetMax = new Vector2(-14f, 0f);
            text.text = string.Empty;

            Text placeholderText = CreateText(rect, "Placeholder", font, 21, FontStyle.Italic, new Color(0.52f, 0.56f, 0.62f), TextAnchor.MiddleLeft);
            placeholderText.rectTransform.anchorMin = Vector2.zero;
            placeholderText.rectTransform.anchorMax = Vector2.one;
            placeholderText.rectTransform.offsetMin = new Vector2(14f, 0f);
            placeholderText.rectTransform.offsetMax = new Vector2(-14f, 0f);
            placeholderText.text = placeholder;

            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private static Button CreateButton(RectTransform parent, string caption, Font font, Color color, float preferredWidth)
        {
            RectTransform rect = CreateRect(caption + " Button", parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            AddLayout(rect.gameObject, preferredWidth, 64f, preferredWidth <= 0f ? 1f : 0f);

            Text text = CreateText(rect, caption, font, 21, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static Text CreateTextBlock(RectTransform parent, string value, Font font, int size, FontStyle style, Color color, float height)
        {
            Text text = CreateText(parent, value, font, size, style, color, TextAnchor.MiddleLeft);
            AddLayout(text.gameObject, 0f, height, 1f);
            return text;
        }

        private static Text CreateText(RectTransform parent, string value, Font font, int size, FontStyle style, Color color, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(value + " Text", parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetUiLayer(go);
            return go.GetComponent<RectTransform>();
        }

        private static void AddLayout(GameObject go, float preferredWidth, float preferredHeight, float flexibleWidth, float flexibleHeight = 0f)
        {
            LayoutElement element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            element.preferredWidth = preferredWidth;
            element.preferredHeight = preferredHeight;
            element.flexibleWidth = flexibleWidth;
            element.flexibleHeight = flexibleHeight;
        }

        private static void AssignReferences(
            DurakScreen screen,
            Button backButton,
            GameObject modePanel,
            Button botModeButton,
            Button multiplayerModeButton,
            GameObject botSetupPanel,
            InputField botCardCountInput,
            InputField botCountInput,
            Button startBotGameButton,
            Button botSetupBackButton,
            Text botSetupStatusText,
            GameObject roomsPanel,
            RectTransform roomsRoot,
            GameObject roomRowPrefab,
            Button refreshRoomsButton,
            Button openCreateRoomButton,
            Button roomsBackButton,
            InputField joinPasswordInput,
            Text roomsStatusText,
            GameObject createRoomPanel,
            InputField roomNameInput,
            InputField roomPasswordInput,
            InputField maxPlayersInput,
            InputField roomCardCountInput,
            Button submitCreateRoomButton,
            Button cancelCreateRoomButton,
            Text createRoomStatusText,
            GameObject gamePanel,
            Text gameTitleText,
            Text gameMetaText,
            Text gameStatusText,
            Text myTurnText,
            RectTransform playersRoot,
            GameObject playerRowPrefab,
            RectTransform tableRoot,
            RectTransform handRoot,
            GameObject cardPrefab,
            Image deckBackImage,
            Text deckCountText,
            Image trumpCardImage,
            Text trumpSuitText,
            Button startRoomButton,
            Button attackButton,
            Button defendButton,
            Button transferButton,
            Button takeButton,
            Button passButton,
            Button refreshGameButton,
            Button leaveGameButton)
        {
            SerializedObject so = new SerializedObject(screen);
            SetReference(so, "backButton", backButton);
            SetString(so, "mainMenuSceneName", "MainMenuScene");
            SetReference(so, "modePanel", modePanel);
            SetReference(so, "botModeButton", botModeButton);
            SetReference(so, "multiplayerModeButton", multiplayerModeButton);
            SetReference(so, "botSetupPanel", botSetupPanel);
            SetReference(so, "botCardCountInput", botCardCountInput);
            SetReference(so, "botCountInput", botCountInput);
            SetReference(so, "startBotGameButton", startBotGameButton);
            SetReference(so, "botSetupBackButton", botSetupBackButton);
            SetReference(so, "botSetupStatusText", botSetupStatusText);
            SetReference(so, "roomsPanel", roomsPanel);
            SetReference(so, "roomsRoot", roomsRoot);
            SetReference(so, "roomRowPrefab", roomRowPrefab);
            SetReference(so, "refreshRoomsButton", refreshRoomsButton);
            SetReference(so, "openCreateRoomButton", openCreateRoomButton);
            SetReference(so, "roomsBackButton", roomsBackButton);
            SetReference(so, "joinPasswordInput", joinPasswordInput);
            SetReference(so, "roomsStatusText", roomsStatusText);
            SetReference(so, "createRoomPanel", createRoomPanel);
            SetReference(so, "roomNameInput", roomNameInput);
            SetReference(so, "roomPasswordInput", roomPasswordInput);
            SetReference(so, "maxPlayersInput", maxPlayersInput);
            SetReference(so, "roomCardCountInput", roomCardCountInput);
            SetReference(so, "submitCreateRoomButton", submitCreateRoomButton);
            SetReference(so, "cancelCreateRoomButton", cancelCreateRoomButton);
            SetReference(so, "createRoomStatusText", createRoomStatusText);
            SetReference(so, "gamePanel", gamePanel);
            SetReference(so, "gameTitleText", gameTitleText);
            SetReference(so, "gameMetaText", gameMetaText);
            SetReference(so, "gameStatusText", gameStatusText);
            SetReference(so, "myTurnText", myTurnText);
            SetReference(so, "playersRoot", playersRoot);
            SetReference(so, "playerRowPrefab", playerRowPrefab);
            SetReference(so, "tableRoot", tableRoot);
            SetReference(so, "handRoot", handRoot);
            SetReference(so, "cardPrefab", cardPrefab);
            SetReference(so, "deckBackImage", deckBackImage);
            SetReference(so, "deckCountText", deckCountText);
            SetReference(so, "trumpCardImage", trumpCardImage);
            SetReference(so, "trumpSuitText", trumpSuitText);
            SetReference(so, "startRoomButton", startRoomButton);
            SetReference(so, "attackButton", attackButton);
            SetReference(so, "defendButton", defendButton);
            SetReference(so, "transferButton", transferButton);
            SetReference(so, "takeButton", takeButton);
            SetReference(so, "passButton", passButton);
            SetReference(so, "refreshGameButton", refreshGameButton);
            SetReference(so, "leaveGameButton", leaveGameButton);
            so.ApplyModifiedProperties();
        }

        private static void SetReference(SerializedObject target, string propertyName, Object value)
        {
            SerializedProperty property = target.FindProperty(propertyName);

            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void SetString(SerializedObject target, string propertyName, string value)
        {
            SerializedProperty property = target.FindProperty(propertyName);

            if (property != null)
                property.stringValue = value;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void SetUiLayer(GameObject go)
        {
            int layer = LayerMask.NameToLayer("UI");

            if (layer >= 0)
                go.layer = layer;
        }
    }
}
