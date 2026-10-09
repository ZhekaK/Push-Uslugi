using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.Messenger
{
    public class MessengerMessageRowView : MonoBehaviour
    {
        [SerializeField] private Text headerText;
        [SerializeField] private Text bodyText;

        public void SetText(string header, string body)
        {
            if (headerText != null)
                headerText.text = header;
            if (bodyText != null)
                bodyText.text = body;
        }

        public void SetReferences(Text header, Text body)
        {
            headerText = header;
            bodyText = body;
        }
    }
}
