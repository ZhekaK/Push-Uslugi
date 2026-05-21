using PushPelmesh.RewardModule;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.RewardModule.EditorTools
{
    public static class RewardUiGenerator
    {
        private const string ScenePath = "Assets/_RewardModule/Scenes/RewardScene.unity";
        private const string CanvasName = "RewardCanvas";
        private const string ControllerName = "RewardController";

        [MenuItem("Tools/Push Uslugi/Reward Module/Generate RewardScene UI")]
        public static void GenerateRewardSceneUi()
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

        [MenuItem("Tools/Push Uslugi/Reward Module/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);
            if (existingCanvas != null)
            {
                bool replace = Application.isBatchMode || EditorUtility.DisplayDialog(
                    "Reward UI",
                    "В текущей сцене уже есть RewardCanvas. Заменить его новым интерфейсом?",
                    "Заменить",
                    "Отмена");

                if (!replace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            RewardScreen screen = FindOrCreateScreen();
            Font font = GetDefaultFont();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateRoot(canvas.transform);

            RectTransform header = CreateHorizontal(root, "Header", 92f, 16f, TextAnchor.MiddleCenter);
            Button backButton = CreateButton(header, "В меню", font, new Color(0.34f, 0.4f, 0.48f), 160f);
            Text title = CreateText(header, "Награды", font, 40, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), TextAnchor.MiddleCenter);
            AddLayout(title.gameObject, 0f, 74f, 1f);
            CreateSpacer(header, 160f, 74f);

            RectTransform tabs = CreateHorizontal(root, "Tabs", 76f, 12f, TextAnchor.MiddleCenter);
            Button championshipsTabButton = CreateButton(tabs, "Чемпионаты", font, new Color(0.12f, 0.42f, 0.74f), 0f);
            Button governmentAwardsTabButton = CreateButton(tabs, "Гос. награды", font, new Color(0.36f, 0.42f, 0.5f), 0f);

            Text tableTitle = CreateTextBlock(root, "Чемпионаты", font, 30, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), 56f);

            RectTransform headerRow = CreateHorizontal(root, "TableHeader", 58f, 2f, TextAnchor.MiddleCenter);
            Text firstHeader = CreateHeaderCell(headerRow, "ФИО", font);
            Text secondHeader = CreateHeaderCell(headerRow, "Дата", font);
            Text thirdHeader = CreateHeaderCell(headerRow, "Название события", font);
            Text fourthHeader = CreateHeaderCell(headerRow, "Место", font);

            CreateScroll(root, out RectTransform rowsRoot);
            GameObject rowPrefab = CreateRowTemplate(canvas.transform, font);
            Text statusText = CreateTextBlock(root, string.Empty, font, 22, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 48f);

            EnsureEventSystem();
            AssignReferences(
                screen,
                backButton,
                championshipsTabButton,
                governmentAwardsTabButton,
                championshipsTabButton.targetGraphic as Image,
                governmentAwardsTabButton.targetGraphic as Image,
                tableTitle,
                firstHeader,
                secondHeader,
                thirdHeader,
                fourthHeader,
                statusText,
                rowsRoot,
                rowPrefab);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static RewardScreen FindOrCreateScreen()
        {
            RewardScreen screen = Object.FindFirstObjectByType<RewardScreen>();
            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            Undo.RegisterCreatedObjectUndo(controller, "Create Reward Controller");
            return controller.AddComponent<RewardScreen>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Reward UI");
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
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            viewport.gameObject.AddComponent<Image>().color = Color.white;

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
            RectTransform row = CreateRect("RewardRowTemplate", parent);
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

            Text first = CreateCell(row, "ФИО", font);
            Text second = CreateCell(row, "Дата", font);
            Text third = CreateCell(row, "Событие", font);
            Text fourth = CreateCell(row, "Место", font);

            RewardRowView rowView = row.gameObject.AddComponent<RewardRowView>();
            SerializedObject serializedRow = new SerializedObject(rowView);
            SetReference(serializedRow, "firstColumnText", first);
            SetReference(serializedRow, "secondColumnText", second);
            SetReference(serializedRow, "thirdColumnText", third);
            SetReference(serializedRow, "fourthColumnText", fourth);
            serializedRow.ApplyModifiedProperties();

            return row.gameObject;
        }

        private static Text CreateHeaderCell(RectTransform parent, string value, Font font)
        {
            RectTransform cell = CreateRect(value + " Header", parent);
            AddLayout(cell.gameObject, 0f, 0f, 1f);

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

        private static Text CreateCell(RectTransform parent, string value, Font font)
        {
            RectTransform cell = CreateRect(value + " Cell", parent);
            AddLayout(cell.gameObject, 0f, 0f, 1f);

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

        private static void CreateSpacer(RectTransform parent, float width, float height)
        {
            RectTransform spacer = CreateRect("Spacer", parent);
            AddLayout(spacer.gameObject, width, height, 0f);
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
            RewardScreen screen,
            Button backButton,
            Button championshipsTabButton,
            Button governmentAwardsTabButton,
            Image championshipsTabBackground,
            Image governmentAwardsTabBackground,
            Text tableTitle,
            Text firstHeader,
            Text secondHeader,
            Text thirdHeader,
            Text fourthHeader,
            Text statusText,
            RectTransform rowsRoot,
            GameObject rowPrefab)
        {
            SerializedObject serializedScreen = new SerializedObject(screen);
            SetReference(serializedScreen, "backButton", backButton);
            SetReference(serializedScreen, "championshipsTabButton", championshipsTabButton);
            SetReference(serializedScreen, "governmentAwardsTabButton", governmentAwardsTabButton);
            SetReference(serializedScreen, "championshipsTabBackground", championshipsTabBackground);
            SetReference(serializedScreen, "governmentAwardsTabBackground", governmentAwardsTabBackground);
            SetReference(serializedScreen, "tableTitleText", tableTitle);
            SetReference(serializedScreen, "firstHeaderText", firstHeader);
            SetReference(serializedScreen, "secondHeaderText", secondHeader);
            SetReference(serializedScreen, "thirdHeaderText", thirdHeader);
            SetReference(serializedScreen, "fourthHeaderText", fourthHeader);
            SetReference(serializedScreen, "statusText", statusText);
            SetReference(serializedScreen, "rowsRoot", rowsRoot);
            SetReference(serializedScreen, "rowPrefab", rowPrefab);
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
    }
}
