using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.VoteModule
{
    public class VotePollListItemView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text metaText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button button;

        private int pollId;
        private VoteScreen owner;

        public void Setup(VoteScreen voteScreen, VotePollDto poll)
        {
            owner = voteScreen;
            pollId = poll != null ? poll.id : 0;

            if (titleText != null)
                titleText.text = string.IsNullOrWhiteSpace(poll != null ? poll.title : null) ? "Без названия" : poll.title;

            if (metaText != null)
                metaText.text = poll == null ? string.Empty : "До " + VoteScreen.FormatDate(poll.endDate);

            if (statusText != null)
            {
                if (poll == null)
                    statusText.text = string.Empty;
                else if (poll.isClosed)
                    statusText.text = "Завершено";
                else if (poll.hasVoted)
                    statusText.text = "Голос принят";
                else
                    statusText.text = "Открыто";
            }

            if (button != null)
            {
                button.onClick.RemoveListener(Open);
                button.onClick.AddListener(Open);
            }
        }

        private void Open()
        {
            if (owner != null && pollId > 0)
                owner.OpenPoll(pollId);
        }
    }
}
