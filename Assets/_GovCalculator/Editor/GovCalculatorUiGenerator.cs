using PushPelmesh.GovCalculator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.GovCalculator.EditorTools
{
    public static class GovCalculatorUiGenerator
    {
        private const string CanvasName = "GovCalculatorCanvas";
        private const string ControllerName = "GovCalculatorController";

        [MenuItem("Tools/Push Uslugi/Gov Calculator/Generate UI In Current Scene")]
        public static void GenerateUiInCurrentScene()
        {
            GameObject existingCanvas = GameObject.Find(CanvasName);
            if (existingCanvas != null)
            {
                bool shouldReplace = EditorUtility.DisplayDialog(
                    "Gov Calculator UI",
                    "В текущей сцене уже есть GovCalculatorCanvas. Заменить его новым сгенерированным интерфейсом?",
                    "Заменить",
                    "Отмена");

                if (!shouldReplace)
                    return;

                Undo.DestroyObjectImmediate(existingCanvas);
            }

            GovCalculatorScreen screen = FindOrCreateScreen();
            Font font = GetDefaultFont();

            Canvas canvas = CreateCanvas();
            RectTransform content = CreateContent(canvas.transform);

            CreateText(content, "Гос-калькулятор продуктов", font, 48, FontStyle.Bold, new Color(0.09f, 0.11f, 0.13f), 92f);
            CreateText(content, "Один человек платит 50%, остальные делят общую сумму по формуле: x = S / (N - 0,5), y = x / 2.", font, 26, FontStyle.Normal, new Color(0.28f, 0.31f, 0.34f), 92f);

            InputField peopleInput = CreateInput(content, "Количество человек", "Например: 5", InputField.ContentType.IntegerNumber, font);
            InputField totalInput = CreateInput(content, "Общая сумма", "Например: 4500", InputField.ContentType.DecimalNumber, font);

            RectTransform buttonRow = CreateRect("Buttons", content);
            HorizontalLayoutGroup buttonLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 18f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = true;
            AddLayout(buttonRow.gameObject, 0f, 82f);

            Button calculateButton = CreateButton(buttonRow, "Рассчитать", font, new Color(0.13f, 0.38f, 0.72f));
            Button clearButton = CreateButton(buttonRow, "Очистить", font, new Color(0.42f, 0.45f, 0.48f));
            Button backButton = CreateButton(buttonRow, "В меню", font, new Color(0.15f, 0.55f, 0.35f));

            Text regularPersonText = CreateText(content, "Обычный человек: -", font, 34, FontStyle.Bold, new Color(0.07f, 0.2f, 0.36f), 78f);
            Text discountPersonText = CreateText(content, "Человек с льготой: -", font, 34, FontStyle.Bold, new Color(0.12f, 0.36f, 0.21f), 78f);
            Text checkText = CreateText(content, "Формула: x = S / (N - 0,5), y = x / 2", font, 24, FontStyle.Normal, new Color(0.24f, 0.27f, 0.3f), 102f);
            Text statusText = CreateText(content, string.Empty, font, 24, FontStyle.Italic, new Color(0.58f, 0.16f, 0.14f), 72f);

            EnsureEventSystem();
            AssignReferences(
                screen,
                peopleInput,
                totalInput,
                regularPersonText,
                discountPersonText,
                checkText,
                statusText,
                calculateButton,
                clearButton,
                backButton);

            Selection.activeGameObject = screen.gameObject;
            EditorUtility.SetDirty(screen);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static GovCalculatorScreen FindOrCreateScreen()
        {
            GovCalculatorScreen screen = Object.FindObjectOfType<GovCalculatorScreen>();
            if (screen != null)
                return screen;

            GameObject controller = new GameObject(ControllerName);
            Undo.RegisterCreatedObjectUndo(controller, "Create Gov Calculator Controller");
            return controller.AddComponent<GovCalculatorScreen>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Gov Calculator UI");
            SetUiLayer(canvasObject);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = canvasObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.13f);

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            return canvas;
        }

        private static RectTransform CreateContent(Transform parent)
        {
            RectTransform content = CreateRect("Content", parent);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(860f, 1160f);

            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(48, 48, 48, 48);
            contentLayout.spacing = 24f;
            contentLayout.childAlignment = TextAnchor.MiddleCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            Image panel = content.gameObject.AddComponent<Image>();
            panel.color = new Color(0.95f, 0.96f, 0.94f);

            return content;
        }

        private static InputField CreateInput(RectTransform parent, string label, string placeholder, InputField.ContentType contentType, Font font)
        {
            RectTransform group = CreateRect(label + " Group", parent);
            VerticalLayoutGroup layout = group.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            AddLayout(group.gameObject, 0f, 142f);

            CreateText(group, label, font, 26, FontStyle.Bold, new Color(0.12f, 0.14f, 0.16f), 38f);

            RectTransform fieldRect = CreateRect(label + " Input", group);
            AddLayout(fieldRect.gameObject, 0f, 84f);

            Image image = fieldRect.gameObject.AddComponent<Image>();
            image.color = Color.white;

            InputField input = fieldRect.gameObject.AddComponent<InputField>();
            input.contentType = contentType;
            input.lineType = InputField.LineType.SingleLine;
            input.targetGraphic = image;

            Text text = CreateInputText(fieldRect, "Text", font, string.Empty, new Color(0.11f, 0.13f, 0.15f));
            Text placeholderText = CreateInputText(fieldRect, "Placeholder", font, placeholder, new Color(0.56f, 0.58f, 0.6f));
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
            rect.offsetMin = new Vector2(24f, 8f);
            rect.offsetMax = new Vector2(-24f, -8f);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 30;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = value;
            text.supportRichText = false;

            return text;
        }

        private static Button CreateButton(RectTransform parent, string caption, Font font, Color color)
        {
            RectTransform rect = CreateRect(caption + " Button", parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText(rect, caption, font, 28, FontStyle.Bold, Color.white, 82f);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static Text CreateText(RectTransform parent, string value, Font font, int size, FontStyle style, Color color, float preferredHeight)
        {
            RectTransform rect = CreateRect(value + " Text", parent);
            AddLayout(rect.gameObject, 0f, preferredHeight);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

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

        private static void AddLayout(GameObject go, float preferredWidth, float preferredHeight)
        {
            LayoutElement element = go.AddComponent<LayoutElement>();
            element.preferredWidth = preferredWidth;
            element.preferredHeight = preferredHeight;
        }

        private static void AssignReferences(
            GovCalculatorScreen screen,
            InputField peopleInput,
            InputField totalInput,
            Text regularPersonText,
            Text discountPersonText,
            Text checkText,
            Text statusText,
            Button calculateButton,
            Button clearButton,
            Button backButton)
        {
            SerializedObject serializedScreen = new SerializedObject(screen);
            SetReference(serializedScreen, "peopleInput", peopleInput);
            SetReference(serializedScreen, "totalInput", totalInput);
            SetReference(serializedScreen, "regularPersonText", regularPersonText);
            SetReference(serializedScreen, "discountPersonText", discountPersonText);
            SetReference(serializedScreen, "checkText", checkText);
            SetReference(serializedScreen, "statusText", statusText);
            SetReference(serializedScreen, "calculateButton", calculateButton);
            SetReference(serializedScreen, "clearButton", clearButton);
            SetReference(serializedScreen, "backButton", backButton);
            SetString(serializedScreen, "mainMenuSceneName", "MainMenuScene");
            SetBool(serializedScreen, "calculateOnEdit", true);
            SetString(serializedScreen, "defaultPeople", "5");
            SetString(serializedScreen, "defaultTotal", "4500");
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

        private static void SetBool(SerializedObject target, string propertyName, bool value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            property.boolValue = value;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
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
