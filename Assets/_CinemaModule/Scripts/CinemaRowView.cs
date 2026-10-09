using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.CinemaModule
{
    public class CinemaRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text ratingText;
        [SerializeField] private Text watchedAtText;
        [SerializeField] private Button linkButton;

        private string url;
        private static readonly Color ExtraColor = new Color(0.89f, 0.83f, 0.98f, 1f);

        private void Awake()
        {
            if (linkButton != null)
                linkButton.onClick.AddListener(OpenUrl);
        }

        private void OnDestroy()
        {
            if (linkButton != null)
                linkButton.onClick.RemoveListener(OpenUrl);
        }

        public void Setup(CinemaMovieDto movie)
        {
            url = movie != null ? movie.url : string.Empty;

            Image background = GetComponent<Image>();
            if (background != null && movie != null && movie.isExtra)
                background.color = ExtraColor;
            if (movie != null && movie.isExtra)
            {
                ColorCell(titleText);
                ColorCell(ratingText);
                ColorCell(watchedAtText);
            }

            if (titleText != null)
                titleText.text = string.IsNullOrWhiteSpace(movie?.title) ? "-" : movie.title;

            if (ratingText != null)
                ratingText.text = movie == null ? "-" : movie.rating.ToString("0.##");

            if (watchedAtText != null)
                watchedAtText.text = string.IsNullOrWhiteSpace(movie?.watchedAt) ? "-" : movie.watchedAt;

            if (linkButton != null)
                linkButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(url));
        }

        private void OpenUrl()
        {
            if (!string.IsNullOrWhiteSpace(url))
                Application.OpenURL(url);
        }

        private static void ColorCell(Text cellText)
        {
            Image cellBackground = cellText != null && cellText.transform.parent != null
                ? cellText.transform.parent.GetComponent<Image>()
                : null;
            if (cellBackground != null)
                cellBackground.color = ExtraColor;
        }
    }
}
