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
        [SerializeField] private Color panelColor = new Color(0.08f, 0.08f, 0.08f, 1f);

        private GameManager gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();
        private bool isAnimating = false;

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();

            if (panelRect == null)
                panelRect = GetComponent<RectTransform>();

            if (panelBackground != null)
            {
                Color c = panelColor;
                c.a = panelOpacity;
                panelBackground.color = c;
            }
        }

        private void Start()
        {
            // Move off-screen after layout is ready
            MoveOffScreen();
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            if (narrativeText != null)
                narrativeText.text = gameEvent.text;

            string pos = string.IsNullOrEmpty(gameEvent.text_position) ? "right" : gameEvent.text_position;
            ApplyLayout(pos, gameEvent.panel_width);

            BuildChoices(gameEvent.choices);

            StopAllCoroutines();
            isAnimating = false;
            StartCoroutine(SlideIn(null));
        }

        private void ApplyLayout(string position, float widthRatio)
        {
            if (panelRect == null) return;

            float w = 1920f * (widthRatio > 0 ? widthRatio : 0.33f);
            panelRect.sizeDelta = new Vector2(w, panelRect.sizeDelta.y);

            switch (position)
            {
                case "left":
                    panelRect.anchorMin = new Vector2(0f, 0f);
                    panelRect.anchorMax = new Vector2(0f, 1f);
                    panelRect.pivot    = new Vector2(0f, 0.5f);
                    break;
                case "center":
                    panelRect.anchorMin = new Vector2(0.5f, 0f);
                    panelRect.anchorMax = new Vector2(0.5f, 1f);
                    panelRect.pivot    = new Vector2(0.5f, 0.5f);
                    break;
                default: // right
                    panelRect.anchorMin = new Vector2(1f, 0f);
                    panelRect.anchorMax = new Vector2(1f, 1f);
                    panelRect.pivot    = new Vector2(1f, 0.5f);
                    break;
            }
        }

        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0) return;
            if (choiceButtonPrefab == null)
            {
                Debug.LogWarning("SlidingPanel: choiceButtonPrefab not assigned!");
                return;
            }

            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                btnObj.SetActive(true);

                ChoiceButton cb = btnObj.GetComponent<ChoiceButton>();
                if (cb != null)
                {
                    int next = choice.next_event;
                    cb.Setup(choice.text, () => gameManager.MakeChoice(next));
                }

                activeButtons.Add(btnObj);
            }
        }

        public void Hide(Action onComplete)
        {
            StopAllCoroutines();
            isAnimating = false;
            StartCoroutine(SlideOut(onComplete));
        }

        private IEnumerator SlideIn(Action onComplete)
        {
            isAnimating = true;
            Vector2 start  = panelRect.anchoredPosition;
            Vector2 target = Vector2.zero;
            float elapsed  = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            panelRect.anchoredPosition = target;
            isAnimating = false;
            onComplete?.Invoke();
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            isAnimating = true;
            Vector2 start  = panelRect.anchoredPosition;
            Vector2 target = new Vector2(panelRect.sizeDelta.x + 100f, 0f);
            float elapsed  = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            panelRect.anchoredPosition = target;
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
