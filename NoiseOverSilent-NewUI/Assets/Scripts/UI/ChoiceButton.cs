// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ChoiceButton.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 2.2
// CHANGES : v2.2 - 2026-02-16 - Subtle hover (1.05x)
//           v2.1 - 2026-02-16 - DRAMATIC hover scale (1.2x)
//           v2.0 - 2026-02-16 - Premium polish: scale, glow, smooth animations
//           v1.1 - 2026-02-16 - Added debug logs
//           v1.0 - 2026-02-14 - Initial version
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace NoiseOverSilent.UI
{
    public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Colors
        private static readonly Color NormalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static readonly Color HoverColor  = new Color(1f, 0.65f, 0f, 1f); // Orange
        private static readonly Color GlowColor   = new Color(1f, 0.5f, 0f, 0.3f);

        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.05f; // Subtle 5%
        [SerializeField] private float scaleSpeed = 8f;
        [SerializeField] private float glowIntensity = 0.4f;
        [SerializeField] private bool useGlow = true;

        private TextMeshProUGUI buttonText;
        private Button button;
        private Image backgroundImage;
        private RectTransform rectTransform;
        private Action onClickCallback;
        
        private Vector3 normalScale = Vector3.one;
        private Vector3 targetScale = Vector3.one;
        private bool isHovering = false;
        private Outline glowOutline;
        private Shadow glowShadow;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            backgroundImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();

            if (button != null)
                button.onClick.AddListener(OnClick);

            // Add glow effects
            if (useGlow)
            {
                glowOutline = gameObject.AddComponent<Outline>();
                glowOutline.effectColor = GlowColor;
                glowOutline.effectDistance = new Vector2(2, -2);
                glowOutline.enabled = false;

                glowShadow = gameObject.AddComponent<Shadow>();
                glowShadow.effectColor = new Color(1f, 0.5f, 0f, 0f);
                glowShadow.effectDistance = new Vector2(0, 0);
            }

            SetColor(NormalColor);
            normalScale = rectTransform.localScale;
        }

        public void Setup(string text, Action callback)
        {
            if (buttonText != null)
                buttonText.text = text;
            onClickCallback = callback;
        }

        private void Update()
        {
            // Smooth scale animation
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale, 
                targetScale, 
                Time.deltaTime * scaleSpeed
            );

            // Pulsing glow when hovering
            if (isHovering && glowShadow != null)
            {
                float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                glowShadow.effectColor = new Color(1f, 0.5f, 0f, pulse * glowIntensity);
            }
        }

        private void OnClick()
        {
            StartCoroutine(ClickAnimation());
            onClickCallback?.Invoke();
        }

        private IEnumerator ClickAnimation()
        {
            // Quick punch effect
            Vector3 punchScale = normalScale * 0.95f;
            rectTransform.localScale = punchScale;
            yield return new WaitForSeconds(0.1f);
            targetScale = normalScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            targetScale = normalScale * hoverScale;
            SetColor(HoverColor);
            
            if (glowOutline != null)
                glowOutline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            targetScale = normalScale;
            SetColor(NormalColor);
            
            if (glowOutline != null)
                glowOutline.enabled = false;
            
            if (glowShadow != null)
                glowShadow.effectColor = new Color(1f, 0.5f, 0f, 0f);
        }

        private void SetColor(Color color)
        {
            if (buttonText != null)
                buttonText.color = color;
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        // Called by SlidingPanel to animate entry
        public void AnimateEntry(float delay)
        {
            StartCoroutine(SlideInAnimation(delay));
        }

        private IEnumerator SlideInAnimation(float delay)
        {
            // Start invisible and offset
            CanvasGroup cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            
            Vector3 startPos = rectTransform.localPosition;
            rectTransform.localPosition = startPos + new Vector3(-50f, 0f, 0f);
            
            yield return new WaitForSeconds(delay);
            
            // Slide in and fade in
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                
                cg.alpha = t;
                rectTransform.localPosition = Vector3.Lerp(
                    startPos + new Vector3(-50f, 0f, 0f),
                    startPos,
                    t
                );
                
                yield return null;
            }
            
            cg.alpha = 1f;
            rectTransform.localPosition = startPos;
            Destroy(cg); // Remove temporary component
        }
    }
}