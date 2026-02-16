// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// AUTHOR  : Noise Over Silence Dev Team
// DESC    : Animated right-side panel showing narrative text
//           and player choices. Slides in/out via coroutines.
//           ShowEvent() called by GameManager.
//           Hide(callback) called before loading next event.
//           All references wired by UIBuilder at runtime.
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
        [SerializeField] private RectTransform   panelRect;
        [SerializeField] private Image           panelBackground;
        [SerializeField] private TextMeshProUGUI narrativeText;
        [SerializeField] private Transform       choiceContainer;
        [SerializeField] private GameObject      choiceButtonPrefab;
        [SerializeField] private float           slideSpeed   = 0.4f;
        [SerializeField] private float           panelOpacity = 0.88f;
        [SerializeField] private Color           panelColor   = new Color(0.08f, 0.08f, 0.08f, 1f);

        private GameManager      gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();
        private bool             eventShown    = false;

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();

            if (panelRect == null)
                panelRect = GetComponent<RectTransform>();

            // Apply opacity to panel background
            if (panelBackground != null)
            {
                Color c = panelColor;
                c.a = panelOpacity;
                panelBackground.color = c;
            }
        }

        private void Start()
        {
            // Only move off-screen if ShowEvent hasn't already been called
            if (!eventShown)
                MoveOffScreen();
        }

        /// <summary>Displays a game event: sets text, builds choices, slides in.</summary>
        public void ShowEvent(GameEvent gameEvent)
        {
            eventShown = true;

            Debug.Log($"[SlidingPanel] ShowEvent id={gameEvent.id} text='{gameEvent.text?.Substring(0, Mathf.Min(30, gameEvent.text?.Length ?? 0))}...'");
            Debug.Log($"[SlidingPanel] narrativeText={narrativeText}, choiceContainer={choiceContainer}, prefab={choiceButtonPrefab}");

            if (narrativeText != null)
                narrativeText.text = gameEvent.text;
            else
                Debug.LogError("[SlidingPanel] narrativeText is NULL!");

            string pos = string.IsNullOrEmpty(gameEvent.text_position) ? "right" : gameEvent.text_position;
            ApplyLayout(pos, gameEvent.panel_width);

            BuildChoices(gameEvent.choices);

            StopAllCoroutines();
            StartCoroutine(SlideIn(null));
        }

        /// <summary>Sets panel anchor and width based on text_position and panel_width from JSON.</summary>
        private void ApplyLayout(string position, float widthRatio)
        {
            if (panelRect == null) return;

            float w = 1920f * (widthRatio > 0f ? widthRatio : 0.33f);
            panelRect.sizeDelta = new Vector2(w, panelRect.sizeDelta.y);

            switch (position)
            {
                case "left":
                    panelRect.anchorMin = new Vector2(0f,   0f);
                    panelRect.anchorMax = new Vector2(0f,   1f);
                    panelRect.pivot     = new Vector2(0f,   0.5f);
                    break;
                case "center":
                    panelRect.anchorMin = new Vector2(0.5f, 0f);
                    panelRect.anchorMax = new Vector2(0.5f, 1f);
                    panelRect.pivot     = new Vector2(0.5f, 0.5f);
                    break;
                default: // right
                    panelRect.anchorMin = new Vector2(1f,   0f);
                    panelRect.anchorMax = new Vector2(1f,   1f);
                    panelRect.pivot     = new Vector2(1f,   0.5f);
                    break;
            }
        }

        /// <summary>Destroys old choice buttons and instantiates new ones from prefab.</summary>
        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0) return;

            if (choiceButtonPrefab == null)
            {
                Debug.LogWarning("[SlidingPanel] choiceButtonPrefab not assigned!");
                return;
            }

            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                btnObj.SetActive(true);

                ChoiceButton cb = btnObj.GetComponent<ChoiceButton>();
                if (cb != null)
                {
                    int next = choice.next_event; // capture for lambda
                    cb.Setup(choice.text, () => gameManager.MakeChoice(next));
                }

                activeButtons.Add(btnObj);
            }
        }

        /// <summary>Slides the panel out then calls onComplete.</summary>
        public void Hide(Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(SlideOut(onComplete));
        }

        private IEnumerator SlideIn(Action onComplete)
        {
            Vector2 start   = panelRect.anchoredPosition;
            Vector2 target  = new Vector2(0f, 0f); // anchored at right edge, so 0 = fully visible
            float   elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            panelRect.anchoredPosition = target;
            onComplete?.Invoke();
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            Vector2 start   = panelRect.anchoredPosition;
            Vector2 target  = new Vector2(panelRect.sizeDelta.x + 100f, 0f);
            float   elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            panelRect.anchoredPosition = target;
            onComplete?.Invoke();
        }

        private void MoveOffScreen()
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(panelRect.sizeDelta.x + 100f, 0f);
        }
    }
}