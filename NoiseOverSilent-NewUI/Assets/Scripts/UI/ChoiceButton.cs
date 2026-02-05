using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace NoiseOverSilent.UI
{
    public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Color normalColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        [SerializeField] private Color hoverColor = new Color(1f, 0.42f, 0.21f, 1f);
        
        private TextMeshProUGUI buttonText;
        
        private void Awake()
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.color = normalColor;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buttonText != null) buttonText.color = hoverColor;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (buttonText != null) buttonText.color = normalColor;
        }
    }
}
