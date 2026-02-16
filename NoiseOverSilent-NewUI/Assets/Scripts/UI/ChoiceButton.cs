// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ChoiceButton.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-14 - Initial version
// DESC    : Individual choice button with orange hover effect.
//           Setup() called by SlidingPanel with text and callback.
//           Uses IPointerEnterHandler / IPointerExitHandler.
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
        // Colors
        private static readonly Color NormalColor = new Color(0.88f, 0.88f, 0.88f, 1f); // #e0e0e0
        private static readonly Color HoverColor  = new Color(1f,    0.65f, 0f,    1f); // orange

        private TextMeshProUGUI buttonText;
        private Button          button;
        private Action          onClickCallback;

        private void Awake()
        {
            button     = GetComponent<Button>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

            if (button != null)
                button.onClick.AddListener(OnClick);

            SetColor(NormalColor);
        }

        /// <summary>Sets button text and click callback. Called by SlidingPanel.</summary>
        public void Setup(string text, Action callback)
        {
            if (buttonText != null)
                buttonText.text = text;

            onClickCallback = callback;
        }

        private void OnClick()
        {
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
