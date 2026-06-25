using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PushPelmesh.Zong
{
    public class ZongDiceAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] diceSprites = new Sprite[6];
        [SerializeField] private float animationDuration = 0.75f;
        [SerializeField] private float frameInterval = 0.055f;

        private Coroutine animationCoroutine;

        public void SetDiceSprites(Sprite[] sprites)
        {
            diceSprites = sprites;
        }

        public void ShowResult(IReadOnlyList<Image> diceImages, IReadOnlyList<int> diceValues)
        {
            StopAnimation();
            ApplyDice(diceImages, diceValues);
        }

        public void AnimateToResult(IReadOnlyList<Image> diceImages, IReadOnlyList<int> diceValues)
        {
            StopAnimation();

            if (diceImages == null || diceImages.Count == 0 || diceValues == null || diceValues.Count == 0)
                return;

            animationCoroutine = StartCoroutine(AnimateDiceRoutine(diceImages, diceValues));
        }

        public void StopAnimation()
        {
            if (animationCoroutine == null)
                return;

            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        private IEnumerator AnimateDiceRoutine(IReadOnlyList<Image> diceImages, IReadOnlyList<int> diceValues)
        {
            float startedAt = Time.unscaledTime;

            while (Time.unscaledTime - startedAt < animationDuration)
            {
                int visibleCount = Mathf.Min(diceImages.Count, diceValues.Count);

                for (int i = 0; i < diceImages.Count; i++)
                {
                    Image image = diceImages[i];

                    if (image == null)
                        continue;

                    bool visible = i < visibleCount;
                    image.gameObject.SetActive(visible);

                    if (visible)
                        image.sprite = GetSprite(Random.Range(1, 7));
                }

                yield return new WaitForSecondsRealtime(frameInterval);
            }

            ApplyDice(diceImages, diceValues);
            animationCoroutine = null;
        }

        private void ApplyDice(IReadOnlyList<Image> diceImages, IReadOnlyList<int> diceValues)
        {
            if (diceImages == null)
                return;

            int visibleCount = diceValues == null ? 0 : Mathf.Min(diceImages.Count, diceValues.Count);

            for (int i = 0; i < diceImages.Count; i++)
            {
                Image image = diceImages[i];

                if (image == null)
                    continue;

                bool visible = i < visibleCount;
                image.gameObject.SetActive(visible);

                if (visible)
                    image.sprite = GetSprite(diceValues[i]);
            }
        }

        private Sprite GetSprite(int value)
        {
            int index = Mathf.Clamp(value, 1, 6) - 1;
            return diceSprites != null && index < diceSprites.Length ? diceSprites[index] : null;
        }
    }
}
