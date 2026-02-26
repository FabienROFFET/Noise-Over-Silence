// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ChapterIntroScreen.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-22
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-22 - Initial chapter intro screen
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.UI
{
    public class ChapterIntroScreen : MonoBehaviour
    {
        [SerializeField] private GameObject introPanel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI chapterNumberText;
        [SerializeField] private TextMeshProUGUI chapterTitleText;
        [SerializeField] private float fadeDuration = 0.5f;

        public void ShowChapterIntro(ChapterInfo chapterInfo, Action onComplete)
        {
            if (chapterInfo == null)
            {
                Debug.LogWarning("[ChapterIntroScreen v1.0] No chapter info - skipping intro");
                onComplete?.Invoke();
                return;
            }

            // Activate panel first so coroutine can start
            introPanel.SetActive(true);
            StartCoroutine(ShowIntroCoroutine(chapterInfo, onComplete));
        }

        private IEnumerator ShowIntroCoroutine(ChapterInfo chapterInfo, Action onComplete)
        {
            // Load chapter intro image
            Sprite chapterSprite = Resources.Load<Sprite>(chapterInfo.intro_image);
            if (chapterSprite != null)
            {
                backgroundImage.sprite = chapterSprite;
                backgroundImage.color = Color.white;
                Debug.Log($"[ChapterIntroScreen v1.0] Loaded chapter image: {chapterInfo.intro_image}");
            }
            else
            {
                // Dark fallback
                backgroundImage.sprite = null;
                backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                Debug.LogWarning($"[ChapterIntroScreen v1.0] Chapter image not found: {chapterInfo.intro_image}");
            }

            // Set text
            chapterNumberText.text = $"CHAPTER {chapterInfo.number}";
            chapterTitleText.text = chapterInfo.title.ToUpper();

            // Get or add CanvasGroup for fading
            CanvasGroup canvasGroup = introPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = introPanel.AddComponent<CanvasGroup>();

            // Fade in
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            Debug.Log($"[ChapterIntroScreen v1.0] Showing: Chapter {chapterInfo.number} - {chapterInfo.title}");

            // Hold for duration
            yield return new WaitForSeconds(chapterInfo.intro_duration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            introPanel.SetActive(false);

            Debug.Log("[ChapterIntroScreen v1.0] Chapter intro complete");
            onComplete?.Invoke();
        }

        public void SkipIntro()
        {
            StopAllCoroutines();
            introPanel.SetActive(false);
            Debug.Log("[ChapterIntroScreen v1.0] Chapter intro skipped");
        }
    }
}