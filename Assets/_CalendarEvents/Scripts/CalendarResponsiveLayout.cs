using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.CalendarEvents
{
    public class CalendarResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private bool autoApply;
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform header;
        [SerializeField] private RectTransform status;
        [SerializeField] private RectTransform weekHeader;
        [SerializeField] private RectTransform calendarGrid;
        [SerializeField] private RectTransform addEventContent;
        [SerializeField] private RectTransform detailsContent;
        [SerializeField] private HorizontalLayoutGroup headerLayout;
        [SerializeField] private GridLayoutGroup weekHeaderGrid;
        [SerializeField] private GridLayoutGroup calendarGridLayout;
        [SerializeField] private LayoutElement headerElement;
        [SerializeField] private LayoutElement statusElement;
        [SerializeField] private LayoutElement weekHeaderElement;
        [SerializeField] private LayoutElement calendarGridElement;

        private Vector2 lastRootSize;

        private void Start()
        {
            if (autoApply)
                Apply();
        }

        private void Update()
        {
            if (!autoApply)
                return;

            if (root == null)
                return;

            Vector2 size = root.rect.size;
            if ((size - lastRootSize).sqrMagnitude > 0.5f)
                Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (autoApply)
                Apply();
        }

        public void Apply()
        {
            if (root == null)
                return;

            Vector2 size = root.rect.size;
            lastRootSize = size;

            bool landscape = size.x > size.y * 1.15f;
            float outerPadding = landscape ? 22f : 32f;
            float spacing = landscape ? 10f : 14f;
            float headerHeight = landscape ? 70f : 92f;
            float statusHeight = landscape ? 30f : 42f;
            float weekHeight = landscape ? 38f : 50f;
            float gridSpacing = landscape ? 6f : 8f;

            root.offsetMin = new Vector2(outerPadding, outerPadding);
            root.offsetMax = new Vector2(-outerPadding, -outerPadding);

            VerticalLayoutGroup rootLayout = root.GetComponent<VerticalLayoutGroup>();
            if (rootLayout != null)
            {
                rootLayout.spacing = spacing;
                rootLayout.childControlHeight = true;
                rootLayout.childForceExpandHeight = false;
            }

            if (headerElement != null)
                headerElement.preferredHeight = headerHeight;

            if (statusElement != null)
                statusElement.preferredHeight = statusHeight;

            if (weekHeaderElement != null)
                weekHeaderElement.preferredHeight = weekHeight;

            if (calendarGridElement != null)
            {
                calendarGridElement.preferredHeight = 0f;
                calendarGridElement.flexibleHeight = 1f;
            }

            if (headerLayout != null)
                headerLayout.spacing = landscape ? 12f : 10f;

            ApplyModalSize(addEventContent, size, landscape ? 0.86f : 0.92f, landscape ? 0.88f : 0.72f);
            ApplyModalSize(detailsContent, size, landscape ? 0.78f : 0.88f, landscape ? 0.76f : 0.54f);

            Canvas.ForceUpdateCanvases();
            ApplyGrid(weekHeader, weekHeaderGrid, 7, 1, gridSpacing, weekHeight);
            ApplyGrid(calendarGrid, calendarGridLayout, 7, 6, gridSpacing, 0f);
        }

        private static void ApplyGrid(
            RectTransform rectTransform,
            GridLayoutGroup grid,
            int columns,
            int rows,
            float spacing,
            float fixedHeight)
        {
            if (rectTransform == null || grid == null)
                return;

            float width = Mathf.Max(1f, rectTransform.rect.width);
            float height = fixedHeight > 0f ? fixedHeight : Mathf.Max(1f, rectTransform.rect.height);

            grid.spacing = new Vector2(spacing, spacing);
            grid.cellSize = new Vector2(
                Mathf.Max(1f, (width - spacing * (columns - 1)) / columns),
                Mathf.Max(1f, (height - spacing * (rows - 1)) / rows));
        }

        private static void ApplyModalSize(RectTransform panel, Vector2 rootSize, float widthFactor, float heightFactor)
        {
            if (panel == null)
                return;

            panel.sizeDelta = new Vector2(
                Mathf.Clamp(rootSize.x * widthFactor, 540f, 920f),
                Mathf.Clamp(rootSize.y * heightFactor, 520f, 980f));
        }
    }
}
