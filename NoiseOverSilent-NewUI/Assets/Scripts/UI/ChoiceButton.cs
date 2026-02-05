using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.UI
{
    public class ChoiceButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        
        private int choiceIndex;
        private Action<int> onChoiceSelected;
        
        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        public void Setup(Choice choice, int index, Action<int> callback)
        {
            choiceIndex = index;
            onChoiceSelected = callback;
            
            if (buttonText != null)
            {
                buttonText.text = choice.text;
            }
            else
            {
                // Fallback to regular Text component if TMP isn't available
                Text regularText = GetComponentInChildren<Text>();
                if (regularText != null)
                {
                    regularText.text = choice.text;
                }
            }
        }
        
        private void OnButtonClicked()
        {
            onChoiceSelected?.Invoke(choiceIndex);
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}