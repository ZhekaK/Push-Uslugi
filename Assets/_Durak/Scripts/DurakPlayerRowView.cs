using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.Durak
{
    public class DurakPlayerRowView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text metaText;
        [SerializeField] private RectTransform cardsRoot;
        [SerializeField] private Image cardBackTemplate;
        [SerializeField] private Outline turnOutline;

        private readonly List<GameObject> spawnedCardBacks = new List<GameObject>();

        public void Setup(DurakPlayerDto player, DurakCardSkinSet skinSet, bool showCardBacks, bool highlightTurn)
        {
            if (nameText != null)
                nameText.text = player == null ? "Игрок" : player.displayName;

            if (player == null)
                return;

            if (metaText != null)
                metaText.gameObject.SetActive(false);

            if (turnOutline != null)
                turnOutline.enabled = highlightTurn;

            RenderCardBacks(player.handCount, skinSet, showCardBacks && !player.isOut);
        }

        private void RenderCardBacks(int count, DurakCardSkinSet skinSet, bool visible)
        {
            for (int i = 0; i < spawnedCardBacks.Count; i++)
            {
                if (spawnedCardBacks[i] != null)
                    Destroy(spawnedCardBacks[i]);
            }

            spawnedCardBacks.Clear();

            if (cardsRoot != null)
                cardsRoot.gameObject.SetActive(visible && count > 0);

            if (!visible || count <= 0 || cardsRoot == null || cardBackTemplate == null)
                return;

            float spread = Mathf.Min(150f, Mathf.Max(0f, (count - 1) * 10f));

            for (int i = 0; i < count; i++)
            {
                Image cardBack = Instantiate(cardBackTemplate, cardsRoot);
                cardBack.gameObject.SetActive(true);
                cardBack.sprite = skinSet != null ? skinSet.CardBackSprite : null;
                cardBack.color = cardBack.sprite != null ? Color.white : new Color(0.2f, 0.32f, 0.46f);
                cardBack.preserveAspect = true;

                RectTransform rect = cardBack.rectTransform;
                float t = count <= 1 ? 0.5f : i / (float)(count - 1);
                rect.anchoredPosition = new Vector2(Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t), Mathf.Sin(t * Mathf.PI) * 5f);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(10f, -10f, t));
                rect.SetAsLastSibling();
                spawnedCardBacks.Add(cardBack.gameObject);
            }
        }
    }
}
