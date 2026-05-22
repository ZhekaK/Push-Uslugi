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

        public void Setup(VoteScreen voteScreen, VoteOptionDto option, bool canVote)
        {
            owner = voteScreen;
            optionId = option != null ? option.id : 0;

            if (titleText != null)
            {
                string selectedPrefix = option != null && option.isSelected ? "✓ " : string.Empty;
                titleText.text = selectedPrefix + (string.IsNullOrWhiteSpace(option != null ? option.text : null) ? "Вариант" : option.text);
            }

            if (resultText != null)
                resultText.text = option == null ? string.Empty : $"{option.votes} голосов • {option.percent:0.#}%";

            if (fillImage != null)
                fillImage.fillAmount = option == null ? 0f : Mathf.Clamp01(option.percent / 100f);

            if (voteButton != null)
            {
                voteButton.onClick.RemoveListener(Vote);
                voteButton.interactable = canVote && optionId > 0;
                voteButton.onClick.AddListener(Vote);
            }
        }

        private void Vote()
        {
            if (owner != null && optionId > 0)
                owner.VoteForOption(optionId);
        }
    }
}
