using PushPelmesh.CinemaModule;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.CinemaModule.EditorTools
{
    public static class CinemaUiGenerator
    {
        private const string ScenePath = "Assets/_CinemaModule/Scenes/CinemaScene.unity";
        private const string CanvasName = "CinemaCanvas";
        private const string ControllerName = "CinemaController";

        [MenuItem("Tools/Push Uslugi/Cinema Module/Generate CinemaScene UI")]
        public static void GenerateCinemaSceneUi()
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

        [MenuItem("Tools/Push Uslugi/Cinema Module/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);
            if (existingCanvas != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Cinema UI",
                    "В текущей сцене уже есть CinemaCanvas. Заменить его новым интерфейсом?",
                    "Заменить",
                    "Отмена");

                if (!replace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            CinemaScreen screen = FindOrCreateScreen();
            Font font = GetDefaultFont();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateRoot(canvas.transform);

            RectTransform header = CreateHorizontal(root, "Header", 92f, 16f, TextAnchor.MiddleCenter);
            Button backButton = CreateButton(header, "В меню", font, new Color(0.34f, 0.4f, 0.48f), 160f);
            Text title = CreateText(header, "Кино", font, 40, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), TextAnchor.MiddleCenter);
            AddLayout(title.gameObject, 0f, 74f, 1f);
            Button addButton = CreateButton(header, "+", font, new Color(0.15f, 0.55f, 0.35f), 90f);

            RectTransform headerRow = CreateHorizontal(root, "TableHeader", 58f, 2f, TextAnchor.MiddleCenter);
            CreateHeaderCell(headerRow, "Название фильма", font, 1.4f);
            CreateHeaderCell(headerRow, "Рейтинг", font, 0.55f);
            CreateHeaderCell(headerRow, "Дата просмотра", font, 0.8f);
            CreateHeaderCell(headerRow, "Ссылка", font, 0.65f);

            CreateScroll(root, out RectTransform rowsRoot);
            GameObject rowPrefab = CreateRowTemplate(canvas.transform, font);
            Text statusText = CreateTextBlock(root, string.Empty, font, 22, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 48f);

            AddPanelRefs panelRefs = CreateAddPanel(canvas.transform, font);

            EnsureEventSystem();
            AssignReferences(
                screen,
                backButton,
                statusText,
                rowsRoot,
                rowPrefab,
                addButton,
                panelRefs);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static CinemaScreen FindOrCreateScreen()
        {
            CinemaScreen screen = Object.FindFirstObjectByType<CinemaScreen>();
            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            Undo.RegisterCreatedObjectUndo(controller, "Create Cinema Controller");
            return controller.AddComponent<CinemaScreen>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Cinema UI");
            SetUiLayer(canvasObject);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = canvasObject.AddComponent<Image>();
            background.color = new Color(0.92f, 0.94f, 0.96f);

            RectTransform rect = canvasObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

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

        private static RectTransform CreateHorizontal(RectTransform parent, string name, float height, float spacing, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            AddLayout(rect.gameObject, 0f, height, 0f);

            HorizontalLayoutGroup layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            return rect;
        }

        private static void CreateScroll(RectTransform parent, out RectTransform content)
        {
            RectTransform scroll = CreateRect("TableScroll", parent);
            AddLayout(scroll.gameObject, 0f, 0f, 0f, 1f);

            Image background = scroll.gameObject.AddComponent<Image>();
            background.color = Color.white;

            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            RectTransform viewport = CreateRect("Viewport", scroll);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<Image>().color = Color.white;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            content = CreateRect("Rows", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 2f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
        }

        private static GameObject CreateRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("CinemaRowTemplate", parent);
            row.gameObject.SetActive(false);
            AddLayout(row.gameObject, 0f, 72f, 0f);

            Image image = row.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.99f, 1f);

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            Text titleText = CreateCell(row, "Фильм", font, 1.4f);
            Text ratingText = CreateCell(row, "0", font, 0.55f);
            Text watchedAtText = CreateCell(row, "yyyy-MM-dd", font, 0.8f);
            Button linkButton = CreateSmallButton(row, "Открыть", font, new Color(0.12f, 0.42f, 0.74f), 0.65f);

            CinemaRowView rowView = row.gameObject.AddComponent<CinemaRowView>();
            SerializedObject serializedRow = new SerializedObject(rowView);
            SetReference(serializedRow, "titleText", titleText);
            SetReference(serializedRow, "ratingText", ratingText);
            SetReference(serializedRow, "watchedAtText", watchedAtText);
            SetReference(serializedRow, "linkButton", linkButton);
            serializedRow.ApplyModifiedProperties();

            return row.gameObject;
        }

        private static AddPanelRefs CreateAddPanel(Transform parent, Font font)
        {
            RectTransform overlay = CreateRect("AddMoviePanel", parent);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;

            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.42f);

            RectTransform panel = CreateRect("AddMovieContent", overlay);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(860f, 900f);
            panel.anchoredPosition = Vector2.zero;

            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.97f, 0.98f, 1f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(42, 42, 42, 42);
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateTextBlock(panel, "Добавить фильм", font, 36, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), 70f);
            InputField titleInput = CreateInput(panel, "Название фильма", "Например: Интерстеллар", font);
            InputField ratingInput = CreateInput(panel, "Рейтинг", "0-10", font);
            InputField watchedAtInput = CreateInput(panel, "Дата просмотра", "yyyy-MM-dd", font);
            InputField urlInput = CreateInput(panel, "Ссылка на фильм", "https://...", font);
            Text statusText = CreateTextBlock(panel, string.Empty, font, 22, FontStyle.Italic, new Color(0.62f, 0.18f, 0.14f), 48f);

            RectTransform buttons = CreateHorizontal(panel, "Buttons", 76f, 16f, TextAnchor.MiddleCenter);
            Button submitButton = CreateButton(buttons, "Добавить", font, new Color(0.15f, 0.55f, 0.35f), 0f);
            Button cancelButton = CreateButton(buttons, "Отмена", font, new Color(0.42f, 0.46f, 0.52f), 0f);

            overlay.gameObject.SetActive(false);

            return new AddPanelRefs
            {
                Panel = overlay.gameObject,
                TitleInput = titleInput,
                RatingInput = ratingInput,
                WatchedAtInput = watchedAtInput,
                UrlInput = urlInput,
                StatusText = statusText,
                SubmitButton = submitButton,
                CancelButton = cancelButton
            };
        }

        private static InputField CreateInput(RectTransform parent, string label, string placeholder, Font font)
        {
            RectTransform group = CreateRect(label + " Group", parent);
            AddLayout(group.gameObject, 0f, 116f, 0f);

            VerticalLayoutGroup layout = group.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateTextBlock(group, label, font, 21, FontStyle.Bold, new Color(0.16f, 0.2f, 0.26f), 32f);

            RectTransform inputRect = CreateRect(label + " Input", group);
            AddLayout(inputRect.gameObject, 0f, 66f, 0f);

            Image image = inputRect.gameObject.AddComponent<Image>();
            image.color = Color.white;

            InputField input = inputRect.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.lineType = InputField.LineType.SingleLine;

            Text text = CreateInputText(inputRect, "Text", font, string.Empty, new Color(0.1f, 0.12f, 0.16f));
            Text placeholderText = CreateInputText(inputRect, "Placeholder", font, placeholder, new Color(0.52f, 0.56f, 0.62f));
            placeholderText.fontStyle = FontStyle.Italic;

            input.textComponent = text;
            input.placeholder = placeholderText;

            return input;
        }

        private static Text CreateInputText(RectTransform parent, string name, Font font, string value, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(18f, 0f);
            rect.offsetMax = new Vector2(-18f, 0f);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 22;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;

            return text;
        }

        private static Text CreateHeaderCell(RectTransform parent, string value, Font font, float flexibleWidth)
        {
            RectTransform cell = CreateRect(value + " Header", parent);
            AddLayout(cell.gameObject, 0f, 0f, flexibleWidth);

            Image image = cell.gameObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.18f, 0.26f);

            Text text = CreateText(cell, value, font, 20, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14f, 0f);
            textRect.offsetMax = new Vector2(-14f, 0f);

            return text;
        }

        private static Text CreateCell(RectTransform parent, string value, Font font, float flexibleWidth)
        {
            RectTransform cell = CreateRect(value + " Cell", parent);
            AddLayout(cell.gameObject, 0f, 0f, flexibleWidth);

            Image image = cell.gameObject.AddComponent<Image>();
            image.color = new Color(0.98f, 0.99f, 1f);

            Text text = CreateText(cell, value, font, 20, FontStyle.Normal, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14f, 0f);
            textRect.offsetMax = new Vector2(-14f, 0f);

            return text;
        }

        private static Button CreateSmallButton(RectTransform parent, string caption, Font font, Color color, float flexibleWidth)
        {
            RectTransform rect = CreateRect(caption + " Cell Button", parent);
            AddLayout(rect.gameObject, 0f, 0f, flexibleWidth);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText(rect, caption, font, 19, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static Button CreateButton(RectTransform parent, string caption, Font font, Color color, float preferredWidth)
        {
            RectTransform rect = CreateRect(caption + " Button", parent);
            AddLayout(rect.gameObject, preferredWidth, 74f, preferredWidth <= 0f ? 1f : 0f);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText(rect, caption, font, 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static Text CreateTextBlock(RectTransform parent, string value, Font font, int size, FontStyle style, Color color, float height)
        {
            Text text = CreateText(parent, value, font, size, style, color, TextAnchor.MiddleLeft);
            AddLayout(text.gameObject, 0f, height, 0f);
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
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetUiLayer(go);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static void AddLayout(GameObject go, float preferredWidth, float preferredHeight, float flexibleWidth, float flexibleHeight = 0f)
        {
            LayoutElement element = go.GetComponent<LayoutElement>();
            if (element == null)
                element = go.AddComponent<LayoutElement>();

            element.preferredWidth = preferredWidth;
            element.preferredHeight = preferredHeight;
            element.flexibleWidth = flexibleWidth;
            element.flexibleHeight = flexibleHeight;
        }

        private static void AssignReferences(
            CinemaScreen screen,
            Button backButton,
            Text statusText,
            RectTransform rowsRoot,
            GameObject rowPrefab,
            Button addButton,
            AddPanelRefs panelRefs)
        {
            SerializedObject serializedScreen = new SerializedObject(screen);
            SetReference(serializedScreen, "backButton", backButton);
            SetReference(serializedScreen, "statusText", statusText);
            SetReference(serializedScreen, "rowsRoot", rowsRoot);
            SetReference(serializedScreen, "rowPrefab", rowPrefab);
            SetReference(serializedScreen, "addButton", addButton);
            SetReference(serializedScreen, "addPanel", panelRefs.Panel);
            SetReference(serializedScreen, "titleInput", panelRefs.TitleInput);
            SetReference(serializedScreen, "ratingInput", panelRefs.RatingInput);
            SetReference(serializedScreen, "watchedAtInput", panelRefs.WatchedAtInput);
            SetReference(serializedScreen, "urlInput", panelRefs.UrlInput);
            SetReference(serializedScreen, "addStatusText", panelRefs.StatusText);
            SetReference(serializedScreen, "submitButton", panelRefs.SubmitButton);
            SetReference(serializedScreen, "cancelButton", panelRefs.CancelButton);
            SetString(serializedScreen, "mainMenuSceneName", "MainMenuScene");
            serializedScreen.ApplyModifiedProperties();
        }

        private static void SetReference(SerializedObject target, string propertyName, Object value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            property.objectReferenceValue = value;
        }

        private static void SetString(SerializedObject target, string propertyName, string value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            property.stringValue = value;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return font;
        }

        private static void SetUiLayer(GameObject go)
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
                go.layer = uiLayer;
        }

        private struct AddPanelRefs
        {
            public GameObject Panel;
            public InputField TitleInput;
            public InputField RatingInput;
            public InputField WatchedAtInput;
            public InputField UrlInput;
            public Text StatusText;
            public Button SubmitButton;
            public Button CancelButton;
        }
    }
}
