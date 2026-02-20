// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ImageDisplay.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 1.3
// CHANGES : v1.3 - 2026-02-16 - Fade DISABLED (useFade=false)
//           v1.2 - 2026-02-16 - Slower fade (1.5s) for visibility
//           v1.1 - 2026-02-16 - Added fade transition between images
//           v1.0 - 2026-02-14 - Initial version
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NoiseOverSilent.UI
{
    public class ImageDisplay : MonoBehaviour
    {
        [SerializeField] private Image eventImage;
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private bool useFade = false; // DISABLED - instant change

        private Coroutine fadeCoroutine;

        public void DisplayImage(string imagePath)
        {
            if (eventImage == null)
            {
                Debug.LogWarning("[ImageDisplay] eventImage reference is null.");
                return;
            }

            if (string.IsNullOrEmpty(imagePath))
            {
                SetPlaceholder();
                return;
            }

            // Strip "images/" prefix
            string path = imagePath;
            if (path.StartsWith("images/"))
                path = path.Substring(7);

            // Strip file extension
            int dot = path.LastIndexOf('.');
            if (dot >= 0)
                path = path.Substring(0, dot);

            // Load from Assets/Resources/Images/Events/
            Sprite sprite = Resources.Load<Sprite>("Images/Events/" + path);

            if (sprite != null)
            {
                if (useFade)
                {
                    if (fadeCoroutine != null)
                        StopCoroutine(fadeCoroutine);
                    fadeCoroutine = StartCoroutine(FadeToNewImage(sprite));
                }
                else
                {
                    eventImage.sprite = sprite;
                    eventImage.color  = Color.white;
                }
                
                Debug.Log($"[ImageDisplay v1.1] Loaded: Images/Events/{path}");
            }
            else
            {
                Debug.LogWarning($"[ImageDisplay v1.1] Not found: Images/Events/{path}");
                SetPlaceholder();
            }
        }

        private IEnumerator FadeToNewImage(Sprite newSprite)
        {
            // Fade out
            float elapsed = 0f;
            Color startColor = eventImage.color;
            
            while (elapsed < fadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / (fadeDuration / 2f));
                eventImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            // Change sprite
            eventImage.sprite = newSprite;

            // Fade in
            elapsed = 0f;
            while (elapsed < fadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / (fadeDuration / 2f));
                eventImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            eventImage.color = Color.white;
            fadeCoroutine = null;
        }

        private void SetPlaceholder()
        {
            eventImage.sprite = null;
            eventImage.color  = new Color(0.08f, 0.08f, 0.08f, 1f);
        }
    }
}