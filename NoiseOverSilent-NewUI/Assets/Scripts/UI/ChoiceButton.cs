// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ChoiceButton.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 3.2
// CHANGES : v3.2 - 2026-02-21 - Integrated button hover and click sounds
//           v3.1 - 2026-02-21 - Added subtle scale on hover (1.05x)
//           v3.0 - 2026-02-21 - SIMPLIFIED - removed ALL animations
// ============================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace NoiseOverSilent.UI
{
    public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color NormalColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        private static readonly Color HoverColor  = new Color(1f, 0.65f, 0f, 1f);

        private TextMeshProUGUI buttonText;
        private Button button;
        private Action onClickCallback;
        private RectTransform rectTransform;
        
        private Vector3 normalScale = Vector3.one;
        private Vector3 targetScale = Vector3.one;
        private float scaleSpeed = 10f;
        private float hoverScale = 1.05f; // 5% bigger

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();

            if (button != null)
                button.onClick.AddListener(OnClick);

            normalScale = rectTransform.localScale;
            SetColor(NormalColor);
        }

        private void Update()
        {
            // Smooth scale animation
            if (rectTransform.localScale != targetScale)
            {
                rectTransform.localScale = Vector3.Lerp(
                    rectTransform.localScale,
                    targetScale,
                    Time.deltaTime * scaleSpeed
                );
            }
        }

        public void Setup(string text, Action callback)
        {
            if (buttonText != null)
                buttonText.text = text;
            onClickCallback = callback;
            Debug.Log($"[ChoiceButton v3.0 SIMPLE] Setup: '{text}' callback={callback != null}");
        }

        private void OnClick()
        {
            Debug.Log($"[ChoiceButton v3.2] CLICKED: '{buttonText?.text}'");
            NoiseOverSilent.Managers.SoundManager.PlayButtonClick();
            onClickCallback?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = normalScale * hoverScale; // Grow to 1.05x
            SetColor(HoverColor);
            NoiseOverSilent.Managers.SoundManager.PlayButtonHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = normalScale; // Back to normal size
            SetColor(NormalColor);
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
    }
}