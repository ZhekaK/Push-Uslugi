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
    }
}
