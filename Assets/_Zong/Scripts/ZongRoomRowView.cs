using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.Zong
{
    public class ZongRoomRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text metaText;
        [SerializeField] private Button button;

        private int roomId;
        private ZongScreen owner;

        public void Setup(ZongScreen screen, ZongRoomListItemDto room)
        {
            owner = screen;
            roomId = room != null ? room.id : 0;

            if (titleText != null)
                titleText.text = string.IsNullOrWhiteSpace(room != null ? room.name : null) ? "Комната" : room.name;

            if (metaText != null && room != null)
            {
                string lockText = room.hasPassword ? "пароль" : "без пароля";
                metaText.text = $"{room.playerCount}/{room.maxPlayers} игроков | цель {room.targetScore} | {room.status} | {lockText}";
            }

            if (button != null)
            {
                button.onClick.RemoveListener(Open);
                button.onClick.AddListener(Open);
            }
        }

        private void Open()
        {
            if (owner != null && roomId > 0)
                owner.OpenJoinRoom(roomId);
        }
    }
}
