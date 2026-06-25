using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.Zong
{
    public class ZongPlayerRowView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text metaText;

        public void Setup(ZongPlayerDto player)
        {
            if (nameText != null)
            {
                string prefix = player != null && player.isCurrentTurn ? "> " : string.Empty;
                string suffix = player != null && player.isWinner ? " [победитель]" : string.Empty;
                nameText.text = prefix + (string.IsNullOrWhiteSpace(player != null ? player.displayName : null) ? "Игрок" : player.displayName) + suffix;
            }

            if (scoreText != null)
                scoreText.text = player == null ? "0" : player.score.ToString();

            if (metaText != null)
                metaText.text = player == null ? string.Empty : $"первый бросок: {player.initialRoll}, порядок: {(player.turnOrder == 0 ? "-" : player.turnOrder.ToString())}";
        }
    }
}
