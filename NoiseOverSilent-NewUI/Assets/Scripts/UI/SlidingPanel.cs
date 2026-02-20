// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 2.2
// CHANGES : v2.2 - 2026-02-16 - Added debug logs for choice clicks
//           v2.1 - 2026-02-16 - Panel 400px, slide positions updated
//           v2.0 - 2026-02-16 - Panel width 600px, slide positions updated
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
        [SerializeField] private float           slideSpeed = 0.4f;

        private GameManager      gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
                Debug.LogError("[SlidingPanel v2.2] GameManager NOT FOUND!");
            else
                Debug.Log($"[SlidingPanel v2.2] GameManager found: {gameManager.name}");
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            StopAllCoroutines();
            StartCoroutine(Show(gameEvent));
        }

        private IEnumerator Show(GameEvent gameEvent)
        {
            yield return null;
            yield return null;

            if (panelBackground != null)
                panelBackground.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            if (narrativeText != null)
            {
                narrativeText.text  = gameEvent.text;
                narrativeText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                narrativeText.alpha = 1f;
                narrativeText.enabled = false;
                narrativeText.enabled = true;
                narrativeText.ForceMeshUpdate();
                Debug.Log($"[SlidingPanel v2.2] Text='{narrativeText.text}' Color={narrativeText.color} Alpha={narrativeText.alpha}");
            }

            BuildChoices(gameEvent.choices);

            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(1f, 0f);
                panelRect.anchorMax = new Vector2(1f, 1f);
                panelRect.pivot     = new Vector2(1f, 0.5f);
                panelRect.sizeDelta = new Vector2(400f, 0f);

                Vector2 startPos = new Vector2(500f, 0f);
                Vector2 endPos   = Vector2.zero;
                float   elapsed  = 0f;

                panelRect.anchoredPosition = startPos;

                while (elapsed < slideSpeed)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                    panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    yield return null;
                }

                panelRect.anchoredPosition = endPos;
                Debug.Log($"[SlidingPanel v2.2] Slide complete. pos={panelRect.anchoredPosition}");
            }
        }

        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0 || choiceButtonPrefab == null)
            {
                Debug.Log($"[SlidingPanel v2.2] No choices to build");
                return;
            }

            Debug.Log($"[SlidingPanel v2.2] Building {choices.Count} choices, gameManager={(gameManager != null ? "OK" : "NULL")}");

            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                btnObj.SetActive(true);
                ChoiceButton cb = btnObj.GetComponent<ChoiceButton>();
                if (cb != null)
                {
                    int next = choice.next_event;
                    Debug.Log($"[SlidingPanel v2.2] Setting up choice '{choice.text}' → event {next}");
                    cb.Setup(choice.text, () => {
                        Debug.Log($"[SlidingPanel v2.2] Choice callback fired! next={next}, gameManager={(gameManager != null ? "OK" : "NULL")}");
                        if (gameManager != null)
                            gameManager.MakeChoice(next);
                        else
                            Debug.LogError("[SlidingPanel v2.2] gameManager is NULL in callback!");
                    });
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
            Vector2 start   = panelRect.anchoredPosition;
            Vector2 end     = new Vector2(500f, 0f);
            float   elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            panelRect.anchoredPosition = end;
            onComplete?.Invoke();
        }
    }
}