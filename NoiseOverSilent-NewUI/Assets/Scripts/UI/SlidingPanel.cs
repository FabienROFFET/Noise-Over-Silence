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
        [Header("References")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private Image panelBackground;
        [SerializeField] private TextMeshProUGUI narrativeText;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Settings")]
        [SerializeField] private float slideSpeed = 0.5f;
        [SerializeField] private float panelOpacity = 0.8f;
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        private GameManager gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();
        private bool isAnimating = false;
        private string currentPosition = "right";

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();

            if (panelRect == null)
                panelRect = GetComponent<RectTransform>();

            // Apply opacity
            if (panelBackground != null)
            {
                Color c = panelColor;
                c.a = panelOpacity;
                panelBackground.color = c;
            }

            // Start hidden off-screen
            MoveOffScreen();
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            // Set text
            if (narrativeText != null)
                narrativeText.text = gameEvent.text;

            // Apply panel layout from JSON
            currentPosition = string.IsNullOrEmpty(gameEvent.text_position) ? "right" : gameEvent.text_position;
            ApplyPanelLayout(currentPosition, gameEvent.panel_width);

            // Build choices
            BuildChoices(gameEvent.choices);

            // Slide in
            Show(null);
        }

        private void ApplyPanelLayout(string position, float widthRatio)
        {
            if (panelRect == null) return;

            float panelWidth = 1920f * (widthRatio > 0 ? widthRatio : 0.33f);

            panelRect.sizeDelta = new Vector2(panelWidth, panelRect.sizeDelta.y);

            switch (position)
            {
                case "left":
                    panelRect.anchorMin = new Vector2(0f, 0f);
                    panelRect.anchorMax = new Vector2(0f, 1f);
                    panelRect.pivot = new Vector2(0f, 0.5f);
                    break;
                case "center":
                    panelRect.anchorMin = new Vector2(0.5f, 0f);
                    panelRect.anchorMax = new Vector2(0.5f, 1f);
                    panelRect.pivot = new Vector2(0.5f, 0.5f);
                    break;
                default: // right
                    panelRect.anchorMin = new Vector2(1f, 0f);
                    panelRect.anchorMax = new Vector2(1f, 1f);
                    panelRect.pivot = new Vector2(1f, 0.5f);
                    break;
            }
        }

        private void BuildChoices(List<Choice> choices)
        {
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
                    int nextEvent = choice.next_event; // Capture for lambda
                    choiceButton.Setup(choice.text, () => gameManager.MakeChoice(nextEvent));
                }

                activeButtons.Add(btnObj);
            }
        }

        public void Show(Action onComplete)
        {
            if (isAnimating) return;
            StartCoroutine(SlideIn(onComplete));
        }

        public void Hide(Action onComplete)
        {
            if (isAnimating) return;
            StartCoroutine(SlideOut(onComplete));
        }

        private IEnumerator SlideIn(Action onComplete)
        {
            isAnimating = true;

            Vector2 startPos = panelRect.anchoredPosition;
            Vector2 targetPos = Vector2.zero;

            float elapsed = 0f;
            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed));
                yield return null;
            }

            panelRect.anchoredPosition = targetPos;
            isAnimating = false;
            onComplete?.Invoke();
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            isAnimating = true;

            Vector2 startPos = panelRect.anchoredPosition;
            Vector2 targetPos = new Vector2(panelRect.sizeDelta.x + 100f, 0f);

            float elapsed = 0f;
            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed));
                yield return null;
            }

            panelRect.anchoredPosition = targetPos;
            isAnimating = false;
            onComplete?.Invoke();
        }

        private void MoveOffScreen()
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(panelRect.sizeDelta.x + 100f, 0f);
        }
    }
}
