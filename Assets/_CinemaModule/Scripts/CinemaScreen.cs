using System;
using System.Collections.Generic;
using System.Globalization;
using PushPelmesh.App;
using PushPelmesh.App.Api;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.CinemaModule
{
    public class CinemaScreen : MonoBehaviour
    {
        private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy" };

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Table")]
        [SerializeField] private Text statusText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private GameObject rowPrefab;

        [Header("Add Panel")]
        [SerializeField] private Button addButton;
        [SerializeField] private GameObject addPanel;
        [SerializeField] private InputField titleInput;
        [SerializeField] private InputField ratingInput;
        [SerializeField] private InputField watchedAtInput;
        [SerializeField] private InputField urlInput;
        [SerializeField] private Text addStatusText;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;

        private readonly List<GameObject> spawnedRows = new List<GameObject>();

        private void Awake()
        {
            ScreenOrientationPolicy.AllowAnyOrientation();

            if (backButton != null)
                backButton.onClick.AddListener(BackToMainMenu);

            if (addButton != null)
                addButton.onClick.AddListener(OpenAddPanel);

            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitMovie);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(CloseAddPanel);

            if (addPanel != null)
                addPanel.SetActive(false);

            if (rowPrefab != null)
                rowPrefab.SetActive(false);
        }

        private async void Start()
        {
            await RefreshPermissionsAsync();
            await LoadMoviesAsync();
        }

        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(BackToMainMenu);

            if (addButton != null)
                addButton.onClick.RemoveListener(OpenAddPanel);

            if (submitButton != null)
                submitButton.onClick.RemoveListener(SubmitMovie);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(CloseAddPanel);
        }

        private async System.Threading.Tasks.Task RefreshPermissionsAsync()
        {
            SetAddButtonVisible(false);

            try
            {
                UserRoleResponse roles = SessionManager.userRole;

                if (roles == null)
                {
                    roles = await AuthService.GetUserRolesAsync();
                    SessionManager.userRole = roles;
                }

                SetAddButtonVisible(HasRole(roles, "President"));
            }
            catch (Exception exception)
            {
                SetAddButtonVisible(false);
                SetStatus("Не удалось проверить права: " + exception.Message);
            }
        }

        private async System.Threading.Tasks.Task LoadMoviesAsync()
        {
            SetStatus("Загрузка фильмов...");

            try
            {
                List<CinemaMovieDto> movies = await CinemaApi.GetMoviesAsync();
                movies.Sort((left, right) => right.rating.CompareTo(left.rating));
                RenderMovies(movies);
                SetStatus(movies.Count == 0 ? "Фильмов пока нет" : "Загружено фильмов: " + movies.Count);
            }
            catch (Exception exception)
            {
                SetStatus("Ошибка загрузки фильмов: " + exception.Message);
            }
        }

        private void RenderMovies(List<CinemaMovieDto> movies)
        {
            ClearRows();

            if (movies == null)
                return;

            for (int i = 0; i < movies.Count; i++)
            {
                CinemaRowView row = CreateRow();

                if (row != null)
                    row.Setup(movies[i]);
            }
        }

        private CinemaRowView CreateRow()
        {
            if (rowPrefab == null || rowsRoot == null)
                return null;

            GameObject rowObject = Instantiate(rowPrefab, rowsRoot);
            rowObject.SetActive(true);
            spawnedRows.Add(rowObject);
            return rowObject.GetComponent<CinemaRowView>();
        }

        private void ClearRows()
        {
            for (int i = 0; i < spawnedRows.Count; i++)
            {
                if (spawnedRows[i] != null)
                    Destroy(spawnedRows[i]);
            }

            spawnedRows.Clear();
        }

        private void OpenAddPanel()
        {
            if (titleInput != null)
                titleInput.text = string.Empty;

            if (ratingInput != null)
                ratingInput.text = string.Empty;

            if (watchedAtInput != null)
                watchedAtInput.text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (urlInput != null)
                urlInput.text = string.Empty;

            if (addStatusText != null)
                addStatusText.text = string.Empty;

            if (addPanel != null)
                addPanel.SetActive(true);
        }

        private void CloseAddPanel()
        {
            if (addPanel != null)
                addPanel.SetActive(false);
        }

        private async void SubmitMovie()
        {
            string title = titleInput != null ? titleInput.text.Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                SetAddStatus("Введите название фильма");
                return;
            }

            string ratingText = ratingInput != null ? ratingInput.text.Trim().Replace(',', '.') : string.Empty;

            if (!float.TryParse(ratingText, NumberStyles.Float, CultureInfo.InvariantCulture, out float rating) || rating < 0f || rating > 10f)
            {
                SetAddStatus("Рейтинг должен быть числом от 0 до 10");
                return;
            }

            string watchedAtText = watchedAtInput != null ? watchedAtInput.text.Trim() : string.Empty;

            if (!TryParseDate(watchedAtText, out DateTime watchedAt))
            {
                SetAddStatus("Дата должна быть в формате yyyy-MM-dd");
                return;
            }

            CreateCinemaMovieRequest request = new CreateCinemaMovieRequest
            {
                Title = title,
                Rating = rating,
                WatchedAt = watchedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Url = urlInput != null ? urlInput.text.Trim() : string.Empty
            };

            SetSubmitInteractable(false);
            SetAddStatus("Отправка...");

            try
            {
                await CinemaApi.CreateMovieAsync(request);
                CloseAddPanel();
                await LoadMoviesAsync();
            }
            catch (ApiException exception)
            {
                SetAddStatus(exception.StatusCode == 403 ? "Добавлять фильмы может только Президент" : "Ошибка сервера: " + exception.StatusCode);
            }
            catch (Exception exception)
            {
                SetAddStatus("Ошибка добавления фильма: " + exception.Message);
            }
            finally
            {
                SetSubmitInteractable(true);
            }
        }

        private static bool TryParseDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private static bool HasRole(UserRoleResponse roles, string roleType)
        {
            if (roles == null || roles.roles == null)
                return false;

            for (int i = 0; i < roles.roles.Count; i++)
            {
                UserRoleResponse.UserRoleDTO role = roles.roles[i];

                if (role != null && string.Equals(role.roleType, roleType, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void SetAddButtonVisible(bool visible)
        {
            if (addButton != null)
                addButton.gameObject.SetActive(visible);
        }

        private void SetSubmitInteractable(bool interactable)
        {
            if (submitButton != null)
                submitButton.interactable = interactable;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void SetAddStatus(string message)
        {
            if (addStatusText != null)
                addStatusText.text = message;
        }

        private void BackToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
