using PushPelmesh.Zong;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.Zong.EditorTools
{
    public static class ZongUiGenerator
    {
        private const string ScenePath = "Assets/_Zong/Scenes/ZongScene.unity";
        private const string CanvasName = "ZongCanvas";
        private const string ControllerName = "ZongController";

        [MenuItem("Tools/Push Uslugi/Zong Module/Generate ZongScene UI")]
        public static void GenerateZongSceneUi()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            if (sceneAsset != null)
                EditorSceneManager.OpenScene(ScenePath);
            else
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GenerateUiInCurrentScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        }

        [MenuItem("Tools/Push Uslugi/Zong Module/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);

            if (existingCanvas != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Zong UI",
                    "В текущей сцене уже есть ZongCanvas. Заменить его новым интерфейсом?",
                    "Заменить",
                    "Отмена");

                if (!replace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            ZongScreen screen = FindOrCreateScreen();
            ZongDiceAnimator diceAnimator = FindOrCreateDiceAnimator(screen);
            Sprite[] diceSprites = LoadDiceSprites();
            Font font = GetDefaultFont();
            Canvas canvas = CreateCanvas();
            RectTransform root = CreateRoot(canvas.transform);

            RectTransform header = CreateHorizontal(root, "Header", 86f, 14f);
            Button backButton = CreateButton(header, "В меню", font, new Color(0.34f, 0.4f, 0.48f), 160f);
            Text title = CreateText(header, "Зонг", font, 42, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), TextAnchor.MiddleCenter);
            AddLayout(title.gameObject, 0f, 70f, 1f);

            GameObject modePanel = CreatePanel(root, "ModePanel");
            CreateTextBlock(modePanel.transform as RectTransform, "Выберите режим", font, 34, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 74f);
            Button botModeButton = CreateButton(modePanel.transform as RectTransform, "Играть с ботом", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button multiplayerModeButton = CreateButton(modePanel.transform as RectTransform, "Мультиплеер", font, new Color(0.12f, 0.58f, 0.32f), 0f);

            GameObject botPanel = CreatePanel(root, "BotPanel");
            Text botScoreText = CreateTextBlock(botPanel.transform as RectTransform, string.Empty, font, 28, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 58f);
            Text botDiceText = CreateTextBlock(botPanel.transform as RectTransform, string.Empty, font, 24, FontStyle.Normal, new Color(0.16f, 0.18f, 0.22f), 58f);
            Image[] botDiceImages = CreateDiceStrip(botPanel.transform as RectTransform, "BotDice", diceSprites, 112f);
            Text botStatusText = CreateTextBlock(botPanel.transform as RectTransform, string.Empty, font, 22, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 88f);
            RectTransform botButtons = CreateHorizontal(botPanel.transform as RectTransform, "BotButtons", 76f, 12f);
            Button botRollButton = CreateButton(botButtons, "Бросить", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button botBankButton = CreateButton(botButtons, "Забрать", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Button botBackButton = CreateButton(botButtons, "Назад", font, new Color(0.42f, 0.46f, 0.52f), 0f);

            GameObject roomsPanel = CreatePanel(root, "RoomsPanel");
            RectTransform roomsHeader = CreateHorizontal(roomsPanel.transform as RectTransform, "RoomsHeader", 76f, 12f);
            Button refreshRoomsButton = CreateButton(roomsHeader, "Обновить", font, new Color(0.12f, 0.42f, 0.74f), 170f);
            InputField joinPasswordInput = CreateInput(roomsHeader, "Пароль комнаты", "если нужен", font, 0f);
            Button openCreateRoomButton = CreateButton(roomsHeader, "+", font, new Color(0.12f, 0.58f, 0.32f), 72f);
            Button roomsBackButton = CreateButton(roomsHeader, "Назад", font, new Color(0.42f, 0.46f, 0.52f), 130f);
            CreateScroll(roomsPanel.transform as RectTransform, "RoomsScroll", out RectTransform roomsRoot);
            Text roomsStatusText = CreateTextBlock(roomsPanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 44f);

            GameObject createRoomPanel = CreatePanel(root, "CreateRoomPanel");
            CreateTextBlock(createRoomPanel.transform as RectTransform, "Создать комнату", font, 32, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 68f);
            InputField roomNameInput = CreateInput(createRoomPanel.transform as RectTransform, "Имя комнаты", "Например: вечерняя партия", font, 64f);
            InputField roomPasswordInput = CreateInput(createRoomPanel.transform as RectTransform, "Пароль", "можно оставить пустым", font, 64f);
            InputField maxPlayersInput = CreateInput(createRoomPanel.transform as RectTransform, "Игроков", "2-8", font, 64f);
            InputField targetScoreInput = CreateInput(createRoomPanel.transform as RectTransform, "Цель очков", "5000 / 10000 / 15000", font, 64f);
            Text createRoomStatusText = CreateTextBlock(createRoomPanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.62f, 0.18f, 0.14f), 44f);
            RectTransform createButtons = CreateHorizontal(createRoomPanel.transform as RectTransform, "CreateRoomButtons", 76f, 12f);
            Button submitCreateRoomButton = CreateButton(createButtons, "Создать", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Button cancelCreateRoomButton = CreateButton(createButtons, "Отмена", font, new Color(0.42f, 0.46f, 0.52f), 0f);

            GameObject roomPanel = CreatePanel(root, "RoomPanel");
            RectTransform roomHeader = CreateHorizontal(roomPanel.transform as RectTransform, "RoomHeader", 74f, 12f);
            Text roomTitleText = CreateText(roomHeader, "Комната", font, 30, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(roomTitleText.gameObject, 0f, 64f, 1f);
            Button refreshRoomButton = CreateButton(roomHeader, "Обновить", font, new Color(0.12f, 0.42f, 0.74f), 160f);
            Button leaveRoomButton = CreateButton(roomHeader, "К комнатам", font, new Color(0.42f, 0.46f, 0.52f), 180f);
            Text roomMetaText = CreateTextBlock(roomPanel.transform as RectTransform, string.Empty, font, 22, FontStyle.Normal, new Color(0.16f, 0.18f, 0.22f), 44f);
            Text roomDiceText = CreateTextBlock(roomPanel.transform as RectTransform, string.Empty, font, 24, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 52f);
            Image[] roomDiceImages = CreateDiceStrip(roomPanel.transform as RectTransform, "RoomDice", diceSprites, 104f);
            Text roomTurnScoreText = CreateTextBlock(roomPanel.transform as RectTransform, string.Empty, font, 22, FontStyle.Normal, new Color(0.16f, 0.18f, 0.22f), 48f);
            CreateScroll(roomPanel.transform as RectTransform, "PlayersScroll", out RectTransform playersRoot, 250f, 0f);
            RectTransform roomButtons = CreateHorizontal(roomPanel.transform as RectTransform, "RoomButtons", 76f, 12f);
            Button startRoomButton = CreateButton(roomButtons, "Старт", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Button rollRoomButton = CreateButton(roomButtons, "Бросить", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button bankRoomButton = CreateButton(roomButtons, "Забрать", font, new Color(0.12f, 0.58f, 0.32f), 0f);
            Text roomStatusText = CreateTextBlock(roomPanel.transform as RectTransform, string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 58f);

            GameObject roomRowPrefab = CreateRoomRowTemplate(canvas.transform, font);
            GameObject playerRowPrefab = CreatePlayerRowTemplate(canvas.transform, font);

            EnsureEventSystem();
            AssignReferences(
                screen,
                backButton,
                modePanel,
                botModeButton,
                multiplayerModeButton,
                botPanel,
                botScoreText,
                botDiceText,
                botStatusText,
                botDiceImages,
                botRollButton,
                botBankButton,
                botBackButton,
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
                targetScoreInput,
                submitCreateRoomButton,
                cancelCreateRoomButton,
                createRoomStatusText,
                roomPanel,
                roomTitleText,
                roomMetaText,
                roomDiceText,
                roomDiceImages,
                roomTurnScoreText,
                roomStatusText,
                playersRoot,
                playerRowPrefab,
                startRoomButton,
                rollRoomButton,
                bankRoomButton,
                refreshRoomButton,
                leaveRoomButton,
                diceAnimator);

            AssignDiceSprites(diceAnimator, diceSprites);

            botPanel.SetActive(false);
            roomsPanel.SetActive(false);
            createRoomPanel.SetActive(false);
            roomPanel.SetActive(false);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static ZongScreen FindOrCreateScreen()
        {
            ZongScreen screen = Object.FindFirstObjectByType<ZongScreen>();

            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            Undo.RegisterCreatedObjectUndo(controller, "Create Zong Controller");
            return controller.AddComponent<ZongScreen>();
        }

        private static ZongDiceAnimator FindOrCreateDiceAnimator(ZongScreen screen)
        {
            ZongDiceAnimator animator = screen.GetComponent<ZongDiceAnimator>();

            if (animator == null)
                animator = screen.gameObject.AddComponent<ZongDiceAnimator>();

            return animator;
        }

        private static Sprite[] LoadDiceSprites()
        {
            Sprite[] sprites = new Sprite[6];

            for (int i = 0; i < sprites.Length; i++)
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Zong/Dice {i + 1}.png");

            return sprites;
        }

        private static void AssignDiceSprites(ZongDiceAnimator animator, Sprite[] sprites)
        {
            SerializedObject so = new SerializedObject(animator);
            SerializedProperty property = so.FindProperty("diceSprites");

            if (property != null)
            {
                property.arraySize = sprites.Length;

                for (int i = 0; i < sprites.Length; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(animator);
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go, "Create Zong Canvas");
            SetUiLayer(go);

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = go.AddComponent<Image>();
            background.color = new Color(0.92f, 0.94f, 0.96f);

            return canvas;
        }

        private static RectTransform CreateRoot(Transform parent)
        {
            RectTransform root = CreateRect("Content", parent);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(40f, 40f);
            root.offsetMax = new Vector2(-40f, -40f);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
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
            image.color = new Color(0.97f, 0.98f, 1f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(26, 26, 24, 24);
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

        private static RectTransform CreateScroll(RectTransform parent, string name, out RectTransform content, float preferredHeight = 0f, float flexibleHeight = 1f)
        {
            RectTransform scroll = CreateRect(name, parent);
            Image background = scroll.gameObject.AddComponent<Image>();
            background.color = Color.white;
            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
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

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            return scroll;
        }

        private static Image[] CreateDiceStrip(RectTransform parent, string name, Sprite[] diceSprites, float height)
        {
            RectTransform strip = CreateHorizontal(parent, name, height, 10f);
            Image[] images = new Image[6];

            for (int i = 0; i < images.Length; i++)
            {
                RectTransform diceRect = CreateRect(name + " " + (i + 1), strip);
                AddLayout(diceRect.gameObject, height - 16f, height - 16f, 0f);

                Image image = diceRect.gameObject.AddComponent<Image>();
                image.color = Color.white;
                image.preserveAspect = true;
                image.sprite = diceSprites != null && i < diceSprites.Length ? diceSprites[i] : null;
                image.gameObject.SetActive(false);
                images[i] = image;
            }

            return images;
        }

        private static GameObject CreateRoomRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("ZongRoomRowTemplate", parent);
            row.gameObject.SetActive(false);
            Image image = row.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.99f, 1f);
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            AddLayout(row.gameObject, 0f, 86f, 1f);

            Text title = CreateText(row, "Комната", font, 24, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(title.gameObject, 0f, 62f, 1f);
            Text meta = CreateText(row, "2/8", font, 19, FontStyle.Normal, new Color(0.34f, 0.4f, 0.48f), TextAnchor.MiddleRight);
            AddLayout(meta.gameObject, 430f, 62f, 0f);

            ZongRoomRowView view = row.gameObject.AddComponent<ZongRoomRowView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "titleText", title);
            SetReference(so, "metaText", meta);
            SetReference(so, "button", button);
            so.ApplyModifiedProperties();

            return row.gameObject;
        }

        private static GameObject CreatePlayerRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("ZongPlayerRowTemplate", parent);
            row.gameObject.SetActive(false);
            Image image = row.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.99f, 1f);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            AddLayout(row.gameObject, 0f, 74f, 1f);

            Text name = CreateText(row, "Игрок", font, 22, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(name.gameObject, 0f, 54f, 1f);
            Text score = CreateText(row, "0", font, 22, FontStyle.Bold, new Color(0.12f, 0.42f, 0.74f), TextAnchor.MiddleCenter);
            AddLayout(score.gameObject, 120f, 54f, 0f);
            Text meta = CreateText(row, "-", font, 17, FontStyle.Normal, new Color(0.34f, 0.4f, 0.48f), TextAnchor.MiddleRight);
            AddLayout(meta.gameObject, 300f, 54f, 0f);

            ZongPlayerRowView view = row.gameObject.AddComponent<ZongPlayerRowView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "nameText", name);
            SetReference(so, "scoreText", score);
            SetReference(so, "metaText", meta);
            so.ApplyModifiedProperties();

            return row.gameObject;
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

            Text text = CreateText(rect, caption, font, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
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
            ZongScreen screen,
            Button backButton,
            GameObject modePanel,
            Button botModeButton,
            Button multiplayerModeButton,
            GameObject botPanel,
            Text botScoreText,
            Text botDiceText,
            Text botStatusText,
            Image[] botDiceImages,
            Button botRollButton,
            Button botBankButton,
            Button botBackButton,
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
            InputField targetScoreInput,
            Button submitCreateRoomButton,
            Button cancelCreateRoomButton,
            Text createRoomStatusText,
            GameObject roomPanel,
            Text roomTitleText,
            Text roomMetaText,
            Text roomDiceText,
            Image[] roomDiceImages,
            Text roomTurnScoreText,
            Text roomStatusText,
            RectTransform playersRoot,
            GameObject playerRowPrefab,
            Button startRoomButton,
            Button rollRoomButton,
            Button bankRoomButton,
            Button refreshRoomButton,
            Button leaveRoomButton,
            ZongDiceAnimator diceAnimator)
        {
            SerializedObject so = new SerializedObject(screen);
            SetReference(so, "backButton", backButton);
            SetString(so, "mainMenuSceneName", "MainMenuScene");
            SetReference(so, "modePanel", modePanel);
            SetReference(so, "botModeButton", botModeButton);
            SetReference(so, "multiplayerModeButton", multiplayerModeButton);
            SetReference(so, "botPanel", botPanel);
            SetReference(so, "botScoreText", botScoreText);
            SetReference(so, "botDiceText", botDiceText);
            SetReference(so, "botStatusText", botStatusText);
            SetArrayReference(so, "botDiceImages", botDiceImages);
            SetReference(so, "botRollButton", botRollButton);
            SetReference(so, "botBankButton", botBankButton);
            SetReference(so, "botBackButton", botBackButton);
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
            SetReference(so, "targetScoreInput", targetScoreInput);
            SetReference(so, "submitCreateRoomButton", submitCreateRoomButton);
            SetReference(so, "cancelCreateRoomButton", cancelCreateRoomButton);
            SetReference(so, "createRoomStatusText", createRoomStatusText);
            SetReference(so, "roomPanel", roomPanel);
            SetReference(so, "roomTitleText", roomTitleText);
            SetReference(so, "roomMetaText", roomMetaText);
            SetReference(so, "roomDiceText", roomDiceText);
            SetArrayReference(so, "roomDiceImages", roomDiceImages);
            SetReference(so, "roomTurnScoreText", roomTurnScoreText);
            SetReference(so, "roomStatusText", roomStatusText);
            SetReference(so, "playersRoot", playersRoot);
            SetReference(so, "playerRowPrefab", playerRowPrefab);
            SetReference(so, "startRoomButton", startRoomButton);
            SetReference(so, "rollRoomButton", rollRoomButton);
            SetReference(so, "bankRoomButton", bankRoomButton);
            SetReference(so, "refreshRoomButton", refreshRoomButton);
            SetReference(so, "leaveRoomButton", leaveRoomButton);
            SetReference(so, "diceAnimator", diceAnimator);
            so.ApplyModifiedProperties();
        }

        private static void SetReference(SerializedObject target, string propertyName, Object value)
        {
            SerializedProperty property = target.FindProperty(propertyName);

            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void SetArrayReference(SerializedObject target, string propertyName, Object[] values)
        {
            SerializedProperty property = target.FindProperty(propertyName);

            if (property == null)
                return;

            property.arraySize = values == null ? 0 : values.Length;

            for (int i = 0; i < property.arraySize; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
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
