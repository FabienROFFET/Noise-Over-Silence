// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ChoiceButton.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 3.0
// CHANGES : v3.0 - 2026-02-21 - SIMPLIFIED - removed ALL animations
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

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

            if (button != null)
                button.onClick.AddListener(OnClick);

            SetColor(NormalColor);
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
            Debug.Log($"[ChoiceButton v3.0 SIMPLE] CLICKED: '{buttonText?.text}'");
            onClickCallback?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetColor(HoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
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