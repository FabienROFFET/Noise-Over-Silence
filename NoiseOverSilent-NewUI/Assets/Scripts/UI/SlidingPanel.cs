// ============================================================
// PROJECT : Noise Over Silence  
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// UPDATED : 2026-03-17
// DESC    : Sliding panel with typewriter effect
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoiseOverSilent.Data;
using NoiseOverSilent.Managers;

namespace NoiseOverSilent.UI
{
    public class SlidingPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private Image panelBackground;
        [SerializeField] private TextMeshProUGUI narrativeText;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private float slideSpeed = 0.4f;
        [SerializeField] private float panelOpacity = 0.88f;

        private GameManager gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();
        private bool isAnimating = false;

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            
            if (panelBackground != null)
            {
                Color c = panelBackground.color;
                c.a = panelOpacity;
                panelBackground.color = c;
            }

            // Start off-screen
            SetOffScreen();
        }

        // PUBLIC method - GameManager can call this
        public void Show(GameEvent gameEvent)
        {
            if (gameEvent == null) return;

            // Set narrative text
            if (narrativeText != null)
                narrativeText.text = gameEvent.text;

            // Build choice buttons
            BuildChoices(gameEvent.choices);

            // Slide in
            if (!isAnimating)
                StartCoroutine(SlideIn());
        }

        public void Hide(Action onComplete = null)
        {
            if (!isAnimating)
                StartCoroutine(SlideOut(onComplete));
        }

        private void BuildChoices(List<Choice> choices)
        {
            // Clear old buttons
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0) return;

            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                ChoiceButton choiceButton = btnObj.GetComponent<ChoiceButton>();

                if (choiceButton != null)
                {
                    Choice capturedChoice = choice; // Capture for lambda
                    choiceButton.Setup(choice.text, () => {
                        if (gameManager != null)
                            gameManager.MakeChoice(capturedChoice);
                    });
                }

                activeButtons.Add(btnObj);
            }
        }

        private IEnumerator SlideIn()
        {
            isAnimating = true;

            Vector2 startPos = panelRect.anchoredPosition;
            Vector2 targetPos = Vector2.zero;

            float elapsed = 0f;
            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = targetPos;
            isAnimating = false;
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            isAnimating = true;

            Vector2 startPos = panelRect.anchoredPosition;
            Vector2 targetPos = new Vector2(panelRect.sizeDelta.x + 100f, 0);

            float elapsed = 0f;
            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = targetPos;
            isAnimating = false;
            onComplete?.Invoke();
        }

        private void SetOffScreen()
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(740f, 0);
        }
    }
}