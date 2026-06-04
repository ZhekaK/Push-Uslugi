using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.VoteModule
{
    public class VoteOptionView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text resultText;
        [SerializeField] private Image fillImage;
        [SerializeField] private Button voteButton;

        private int optionId;
        private VoteScreen owner;
        private bool selectionMode;
        private bool selectedForSubmit;
        private string optionTitle = string.Empty;

        public int OptionId => optionId;
        public bool SelectedForSubmit => selectedForSubmit;

        public void Setup(VoteScreen voteScreen, VoteOptionDto option, bool canVote, bool selectionMode = false)
        {
            owner = voteScreen;
            this.selectionMode = selectionMode;
            optionId = option != null ? option.id : 0;
            selectedForSubmit = option != null && option.isSelected;
            optionTitle = string.IsNullOrWhiteSpace(option != null ? option.text : null) ? "Вариант" : option.text;

            if (titleText != null)
            {
                string selectedPrefix = option != null && option.isSelected ? "[x] " : string.Empty;
                titleText.text = selectedPrefix + optionTitle;
            }

            if (resultText != null)
                resultText.text = option == null ? string.Empty : $"{option.votes} голосов - {option.percent:0.#}%";

            if (fillImage != null)
                fillImage.fillAmount = option == null ? 0f : Mathf.Clamp01(option.percent / 100f);

            if (voteButton != null)
            {
                voteButton.onClick.RemoveListener(Vote);
                voteButton.onClick.RemoveListener(ToggleSelection);
                voteButton.interactable = canVote && optionId > 0;
                voteButton.onClick.AddListener(selectionMode ? ToggleSelection : Vote);
            }

            RefreshSelectionVisual();
        }

        public void SetSelectedForSubmit(bool selected)
        {
            selectedForSubmit = selected;
            RefreshSelectionVisual();
        }

        private void Vote()
        {
            if (owner != null && optionId > 0)
                owner.VoteForOption(optionId);
        }

        private void ToggleSelection()
        {
            if (owner != null && optionId > 0)
                owner.ToggleOptionSelection(this);
        }

        private void RefreshSelectionVisual()
        {
            if (!selectionMode)
                return;

            if (titleText != null)
                titleText.text = (selectedForSubmit ? "[x] " : string.Empty) + optionTitle;

            Image background = voteButton != null ? voteButton.targetGraphic as Image : null;

            if (background != null)
                background.color = selectedForSubmit ? new Color(0.78f, 0.88f, 1f, 1f) : Color.white;
        }
    }
}
