using PushPelmesh.VoteModule;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.VoteModule.EditorTools
{
    public static class VoteUiGenerator
    {
        private const string ScenePath = "Assets/_VoteModule/Scenes/VoteScene.unity";
        private const string CanvasName = "VoteCanvas";
        private const string ControllerName = "VoteController";

        [MenuItem("Tools/Push Uslugi/Vote Module/Generate VoteScene UI")]
        public static void GenerateVoteSceneUi()
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

        [MenuItem("Tools/Push Uslugi/Vote Module/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);

            if (existingCanvas != null)
            {
                bool replace = Application.isBatchMode || EditorUtility.DisplayDialog(
                    "Vote UI",
                    "В текущей сцене уже есть VoteCanvas. Заменить его новым интерфейсом?",
                    "Заменить",
                    "Отмена");

                if (!replace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            VoteScreen screen = FindOrCreateScreen();
            Font font = GetDefaultFont();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateRoot(canvas.transform);

            RectTransform header = CreateHorizontal(root, "Header", 86f, 14f, TextAnchor.MiddleCenter);
            Button backButton = CreateButton(header, "В меню", font, new Color(0.34f, 0.4f, 0.48f), 156f);
            Text title = CreateText(header, "Голосования", font, 38, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), TextAnchor.MiddleCenter);
            AddLayout(title.gameObject, 0f, 72f, 1f);
            Button refreshButton = CreateButton(header, "Обновить", font, new Color(0.12f, 0.42f, 0.74f), 168f);

            GameObject listPanel = CreatePanel(root, "ListPanel");
            RectTransform listHeader = CreateHorizontal(listPanel.GetComponent<RectTransform>(), "ListHeader", 76f, 12f, TextAnchor.MiddleCenter);
            Text listTitle = CreateText(listHeader, "Открытые и завершенные голосования", font, 26, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(listTitle.gameObject, 0f, 66f, 1f);
            Button openCreateButton = CreateButton(listHeader, "+", font, new Color(0.12f, 0.58f, 0.32f), 66f);
            CreateScroll(listPanel.GetComponent<RectTransform>(), "PollsScroll", out RectTransform pollsRoot);
            Text listStatus = CreateTextBlock(listPanel.GetComponent<RectTransform>(), string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 42f);

            GameObject detailsPanel = CreatePanel(root, "DetailsPanel");
            RectTransform detailsHeader = CreateHorizontal(detailsPanel.GetComponent<RectTransform>(), "DetailsHeader", 74f, 12f, TextAnchor.MiddleCenter);
            Button closeDetailsButton = CreateButton(detailsHeader, "Назад", font, new Color(0.34f, 0.4f, 0.48f), 130f);
            Text detailsTitle = CreateText(detailsHeader, "Голосование", font, 30, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(detailsTitle.gameObject, 0f, 64f, 1f);
            Text detailsDescription = CreateTextBlock(detailsPanel.GetComponent<RectTransform>(), string.Empty, font, 22, FontStyle.Normal, new Color(0.16f, 0.18f, 0.22f), 96f);
            Text detailsMeta = CreateTextBlock(detailsPanel.GetComponent<RectTransform>(), string.Empty, font, 19, FontStyle.Italic, new Color(0.34f, 0.4f, 0.48f), 42f);
            CreateScroll(detailsPanel.GetComponent<RectTransform>(), "OptionsScroll", out RectTransform optionsRoot);
            Text detailsStatus = CreateTextBlock(detailsPanel.GetComponent<RectTransform>(), string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), 44f);

            GameObject createPanel = CreatePanel(root, "CreatePanel");
            RectTransform createHeader = CreateHorizontal(createPanel.GetComponent<RectTransform>(), "CreateHeader", 74f, 12f, TextAnchor.MiddleCenter);
            Button closeCreateButton = CreateButton(createHeader, "Назад", font, new Color(0.34f, 0.4f, 0.48f), 130f);
            Text createTitle = CreateText(createHeader, "Создать голосование", font, 30, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(createTitle.gameObject, 0f, 64f, 1f);
            InputField titleInput = CreateInput(createPanel.GetComponent<RectTransform>(), "TitleInput", "Название", font, 58f);
            InputField descriptionInput = CreateInput(createPanel.GetComponent<RectTransform>(), "DescriptionInput", "Описание", font, 86f);
            InputField endDateInput = CreateInput(createPanel.GetComponent<RectTransform>(), "EndDateInput", "Дата окончания yyyy-MM-dd", font, 58f);

            CreateScroll(createPanel.GetComponent<RectTransform>(), "OptionInputsScroll", out RectTransform optionsBlock, 190f, 0f);
            GameObject optionInputRowPrefab = CreateOptionInputRowTemplate(createPanel.transform, font);
            Button addOptionButton = CreateButton(createPanel.GetComponent<RectTransform>(), "+ вариант", font, new Color(0.12f, 0.42f, 0.74f), 0f);

            GameObject audiencePanel = CreatePanel(createPanel.GetComponent<RectTransform>(), "AudiencePanel");
            RectTransform audienceRow = CreateHorizontal(audiencePanel.GetComponent<RectTransform>(), "AudienceRow", 54f, 16f, TextAnchor.MiddleLeft);
            Text audienceLabel = CreateText(audienceRow, "Доступно:", font, 21, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(audienceLabel.gameObject, 170f, 48f, 0f);
            Toggle regularUsersToggle = CreateToggle(audienceRow, "Обычные пользователи", font);
            Toggle ministersToggle = CreateToggle(audienceRow, "Министры", font);

            RectTransform createFooter = CreateHorizontal(createPanel.GetComponent<RectTransform>(), "CreateFooter", 70f, 12f, TextAnchor.MiddleCenter);
            Text createStatus = CreateText(createFooter, string.Empty, font, 20, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), TextAnchor.MiddleLeft);
            AddLayout(createStatus.gameObject, 0f, 58f, 1f);
            Button submitCreateButton = CreateButton(createFooter, "Создать", font, new Color(0.12f, 0.58f, 0.32f), 170f);

            GameObject pollRowPrefab = CreatePollRowTemplate(canvas.transform, font);
            GameObject optionPrefab = CreateOptionTemplate(canvas.transform, font);

            EnsureEventSystem();
            AssignReferences(
                screen,
                backButton,
                listPanel,
                pollsRoot,
                pollRowPrefab,
                refreshButton,
                openCreateButton,
                listStatus,
                detailsPanel,
                closeDetailsButton,
                detailsTitle,
                detailsDescription,
                detailsMeta,
                detailsStatus,
                optionsRoot,
                optionPrefab,
                createPanel,
                closeCreateButton,
                submitCreateButton,
                titleInput,
                descriptionInput,
                endDateInput,
                optionsBlock,
                optionInputRowPrefab,
                addOptionButton,
                audiencePanel,
                regularUsersToggle,
                ministersToggle,
                createStatus);

            detailsPanel.SetActive(false);
            createPanel.SetActive(false);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static VoteScreen FindOrCreateScreen()
        {
            VoteScreen screen = Object.FindFirstObjectByType<VoteScreen>();
            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            SetUiLayer(controller);
            Undo.RegisterCreatedObjectUndo(controller, "Create Vote Controller");
            return controller.AddComponent<VoteScreen>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SetUiLayer(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Vote Canvas");

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            return canvas;
        }

        private static RectTransform CreateRoot(Transform parent)
        {
            RectTransform root = CreateRect("Content", parent);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(80f, 54f);
            root.offsetMax = new Vector2(-80f, -54f);

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
            image.color = new Color(0.91f, 0.92f, 0.91f, 1f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 24, 24);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLayout(panel.gameObject, 0f, 0f, 1f, 1f);
            return panel.gameObject;
        }

        private static RectTransform CreateHorizontal(RectTransform parent, string name, float height, float spacing, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            HorizontalLayoutGroup layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
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
            Image background = scroll.gameObject.AddComponent<Image>();
            background.color = new Color(0.84f, 0.86f, 0.87f, 1f);
            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            AddLayout(scroll.gameObject, 0f, preferredHeight, 1f, flexibleHeight);

            RectTransform viewport = CreateRect("Viewport", scroll);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(10f, 10f);
            viewport.offsetMax = new Vector2(-10f, -10f);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.horizontal = false;

            return scroll;
        }

        private static GameObject CreatePollRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("VotePollRowTemplate", parent);
            row.gameObject.SetActive(false);
            Image image = row.gameObject.AddComponent<Image>();
            image.color = Color.white;
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            AddLayout(row.gameObject, 0f, 76f, 1f);

            Text title = CreateText(row, "Название", font, 22, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(title.gameObject, 0f, 56f, 1f);
            Text meta = CreateText(row, "До даты", font, 18, FontStyle.Normal, new Color(0.34f, 0.4f, 0.48f), TextAnchor.MiddleCenter);
            AddLayout(meta.gameObject, 260f, 56f, 0f);
            Text status = CreateText(row, "Открыто", font, 18, FontStyle.Bold, new Color(0.12f, 0.42f, 0.74f), TextAnchor.MiddleCenter);
            AddLayout(status.gameObject, 180f, 56f, 0f);

            VotePollListItemView view = row.gameObject.AddComponent<VotePollListItemView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "titleText", title);
            SetReference(so, "metaText", meta);
            SetReference(so, "statusText", status);
            SetReference(so, "button", button);

            return row.gameObject;
        }

        private static GameObject CreateOptionTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateRect("VoteOptionTemplate", parent);
            row.gameObject.SetActive(false);
            Image image = row.gameObject.AddComponent<Image>();
            image.color = Color.white;
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            AddLayout(row.gameObject, 0f, 72f, 1f);

            Text title = CreateText(row, "Вариант", font, 22, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(title.gameObject, 0f, 52f, 1f);
            Image fill = CreateRect("ResultFill", row).gameObject.AddComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.color = new Color(0.12f, 0.42f, 0.74f, 0.28f);
            AddLayout(fill.gameObject, 160f, 52f, 0f);
            Text result = CreateText(row, "0 голосов • 0%", font, 18, FontStyle.Normal, new Color(0.34f, 0.4f, 0.48f), TextAnchor.MiddleCenter);
            AddLayout(result.gameObject, 220f, 52f, 0f);

            VoteOptionView view = row.gameObject.AddComponent<VoteOptionView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "titleText", title);
            SetReference(so, "resultText", result);
            SetReference(so, "fillImage", fill);
            SetReference(so, "voteButton", button);

            return row.gameObject;
        }

        private static GameObject CreateOptionInputRowTemplate(Transform parent, Font font)
        {
            RectTransform row = CreateHorizontal(parent as RectTransform ?? parent.GetComponent<RectTransform>(), "OptionInputRowTemplate", 54f, 8f, TextAnchor.MiddleCenter);
            row.gameObject.SetActive(false);

            InputField input = CreateInput(row, "OptionInput", "Вариант", font, 46f);
            Button removeButton = CreateButton(row, "x", font, new Color(0.72f, 0.16f, 0.16f), 54f);

            VoteCreateOptionInputView view = row.gameObject.AddComponent<VoteCreateOptionInputView>();
            SerializedObject so = new SerializedObject(view);
            SetReference(so, "input", input);
            SetReference(so, "removeButton", removeButton);

            return row.gameObject;
        }

        private static InputField CreateInput(RectTransform parent, string name, string placeholder, Font font, float height)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            InputField input = rect.gameObject.AddComponent<InputField>();
            AddLayout(rect.gameObject, 0f, height, 1f);

            Text text = CreateText(rect, "Text", font, 21, FontStyle.Normal, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(14f, 4f);
            text.rectTransform.offsetMax = new Vector2(-14f, -4f);
            text.text = string.Empty;

            Text placeholderText = CreateText(rect, placeholder, font, 21, FontStyle.Italic, new Color(0.52f, 0.56f, 0.62f), TextAnchor.MiddleLeft);
            placeholderText.rectTransform.anchorMin = Vector2.zero;
            placeholderText.rectTransform.anchorMax = Vector2.one;
            placeholderText.rectTransform.offsetMin = new Vector2(14f, 4f);
            placeholderText.rectTransform.offsetMax = new Vector2(-14f, -4f);

            input.textComponent = text;
            input.placeholder = placeholderText;
            input.targetGraphic = image;
            return input;
        }

        private static Toggle CreateToggle(RectTransform parent, string caption, Font font)
        {
            RectTransform rect = CreateRect(caption + " Toggle", parent);
            HorizontalLayoutGroup layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            AddLayout(rect.gameObject, 280f, 48f, 0f);

            Image checkmark = CreateRect("Checkmark", rect).gameObject.AddComponent<Image>();
            checkmark.color = new Color(0.12f, 0.58f, 0.32f);
            AddLayout(checkmark.gameObject, 34f, 34f, 0f);

            Text label = CreateText(rect, caption, font, 20, FontStyle.Normal, new Color(0.1f, 0.12f, 0.16f), TextAnchor.MiddleLeft);
            AddLayout(label.gameObject, 0f, 42f, 1f);

            Toggle toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.graphic = checkmark;
            toggle.isOn = true;
            return toggle;
        }

        private static Button CreateButton(RectTransform parent, string caption, Font font, Color color, float preferredWidth)
        {
            RectTransform rect = CreateRect(caption + " Button", parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            AddLayout(rect.gameObject, preferredWidth, 58f, preferredWidth <= 0f ? 1f : 0f);

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
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            SetUiLayer(go);
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void AddLayout(GameObject go, float preferredWidth, float preferredHeight, float flexibleWidth, float flexibleHeight = 0f)
        {
            LayoutElement layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            layout.preferredWidth = preferredWidth;
            layout.preferredHeight = preferredHeight;
            layout.flexibleWidth = flexibleWidth;
            layout.flexibleHeight = flexibleHeight;
        }

        private static void AssignReferences(
            VoteScreen screen,
            Button backButton,
            GameObject listPanel,
            RectTransform pollsRoot,
            GameObject pollRowPrefab,
            Button refreshButton,
            Button openCreateButton,
            Text listStatus,
            GameObject detailsPanel,
            Button closeDetailsButton,
            Text detailsTitle,
            Text detailsDescription,
            Text detailsMeta,
            Text detailsStatus,
            RectTransform optionsRoot,
            GameObject optionPrefab,
            GameObject createPanel,
            Button closeCreateButton,
            Button submitCreateButton,
            InputField titleInput,
            InputField descriptionInput,
            InputField endDateInput,
            RectTransform optionInputsRoot,
            GameObject optionInputRowPrefab,
            Button addOptionButton,
            GameObject audiencePanel,
            Toggle regularUsersToggle,
            Toggle ministersToggle,
            Text createStatus)
        {
            SerializedObject so = new SerializedObject(screen);
            SetReference(so, "backButton", backButton);
            SetString(so, "mainMenuSceneName", "MainMenuScene");
            SetReference(so, "listPanel", listPanel);
            SetReference(so, "pollsRoot", pollsRoot);
            SetReference(so, "pollRowPrefab", pollRowPrefab);
            SetReference(so, "refreshButton", refreshButton);
            SetReference(so, "openCreateButton", openCreateButton);
            SetReference(so, "listStatusText", listStatus);
            SetReference(so, "detailsPanel", detailsPanel);
            SetReference(so, "closeDetailsButton", closeDetailsButton);
            SetReference(so, "detailsTitleText", detailsTitle);
            SetReference(so, "detailsDescriptionText", detailsDescription);
            SetReference(so, "detailsMetaText", detailsMeta);
            SetReference(so, "detailsStatusText", detailsStatus);
            SetReference(so, "optionsRoot", optionsRoot);
            SetReference(so, "optionPrefab", optionPrefab);
            SetReference(so, "createPanel", createPanel);
            SetReference(so, "closeCreateButton", closeCreateButton);
            SetReference(so, "submitCreateButton", submitCreateButton);
            SetReference(so, "titleInput", titleInput);
            SetReference(so, "descriptionInput", descriptionInput);
            SetReference(so, "endDateInput", endDateInput);
            SetReference(so, "optionInputsRoot", optionInputsRoot);
            SetReference(so, "optionInputRowPrefab", optionInputRowPrefab);
            SetReference(so, "addOptionButton", addOptionButton);
            SetReference(so, "audiencePanel", audiencePanel);
            SetReference(so, "regularUsersToggle", regularUsersToggle);
            SetReference(so, "ministersToggle", ministersToggle);
            SetReference(so, "createStatusText", createStatus);
            so.ApplyModifiedProperties();
        }

        private static void SetReference(SerializedObject target, string propertyName, Object value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
            target.ApplyModifiedProperties();
        }

        private static void SetString(SerializedObject target, string propertyName, string value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            if (property != null)
                property.stringValue = value;
            target.ApplyModifiedProperties();
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
