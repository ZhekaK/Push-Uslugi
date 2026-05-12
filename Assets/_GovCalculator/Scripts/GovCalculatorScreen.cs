using System.Globalization;
using PushPelmesh.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushPelmesh.GovCalculator
{
    public class GovCalculatorScreen : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputField peopleInput;
        [SerializeField] private InputField totalInput;

        [Header("Output")]
        [SerializeField] private Text regularPersonText;
        [SerializeField] private Text discountPersonText;
        [SerializeField] private Text checkText;
        [SerializeField] private Text statusText;

        [Header("Buttons")]
        [SerializeField] private Button calculateButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button backButton;

        [Header("Navigation")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Defaults")]
        [SerializeField] private bool calculateOnEdit = true;
        [SerializeField] private string defaultPeople = "5";
        [SerializeField] private string defaultTotal = "4500";

        private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

        private void Awake()
        {
            PushPelmesh.App.ScreenOrientationPolicy.AllowAnyOrientation();

            if (!HasRequiredReferences())
            {
                Debug.LogError("GovCalculatorScreen: UI references are not assigned. Use Tools/Push Uslugi/Gov Calculator/Generate UI In Current Scene.");
                enabled = false;
                return;
            }

            BindEvents();
        }

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(defaultPeople) && string.IsNullOrWhiteSpace(peopleInput.text))
                peopleInput.text = defaultPeople;

            if (!string.IsNullOrWhiteSpace(defaultTotal) && string.IsNullOrWhiteSpace(totalInput.text))
                totalInput.text = defaultTotal;

            Calculate();
        }

        private void OnDestroy()
        {
            if (calculateButton != null)
                calculateButton.onClick.RemoveListener(Calculate);

            if (clearButton != null)
                clearButton.onClick.RemoveListener(Clear);

            if (backButton != null)
                backButton.onClick.RemoveListener(BackToMainMenu);

            if (peopleInput != null)
                peopleInput.onValueChanged.RemoveListener(OnInputChanged);

            if (totalInput != null)
                totalInput.onValueChanged.RemoveListener(OnInputChanged);
        }

        public void Calculate()
        {
            if (!HasRequiredReferences())
                return;

            if (!TryReadPeople(out int people) || !TryReadTotal(out decimal total))
                return;

            decimal regularPayment = total / (people - 0.5m);
            decimal discountPayment = regularPayment / 2m;
            int regularPeopleCount = Mathf.Max(people - 1, 0);

            regularPersonText.text = $"Обычный человек: {FormatMoney(regularPayment)}";
            discountPersonText.text = $"Человек с льготой: {FormatMoney(discountPayment)}";

            if (checkText != null)
            {
                checkText.text =
                    $"Проверка: {regularPeopleCount} обычн. x {FormatMoney(regularPayment)} + " +
                    $"1 льготн. x {FormatMoney(discountPayment)} = {FormatMoney(total)}";
            }

            SetStatus("Расчёт выполнен");
        }

        public void Clear()
        {
            peopleInput.text = string.Empty;
            totalInput.text = string.Empty;
            regularPersonText.text = "Обычный человек: -";
            discountPersonText.text = "Человек с льготой: -";

            if (checkText != null)
                checkText.text = "Введите количество человек и общую сумму.";

            SetStatus(string.Empty);
        }

        public void BackToMainMenu()
        {
            if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        private void BindEvents()
        {
            calculateButton.onClick.AddListener(Calculate);
            clearButton.onClick.AddListener(Clear);
            backButton.onClick.AddListener(BackToMainMenu);

            if (calculateOnEdit)
            {
                peopleInput.onValueChanged.AddListener(OnInputChanged);
                totalInput.onValueChanged.AddListener(OnInputChanged);
            }
        }

        private void OnInputChanged(string _)
        {
            Calculate();
        }

        private bool TryReadPeople(out int people)
        {
            string input = peopleInput.text.Trim();

            if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out people))
            {
                SetError("Введите количество человек целым числом.");
                return false;
            }

            if (people < 1)
            {
                SetError("Количество человек должно быть не меньше 1.");
                return false;
            }

            return true;
        }

        private bool TryReadTotal(out decimal total)
        {
            string input = totalInput.text.Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty)
                .Replace(',', '.');

            if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out total))
            {
                SetError("Введите общую сумму числом.");
                return false;
            }

            if (total < 0m)
            {
                SetError("Общая сумма не может быть отрицательной.");
                return false;
            }

            return true;
        }

        private bool HasRequiredReferences()
        {
            return peopleInput != null &&
                   totalInput != null &&
                   regularPersonText != null &&
                   discountPersonText != null &&
                   calculateButton != null &&
                   clearButton != null &&
                   backButton != null;
        }

        private static string FormatMoney(decimal value)
        {
            return value.ToString("N2", RuCulture) + " руб.";
        }

        private void SetError(string message)
        {
            regularPersonText.text = "Обычный человек: -";
            discountPersonText.text = "Человек с льготой: -";

            if (checkText != null)
                checkText.text = "Формула: x = S / (N - 0,5), y = x / 2";

            SetStatus(message);
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
