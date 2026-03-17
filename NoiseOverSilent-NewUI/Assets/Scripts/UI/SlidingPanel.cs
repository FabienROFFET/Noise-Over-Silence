// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// UPDATED : 2026-03-17 (v3.7)
// CHANGES : v3.7 - Added SetTextOnly() — sets bottom text without
//                  animating the choice panel. Used by GameManager
//                  when choice hotspots are on screen.
// DESC    : Sliding panel — right-side choices + bottom narrative text
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
        [SerializeField] private float           slideSpeed    = 0.4f;
        [SerializeField] private float           panelOpacity  = 0.88f;
        [SerializeField] private bool            useTypewriter = true;
        [SerializeField] private TypewriterEffect typewriter;

        private GameManager      gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();
        private bool             isAnimating   = false;

        public void SetGameManager(GameManager gm)
        {
            gameManager = gm;
            Debug.Log($"[SlidingPanel v3.7] GameManager set: {(gameManager != null ? gameManager.name : "NULL")}");
        }

        private void Awake()
        {
            if (panelBackground != null)
            {
                Color c = panelBackground.color;
                c.a = panelOpacity;
                panelBackground.color = c;
            }
            SetOffScreen();
        }

        // ── PUBLIC API ─────────────────────────────────────────────────────

        /// <summary>
        /// Full show: writes text to bottom strip, builds choice buttons,
        /// and slides the choice panel in from the right.
        /// </summary>
        public void ShowEvent(GameEvent gameEvent)
        {
            StopAllCoroutines();
            isAnimating = false;
            StartCoroutine(Show(gameEvent));
        }

        /// <summary>
        /// Sets ONLY the bottom narrative text — does not slide the choice
        /// panel in. Call this when choice hotspots handle navigation instead.
        /// </summary>
        public void SetTextOnly(GameEvent gameEvent)
        {
            if (gameEvent == null) return;

            // Write text synchronously — no coroutine, no yield
            if (narrativeText != null)
            {
                if (useTypewriter && typewriter != null)
                {
                    narrativeText.text = "";
                    typewriter.TypeText(gameEvent.text);
                }
                else
                {
                    narrativeText.text = gameEvent.text;
                }
            }

            // Clear any leftover choice buttons — they're not needed
            ClearButtons();

            Debug.Log("[SlidingPanel v3.7] SetTextOnly — panel stays off-screen.");
        }

        public void Hide(Action onComplete = null)
        {
            StopAllCoroutines();
            isAnimating = false;
            StartCoroutine(SlideOut(onComplete));
        }

        // ── INTERNALS ──────────────────────────────────────────────────────

        private IEnumerator Show(GameEvent gameEvent)
        {
            yield return null;
            yield return null;

            if (panelBackground != null)
                panelBackground.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            if (narrativeText != null)
            {
                narrativeText.color   = new Color(0.9f, 0.9f, 0.9f, 1f);
                narrativeText.alpha   = 1f;
                narrativeText.enabled = false;
                narrativeText.enabled = true;
                narrativeText.ForceMeshUpdate();

                if (useTypewriter && typewriter != null)
                {
                    narrativeText.text = "";
                    typewriter.TypeText(gameEvent.text);
                }
                else
                {
                    narrativeText.text = gameEvent.text;
                }

                Debug.Log($"[SlidingPanel v3.7] Text shown (typewriter={(useTypewriter ? "ON" : "OFF")})");
            }

            BuildChoices(gameEvent.choices);

            if (panelRect != null)
            {
                SoundManager.PlayPanelSlide();

                Vector2 startPos = new Vector2(450f, 0f);
                Vector2 endPos   = Vector2.zero;
                float   elapsed  = 0f;

                panelRect.anchoredPosition = startPos;
                isAnimating = true;

                while (elapsed < slideSpeed)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                    panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    yield return null;
                }

                panelRect.anchoredPosition = endPos;
                isAnimating = false;
            }
        }

        private void BuildChoices(List<Choice> choices)
        {
            ClearButtons();
            if (choices == null || choices.Count == 0) return;

            int index = 0;
            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                btnObj.name       = $"ChoiceButton_{index + 1}";
                btnObj.SetActive(true);

                ChoiceButton cb = btnObj.GetComponent<ChoiceButton>();
                if (cb != null)
                {
                    Choice current = choice;
                    cb.Setup(choice.text, () => {
                        if (gameManager != null)
                            gameManager.MakeChoice(current);
                    });
                }

                TextMeshProUGUI textComponent = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    RectTransform textRect = textComponent.GetComponent<RectTransform>();
                    textRect.offsetMin = new Vector2(10f, 5f);
                    textRect.offsetMax = new Vector2(-10f, -5f);
                }

                activeButtons.Add(btnObj);
                index++;
            }
        }

        private void ClearButtons()
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            if (panelRect == null) yield break;

            Vector2 start   = panelRect.anchoredPosition;
            Vector2 end     = new Vector2(450f, 0f);
            float   elapsed = 0f;
            isAnimating     = true;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            panelRect.anchoredPosition = end;
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