// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 2.1
// CHANGES : v1.6 - 2026-02-16 - Restored dark panel color, confirmed working
//           v2.1 - 2026-02-16 - Panel 400px, slide positions updated
//           v2.0 - 2026-02-16 - Panel width 600px, slide positions updated
//           v1.9 - 2026-02-16 - Slide animation enabled, panel width 800px
//           v1.8 - 2026-02-16 - Updated for 800px panel width
//           v1.7 - 2026-02-16 - Force TMP redraw via enable toggle
//           v1.6 - 2026-02-16 - Slide animation restored, dark panel color
//           v1.5 - 2026-02-16 - RED panel visibility test
//           v1.4 - 2026-02-16 - 2-frame delay coroutine, no MoveOffScreen, force color/alpha
//           v1.3 - 2026-02-16 - Forced panel background color, ForceMeshUpdate
//           v1.2 - 2026-02-16 - Fixed Start() vs ShowEvent() race condition
//           v1.1 - 2026-02-15 - Added ShowEvent(), Hide(callback), ApplyLayout()
//           v1.0 - 2026-02-14 - Initial version
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
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            StopAllCoroutines();
            StartCoroutine(Show(gameEvent));
        }

        private IEnumerator Show(GameEvent gameEvent)
        {
            // Wait 2 frames for everything to settle
            yield return null;
            yield return null;

            // Panel background — bright red for visibility test
            if (panelBackground != null)
                panelBackground.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            // Text — set AFTER layout settles
            if (narrativeText != null)
            {
                narrativeText.text  = gameEvent.text;
                narrativeText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                narrativeText.alpha = 1f;
                narrativeText.enabled = false;
                narrativeText.enabled = true; // force TMP redraw
                narrativeText.ForceMeshUpdate();
                Debug.Log($"[SlidingPanel v2.1] Text='{narrativeText.text}' Color={narrativeText.color} Alpha={narrativeText.alpha}");
            }
            else
            {
                Debug.LogError("[SlidingPanel v2.1] narrativeText IS NULL");
            }

            // Choices
            BuildChoices(gameEvent.choices);

            // Slide in from right
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(1f, 0f);
                panelRect.anchorMax = new Vector2(1f, 1f);
                panelRect.pivot     = new Vector2(1f, 0.5f);
                panelRect.sizeDelta = new Vector2(400f, 0f);

                // Animate from off-screen to visible
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
                Debug.Log($"[SlidingPanel v2.1] Slide complete. pos={panelRect.anchoredPosition}");
            }
        }

        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0 || choiceButtonPrefab == null) return;

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
            Vector2 start   = panelRect.anchoredPosition;
            Vector2 end     = new Vector2(500f, 0f);
            float   elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                panelRect.anchoredPosition = Vector2.Lerp(start, end,
                    Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed));
                yield return null;
            }

            panelRect.anchoredPosition = end;
            onComplete?.Invoke();
        }
    }
}