using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.RewardModule
{
    public class RewardRowView : MonoBehaviour
    {
        [SerializeField] private Text firstColumnText;
        [SerializeField] private Text secondColumnText;
        [SerializeField] private Text thirdColumnText;
        [SerializeField] private Text fourthColumnText;

        public void Setup(string firstColumn, string secondColumn, string thirdColumn, string fourthColumn)
        {
            if (firstColumnText != null)
                firstColumnText.text = string.IsNullOrWhiteSpace(firstColumn) ? "-" : firstColumn;

            if (secondColumnText != null)
                secondColumnText.text = string.IsNullOrWhiteSpace(secondColumn) ? "-" : secondColumn;

            if (thirdColumnText != null)
                thirdColumnText.text = string.IsNullOrWhiteSpace(thirdColumn) ? "-" : thirdColumn;

            if (fourthColumnText != null)
                fourthColumnText.text = string.IsNullOrWhiteSpace(fourthColumn) ? "-" : fourthColumn;
        }
    }
}
