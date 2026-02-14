using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace NoiseOverSilent.UI
{
    public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;

        [Header("Hover Colors")]
        [SerializeField] private Color normalColor = new Color(0.878f, 0.878f, 0.878f, 1f); // #e0e0e0
        [SerializeField] private Color hoverColor = new Color(1f, 0.647f, 0f, 1f);           // Orange

        private Action onClickCallback;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (buttonText == null)
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
