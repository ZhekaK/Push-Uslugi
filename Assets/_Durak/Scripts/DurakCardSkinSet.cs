using System;
using System.Collections.Generic;
using UnityEngine;

namespace PushPelmesh.Durak
{
    [CreateAssetMenu(menuName = "Push Uslugi/Durak/Card Skin Set", fileName = "DurakCardSkinSet")]
    public class DurakCardSkinSet : ScriptableObject
    {
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private List<CardSpriteEntry> cards = new List<CardSpriteEntry>();

        public Sprite CardBackSprite => cardBackSprite;

        public Sprite GetSprite(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            for (int i = 0; i < cards.Count; i++)
            {
                if (string.Equals(cards[i].code, code, StringComparison.OrdinalIgnoreCase))
                    return cards[i].sprite;
            }

            return null;
        }
    }

    [Serializable]
    public class CardSpriteEntry
    {
        public string code;
        public Sprite sprite;
    }
}
