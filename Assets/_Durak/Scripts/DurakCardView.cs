using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PushPelmesh.Durak
{
    public class DurakCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private Text labelText;
        [SerializeField] private Button button;

        private DurakScreen owner;
        private string cardCode;
        private bool selectable;
        private bool draggable;
        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Vector3 originalPosition;

        public void Setup(DurakScreen screen, DurakCardDto card, DurakCardSkinSet skinSet, bool canSelect, bool canDrag = false)
        {
            owner = screen;
            cardCode = card != null ? card.code : string.Empty;
            selectable = canSelect;
            draggable = canDrag;
            rectTransform = transform as RectTransform;
            rootCanvas = GetComponentInParent<Canvas>();
            EnsureCanvasGroup();

            if (image == null)
                image = GetComponent<Image>();

            Sprite sprite = skinSet != null ? skinSet.GetSprite(cardCode) : null;

            if (image != null)
            {
                image.sprite = sprite;
                image.color = sprite != null ? Color.white : new Color(0.98f, 0.98f, 0.95f);
                image.preserveAspect = true;
            }

            if (labelText != null)
            {
                labelText.text = sprite == null ? DurakCardText.Format(card) : string.Empty;
                labelText.enabled = sprite == null;
            }

            if (button != null)
            {
                button.interactable = selectable;
                button.onClick.RemoveListener(Select);
                button.onClick.AddListener(Select);
            }

            transform.localScale = Vector3.one * 0.92f;
            transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!draggable || rectTransform == null || rootCanvas == null)
                return;

            EnsureCanvasGroup();

            if (canvasGroup == null)
                return;

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            originalPosition = transform.position;
            canvasGroup.blocksRaycasts = false;
            Transform dragParent = owner != null && owner.DragLayer != null ? owner.DragLayer : rootCanvas.transform;
            transform.SetParent(dragParent, true);
            transform.SetAsLastSibling();
            transform.DOKill();
            transform.DOScale(Vector3.one * 1.08f, 0.12f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggable || originalParent == null)
                return;

            rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggable || originalParent == null)
                return;

            EnsureCanvasGroup();

            if (canvasGroup == null)
            {
                transform.SetParent(originalParent, true);
                originalParent = null;
                return;
            }

            canvasGroup.blocksRaycasts = true;
            bool accepted = owner != null && owner.TryDropCard(cardCode, eventData.position, eventData.pressEventCamera);

            if (accepted)
            {
                transform.DOMove(owner.TableCenter, 0.16f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                transform.SetParent(originalParent, true);
                transform.SetSiblingIndex(originalSiblingIndex);
                transform.DOMove(originalPosition, 0.18f).SetEase(Ease.OutQuad);
                transform.DOScale(Vector3.one, 0.12f);
            }

            originalParent = null;
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
                return;

            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Select()
        {
            if (!selectable || owner == null || string.IsNullOrWhiteSpace(cardCode))
                return;

            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.08f, 0.18f, 4, 0.4f);
            owner.OnCardClicked(cardCode);
        }
    }

    public static class DurakCardText
    {
        public static string Format(DurakCardDto card)
        {
            if (card == null)
                return "";

            return $"{card.rank}{SuitSymbol(card.suit)}";
        }

        public static string SuitSymbol(string suit)
        {
            switch (suit)
            {
                case "Hearts":
                    return "♥";
                case "Diamonds":
                    return "♦";
                case "Clubs":
                    return "♣";
                case "Spades":
                    return "♠";
                default:
                    return suit;
            }
        }
    }
}
