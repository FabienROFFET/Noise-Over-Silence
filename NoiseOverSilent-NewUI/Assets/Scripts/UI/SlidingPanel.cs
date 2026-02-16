// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
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

        private GameManager      gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            Debug.Log($"[SlidingPanel] ShowEvent called. narrativeText={(narrativeText != null ? "OK" : "NULL")}");

            // Force panel visible
            if (panelRect != null)
            {
                panelRect.anchorMin        = new Vector2(1f, 0f);
                panelRect.anchorMax        = new Vector2(1f, 1f);
                panelRect.pivot            = new Vector2(1f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.sizeDelta        = new Vector2(640f, 0f);
            }

            // Force panel background color — Image alpha resets to 0 at runtime
            if (panelBackground != null)
                panelBackground.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

            if (narrativeText != null)
            {
                narrativeText.text  = gameEvent.text;
                narrativeText.color = new Color(0.88f, 0.88f, 0.88f, 1f);
                narrativeText.alpha = 1f;
                narrativeText.ForceMeshUpdate();
                Debug.Log($"[SlidingPanel] Text set to: {narrativeText.text}");
            }
            else
            {
                Debug.LogError("[SlidingPanel] narrativeText IS NULL!");
            }

            BuildChoices(gameEvent.choices);

            StopAllCoroutines();
            StartCoroutine(SlideIn());
        }

        private IEnumerator SlideIn()
        {
            // Start from right side off screen
            Vector2 startPos = new Vector2(740f, 0f);
            Vector2 endPos   = Vector2.zero;

            panelRect.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            panelRect.anchoredPosition = endPos;
        }

        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0) return;
            if (choiceButtonPrefab == null) return;

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
            StartCoroutine(SlideOut(onComplete));
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            Vector2 startPos = panelRect.anchoredPosition;
            Vector2 endPos   = new Vector2(740f, 0f);
            float   elapsed  = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = endPos;
            onComplete?.Invoke();
        }
    }
}