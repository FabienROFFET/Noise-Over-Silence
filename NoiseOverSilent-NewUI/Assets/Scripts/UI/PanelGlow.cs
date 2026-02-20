// ============================================================
// PROJECT : Noise Over Silence
// FILE    : PanelGlow.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-16
// VERSION : 1.1
// CHANGES : v1.1 - 2026-02-16 - Brighter glow (3-7px pulse)
//           v1.0 - 2026-02-16 - Initial version
// DESC    : Adds subtle pulsing glow to panel edges
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace NoiseOverSilent.UI
{
    public class PanelGlow : MonoBehaviour
    {
        [SerializeField] private Color glowColor = new Color(0.4f, 0.5f, 0.8f, 0.8f); // Brighter blue
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseIntensity = 0.8f; // Much stronger

        private Outline outline;

        private void Awake()
        {
            outline = gameObject.AddComponent<Outline>();
            outline.effectColor = glowColor;
            outline.effectDistance = new Vector2(0, 0);
        }

        private void Update()
        {
            // DRAMATIC breathing glow
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float distance = 3f + (pulse * pulseIntensity * 5f); // 3-7px
            
            outline.effectDistance = new Vector2(distance, -distance);
            
            // Pulse alpha dramatically
            Color color = glowColor;
            color.a = glowColor.a * (0.5f + pulse * 0.5f);
            outline.effectColor = color;
        }
    }
}