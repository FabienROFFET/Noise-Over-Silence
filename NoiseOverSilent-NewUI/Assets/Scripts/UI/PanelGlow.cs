// ============================================================
// PROJECT : Noise Over Silence
// FILE    : PanelGlow.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-21
// VERSION : 1.3
// CHANGES : v1.2 - 2026-02-21 - Top-only glow (light from above effect)
//           v1.2 - 2026-02-21 - Fixed top-only glow with proper anchoring
//           v1.1 - 2026-02-21 - Changed to very subtle light grey glow (just a touch)
//           v1.0 - 2026-02-21 - Initial version with subtle dystopian glow
// DESC    : Adds a subtle glowing border around the narrative panel
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace NoiseOverSilent.UI
{
    public class PanelGlow : MonoBehaviour
    {
        [Header("Glow Settings")]
        [SerializeField] private Color glowColor = new Color(0.9f, 0.9f, 0.9f, 0.4f); // Brighter grey, more visible
        [SerializeField] private float glowSize = 20f; // Larger for visibility
        [SerializeField] private bool enablePulse = false;
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseIntensity = 0.3f;

        private Image glowImage;
        private float baseBrightness;
        private float pulseTimer = 0f;

        private void Awake()
        {
            baseBrightness = glowColor.a;
            CreateGlow();
        }

        private void CreateGlow()
        {
            // Create glow GameObject
            GameObject glowGO = new GameObject("PanelGlowTop");
            glowGO.transform.SetParent(transform, false);
            glowGO.transform.SetSiblingIndex(0); // Behind panel content

            RectTransform glowRect = glowGO.AddComponent<RectTransform>();
            
            // Anchor to TOP edge only
            glowRect.anchorMin = new Vector2(0f, 1f);  // Top-left corner
            glowRect.anchorMax = new Vector2(1f, 1f);  // Top-right corner
            glowRect.pivot = new Vector2(0.5f, 0f);    // Bottom-center of glow rect
            
            // Size: full width, extends UPWARD by glowSize pixels
            glowRect.sizeDelta = new Vector2(0f, glowSize);
            glowRect.anchoredPosition = new Vector2(0f, 0f);

            glowImage = glowGO.AddComponent<Image>();
            glowImage.raycastTarget = false;
            
            // Create glow gradient (bright at panel edge, fades upward)
            CreateTopGlowSprite();
            
            Debug.Log($"[PanelGlow v1.3] Top glow created: size={glowSize}px, color={glowColor}, alpha={glowColor.a}");
        }

        private void CreateTopGlowSprite()
        {
            // Create a vertical gradient texture for top glow only
            int width = 64;
            int height = 64;
            Texture2D tex = new Texture2D(width, height);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Vertical gradient: bright at BOTTOM (panel edge), fades to TOP
                    float verticalGradient = 1f - ((float)y / height);
                    float alpha = Mathf.Pow(verticalGradient, 2f); // Softer falloff
                    
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            
            Sprite glowSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f));
            glowImage.sprite = glowSprite;
            glowImage.color = glowColor;
            glowImage.type = Image.Type.Sliced;
        }

        private void Update()
        {
            if (!enablePulse || glowImage == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
            
            Color currentColor = glowColor;
            currentColor.a = baseBrightness + pulse;
            glowImage.color = currentColor;
        }

        public void SetGlowColor(Color color)
        {
            glowColor = color;
            baseBrightness = color.a;
            if (glowImage != null)
                glowImage.color = color;
        }

        public void SetGlowIntensity(float intensity)
        {
            Color newColor = glowColor;
            newColor.a = intensity;
            glowColor = newColor;
            baseBrightness = intensity;
            if (glowImage != null)
                glowImage.color = glowColor;
        }
    }
}