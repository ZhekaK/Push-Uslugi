using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.EditorTools
{
    public class CanvasUiScaleRestorer : EditorWindow
    {
        private Canvas targetCanvas;
        private Vector2 originalReferenceResolution = new Vector2(1920f, 1080f);
        private bool scaleRectTransforms = true;
        private bool scaleLayoutComponents = true;
        private bool scaleTextComponents = true;
        private bool includeInactive = true;

        [MenuItem("Tools/Push Uslugi/UI/Canvas Scale Restorer")]
        public static void Open()
        {
            CanvasUiScaleRestorer window = GetWindow<CanvasUiScaleRestorer>("Canvas Scale Restorer");
            window.minSize = new Vector2(460f, 360f);
            window.FindCanvasFromSelectionOrScene();
            window.Show();
        }

        [MenuItem("Tools/Push Uslugi/UI/Restore Selected Canvas Scale From 1920x1080")]
        public static void RestoreSelectedCanvasFromDefault()
        {
            Canvas canvas = FindCanvasFromSelection();
            if (canvas == null)
                canvas = Object.FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                EditorUtility.DisplayDialog(
                    "Canvas Scale Restorer",
                    "Canvas was not found in the current scene.",
                    "OK");
                return;
            }

            ApplyScaleCompensation(
                canvas,
                new Vector2(1920f, 1080f),
                scaleRectTransforms: true,
                scaleLayoutComponents: true,
                scaleTextComponents: true,
                includeInactive: true);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Restore UI sizes after Canvas Scaler reference changes", EditorStyles.boldLabel);
            EditorGUILayout.Space(8f);

            targetCanvas = (Canvas)EditorGUILayout.ObjectField(
                "Target Canvas",
                targetCanvas,
                typeof(Canvas),
                true);

            using (new EditorGUILayout.HorizontalScope())
            {
                originalReferenceResolution = EditorGUILayout.Vector2Field(
                    "Original Reference",
                    originalReferenceResolution);

                if (GUILayout.Button("Use 1920x1080", GUILayout.Width(116f)))
                    originalReferenceResolution = new Vector2(1920f, 1080f);
            }

            CanvasScaler scaler = GetCanvasScaler(targetCanvas);

            if (targetCanvas != null && scaler == null)
            {
                EditorGUILayout.HelpBox(
                    "Target Canvas has no CanvasScaler component. Add CanvasScaler first.",
                    MessageType.Warning);
            }

            if (scaler != null)
            {
                EditorGUILayout.Vector2Field("Current Reference", scaler.referenceResolution);
                EditorGUILayout.LabelField("Screen Match Mode", scaler.screenMatchMode.ToString());
                EditorGUILayout.LabelField("Compensation Factor", GetCompensationFactor(scaler, originalReferenceResolution).ToString("0.###"));
            }

            EditorGUILayout.Space(8f);
            scaleRectTransforms = EditorGUILayout.Toggle("Scale RectTransforms", scaleRectTransforms);
            scaleLayoutComponents = EditorGUILayout.Toggle("Scale Layout Values", scaleLayoutComponents);
            scaleTextComponents = EditorGUILayout.Toggle("Scale Text Sizes", scaleTextComponents);
            includeInactive = EditorGUILayout.Toggle("Include Inactive Objects", includeInactive);

            EditorGUILayout.Space(12f);

            using (new EditorGUI.DisabledScope(targetCanvas == null || scaler == null))
            {
                if (GUILayout.Button("Apply Compensation To Canvas Children", GUILayout.Height(38f)))
                {
                    ApplyScaleCompensation(
                        targetCanvas,
                        originalReferenceResolution,
                        scaleRectTransforms,
                        scaleLayoutComponents,
                        scaleTextComponents,
                        includeInactive);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Canvas"))
                    targetCanvas = FindCanvasFromSelection();

                if (GUILayout.Button("Find First Canvas"))
                    targetCanvas = Object.FindFirstObjectByType<Canvas>();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "Use this after changing Canvas Scaler Reference Resolution. The tool scales every UI object inside the selected Canvas so the visual size returns close to how it looked at the original reference resolution. The operation supports normal Unity Undo.",
                MessageType.Info);
        }

        private void FindCanvasFromSelectionOrScene()
        {
            targetCanvas = FindCanvasFromSelection();

            if (targetCanvas == null)
                targetCanvas = Object.FindFirstObjectByType<Canvas>();
        }

        private static Canvas FindCanvasFromSelection()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
                return null;

            Canvas canvas = selected.GetComponent<Canvas>();
            if (canvas != null)
                return canvas;

            CanvasScaler scaler = selected.GetComponent<CanvasScaler>();
            if (scaler != null)
                return scaler.GetComponent<Canvas>();

            return selected.GetComponentInParent<Canvas>();
        }

        private static CanvasScaler GetCanvasScaler(Canvas canvas)
        {
            return canvas == null ? null : canvas.GetComponent<CanvasScaler>();
        }

        private static void ApplyScaleCompensation(
            Canvas canvas,
            Vector2 originalReference,
            bool scaleRectTransforms,
            bool scaleLayoutComponents,
            bool scaleTextComponents,
            bool includeInactive)
        {
            CanvasScaler scaler = GetCanvasScaler(canvas);
            if (canvas == null || scaler == null)
                return;

            float factor = GetCompensationFactor(scaler, originalReference);

            if (Mathf.Approximately(factor, 1f))
            {
                EditorUtility.DisplayDialog(
                    "Canvas Scale Restorer",
                    "Compensation factor is 1. Nothing to change.",
                    "OK");
                return;
            }

            bool apply = EditorUtility.DisplayDialog(
                "Canvas Scale Restorer",
                $"Apply UI size compensation x{factor:0.###} to all children of '{canvas.gameObject.name}'?",
                "Apply",
                "Cancel");

            if (!apply)
                return;

            Undo.SetCurrentGroupName("Restore Canvas UI Scale");
            int undoGroup = Undo.GetCurrentGroup();

            RectTransform root = canvas.transform as RectTransform;

            if (scaleRectTransforms && root != null)
                ScaleRectTransforms(root, factor, includeInactive);

            if (scaleLayoutComponents)
                ScaleLayoutComponents(canvas.gameObject, factor, includeInactive);

            if (scaleTextComponents)
                ScaleTextComponents(canvas.gameObject, factor, includeInactive);

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        private static float GetCompensationFactor(CanvasScaler scaler, Vector2 originalReference)
        {
            originalReference.x = Mathf.Max(1f, originalReference.x);
            originalReference.y = Mathf.Max(1f, originalReference.y);

            Vector2 currentReference = scaler.referenceResolution;
            currentReference.x = Mathf.Max(1f, currentReference.x);
            currentReference.y = Mathf.Max(1f, currentReference.y);

            float originalScale = CalculateScaleFactor(
                originalReference,
                originalReference,
                scaler.screenMatchMode,
                scaler.matchWidthOrHeight);

            float currentScale = CalculateScaleFactor(
                originalReference,
                currentReference,
                scaler.screenMatchMode,
                scaler.matchWidthOrHeight);

            if (Mathf.Approximately(currentScale, 0f))
                return 1f;

            return originalScale / currentScale;
        }

        private static float CalculateScaleFactor(
            Vector2 screenSize,
            Vector2 referenceResolution,
            CanvasScaler.ScreenMatchMode screenMatchMode,
            float matchWidthOrHeight)
        {
            float widthScale = screenSize.x / referenceResolution.x;
            float heightScale = screenSize.y / referenceResolution.y;

            switch (screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    float logWidth = Mathf.Log(widthScale, 2f);
                    float logHeight = Mathf.Log(heightScale, 2f);
                    return Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight, matchWidthOrHeight));

                case CanvasScaler.ScreenMatchMode.Expand:
                    return Mathf.Min(widthScale, heightScale);

                case CanvasScaler.ScreenMatchMode.Shrink:
                    return Mathf.Max(widthScale, heightScale);

                default:
                    return 1f;
            }
        }

        private static void ScaleRectTransforms(RectTransform root, float factor, bool includeInactive)
        {
            RectTransform[] rectTransforms = root.GetComponentsInChildren<RectTransform>(includeInactive);

            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform rect = rectTransforms[i];
                if (rect == root)
                    continue;

                Undo.RecordObject(rect, "Scale RectTransform");
                ScaleRectTransform(rect, factor);
            }
        }

        private static void ScaleRectTransform(RectTransform rect, float factor)
        {
            bool stretchX = !Mathf.Approximately(rect.anchorMin.x, rect.anchorMax.x);
            bool stretchY = !Mathf.Approximately(rect.anchorMin.y, rect.anchorMax.y);

            Vector2 anchoredPosition = rect.anchoredPosition;
            Vector2 sizeDelta = rect.sizeDelta;

            if (!stretchX)
            {
                anchoredPosition.x *= factor;
                sizeDelta.x *= factor;
            }

            if (!stretchY)
            {
                anchoredPosition.y *= factor;
                sizeDelta.y *= factor;
            }

            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            if (stretchX || stretchY)
            {
                Vector2 offsetMin = rect.offsetMin;
                Vector2 offsetMax = rect.offsetMax;

                if (stretchX)
                {
                    offsetMin.x *= factor;
                    offsetMax.x *= factor;
                }

                if (stretchY)
                {
                    offsetMin.y *= factor;
                    offsetMax.y *= factor;
                }

                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
            }
        }

        private static void ScaleLayoutComponents(GameObject root, float factor, bool includeInactive)
        {
            GridLayoutGroup[] grids = root.GetComponentsInChildren<GridLayoutGroup>(includeInactive);
            for (int i = 0; i < grids.Length; i++)
            {
                Undo.RecordObject(grids[i], "Scale GridLayoutGroup");
                grids[i].cellSize *= factor;
                grids[i].spacing *= factor;
                grids[i].padding = ScalePadding(grids[i].padding, factor);
            }

            HorizontalOrVerticalLayoutGroup[] layoutGroups = root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(includeInactive);
            for (int i = 0; i < layoutGroups.Length; i++)
            {
                Undo.RecordObject(layoutGroups[i], "Scale LayoutGroup");
                layoutGroups[i].spacing *= factor;
                layoutGroups[i].padding = ScalePadding(layoutGroups[i].padding, factor);
            }

            LayoutElement[] layoutElements = root.GetComponentsInChildren<LayoutElement>(includeInactive);
            for (int i = 0; i < layoutElements.Length; i++)
            {
                Undo.RecordObject(layoutElements[i], "Scale LayoutElement");
                layoutElements[i].minWidth = ScalePositive(layoutElements[i].minWidth, factor);
                layoutElements[i].minHeight = ScalePositive(layoutElements[i].minHeight, factor);
                layoutElements[i].preferredWidth = ScalePositive(layoutElements[i].preferredWidth, factor);
                layoutElements[i].preferredHeight = ScalePositive(layoutElements[i].preferredHeight, factor);
            }
        }

        private static void ScaleTextComponents(GameObject root, float factor, bool includeInactive)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(includeInactive);

            for (int i = 0; i < texts.Length; i++)
            {
                Undo.RecordObject(texts[i], "Scale Text");
                texts[i].fontSize = Mathf.Max(1, Mathf.RoundToInt(texts[i].fontSize * factor));

                if (texts[i].resizeTextForBestFit)
                {
                    texts[i].resizeTextMinSize = Mathf.Max(1, Mathf.RoundToInt(texts[i].resizeTextMinSize * factor));
                    texts[i].resizeTextMaxSize = Mathf.Max(1, Mathf.RoundToInt(texts[i].resizeTextMaxSize * factor));
                }
            }
        }

        private static RectOffset ScalePadding(RectOffset source, float factor)
        {
            return new RectOffset(
                Mathf.RoundToInt(source.left * factor),
                Mathf.RoundToInt(source.right * factor),
                Mathf.RoundToInt(source.top * factor),
                Mathf.RoundToInt(source.bottom * factor));
        }

        private static float ScalePositive(float value, float factor)
        {
            return value >= 0f ? value * factor : value;
        }
    }
}
