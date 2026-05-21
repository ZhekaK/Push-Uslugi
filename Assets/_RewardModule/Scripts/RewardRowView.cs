using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.RewardModule
{
    public class RewardRowView : MonoBehaviour
    {
        [SerializeField] private Text firstColumnText;
        [SerializeField] private Text secondColumnText;
        [SerializeField] private Text thirdColumnText;

        public void Setup(string firstColumn, string secondColumn, string thirdColumn)
        {
            if (firstColumnText != null)
                firstColumnText.text = string.IsNullOrWhiteSpace(firstColumn) ? "-" : firstColumn;

            if (secondColumnText != null)
                secondColumnText.text = string.IsNullOrWhiteSpace(secondColumn) ? "-" : secondColumn;

            if (thirdColumnText != null)
                thirdColumnText.text = string.IsNullOrWhiteSpace(thirdColumn) ? "-" : thirdColumn;
        }
    }
}
