using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.UI
{
    public class SlidingPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private TextMeshProUGUI narrativeText;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private float slideSpeed = 0.5f;
        
        private List<GameObject> activeChoices = new List<GameObject>();
        private Coroutine slideCoroutine;
        
        public void ShowPanel(string text, List<Choice> choices, string position = "right", float width = 0.33f)
        {
            if (narrativeText != null) narrativeText.text = text;
            DisplayChoices(choices);
            SetPanelWidth(width);
            gameObject.SetActive(true);
            SlideIn(position);
        }
        
        public void HidePanel()
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(AnimateSlide("right", false));
        }
        
        private void SetPanelWidth(float widthPercent)
        {
            if (panelRect != null) {
                Vector2 sizeDelta = panelRect.sizeDelta;
                sizeDelta.x = Screen.width * widthPercent;
                panelRect.sizeDelta = sizeDelta;
            }
        }
        
        private void SlideIn(string position)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(AnimateSlide(position, true));
        }
        
        private IEnumerator AnimateSlide(string position, bool slideIn)
        {
            float panelWidth = panelRect.sizeDelta.x;
            float startX = position == "left" ? (slideIn ? -panelWidth : 0f) : (slideIn ? panelWidth : 0f);
            float endX = position == "left" ? (slideIn ? 0f : -panelWidth) : (slideIn ? 0f : panelWidth);
            
            if (position == "left") {
                panelRect.anchorMin = new Vector2(0, 0);
                panelRect.anchorMax = new Vector2(0, 1);
                panelRect.pivot = new Vector2(0, 0.5f);
            } else {
                panelRect.anchorMin = new Vector2(1, 0);
                panelRect.anchorMax = new Vector2(1, 1);
                panelRect.pivot = new Vector2(1, 0.5f);
            }
            
            float elapsed = 0f;
            while (elapsed < slideSpeed) {
                elapsed += Time.deltaTime;
                float t = elapsed / slideSpeed;
                panelRect.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), 0f);
                yield return null;
            }
            panelRect.anchoredPosition = new Vector2(endX, 0f);
            if (!slideIn) gameObject.SetActive(false);
        }
        
        private void DisplayChoices(List<Choice> choices)
        {
            foreach (GameObject button in activeChoices) Destroy(button);
            activeChoices.Clear();
            if (choices == null || choices.Count == 0) return;
            
            foreach (Choice choice in choices) {
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = choice.text;
                Button button = buttonObj.GetComponent<Button>();
                if (button != null) {
                    Choice choiceCopy = choice;
                    button.onClick.AddListener(() => OnChoiceClicked(choiceCopy));
                }
                activeChoices.Add(buttonObj);
            }
        }
        
        private void OnChoiceClicked(Choice choice)
        {
            var gameManager = FindObjectOfType<NoiseOverSilent.Managers.GameManager>();
            if (gameManager != null) gameManager.MakeChoice(choice);
        }
    }
}
