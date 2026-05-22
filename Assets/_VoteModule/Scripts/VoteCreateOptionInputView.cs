using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.VoteModule
{
    public class VoteCreateOptionInputView : MonoBehaviour
    {
        [SerializeField] private InputField input;
        [SerializeField] private Button removeButton;

        private VoteScreen owner;

        public InputField Input => input;

        public void Setup(VoteScreen voteScreen, string placeholder)
        {
            owner = voteScreen;

            if (input != null)
            {
                input.text = string.Empty;

                Text placeholderText = input.placeholder as Text;

                if (placeholderText != null)
                    placeholderText.text = placeholder;
            }

            if (removeButton != null)
            {
                removeButton.onClick.RemoveListener(Remove);
                removeButton.onClick.AddListener(Remove);
            }
        }

        public void SetRemoveVisible(bool visible)
        {
            if (removeButton != null)
                removeButton.gameObject.SetActive(visible);
        }

        private void Remove()
        {
            if (owner != null)
                owner.RemoveCreateOptionInput(this);
        }
    }
}
