using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace NoiseOverSilent.UI
{
    public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TextMeshProUGUI buttonText;
        private Button button;
        private Action onClickCallback;

        private Color normalColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        private Color hoverColor  = new Color(1f, 0.65f, 0f, 1f); // orange

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

            if (button != null)
                button.onClick.AddListener(OnClick);

            SetColor(normalColor);
        }

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
            SetColor(hoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetColor(normalColor);
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
