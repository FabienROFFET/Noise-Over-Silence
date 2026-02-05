using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NoiseOverSilent.Data;
using NoiseOverSilent.Managers;

namespace NoiseOverSilent.UI
{
    public class SlidingPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private float slideSpeed = 500f;
        [SerializeField] private bool startHidden = true;
        
        [Header("Slide Positions")]
        [SerializeField] private float hiddenPositionX = -400f;
        [SerializeField] private float visiblePositionX = 0f;
        
        [Header("Choice Buttons")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        
        private bool isVisible = false;
        private bool isAnimating = false;
        private List<GameObject> activeChoiceButtons = new List<GameObject>();
        private GameManager gameManager;
        
        private void Awake()
        {
            if (panelTransform == null)
                panelTransform = GetComponent<RectTransform>();
            
            // Use the new Unity API instead of the deprecated one
            gameManager = FindFirstObjectByType<GameManager>();
            
            if (startHidden)
            {
                SetPosition(hiddenPositionX);
                isVisible = false;
            }
            else
            {
                SetPosition(visiblePositionX);
                isVisible = true;
            }
        }
        
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }
        
        public void Show()
        {
            if (!isAnimating && !isVisible)
            {
                StartCoroutine(SlideToPosition(visiblePositionX, true));
            }
        }
        
        public void Hide()
        {
            if (!isAnimating && isVisible)
            {
                StartCoroutine(SlideToPosition(hiddenPositionX, false));
            }
        }
        
        private IEnumerator SlideToPosition(float targetX, bool willBeVisible)
        {
            isAnimating = true;
            
            Vector2 startPos = panelTransform.anchoredPosition;
            Vector2 targetPos = new Vector2(targetX, startPos.y);
            
            float elapsedTime = 0f;
            float distance = Mathf.Abs(targetX - startPos.x);
            float duration = distance / slideSpeed;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                panelTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            panelTransform.anchoredPosition = targetPos;
            isVisible = willBeVisible;
            isAnimating = false;
        }
        
        private void SetPosition(float x)
        {
            if (panelTransform != null)
            {
                Vector2 pos = panelTransform.anchoredPosition;
                pos.x = x;
                panelTransform.anchoredPosition = pos;
            }
        }
        
        public void DisplayChoices(List<Choice> choices)
        {
            ClearChoices();
            
            if (choices == null || choices.Count == 0)
            {
                Hide();
                return;
            }
            
            for (int i = 0; i < choices.Count; i++)
            {
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
                ChoiceButton choiceButton = buttonObj.GetComponent<ChoiceButton>();
                
                if (choiceButton != null)
                {
                    int choiceIndex = i; // Capture index for lambda
                    choiceButton.Setup(choices[i], choiceIndex, OnChoiceSelected);
                }
                
                activeChoiceButtons.Add(buttonObj);
            }
            
            Show();
        }
        
        private void OnChoiceSelected(int choiceIndex)
        {
            if (gameManager != null)
            {
                gameManager.MakeChoice(choiceIndex);
            }
            
            Hide();
        }
        
        public void ClearChoices()
        {
            foreach (GameObject button in activeChoiceButtons)
            {
                if (button != null)
                    Destroy(button);
            }
            
            activeChoiceButtons.Clear();
        }
    }
}