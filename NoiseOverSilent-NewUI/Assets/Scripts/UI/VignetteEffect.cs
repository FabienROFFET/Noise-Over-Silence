// ============================================================
// PROJECT : Noise Over Silence
// FILE    : VignetteEffect.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-16
// VERSION : 1.2
// CHANGES : v1.2 - 2026-02-16 - Pulse DISABLED (no breathing)
//           v1.1 - 2026-02-16 - Dramatic intensity (0.8) with pulse enabled
//           v1.0 - 2026-02-16 - Screen vignette with pulse
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace NoiseOverSilent.UI
{
    public class VignetteEffect : MonoBehaviour
    {
        [Header("Vignette Settings")]
        [SerializeField] private float vignetteIntensity = 0.8f;
        [SerializeField] private float vignetteSoftness = 0.4f;
        [SerializeField] private bool enablePulse = false; // DISABLED - no pulse
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.2f;

        private Image vignetteImage;
        private Material vignetteMaterial;

        private void Awake()
        {
            // Create vignette overlay
            GameObject vignetteGO = new GameObject("Vignette");
            vignetteGO.transform.SetParent(transform, false);
            
            RectTransform rect = vignetteGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsLastSibling(); // On top of everything
            
            vignetteImage = vignetteGO.AddComponent<Image>();
            vignetteImage.raycastTarget = false;
            
            // Create simple radial gradient material
            CreateVignetteMaterial();
        }

        private void CreateVignetteMaterial()
        {
            // Use a simple sprite with radial gradient
            // For now, use color overlay (you can add custom sprite later)
            vignetteImage.color = new Color(0f, 0f, 0f, vignetteIntensity);
            
            // TODO: Add custom vignette sprite for better effect
            // Sprite should be: black center fading to transparent edges
        }

        private void Update()
        {
            if (enablePulse)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                float intensity = vignetteIntensity + (pulse * pulseAmount);
                vignetteImage.color = new Color(0f, 0f, 0f, intensity);
            }
        }

        public void SetIntensity(float intensity)
        {
            vignetteIntensity = Mathf.Clamp01(intensity);
            if (!enablePulse)
                vignetteImage.color = new Color(0f, 0f, 0f, vignetteIntensity);
        }

        public void Pulse(float duration = 2f)
        {
            StartCoroutine(PulseCoroutine(duration));
        }

        private System.Collections.IEnumerator PulseCoroutine(float duration)
        {
            float elapsed = 0f;
            float originalIntensity = vignetteIntensity;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float wave = Mathf.Sin(elapsed * Mathf.PI / duration);
                float intensity = originalIntensity + (wave * pulseAmount);
                vignetteImage.color = new Color(0f, 0f, 0f, intensity);
                yield return null;
            }
            
            vignetteImage.color = new Color(0f, 0f, 0f, originalIntensity);
        }
    }
}