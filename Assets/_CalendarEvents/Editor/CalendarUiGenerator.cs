using System.Collections.Generic;
using PushPelmesh.CalendarEvents;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.CalendarEvents.EditorTools
{
    public static class CalendarUiGenerator
    {
        private const string ScenePath = "Assets/_CalendarEvents/CalendarScene.unity";
        private const string CanvasName = "CalendarCanvas";
        private const string ControllerName = "CalendarController";
        private const string EventBadgeTemplateName = "EventBadgeTemplate";

        [MenuItem("Tools/Push Uslugi/Calendar Events/Generate CalendarScene UI")]
        public static void GenerateCalendarSceneUi()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset != null)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
            else
            {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            }

            GenerateUiInCurrentScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        }

        [MenuItem("Tools/Push Uslugi/Calendar Events/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);
            if (existingCanvas != null)
            {
                bool shouldReplace = EditorUtility.DisplayDialog(
                    "Calendar Events UI",
                    "Р’ С‚РµРєСѓС‰РµР№ СЃС†РµРЅРµ СѓР¶Рµ РµСЃС‚СЊ CalendarCanvas. Р—Р°РјРµРЅРёС‚СЊ РµРіРѕ РЅРѕРІС‹Рј СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Рј РёРЅС‚РµСЂС„РµР№СЃРѕРј?",
                    "Р—Р°РјРµРЅРёС‚СЊ",
                    "РћС‚РјРµРЅР°");

                if (!shouldReplace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            CalendarScreen screen = FindOrCreateScreen();
            Font font = GetDefaultFont();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateRoot(canvas.transform);
            RectTransform header = CreateHeader(root);

            Button backButton = CreateButton(header, "Р’ РјРµРЅСЋ", font, new Color(0.32f, 0.38f, 0.45f), 132f);
            Button previousMonthButton = CreateButton(header, "<", font, new Color(0.16f, 0.42f, 0.72f), 72f);
            Text monthTitleText = CreateText(header, "РјР°Р№ 2026", font, 38, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), TextAnchor.MiddleCenter);
            AddLayout(monthTitleText.gameObject, 0f, 86f, 1f);
            Button nextMonthButton = CreateButton(header, ">", font, new Color(0.16f, 0.42f, 0.72f), 72f);
            Button addEventButton = CreateButton(header, "+", font, new Color(0.15f, 0.55f, 0.35f), 72f);

            Text statusText = CreateText(root, string.Empty, font, 24, FontStyle.Italic, new Color(0.36f, 0.42f, 0.48f), TextAnchor.MiddleLeft);
            AddLayout(statusText.gameObject, 0f, 42f, 0f);

            RectTransform weekHeader = CreateWeekHeader(root, font);
            RectTransform grid = CreateCalendarGrid(root);
            List<CalendarDayCell> dayCells = CreateDayCells(grid, font);

            GameObject templates = new GameObject("Templates", typeof(RectTransform));
            templates.transform.SetParent(canvas.transform, false);
            SetUiLayer(templates);
            GameObject eventBadgeTemplate = CreateEventBadgeTemplate(templates.transform, font);
            eventBadgeTemplate.SetActive(false);

            AddPanelRefs addPanelRefs = CreateAddEventPanel(canvas.transform, font);
            DetailsPanelRefs detailsPanelRefs = CreateDetailsPanel(canvas.transform, font);

            EnsureEventSystem();
            AssignReferences(
                screen,
                monthTitleText,
                statusText,
                previousMonthButton,
                nextMonthButton,
                backButton,
                addEventButton,
                dayCells,
                eventBadgeTemplate,
                addPanelRefs,
                detailsPanelRefs);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static CalendarScreen FindOrCreateScreen()
        {
            CalendarScreen screen = Object.FindFirstObjectByType<CalendarScreen>();
            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            Undo.RegisterCreatedObjectUndo(controller, "Create Calendar Controller");
            return controller.AddComponent<CalendarScreen>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Calendar UI");
            SetUiLayer(canvasObject);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = canvasObject.AddComponent<Image>();
            background.color = new Color(0.92f, 0.95f, 0.98f);

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

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
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return root;
        }

        private static RectTransform CreateHeader(RectTransform parent)
        {
            RectTransform header = CreateRect("Header", parent);
            AddLayout(header.gameObject, 0f, 92f, 0f);

            HorizontalLayoutGroup layout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            return header;
        }

        private static RectTransform CreateWeekHeader(RectTransform parent, Font font)
        {
            RectTransform weekHeader = CreateRect("WeekHeader", parent);
            AddLayout(weekHeader.gameObject, 0f, 54f, 0f);

            GridLayoutGroup grid = weekHeader.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
            grid.cellSize = new Vector2(136f, 54f);
            grid.spacing = new Vector2(8f, 0f);

            string[] labels = { "РџРЅ", "Р’С‚", "РЎСЂ", "Р§С‚", "РџС‚", "РЎР±", "Р’СЃ" };
            for (int i = 0; i < labels.Length; i++)
            {
                Text label = CreateText(weekHeader, labels[i], font, 24, FontStyle.Bold, new Color(0.24f, 0.3f, 0.38f), TextAnchor.MiddleCenter);
                label.raycastTarget = false;
            }

            return weekHeader;
        }

        private static RectTransform CreateCalendarGrid(RectTransform parent)
        {
            RectTransform gridRect = CreateRect("CalendarGrid", parent);
            AddLayout(gridRect.gameObject, 0f, 0f, 0f, 1f);

            GridLayoutGroup grid = gridRect.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
            grid.cellSize = new Vector2(136f, 178f);
            grid.spacing = new Vector2(8f, 8f);

            return gridRect;
        }

        private static List<CalendarDayCell> CreateDayCells(RectTransform parent, Font font)
        {
            List<CalendarDayCell> cells = new List<CalendarDayCell>();

            for (int i = 0; i < 42; i++)
            {
                RectTransform cellRect = CreateRect("DayCell " + (i + 1), parent);
                Image background = cellRect.gameObject.AddComponent<Image>();
                background.color = Color.white;

                Button button = cellRect.gameObject.AddComponent<Button>();
                button.targetGraphic = background;

                Text dayText = CreateText(cellRect, "1", font, 25, FontStyle.Bold, new Color(0.1f, 0.12f, 0.16f), TextAnchor.UpperLeft);
                RectTransform dayTextRect = dayText.GetComponent<RectTransform>();
                dayTextRect.anchorMin = new Vector2(0f, 1f);
                dayTextRect.anchorMax = new Vector2(1f, 1f);
                dayTextRect.pivot = new Vector2(0.5f, 1f);
                dayTextRect.offsetMin = new Vector2(12f, -40f);
                dayTextRect.offsetMax = new Vector2(-12f, -8f);

                RectTransform eventsRoot = CreateRect("Events", cellRect);
                eventsRoot.anchorMin = new Vector2(0f, 0f);
                eventsRoot.anchorMax = new Vector2(1f, 1f);
                eventsRoot.offsetMin = new Vector2(8f, 8f);
                eventsRoot.offsetMax = new Vector2(-8f, -46f);

                VerticalLayoutGroup eventsLayout = eventsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
                eventsLayout.spacing = 4f;
                eventsLayout.childControlWidth = true;
                eventsLayout.childControlHeight = false;
                eventsLayout.childForceExpandWidth = true;
                eventsLayout.childForceExpandHeight = false;
                eventsLayout.childAlignment = TextAnchor.UpperCenter;

                CalendarDayCell cell = cellRect.gameObject.AddComponent<CalendarDayCell>();
                SerializedObject serializedCell = new SerializedObject(cell);
                SetReference(serializedCell, "dateButton", button);
                SetReference(serializedCell, "backgroundImage", background);
                SetReference(serializedCell, "dayText", dayText);
                SetReference(serializedCell, "eventsRoot", eventsRoot);
                serializedCell.ApplyModifiedProperties();

                cells.Add(cell);
            }

            return cells;
        }

        private static GameObject CreateEventBadgeTemplate(Transform parent, Font font)
        {
            RectTransform rect = CreateRect(EventBadgeTemplateName, parent);
            rect.sizeDelta = new Vector2(120f, 28f);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.14f, 0.43f, 0.75f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text label = CreateText(rect, "РЎРѕР±С‹С‚РёРµ", font, 16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 30f;

            return rect.gameObject;
        }

        private static AddPanelRefs CreateAddEventPanel(Transform parent, Font font)
        {
            GameObject overlay = CreateOverlay("AddEventPanel", parent);
            RectTransform panel = CreateModalPanel("AddEventContent", overlay.transform, new Vector2(860f, 980f));

            CreateTextBlock(panel, "Р”РѕР±Р°РІРёС‚СЊ СЃРѕР±С‹С‚РёРµ", font, 38, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), 72f);
            InputField titleInput = CreateInput(panel, "РќР°Р·РІР°РЅРёРµ", "РќР°РїСЂРёРјРµСЂ: Р·Р°СЃРµРґР°РЅРёРµ", font, false);
            InputField descriptionInput = CreateInput(panel, "РћРїРёСЃР°РЅРёРµ", "РљРѕСЂРѕС‚РєРѕРµ РѕРїРёСЃР°РЅРёРµ СЃРѕР±С‹С‚РёСЏ", font, true);
            InputField dateInput = CreateInput(panel, "Р”Р°С‚Р°", "yyyy-MM-dd", font, false);

            Text statusText = CreateTextBlock(panel, string.Empty, font, 22, FontStyle.Italic, new Color(0.62f, 0.18f, 0.14f), 52f);

            RectTransform buttons = CreateRect("Buttons", panel);
            AddLayout(buttons.gameObject, 0f, 76f, 0f);
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 16f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = true;

            Button submitButton = CreateButton(buttons, "РЎРѕР·РґР°С‚СЊ", font, new Color(0.15f, 0.55f, 0.35f), 0f);
            Button cancelButton = CreateButton(buttons, "РћС‚РјРµРЅР°", font, new Color(0.42f, 0.46f, 0.52f), 0f);

            overlay.SetActive(false);

            return new AddPanelRefs
            {
                Panel = overlay,
                Content = panel,
                TitleInput = titleInput,
                DescriptionInput = descriptionInput,
                DateInput = dateInput,
                StatusText = statusText,
                SubmitButton = submitButton,
                CancelButton = cancelButton
            };
        }

        private static DetailsPanelRefs CreateDetailsPanel(Transform parent, Font font)
        {
            GameObject overlay = CreateOverlay("EventDetailsPanel", parent);
            RectTransform panel = CreateModalPanel("EventDetailsContent", overlay.transform, new Vector2(820f, 760f));

            Text titleText = CreateTextBlock(panel, "РќР°Р·РІР°РЅРёРµ СЃРѕР±С‹С‚РёСЏ", font, 38, FontStyle.Bold, new Color(0.08f, 0.1f, 0.14f), 86f);
            Text dateText = CreateTextBlock(panel, "Р”Р°С‚Р°:", font, 28, FontStyle.Bold, new Color(0.16f, 0.25f, 0.35f), 60f);
            Text createdByText = CreateTextBlock(panel, "РЎРѕР·РґР°Р»:", font, 26, FontStyle.Normal, new Color(0.24f, 0.3f, 0.38f), 58f);
            Text descriptionText = CreateTextBlock(panel, "РћРїРёСЃР°РЅРёРµ", font, 26, FontStyle.Normal, new Color(0.12f, 0.14f, 0.18f), 360f);
            descriptionText.alignment = TextAnchor.UpperLeft;

            Button closeButton = CreateButton(panel, "Р—Р°РєСЂС‹С‚СЊ", font, new Color(0.16f, 0.42f, 0.72f), 76f);

            overlay.SetActive(false);

            return new DetailsPanelRefs
            {
                Panel = overlay,
                Content = panel,
                TitleText = titleText,
                DescriptionText = descriptionText,
                DateText = dateText,
                CreatedByText = createdByText,
                CloseButton = closeButton
            };
        }

        private static GameObject CreateOverlay(string name, Transform parent)
        {
            RectTransform overlay = CreateRect(name, parent);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;

            Image image = overlay.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.42f);

            return overlay.gameObject;
        }

        private static RectTransform CreateModalPanel(string name, Transform parent, Vector2 size)
        {
            RectTransform panel = CreateRect(name, parent);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = size;
            panel.anchoredPosition = Vector2.zero;

            Image image = panel.gameObject.AddComponent<Image>();
            image.color = new Color(0.97f, 0.98f, 1f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(42, 42, 42, 42);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return panel;
        }

        private static Text CreateTextBlock(RectTransform parent, string value, Font font, int size, FontStyle style, Color color, float height)
        {
            Text text = CreateText(parent, value, font, size, style, color, TextAnchor.MiddleLeft);
            AddLayout(text.gameObject, 0f, height, 0f);
            return text;
        }

        private static InputField CreateInput(RectTransform parent, string label, string placeholder, Font font, bool multiline)
        {
            RectTransform group = CreateRect(label + " Group", parent);
            AddLayout(group.gameObject, 0f, multiline ? 190f : 132f, 0f);

            VerticalLayoutGroup layout = group.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateTextBlock(group, label, font, 22, FontStyle.Bold, new Color(0.16f, 0.2f, 0.26f), 34f);

            RectTransform inputRect = CreateRect(label + " Input", group);
            AddLayout(inputRect.gameObject, 0f, multiline ? 130f : 78f, 0f);

            Image image = inputRect.gameObject.AddComponent<Image>();
            image.color = Color.white;

            InputField input = inputRect.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;

            Text text = CreateInputText(inputRect, "Text", font, string.Empty, new Color(0.1f, 0.12f, 0.16f), multiline);
            Text placeholderText = CreateInputText(inputRect, "Placeholder", font, placeholder, new Color(0.52f, 0.56f, 0.62f), multiline);
            placeholderText.fontStyle = FontStyle.Italic;

            input.textComponent = text;
            input.placeholder = placeholderText;

            return input;
        }

        private static Text CreateInputText(RectTransform parent, string name, Font font, string value, Color color, bool multiline)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(18f, 8f);
            rect.offsetMax = new Vector2(-18f, -8f);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 24;
            text.color = color;
            text.alignment = multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;

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

            Text text = CreateText(rect, caption, font, 27, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
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
            text.verticalOverflow = VerticalWrapMode.Overflow;
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

        private static void AddLayout(
            GameObject go,
            float preferredWidth,
            float preferredHeight,
            float flexibleWidth,
            float flexibleHeight = 0f)
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
            CalendarScreen screen,
            Text monthTitleText,
            Text statusText,
            Button previousMonthButton,
            Button nextMonthButton,
            Button backButton,
            Button addEventButton,
            List<CalendarDayCell> dayCells,
            GameObject eventBadgePrefab,
            AddPanelRefs addPanelRefs,
            DetailsPanelRefs detailsPanelRefs)
        {
            SerializedObject serializedScreen = new SerializedObject(screen);
            SetReference(serializedScreen, "monthTitleText", monthTitleText);
            SetReference(serializedScreen, "statusText", statusText);
            SetReference(serializedScreen, "previousMonthButton", previousMonthButton);
            SetReference(serializedScreen, "nextMonthButton", nextMonthButton);
            SetReference(serializedScreen, "backButton", backButton);
            SetReference(serializedScreen, "addEventButton", addEventButton);
            SetReference(serializedScreen, "eventBadgePrefab", eventBadgePrefab);
            SetReference(serializedScreen, "addEventPanel", addPanelRefs.Panel);
            SetReference(serializedScreen, "titleInput", addPanelRefs.TitleInput);
            SetReference(serializedScreen, "descriptionInput", addPanelRefs.DescriptionInput);
            SetReference(serializedScreen, "dateInput", addPanelRefs.DateInput);
            SetReference(serializedScreen, "addEventStatusText", addPanelRefs.StatusText);
            SetReference(serializedScreen, "submitEventButton", addPanelRefs.SubmitButton);
            SetReference(serializedScreen, "cancelAddEventButton", addPanelRefs.CancelButton);
            SetReference(serializedScreen, "detailsPanel", detailsPanelRefs.Panel);
            SetReference(serializedScreen, "detailsTitleText", detailsPanelRefs.TitleText);
            SetReference(serializedScreen, "detailsDescriptionText", detailsPanelRefs.DescriptionText);
            SetReference(serializedScreen, "detailsDateText", detailsPanelRefs.DateText);
            SetReference(serializedScreen, "detailsCreatedByText", detailsPanelRefs.CreatedByText);
            SetReference(serializedScreen, "closeDetailsButton", detailsPanelRefs.CloseButton);
            SetString(serializedScreen, "mainMenuSceneName", "MainMenuScene");

            SerializedProperty dayCellsProperty = serializedScreen.FindProperty("dayCells");
            dayCellsProperty.arraySize = dayCells.Count;
            for (int i = 0; i < dayCells.Count; i++)
                dayCellsProperty.GetArrayElementAtIndex(i).objectReferenceValue = dayCells[i];

            serializedScreen.ApplyModifiedProperties();
        }

        private static void AssignResponsiveReferences(
            CalendarResponsiveLayout responsiveLayout,
            RectTransform root,
            RectTransform header,
            RectTransform status,
            RectTransform weekHeader,
            RectTransform calendarGrid,
            RectTransform addEventContent,
            RectTransform detailsContent)
        {
            SerializedObject serializedLayout = new SerializedObject(responsiveLayout);
            SetReference(serializedLayout, "root", root);
            SetReference(serializedLayout, "header", header);
            SetReference(serializedLayout, "status", status);
            SetReference(serializedLayout, "weekHeader", weekHeader);
            SetReference(serializedLayout, "calendarGrid", calendarGrid);
            SetReference(serializedLayout, "addEventContent", addEventContent);
            SetReference(serializedLayout, "detailsContent", detailsContent);
            SetReference(serializedLayout, "headerLayout", header.GetComponent<HorizontalLayoutGroup>());
            SetReference(serializedLayout, "weekHeaderGrid", weekHeader.GetComponent<GridLayoutGroup>());
            SetReference(serializedLayout, "calendarGridLayout", calendarGrid.GetComponent<GridLayoutGroup>());
            SetReference(serializedLayout, "headerElement", header.GetComponent<LayoutElement>());
            SetReference(serializedLayout, "statusElement", status.GetComponent<LayoutElement>());
            SetReference(serializedLayout, "weekHeaderElement", weekHeader.GetComponent<LayoutElement>());
            SetReference(serializedLayout, "calendarGridElement", calendarGrid.GetComponent<LayoutElement>());
            serializedLayout.ApplyModifiedProperties();
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

        private static void SetUiLayerRecursive(GameObject go)
        {
            SetUiLayer(go);
            foreach (Transform child in go.transform)
                SetUiLayerRecursive(child.gameObject);
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
            public RectTransform Content;
            public InputField TitleInput;
            public InputField DescriptionInput;
            public InputField DateInput;
            public Text StatusText;
            public Button SubmitButton;
            public Button CancelButton;
        }

        private struct DetailsPanelRefs
        {
            public GameObject Panel;
            public RectTransform Content;
            public Text TitleText;
            public Text DescriptionText;
            public Text DateText;
            public Text CreatedByText;
            public Button CloseButton;
        }
    }
}
