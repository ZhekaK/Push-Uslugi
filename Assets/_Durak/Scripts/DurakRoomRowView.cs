using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.Durak
{
    public class DurakRoomRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text metaText;
        [SerializeField] private Button button;

        private DurakScreen owner;
        private int roomId;

        public void Setup(DurakScreen screen, DurakRoomListItemDto room)
        {
            owner = screen;
            roomId = room != null ? room.id : 0;

            if (titleText != null)
                titleText.text = string.IsNullOrWhiteSpace(room != null ? room.name : null) ? "Комната" : room.name;

            if (metaText != null && room != null)
            {
                string lockText = room.hasPassword ? "пароль" : "без пароля";
                metaText.text = $"{room.playerCount}/{room.maxPlayers} игроков | {room.cardCount} карт | {room.status} | {lockText}";
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
