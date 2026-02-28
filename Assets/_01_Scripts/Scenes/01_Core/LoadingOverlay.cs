using System.Collections;
using UnityEngine;

namespace Game.Scenes.Core
{
    public class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInTime = 0.5f;
        [SerializeField] private float fadeOutTime = 0.5f;

        public IEnumerator FadeInBlack()
        {
            yield return FadeTo(1f, fadeInTime);
        }

        public IEnumerator FadeOutBlack()
        {
            yield return FadeTo(0f, fadeOutTime); ;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;
            float elasped = 0f;
            while (elasped < duration)
            {
                elasped += Time.deltaTime;
                float t = Mathf.Clamp01(elasped / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
        }
    }
}